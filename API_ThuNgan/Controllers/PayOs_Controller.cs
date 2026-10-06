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
    private readonly ILogger<PayOs_Controller> _logger;

    public PayOs_Controller(PayOsPayment_DAL paymentDal, PayOsService payOs, ILogger<PayOs_Controller> logger)
    {
        _paymentDal = paymentDal;
        _payOs = payOs;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,ThuNgan")]
    [HttpPost("create")]
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
                string description = "HD" + orderCode.ToString(CultureInfo.InvariantCulture);
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
    [HttpGet("status")]
    public IActionResult GetPaymentStatus([FromQuery] string? maHoaDon)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon) || maHoaDon.Trim().Length > 15)
            return BadRequest(new { success = false, message = "Mã hóa đơn không hợp lệ." });

        try
        {
            var rows = _paymentDal.GetLatestPayOsByInvoice(maHoaDon.Trim());
            if (rows.Rows.Count == 0)
                return Ok(new { success = true, data = new { maHoaDon = maHoaDon.Trim(), trangThai = "Chưa thanh toán" } });

            var row = rows.Rows[0];
            return Ok(new
            {
                success = true,
                data = new
                {
                    maHoaDon = row["MAHDBAN"]?.ToString()?.Trim(),
                    trangThai = row["TRANGTHAI"]?.ToString()?.Trim(),
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
    [HttpPost("webhook")]
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

}

public sealed class CreatePayOsPaymentRequest
{
    public string? MaHoaDon { get; set; }
}
