namespace API_ThuNgan.Services;

public sealed class PayOsOptions
{
    public string? ClientId { get; set; }
    public string? ApiKey { get; set; }
    public string? ChecksumKey { get; set; }
    public string? PartnerCode { get; set; }
    public string ReturnUrl { get; set; } = "http://127.0.0.1:5500/?thanhtoan=payos";
    public string CancelUrl { get; set; } = "http://127.0.0.1:5500/?thanhtoan=huy";
}
