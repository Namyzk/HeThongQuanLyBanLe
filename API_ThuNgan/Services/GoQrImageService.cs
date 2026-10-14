namespace API_ThuNgan.Services;


/// Chuyển chuỗi QR do PayOS trả về thành ảnh PNG bằng API goQR.

public sealed class GoQrImageService(HttpClient httpClient)
{
    private const int MaxPayloadLength = 900;
    private const int MaxImageBytes = 2 * 1024 * 1024;

    public async Task<string> TaoAnhPngDataUrlAsync(string qrPayload, CancellationToken cancellationToken)
    {
        string url = TaoUrlAnhQr(qrPayload);

        using var response = await httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        if (response.Content.Headers.ContentType?.MediaType is not "image/png")
            throw new InvalidOperationException("goQR không trả về ảnh PNG hợp lệ.");

        if (response.Content.Headers.ContentLength is > MaxImageBytes)
            throw new InvalidOperationException("Ảnh QR do goQR trả về vượt quá kích thước cho phép.");

        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (imageBytes.Length == 0 || imageBytes.Length > MaxImageBytes)
            throw new InvalidOperationException("Ảnh QR do goQR trả về rỗng hoặc quá lớn.");

        return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
    }

    
    /// Trả URL ảnh goQR để trình duyệt có thể tải trực tiếp nếu backend không
    /// truy cập được dịch vụ goQR.
  
    public string TaoUrlAnhQr(string qrPayload)
    {
        if (string.IsNullOrWhiteSpace(qrPayload))
            throw new ArgumentException("Dữ liệu QR không được để trống.", nameof(qrPayload));

        if (qrPayload.Length > MaxPayloadLength)
            throw new ArgumentException("Dữ liệu QR vượt quá giới hạn hỗ trợ.", nameof(qrPayload));

        return "https://api.qrserver.com/v1/create-qr-code/" +
               "?size=320x320&format=png&charset-source=UTF-8&data=" +
               Uri.EscapeDataString(qrPayload);
    }
}
