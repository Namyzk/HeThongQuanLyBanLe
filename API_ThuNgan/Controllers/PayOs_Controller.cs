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
            return BadRequest(new { success = false, message = "MÃ£ hÃ³a Ä‘Æ¡n khÃ´ng Ä‘Æ°á»£c Ä‘á»ƒ trá»‘ng." });

        string maHoaDon = request.MaHoaDon.Trim();
        if (maHoaDon.Length > 15)
            return BadRequest(new { success = false, message = "MÃ£ hÃ³a Ä‘Æ¡n khÃ´ng Ä‘Æ°á»£c vÆ°á»£t quÃ¡ 15 kÃ½ tá»±." });

        if (!_payOs.IsConfigured)
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { success = false, message = "Backend chÆ°a Ä‘Æ°á»£c cáº¥u hÃ¬nh thÃ´ng tin káº¿t ná»‘i PayOS." });

        try
        {
            var existing = _paymentDal.GetLatestPayOsByInvoice(maHoaDon);
            if (existing.Rows.Count > 0)
            {
                var row = existing.Rows[0];
                string status = row["TRANGTHAI"]?.ToString()?.Trim() ?? "";
                if (status == "ÄÃ£ thanh toÃ¡n")
                    return Conflict(new { success = false, message = "HÃ³a Ä‘Æ¡n nÃ y Ä‘Ã£ Ä‘Æ°á»£c thanh toÃ¡n." });

                if (status == "ChÆ°a thanh toÃ¡n" && row["PAYOS_ORDER_CODE"] != DBNull.Value &&
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
                return NotFound(new { success = false, message = "KhÃ´ng tÃ¬m tháº¥y hÃ³a Ä‘Æ¡n hoáº·c hÃ³a Ä‘Æ¡n chÆ°a cÃ³ chi tiáº¿t sáº£n pháº©m." });

            decimal roundedAmount = Math.Round(dueAmount.Value, 0, MidpointRounding.AwayFromZero);
            if (roundedAmount <= 0 || roundedAmount > int.MaxValue)
                return BadRequest(new { success = false, message = "Sá»‘ tiá»n hÃ³a Ä‘Æ¡n khÃ´ng há»£p lá»‡ Ä‘á»ƒ táº¡o thanh toÃ¡n QR." });

            int amount = decimal.ToInt32(roundedAmount);
            long orderCode = TaoMaDonHang();
            string maThanhToan = "P" + orderCode.ToString(CultureInfo.InvariantCulture);

            if (!_paymentDal.CreatePendingPayment(maThanhToan, maHoaDon, amount, orderCode))
                return Conflict(new { success = false, message = "KhÃ´ng thá»ƒ táº¡o giao dá»‹ch; hÃ³a Ä‘Æ¡n cÃ³ thá»ƒ Ä‘Ã£ Ä‘Æ°á»£c thanh toÃ¡n." });

            try
            {
                // DÃ¹ng mÃ£ hÃ³a Ä‘Æ¡n lÃ m ná»™i dung chuyá»ƒn khoáº£n Ä‘á»ƒ ngÆ°á»i bÃ¡n dá»… Ä‘á»‘i soÃ¡t.
                // Má»™t sá»‘ kÃªnh ngÃ¢n hÃ ng giá»›i háº¡n description 9 kÃ½ tá»±; mÃ£ dÃ i hÆ¡n dÃ¹ng
                // mÃ£ rÃºt gá»n sinh tá»« OrderCode, cÃ²n webhook váº«n ghÃ©p chÃ­nh xÃ¡c báº±ng OrderCode.
                string description = maHoaDon.Length <= 9
                    ? maHoaDon
                    : "HD" + (orderCode % 10_000_000).ToString("D7", CultureInfo.InvariantCulture);
                var link = await _payOs.CreatePaymentLinkAsync(orderCode, amount, description, cancellationToken);

                if (link.OrderCode != orderCode || link.Amount != amount ||
                    !_paymentDal.SavePayOsLink(orderCode, link.PaymentLinkId, link.CheckoutUrl, link.QrCode))
                    throw new InvalidOperationException("KhÃ´ng thá»ƒ lÆ°u thÃ´ng tin link thanh toÃ¡n PayOS.");

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
                        trangThai = "ChÆ°a thanh toÃ¡n"
                    }
                });
            }
            catch
            {
                _paymentDal.MarkPayOsLinkFailed(orderCode);
                throw;
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.StartsWith("ChÆ°a cáº¥u hÃ¬nh PayOS", StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { success = false, message = "Backend chÆ°a Ä‘Æ°á»£c cáº¥u hÃ¬nh thÃ´ng tin káº¿t ná»‘i PayOS." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KhÃ´ng táº¡o Ä‘Æ°á»£c link PayOS cho hÃ³a Ä‘Æ¡n {MaHoaDon}.", maHoaDon);
            return StatusCode(StatusCodes.Status502BadGateway,
                new { success = false, message = "KhÃ´ng thá»ƒ táº¡o mÃ£ QR thanh toÃ¡n qua PayOS. HÃ³a Ä‘Æ¡n váº«n á»Ÿ tráº¡ng thÃ¡i chÆ°a thanh toÃ¡n." });
        }
    }



    [Authorize(Roles = "Admin,ThuNgan")]
    [Route("status")]
    [HttpGet]
    public async Task<IActionResult> GetPaymentStatus([FromQuery] string? maHoaDon, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(maHoaDon) || maHoaDon.Trim().Length > 15)
            return BadRequest(new { success = false, message = "MÃ£ hÃ³a Ä‘Æ¡n khÃ´ng há»£p lá»‡." });

        try
        {
            var rows = _paymentDal.GetLatestPayOsByInvoice(maHoaDon.Trim());
            if (rows.Rows.Count == 0)
                return Ok(new { success = true, data = new { maHoaDon = maHoaDon.Trim(), trangThai = "ChÆ°a thanh toÃ¡n" } });

            var row = rows.Rows[0];
            string trangThai = row["TRANGTHAI"]?.ToString()?.Trim() ?? "ChÆ°a thanh toÃ¡n";

            // Webhook lÃ  luá»“ng chÃ­nh. Náº¿u webhook khÃ´ng tá»›i Ä‘Æ°á»£c (vÃ­ dá»¥ backend cháº¡y
            // local), há»i PayOS trá»±c tiáº¿p Ä‘á»ƒ Ä‘á»‘i soÃ¡t vÃ  cáº­p nháº­t giao dá»‹ch Ä‘Ã£ tráº£ tiá»n.
            if (trangThai == "ChÆ°a thanh toÃ¡n" && _payOs.IsConfigured &&
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
                                trangThai = "ÄÃ£ thanh toÃ¡n";
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
                    // Tráº¡ng thÃ¡i SQL hiá»‡n táº¡i váº«n Ä‘Æ°á»£c tráº£ vá»; láº§n polling sau sáº½ thá»­ láº¡i.
                    _logger.LogWarning(ex, "KhÃ´ng Ä‘á»‘i soÃ¡t Ä‘Æ°á»£c tráº¡ng thÃ¡i PayOS cá»§a hÃ³a Ä‘Æ¡n {MaHoaDon}.", maHoaDon);
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
            _logger.LogError(ex, "KhÃ´ng Ä‘á»c Ä‘Æ°á»£c tráº¡ng thÃ¡i PayOS cá»§a hÃ³a Ä‘Æ¡n {MaHoaDon}.", maHoaDon);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "KhÃ´ng thá»ƒ láº¥y tráº¡ng thÃ¡i thanh toÃ¡n." });
        }
    }

 
    [AllowAnonymous]
    [Route("webhook")]
    [HttpPost]
    public async Task<IActionResult> ReceiveWebhook([FromBody] Webhook? webhook, CancellationToken cancellationToken)
    {
        if (webhook == null)
            return BadRequest(new { success = false, message = "Dá»¯ liá»‡u webhook khÃ´ng há»£p lá»‡." });

        WebhookData data;
        try
        {
            data = await _payOs.VerifyWebhookAsync(webhook);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayOS gá»­i webhook cÃ³ chá»¯ kÃ½ khÃ´ng há»£p lá»‡.");
            return BadRequest(new { success = false, message = "Chá»¯ kÃ½ webhook khÃ´ng há»£p lá»‡." });
        }

        long orderCode = data.OrderCode;
        if (data.Amount > int.MaxValue)
            return BadRequest(new { success = false, message = "Sá»‘ tiá»n webhook vÆ°á»£t pháº¡m vi há»— trá»£." });

        int amount = checked((int)data.Amount);
        string paymentLinkId = data.PaymentLinkId ?? string.Empty;
        if (orderCode <= 0 || amount <= 0 || string.IsNullOrWhiteSpace(paymentLinkId))
            return BadRequest(new { success = false, message = "Dá»¯ liá»‡u webhook PayOS thiáº¿u trÆ°á»ng báº¯t buá»™c." });

        bool paymentSucceeded = webhook.Success && webhook.Code == "00" && data.Code == "00";

        if (!paymentSucceeded)
            return Ok(new { success = true, message = "Webhook Ä‘Ã£ Ä‘Æ°á»£c nháº­n; giao dá»‹ch chÆ°a thÃ nh cÃ´ng." });

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
                _logger.LogWarning("Webhook PayOS há»£p lá»‡ nhÆ°ng khÃ´ng khá»›p giao dá»‹ch Ä‘Ã£ táº¡o. OrderCode={OrderCode}, Amount={Amount}.", orderCode, amount);
              
                return Ok(new { success = true, message = "Webhook Ä‘Ã£ nháº­n; khÃ´ng cÃ³ giao dá»‹ch phÃ¹ há»£p Ä‘á»ƒ cáº­p nháº­t." });
            }

            return Ok(new { success = true, message = "ÄÃ£ xÃ¡c nháº­n thanh toÃ¡n." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lá»—i cáº­p nháº­t thanh toÃ¡n tá»« webhook PayOS. OrderCode={OrderCode}.", orderCode);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "Lá»—i khi ghi nháº­n webhook thanh toÃ¡n." });
        }
    }

    private static long TaoMaDonHang()
    {
       
        Span<byte> bytes = stackalloc byte[8];
        RandomMuÛm¢G§²ÚîÆ­yÒ&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚&FVÆWFRÖ6†—F–WF&â"•ĞĞ¢´‡GGFVÆWFUĞĞ¢V&Æ–2”7F–öå&W7VÇBFVÆWFT6†•F–WB…´g&öÕVW'•Ò7G&–æsòÖ„D"Â´g&öÕVW'•Ò7G&–æsòÖ5Ğ¢°Ğ¢–b…fÆ–FFTÖ†Ö„D"Â$Ü:2Œ;6Ijâ"ÂRÂ÷WB7G&–ær6ÆVä„B’—2”7F–öå&W7VÇBW'$„B’&WGW&âW'$„C°Ğ¢–b…fÆ–FFTÖ†Ö5Â$Ü:2>ª6âª–Ò"ÂRÂ÷WB7G&–ær6ÆVå5’—2”7F–öå&W7VÇBW'%5’&WGW&âW'%5°Ğ Ğ¢G'Ğ¢°Ğ¢&ööÂ&W7VÇBÒö7F$&ÆÂå†ö†6ÆVä„BÂ6ÆVå5“°Ğ¢–b‚&W7VÇBĞ¢&WGW&âæ÷Df÷VæB†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$¶Œ;FærL:ÆÒFªW’6†’F«÷B,:âI¸2Œ;6â"Ò“°Ğ Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂÖW76vRÒ%Œ;66†’F«÷B,:âFŒ:æ‚<;Færâ"Ò“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚&vWBÖÆÂ×F†æ‡Föâ"•ĞĞ¢´‡GGvWEĞĞ¢V&Æ–2”7F–öå&W7VÇBvWDÆÅõF†æ…Föâ‚Ğ¢°Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒ÷GD&ÆÂævWDÆÂ‚“°Ğ¢f"Æ—7BÒFôF–7F–öæ'”Æ—7B†GB“°Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂ6÷VçBÒÆ—7Bä6÷VçBÂFFÒÆ—7BÒ“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ Ğ¢µ&÷WFR‚&vWBÖ'”–B×F†æ‡Föâ"•ĞĞ¢´‡GGvWEĞĞ¢V&Æ–2”7F–öå&W7VÇBvWEõF†æ…Föåô'”–B…´g&öÕVW'•Ò7G&–æsòÖĞ¢°Ğ¢–b…fÆ–FFTÖ†ÖÂ$Ü:2F†æ‚Fü:â"ÂRÂ÷WB7G&–ær6ÆVäÖ’—2”7F–öå&W7VÇBfÄW'"Ğ¢&WGW&âfÄW'#°Ğ Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒ÷GD&ÆÂävWD'”–B†6ÆVäÖ“°Ğ¢f"Æ—7BÒFôF–7F–öæ'”Æ—7B†GB“°Ğ Ğ¢–b†Æ—7Bä6÷VçBÓÒĞ¢&WGW&âæ÷Df÷VæB†æWr²7V66W72ÒfÇ6RÂÖW76vRÒB$¶Œ;FærL:ÆÒFªW’F†æ‚Fü:â<;2Ü:2w¶6ÆVäÖÒrâ"Ò“°Ğ Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂFFÒÆ—7E³ÒÒ“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚&–ç6W'B×F†æ‡Föâ"•ĞĞ¢´‡GG÷7EĞĞ¢V&Æ–2”7F–öå&W7VÇB7&VFUF†æ…Föâ…´g&öÔ&öG•ÒF†æ…FöãòÖöFVÂĞ¢°Ğ¢–b†ÖöFVÂÓÒçVÆÂĞ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$NºòÆ¸wRF†æ‚Fü:â¶Œ;FærIkº62I¸2G.¹ærâ"Ò“°Ğ Ğ¢–b…fÆ–FFTÖ†ÖöFVÂäÖF†æ…FöâÂ$Ü:2F†æ‚Fü:â"ÂRÂ÷WB7G&–ær6ÆVäÖEB’—2”7F–öå&W7VÇBW'%EB’&WGW&âW'%EC°Ğ¢–b…fÆ–FFTÖ†ÖöFVÂäÖ„D&âÂ$Ü:2Œ;6Ijâ"ÂRÂ÷WB7G&–ær6ÆVäÖ„B’—2”7F–öå&W7VÇBW'$„B’&WGW&âW'$„C°Ğ¢–b…fÆ–FFTÖ†ÖöFVÂå‡VöæuF‡V2Â%ŒkjærFº–2F†æ‚Fü:â"ÂSÂ÷WB7G&–ær6ÆVåB’—2”7F–öå&W7VÇBW'%B’&WGW&âW'%C°Ğ¢–b…fÆ–FFTÖ†ÖöFVÂåG&æuF†’Â%G.ªærFŒ:’F†æ‚Fü:â"ÂSÂ÷WB7G&–ær6ÆVåEB’—2”7F–öå&W7VÇBW'%7FGW2’&WGW&âW'%7FGW3°Ğ¢–b‚F†æ…Föåô$ÄÂåG&æuF†”†÷ÆR†6ÆVåEB’Ğ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ%G.ªærFŒ:’F†æ‚Fü:â¶Œ;Færº7Î¸râ"Ò“°Ğ Ğ¢–b†ÖöFVÂå6õF–VåF†æ…FöâÃÒĞ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ%>¹F¸âF†æ‚Fü:âª6’Î¹¶âŒjââ"Ò“°Ğ Ğ¢ÖöFVÂäÖF†æ…FöâÒ6ÆVäÖEC°Ğ¢ÖöFVÂäÖ„D&âÒ6ÆVäÖ„C°Ğ¢ÖöFVÂå‡VöæuF‡V2Ò6ÆVåC°Ğ¢ÖöFVÂåG&æuF†’Ò6ÆVåEC°Ğ Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒ÷GD&ÆÂävWD'”–B†ÖöFVÂäÖF†æ…Föâ“°Ğ¢–b†GBÒçVÆÂbbGBå&÷w2ä6÷VçBâĞ¢&WGW&â6öæfÆ–7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Ü:2F†æ‚Fü:âI:2N¹6âNª’â"Ò“°Ğ Ğ¢÷GD&ÆÂä7&VFR†ÖöFVÂ“°Ğ¢&WGW&â7FGW46öFRƒ#ÂæWr²7V66W72ÒG'VRÂÖW76vRÒ%FŒ:¦ÒFŒ;FærF–âF†æ‚Fü:âFŒ:æ‚<;Færâ"Ò“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚'WFFR×F†æ‡Föâ"•ĞĞ¢´‡GGWEĞĞ¢V&Æ–2”7F–öå&W7VÇBWFFUF†æ…Föâ…´g&öÔ&öG•ÒF†æ…FöãòÖöFVÂĞ¢°Ğ¢–b†ÖöFVÂÓÒçVÆÂĞ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$NºòÆ¸wRF†æ‚Fü:â¶Œ;FærIkº62I¸2G.¹ærâ"Ò“°Ğ Ğ¢–b…fÆ–FFTÖ†ÖöFVÂäÖF†æ…FöâÂ$Ü:2F†æ‚Fü:â"ÂRÂ÷WB7G&–ær6ÆVäÖEB’—2”7F–öå&W7VÇBW'%EB’&WGW&âW'%EC°Ğ¢–b…fÆ–FFTÖ†ÖöFVÂå‡VöæuF‡V2Â%ŒkjærFº–2F†æ‚Fü:â"ÂSÂ÷WB7G&–ær6ÆVåB’—2”7F–öå&W7VÇBW'%B’&WGW&âW'%C°Ğ¢–b…fÆ–FFTÖ†ÖöFVÂåG&æuF†’Â%G.ªærFŒ:’F†æ‚Fü:â"ÂSÂ÷WB7G&–ær6ÆVåEB’—2”7F–öå&W7VÇBW'%7FGW2’&WGW&âW'%7FGW3°Ğ¢–b‚F†æ…Föåô$ÄÂåG&æuF†”†÷ÆR†6ÆVåEB’Ğ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ%G.ªærFŒ:’F†æ‚Fü:â¶Œ;Færº7Î¸râ"Ò“°Ğ Ğ¢ÖöFVÂäÖF†æ…FöâÒ6ÆVäÖEC°Ğ¢ÖöFVÂå‡VöæuF‡V2Ò6ÆVåC°Ğ¢ÖöFVÂåG&æuF†’Ò6ÆVåEC°Ğ Ğ¢–b†ÖöFVÂå6õF–VåF†æ…FöâÃÒĞ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ%>¹F¸âF†æ‚Fü:âª6’Î¹¶âŒjââ"Ò“°Ğ¢–b†ÖöFVÂäæv•F†æ…FöâÓÒFVfVÇBĞ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$æ|:’F†æ‚Fü:â¶Œ;Færº7Î¸râ"Ò“°Ğ Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒ÷GD&ÆÂävWD'”–B†ÖöFVÂäÖF†æ…Föâ“°Ğ¢–b†GBÓÒçVÆÂÇÂGBå&÷w2ä6÷VçBÓÒĞ¢&WGW&âæ÷Df÷VæB†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$¶Œ;FærN¹6âNª’F†æ‚Fü:â<;2Ü:2ì:’â"Ò“°Ğ Ğ¢÷GD&ÆÂåWFFR†ÖöFVÂ“°Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂÖW76vRÒ%F†’I¹V’FŒ;FærF–âF†æ‚Fü:âFŒ:æ‚<;Færâ"Ò“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚&FVÂ×F†æ‡Föâ"•ĞĞ¢´‡GGFVÆWFUĞĞ¢V&Æ–2”7F–öå&W7VÇBFVÆWFUF†æ…Föâ…´g&öÕVW'•Ò7G&–æsòÖĞ¢°Ğ¢–b…fÆ–FFTÖ†ÖÂ$Ü:2F†æ‚Fü:â"ÂRÂ÷WB7G&–ær6ÆVäÖ’—2”7F–öå&W7VÇBfÄW'"Ğ¢&WGW&âfÄW'#°Ğ Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒ÷GD&ÆÂävWD'”–B†6ÆVäÖ“°Ğ¢–b†GBÓÒçVÆÂÇÂGBå&÷w2ä6÷VçBÓÒĞ¢&WGW&âæ÷Df÷VæB†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$¶Œ;FærL:ÆÒFªW’FŒ;FærF–âF†æ‚Fü:âI¸2Œ;6â"Ò“°Ğ Ğ¢÷GD&ÆÂäFVÆWFR†6ÆVäÖ“°Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂÖW76vRÒ%Œ;6FŒ;FærF–âF†æ‚Fü:âFŒ:æ‚<;Færâ"Ò“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ Ğ¢µ&÷WFR‚'&W6WB×6÷F–VçF†æ‡FöâÖ'’ÖÖ†F&â"•ĞĞ¢´‡GG÷7EĞĞ¢V&Æ–2”7F–öå&W7VÇB&W6WE6õF–VåF†æ…Föä'”†öFöâ…´g&öÕVW'•Ò7G&–æsòÖ„D&âÂ´g&öÕVW'•ÒFV6–ÖÂ6õF–VäÖö’Ğ¢°Ğ¢–b…fÆ–FFTÖ†Ö„D&âÂ$Ü:2Œ;6Ijâ,:â"ÂRÂ÷WB7G&–ær6ÆVäÖ’—2”7F–öå&W7VÇBfÄW'"Ğ¢&WGW&âfÄW'#°Ğ Ğ¢–b‡6õF–VäÖö’ÂĞ¢&WGW&â&E&WVW7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ%>¹F¸âŞ¹¶’¶Œ;FærIkº62æ¸òŒjââ"Ò“°Ğ Ğ¢G'Ğ¢°Ğ¢FFF&ÆRGBÒ÷GD&ÆÂå&W6WE6õF–Vä'”†öFöâ†6ÆVäÖÂ6õF–VäÖö’“°Ğ¢–b†GBÓÒçVÆÂÇÂGBå&÷w2ä6÷VçBÓÒĞ¢&WGW&âæ÷Df÷VæB†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$¶Œ;FærL:ÆÒFªW’F†æ‚Fü:â>ºvŒ;6IjâI¸2>ª×æª×Bâ"Ò“°Ğ Ğ¢FF6öÇVÖãòÖ÷VçD6öÇVÖâÒGBä6öÇVÖç2ä67CÄFF6öÇVÖãâ‚Ğ¢äf—'7D÷$FVfVÇB†6öÇVÖâÓâ7G&–æräWVÇ2†6öÇVÖâä6öÇVÖäæÖRÂ%4õD”TåD„ä…Dôâ"Â7G&–æt6ö×&—6öâä÷&F–æÄ–væ÷&T66R’“°Ğ¢–b†Ö÷VçD6öÇVÖâÓÒçVÆÂÇÂGBå&÷w2ä67CÄFF&÷sâ‚’äç’‡&÷rÓàĞ¢&÷u¶Ö÷VçD6öÇVÖåÒÓÒD$çVÆÂåfÇVRÇÀĞ¢ÖF‚ä'2„6öçfW'BåFôFV6–ÖÂ‡&÷u¶Ö÷VçD6öÇVÖåÒ’Ò6õF–VäÖö’’ãÒãÒ’Ğ¢°Ğ¢&WGW&â6öæfÆ–7B†æWr²7V66W72ÒfÇ6RÂÖW76vRÒ$¶Œ;FærF¸2Œ:2æªÖâ>¹F¸âF†æ‚Fü:â6R¶†’>ª×æª×Bâ"Ò“°Ğ¢ĞĞ Ğ¢f"Æ—7BÒFôF–7F–öæ'”Æ—7B†GB“°Ğ Ğ¢&WGW&âö²†æWr²7V66W72ÒG'VRÂÖW76vRÒ,I:2&W6WB>¹F¸âF†æ‚Fü:âF†VòŒ;6Ijââ"ÂFFÒÆ—7BÒ“°Ğ¢ĞĞ¢6F6‚„W†6WF–öâW‚Ğ¢°Ğ¢&WGW&â7FGW46öFRƒSÂæWr²7V66W72ÒfÇ6RÂÖW76vRÒ$Î¹v’¸rF¹ærâ"Ò“°Ğ¢ĞĞ¢ĞĞ¢ĞĞ§ĞĞ Ğ 