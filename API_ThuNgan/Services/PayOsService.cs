using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace API_ThuNgan.Services;

public sealed class PayOsService
{
    private readonly PayOSClient? _client;
    private readonly PayOsOptions _options;

    public PayOsService(IOptions<PayOsOptions> options)
    {
        _options = options.Value;
        if (IsConfigured)
        {
            _client = new PayOSClient(_options.ClientId!, _options.ApiKey!, _options.ChecksumKey!);
        }
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ClientId) &&
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !string.IsNullOrWhiteSpace(_options.ChecksumKey);

    public async Task<PayOsPaymentLink> CreatePaymentLinkAsync(
        long orderCode, int amount, string description, CancellationToken cancellationToken)
    {
        var client = GetClient();
        var request = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = amount,
            Description = description,
            ReturnUrl = RequireAbsoluteUrl(_options.ReturnUrl, "PayOS:ReturnUrl"),
            CancelUrl = RequireAbsoluteUrl(_options.CancelUrl, "PayOS:CancelUrl")
        };

        var result = await client.PaymentRequests.CreateAsync(request);
        if (result.OrderCode != orderCode || result.Amount != amount ||
            string.IsNullOrWhiteSpace(result.PaymentLinkId) ||
            string.IsNullOrWhiteSpace(result.CheckoutUrl) ||
            string.IsNullOrWhiteSpace(result.QrCode))
            throw new InvalidOperationException("PayOS không trả về đủ thông tin link thanh toán.");

        return new PayOsPaymentLink(
            result.OrderCode, checked((int)result.Amount), result.PaymentLinkId,
            result.CheckoutUrl, result.QrCode);
    }

    public Task<WebhookData> VerifyWebhookAsync(Webhook webhook)
    {
        var client = GetClient();
        return client.Webhooks.VerifyAsync(webhook);
    }

    private PayOSClient GetClient() => _client ??
        throw new InvalidOperationException("Chưa cấu hình PayOS ClientId, ApiKey và ChecksumKey.");

    private static string RequireAbsoluteUrl(string? value, string key)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException($"Cấu hình {key} phải là URL HTTP/HTTPS hợp lệ.");
        return uri.ToString();
    }
}

public sealed record PayOsPaymentLink(long OrderCode, int Amount, string PaymentLinkId, string CheckoutUrl, string QrCode);
