using System.Globalization;
using System.Security.Cryptography;
using API_ThuNgan.Services;
using DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

namespace API_ThuNgan.Controllers;

[ApiController]
[Route("api/QuanLyBanHang/payos")]
public sealed class PayOs_Controller : ControllerBase
{
    private readonly PayOsPayment_DAL _paymentDal;
    private readonly PayOsService _payOs;
    private readonly GoQrImageService _goQrImage;
    private readonly ILogger<PayOs_Controller> _logger;

    public PayOs_Controller(
        PayOsPayment_DAL paymentDal,
        PayOsService payOs,
        GoQrImageService goQrImage,
        ILogger<PayOs_Controller> logger)
    {
        _paymentDal = paymentDal;
        _payOs = payOs;
        _goQrImage = goQrImage;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,ThuNgan")]
    [Route("create")]
    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePayOsPaymentRequest? request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.MaHoaDon))
            return BadRequest(new { success = false, message = "Mã hóa đơn không được để trống." });

        string maHoaDon = request.MaHoaDon.Trim();
        if (maHoaDon.Length > 15)
            return BadRequest(new { success = false, message = "Mã hóa đơn không được vượt quá 15 ký tự." });

        if (!_payOs.IsConfigured)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { success = false, message = "Backend chưa được cấu hình thông tin kết nối PayOS." });

        try
        {
            var existing = _paymentDal.GetLatestPayOsByInvoice(maHoaDon);
            if (existing.Rows.Count > 0)
            {
                var row = existing.Rows[0];
                string status = row["TRANGTHAI"]?.ToString()?.Trim() ?? "";
                if (status == "Đã thanh toán")
                    return Conflict(new { success = false, message = "Hóa đơn này đã được thanh toán." });

                if (status == "Chưa thanh toán" && row["PAYOS_ORDER_CODE"] != DBNull.Value &&
                    row["PAYOS_CHECKOUT_URL"] != DBNull.Value && row["PAYOS_QR_CODE"] != DBNull.Value)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            maHoaDon,
                            orderCode = Convert.ToInt64(row["PAYOS_ORDER_CODE"]),
                            amount = Convert.ToInt32(Math.Round(Convert.ToDecimal(row["SOTIENTHANHTOAN"]))),
                            paymentLinkId = row["PAYOS_PAYMENT_LINK_ID"]?.ToString()?.Trim(),
                            checkoutUrl = row["PAYOS_CHECKOUT_URL"]?.ToString()?.Trim(),
                            qrCode = row["PAYOS_QR_CODE"]?.ToString(),
                            qrImageDataUrl = await TaoAnhQrAnToanAsync(
                                row["PAYOS_QR_CODE"]?.ToString(), maHoaDon, cancellationToken),
                            qrImageUrl = TaoUrlQrAnToan(row["PAYOS_QR_CODE"]?.ToString(), maHoaDon),
                            trangThai = status
                        }
                    });
                }
            }

            decimal? dueAmount = _paymentDal.GetInvoiceAmountDue(maHoaDon);
            if (!dueAmount.HasValue)
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn hoặc hóa đơn chưa có chi tiết sản phẩm." });

            decimal roundedAmount = Math.Round(dueAmount.Value, 0, MidpointRounding.AwayFromZero);
            if (roundedAmount <= 0 || roundedAmount > int.MaxValue)
                return BadRequest(new { success = false, message = "Số tiền hóa đơn không hợp lệ để tạo thanh toán QR." });

            int amount = decimal.ToInt32(roundedAmount);
            long orderCode = TaoMaDonHang();
            string maThanhToan = "P" + orderCode.ToString(CultureInfo.InvariantCulture);

            if (!_paymentDal.CreatePendingPayment(maThanhToan, maHoaDon, amount, orderCode))
                return Conflict(new { success = false, message = "Không thể tạo giao dịch; hóa đơn có thể đã được thanh toán." });

            try
            {
                // Dùng mã hóa đơn làm nội dung chuyển khoản để người bán dễ đối soát.
                // Một số kênh ngân hàng giới hạn description 9 ký tự; mã dài hơn dùng
                // mã rút gọn sinh từ OrderCode, còn webhook vẫn ghép chính xác bằng OrderCode.
                string description = maHoaDon.Length <= 9
                    ? maHoaDon
                    : "HD" + (orderCode % 10_000_000).ToString("D7", CultureInfo.InvariantCulture);
                var link = await _payOs.CreatePaymentLinkAsync(orderCode, amount, description, cancellationToken);

                if (link.OrderCode != orderCode || link.Amount != amount ||
                    !_paymentDal.SavePayOsLink(orderCode, link.PaymentLinkId, link.CheckoutUrl, link.QrCode))
                    throw new InvalidOperationException("Không thể lưu thông tin link thanh toán PayOS.");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        maHoaDon,  orderCode,  amount,
                        paymentLinkId = link.PaymentLinkId,
                        checkoutUrl = link.CheckoutUrl,
                        qrCode = link.QrCode,
                        qrImageDataUrl = await TaoAnhQrAnToanAsync(link.QrCode, maHoaDon, cancellationToken),
                        qrImageUrl = TaoUrlQrAnToan(link.QrCode, maHoaDon),
                        trangThai = "Chưa thanh toán"
                    }
                });
            }
            catch
            {
                _paymentDal.MarkPayOsLinkFailed(orderCode);
                throw;
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("Chưa cấu hình PayOS", StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { success = false, message = "Backend chưa được cấu hình thông tin kết nối PayOS." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không tạo được link PayOS cho hóa đơn {MaHoaDon}.", maHoaDon);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { success = false, message = "Không thể tạo mã QR thanh toán qua PayOS. Hóa đơn vẫn ở trạng thái chưa thanh toán." });
        }
    }



    [Authorize(Roles = "Admin,ThuNgan")]
    [Route("status")]
    [HttpGet]
    public async Task<IActionResult> GetPaymentStatus([FromQuery] string? maHoaDon, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon) || maHoaDon.Trim().Length > 15)
            return BadRequest(new { success = false, message = "Mã hóa đơn không hợp lệ." });

        try
        {
            var rows = _paymentDal.GetLatestPayOsByInvoice(maHoaDon.Trim());
            if (rows.Rows.Count == 0)
                return Ok(new { success = true, data = new { maHoaDon = maHoaDon.Trim(), trangThai = "Chưa thanh toán" } });

            var row = rows.Rows[0];
            string trangThai = row["TRANGTHAI"]?.ToString()?.Trim() ?? "Chưa thanh toán";

            // Webhook là luồng chính. Nếu webhook không tới được (ví dụ backend chạy
            // local), hỏi PayOS trực tiếp để đối soát và cập nhật giao dịch đã trả tiền.
            if (trangThai == "Chưa thanh toán" && _payOs.IsConfigured &&
                row["PAYOS_ORDER_CODE"] != DBNull.Value)
            {
                try
                {
                    long orderCode = Convert.ToInt64(row["PAYOS_ORDER_CODE"]);
                    int expectedAmount = checked((int)Math.Round(
                        Convert.ToDecimal(row["SOTIENTHANHTOAN"]), MidpointRounding.AwayFromZero));
                    string savedLinkId = row["PAYOS_PAYMENT_LINK_ID"] == DBNull.Value
                        ? string.Empty
                        : row["PAYOS_PAYMENT_LINK_ID"]?.ToString()?.Trim() ?? string.Empty;
                    var payOsLink = await _payOs.GetPaymentLinkAsync(orderCode);

                    if (payOsLink.OrderCode == orderCode &&
                        (string.IsNullOrEmpty(savedLinkId) || string.Equals(savedLinkId, payOsLink.Id, StringComparison.Ordinal)) &&
                        payOsLink.Status == PayOS.Models.V2.PaymentRequests.PaymentLinkStatus.Paid &&
                        payOsLink.Amount == expectedAmount &&
                        payOsLink.AmountPaid == expectedAmount &&
                        payOsLink.AmountRemaining == 0)
                    {
                        var transaction = payOsLink.Transactions?.FirstOrDefault(t => t.Amount > 0);
                        if (transaction != null || payOsLink.AmountPaid == expectedAmount)
                        {
                            DateTime paidAt = DateTime.TryParse(
                                transaction?.TransactionDateTime,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeLocal,
                                out var parsedPaidAt)
                                ? parsedPaidAt
                                : DateTime.Now;

                            if (_paymentDal.ConfirmPayOsPayment(
                                orderCode,
                                expectedAmount,
                                string.IsNullOrWhiteSpace(transaction?.Reference) ? payOsLink.Id : transaction.Reference,
                                payOsLink.Id,
                                paidAt))
                            {
                                trangThai = "Đã thanh toán";
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Trạng thái SQL hiện tại vẫn được trả về; lần polling sau sẽ thử lại.
                    _logger.LogWarning(ex, "Không đối soát được trạng thái PayOS của hóa đơn {MaHoaDon}.", maHoaDon);
                }
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    maHoaDon = row["MAHDBAN"]?.ToString()?.Trim(),
                    trangThai,
                    soTien = row["SOTIENTHANHTOAN"] == DBNull.Value ? 0 : Convert.ToDecimal(row["SOTIENTHANHTOAN"]),
                    maGiaoDichNganHang = row["PAYOS_TRANSACTION_REFERENCE"] == DBNull.Value
                        ? null
                        : row["PAYOS_TRANSACTION_REFERENCE"]?.ToString()?.Trim()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không đọc được trạng thái PayOS của hóa đơn {MaHoaDon}.", maHoaDon);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "Không thể lấy trạng thái thanh toán." });
        }
    }

 
    [AllowAnonymous]
    [Route("webhook")]
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] Webhook? webhook, CancellationToken cancellationToken)
    {
        if (webhook == null)
            return BadRequest(new { success = false, message = "Dữ liệu webhook không hợp lệ." });

        WebhookData data;
        try
        {
            data = await _payOs.VerifyWebhookAsync(webhook);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayOS gửi webhook có chữ ký không hợp lệ.");
            return BadRequest(new { success = false, message = "Chữ ký webhook không hợp lệ." });
        }

        long orderCode = data.OrderCode;
        if (data.Amount > int.MaxValue)
            return BadRequest(new { success = false, message = "Số tiền webhook vượt phạm vi hỗ trợ." });

        int amount = checked((int)data.Amount);
        string paymentLinkId = data.PaymentLinkId ?? string.Empty;
        if (orderCode <= 0 || amount <= 0 || string.IsNullOrWhiteSpace(paymentLinkId))
            return BadRequest(new { success = false, message = "Dữ liệu webhook PayOS thiếu trường bắt buộc." });

        bool paymentSucceeded = webhook.Success && webhook.Code == "00" && data.Code == "00";

        if (!paymentSucceeded)
            return Ok(new { success = true, message = "Webhook đã được nhận; giao dịch chưa thành công." });

        string reference = !string.IsNullOrWhiteSpace(data.Reference)
            ? data.Reference
            : paymentLinkId;
        DateTime paidAt = !string.IsNullOrWhiteSpace(data.TransactionDateTime) &&
                          DateTime.TryParse(data.TransactionDateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedDate)
            ? parsedDate
            : DateTime.Now;

        try
        {
            if (!_paymentDal.ConfirmPayOsPayment(orderCode, amount, reference, paymentLinkId, paidAt))
            {
                _logger.LogWarning("Webhook PayOS hợp lệ nhưng không khớp giao dịch đã tạo. OrderCode={OrderCode}, Amount={Amount}.", orderCode, amount);
              
                return Ok(new { success = true, message = "Webhook đã nhận; không có giao dịch phù hợp để cập nhật." });
            }

            return Ok(new { success = true, message = "Đã xác nhận thanh toán." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi cập nhật thanh toán từ webhook PayOS. OrderCode={OrderCode}.", orderCode);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "Lỗi khi ghi nhận webhook thanh toán." });
        }
    }

    private static long TaoMaDonHang()
    {
       
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        ulong random = BitConverter.ToUInt64(bytes);
        return (long)(10_000_000_000_000UL + random % 90_000_000_000_000UL);
    }

    private async Task<string?> TaoAnhQrAnToanAsync(string? qrPayload,  string maHoaDon,   CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(qrPayload))
            return null;

        try
        {
            return await _goQrImage.TaoAnhPngDataUrlAsync(qrPayload, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "goQR không tạo được ảnh QR cho hóa đơn {MaHoaDon}.", maHoaDon);
            return null;
        }
    }

    private string? TaoUrlQrAnToan(string? qrPayload, string maHoaDon)
    {
        if (string.IsNullOrWhiteSpace(qrPayload))
            return null;

        try
        {
            return _goQrImage.TaoUrlAnhQr(qrPayload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không tạo được URL ảnh QR dự phòng cho hóa đơn {MaHoaDon}.", maHoaDon);
            return null;
        }
    }

}

public sealed class CreatePayOsPaymentRequest
{
    public string? MaHoaDon { get; set; }
}
