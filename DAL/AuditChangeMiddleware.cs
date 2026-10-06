using DAL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace DAL.Middleware;


/// Ghi audit cho các request có khả năng làm thay đổi dữ liệu.
/// Chỉ lưu các mã định danh đã chọn, không lưu toàn bộ body hoặc thông tin bí mật.

public sealed class AuditChangeMiddleware
{
    private const int MaxAuditRequestBodyBytes = 256 * 1024;
    private readonly RequestDelegate _next;
    private readonly string _serviceName;

    public AuditChangeMiddleware(RequestDelegate next, string serviceName)
    {
        _next = next;
        _serviceName = serviceName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var operation = GetOperation(request.Method, request.Path.Value);
        if (operation == null)
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        string? recordKey = await FindRecordKeyAsync(request);
        Exception? requestException = null;
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            requestException = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = requestException == null
                ? context.Response.StatusCode
                : StatusCodes.Status500InternalServerError;
            string result = await GetResultAsync(responseBuffer, statusCode, requestException);

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;

            try
            {
                var identity = context.User;
                var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? identity.FindFirst("sub")?.Value
                    ?? identity.FindFirst("MaTaiKhoan")?.Value;
                var userName = identity.FindFirst(ClaimTypes.Name)?.Value
                    ?? identity.Identity?.Name;
                var role = identity.FindFirst(ClaimTypes.Role)?.Value
                    ?? identity.FindFirst("role")?.Value;

                var descriptor = GetEntityDescriptor(request.Path.Value);
                await context.RequestServices.GetRequiredService<AuditLog_DAL>().WriteChangeAsync(
                    serviceName: _serviceName,
                    method: request.Method,
                    path: request.Path.Value ?? "/",
                    statusCode: statusCode,
                    operationType: operation,
                    entityName: descriptor.EntityName,
                    recordKey: recordKey,
                    result: result,
                    userId: userId,
                    userName: userName,
                    role: role,
                    clientIp: context.Connection.RemoteIpAddress?.ToString(),
                    traceId: context.TraceIdentifier,
                    elapsedMilliseconds: stopwatch.ElapsedMilliseconds,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception auditException)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<AuditChangeMiddleware>>();
                logger.LogError(auditException, "Không ghi được audit log cho thao tác {Operation} tại {Path}, TraceId {TraceId}.",
                    operation, request.Path.Value, context.TraceIdentifier);
            }
        }
    }

    private static string? GetOperation(string method, string? path)
    {
        var route = path ?? string.Empty;
        if (HttpMethods.IsPut(method) || HttpMethods.IsPatch(method))
            return "UPDATE";
        if (HttpMethods.IsDelete(method))
            return "DELETE";
        if (HttpMethods.IsPost(method))
        {
            var lowered = route.ToLowerInvariant();
            return lowered.Contains("update") || lowered.Contains("reset") || lowered.Contains("capnhat")
                ? "UPDATE"
                : "CREATE";
        }

        return null;
    }

    private static (string EntityName, string[] Keys) GetEntityDescriptor(string? path)
    {
        var route = (path ?? string.Empty).ToLowerInvariant();
        if (route.Contains("chitietban")) return ("CHITIETBAN", new[] { "MAHDBAN", "MASP" });
        if (route.Contains("chitietnhap")) return ("CHITIETNHAP", new[] { "MAPHIEUNHAP", "MASP" });
        if (route.Contains("phieunhap")) return ("PHIEUNHAPKHO", new[] { "MAPHIEUNHAP" });
        if (route.Contains("hoadonban")) return ("HOADONBAN", new[] { "MAHDBAN" });
        if (route.Contains("thanhtoan")) return ("THANHTOAN", new[] { "MATHANHTOAN", "MAHDBAN" });
        if (route.Contains("nhacungcap")) return ("NHACUNGCAP", new[] { "MANCC" });
        if (route.Contains("khachhang")) return ("KHACHHANG", new[] { "MAKH" });
        if (route.Contains("nhanvien")) return ("NHANVIEN", new[] { "MANV" });
        if (route.Contains("khuyenmai")) return ("KHUYENMAI", new[] { "MAKM" });
        if (route.Contains("taikhoan")) return ("TAIKHOAN", new[] { "MATAIKHOAN" });
        if (route.Contains("sanpham")) return ("SANPHAM", new[] { "MASP" });
        if (route.Contains("danhmuc")) return ("DANHMUC", new[] { "MADANHMUC" });

        var segments = (path ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
        var entity = segments.Length > 1 ? segments[1] : "UNKNOWN";
        return (entity.Length > 128 ? entity[..128] : entity, Array.Empty<string>());
    }

    private static async Task<string?> FindRecordKeyAsync(HttpRequest request)
    {
        var descriptor = GetEntityDescriptor(request.Path.Value);
        var values = new List<string>();

        foreach (var key in descriptor.Keys)
        {
            var queryValue = request.Query.FirstOrDefault(item =>
                string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase)).Value.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(queryValue))
                values.Add($"{key}={Limit(queryValue.Trim(), 100)}");
        }

        if (values.Count < descriptor.Keys.Length && request.ContentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true)
        {
            try
            {
                if (request.ContentLength is > MaxAuditRequestBodyBytes)
                    return JoinKeys(values);

                request.EnableBuffering(bufferThreshold: 30 * 1024, bufferLimit: MaxAuditRequestBodyBytes);
                request.Body.Position = 0;
                using var document = await JsonDocument.ParseAsync(request.Body, cancellationToken: CancellationToken.None);
                foreach (var key in descriptor.Keys)
                {
                    if (values.Any(value => value.StartsWith(key + "=", StringComparison.OrdinalIgnoreCase)))
                        continue;

                    if (TryFindJsonValue(document.RootElement, key, out var value) && !string.IsNullOrWhiteSpace(value))
                        values.Add($"{key}={Limit(value.Trim(), 100)}");
                }
            }
            catch (JsonException)
            {
                
            }
            catch (IOException)
            {
                
            }
            finally
            {
                if (request.Body.CanSeek)
                    request.Body.Position = 0;
            }
        }

        return JoinKeys(values);
    }

    private static bool TryFindJsonValue(JsonElement element, string propertyName, out string? value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.String)
                {
                    value = property.Value.GetString();
                    return true;
                }

                if (TryFindJsonValue(property.Value, propertyName, out value))
                    return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryFindJsonValue(item, propertyName, out value))
                    return true;
            }
        }

        value = null;
        return false;
    }

    private static string? JoinKeys(List<string> values) => values.Count == 0 ? null : string.Join(";", values);

    private static async Task<string> GetResultAsync(MemoryStream response, int statusCode, Exception? exception)
    {
        if (exception != null || statusCode >= 400)
            return "FAILED";

        if (response.Length == 0 || response.Length > 1024 * 1024)
            return "SUCCESS";

        try
        {
            response.Position = 0;
            using var document = await JsonDocument.ParseAsync(response, cancellationToken: CancellationToken.None);
            if (TryFindSuccessFalse(document.RootElement))
                return "FAILED";
        }
        catch (JsonException)
        {
            // Một số API trả thông báo dạng text; khi HTTP status thành công xem là thành công.
        }
        finally
        {
            response.Position = 0;
        }

        return "SUCCESS";
    }

    private static bool TryFindSuccessFalse(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "success", StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.False)
                    return true;
                if (TryFindSuccessFalse(property.Value))
                    return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                if (TryFindSuccessFalse(item)) return true;
        }

        return false;
    }

    private static string Limit(string value, int length) => value.Length <= length ? value : value[..length];
}



