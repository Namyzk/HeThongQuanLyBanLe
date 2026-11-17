using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API_GateWay.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Cấu hình chống Brute-Force cho POST /api/Login/login
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(3);
    private const int MaxLoginBodyChars = 16 * 1024;
    private static readonly ConcurrentDictionary<string, AttemptBucket> Buckets = new(StringComparer.Ordinal);

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            if (HttpMethods.IsPost(context.Request.Method) &&
                string.Equals(context.Request.Path.Value, "/api/Login/login", StringComparison.OrdinalIgnoreCase))
            {
                await HandleLoginWithBruteForceProtectionAsync(context);
                return;
            }

            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unhandled Gateway exception for {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, context.Request.Path, context.TraceIdentifier);

            if (context.Response.HasStarted)
                throw;

            var (statusCode, title, safeMessage) = MapException(exception);
            var message = _environment.IsDevelopment() ? exception.ToString() : safeMessage;

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json; charset=utf-8";
            var responseBody = new
            {
                success = false,
                title,
                status = statusCode,
                message,
                instance = context.Request.Path.Value,
                traceId = context.TraceIdentifier
            };

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                responseBody,
                JsonOptions,
                context.RequestAborted);
        }
    }

    private async Task HandleLoginWithBruteForceProtectionAsync(HttpContext context)
    {
        string username = await ReadUsernameAsync(context.Request);
        var remoteIp = context.Connection.RemoteIpAddress;
        string ip = remoteIp is null ? "unknown" :
            (remoteIp.IsIPv4MappedToIPv6 ? remoteIp.MapToIPv4() : remoteIp).ToString();
        string key = $"{ip}|{username.Trim().ToUpperInvariant()}";
        AttemptBucket bucket = Buckets.GetOrAdd(key, static _ => new AttemptBucket());
        DateTimeOffset now = DateTimeOffset.UtcNow;
        TimeSpan retryAfter = TimeSpan.Zero;

        lock (bucket.Sync)
        {
            if (bucket.LockedUntilUtc is DateTimeOffset lockedUntil)
            {
                if (lockedUntil > now)
                {
                    retryAfter = lockedUntil - now;
                }
                else
                {
                    bucket.LockedUntilUtc = null;
                    bucket.Failures.Clear();
                }
            }

            PruneExpired(bucket, now);
            if (retryAfter <= TimeSpan.Zero && bucket.Failures.Count + bucket.InFlight >= MaxFailedAttempts)
            {
                retryAfter = TimeSpan.FromSeconds(1);
            }
            else if (retryAfter <= TimeSpan.Zero)
            {
                bucket.InFlight++;
            }
        }

        if (retryAfter > TimeSpan.Zero)
        {
            int retrySeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = retrySeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Bạn đã đăng nhập sai quá nhiều lần. Vui lòng thử lại sau {retrySeconds} giây.",
                retryAfterSeconds = retrySeconds
            }, cancellationToken: context.RequestAborted);
            return;
        }

        try
        {
            await _next(context);
        }
        finally
        {
            DateTimeOffset completedAt = DateTimeOffset.UtcNow;
            lock (bucket.Sync)
            {
                bucket.InFlight = Math.Max(0, bucket.InFlight - 1);
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    PruneExpired(bucket, completedAt);
                    bucket.Failures.Enqueue(completedAt);
                    if (bucket.Failures.Count >= MaxFailedAttempts)
                        bucket.LockedUntilUtc = completedAt.Add(LockoutDuration);
                }
                else if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    bucket.Failures.Clear();
                    bucket.LockedUntilUtc = null;
                }
            }
        }
    }

    private static void PruneExpired(AttemptBucket bucket, DateTimeOffset now)
    {
        while (bucket.Failures.Count > 0 && now - bucket.Failures.Peek() >= FailureWindow)
            bucket.Failures.Dequeue();
    }

    private static async Task<string> ReadUsernameAsync(HttpRequest request)
    {
        if (request.ContentLength > MaxLoginBodyChars)
            return "[oversized-request]";
        request.EnableBuffering();
        if (!request.Body.CanSeek)
            return "[unknown]";

        long originalPosition = request.Body.Position;
        try
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            char[] buffer = new char[MaxLoginBodyChars + 1];
            int read = await reader.ReadBlockAsync(buffer.AsMemory(0, buffer.Length), request.HttpContext.RequestAborted);
            if (read == 0 || read > MaxLoginBodyChars)
                return "[invalid-request]";

            using JsonDocument document = JsonDocument.Parse(new string(buffer, 0, read));
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return "[invalid-request]";

            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (property.Name.Equals("username", StringComparison.OrdinalIgnoreCase) &&
                    property.Value.ValueKind == JsonValueKind.String)
                {
                    return property.Value.GetString()?.Trim() is { Length: > 0 } value
                        ? value
                        : "[empty-username]";
                }
            }
            return "[missing-username]";
        }
        catch (JsonException)
        {
            return "[invalid-request]";
        }
        finally
        {
            request.Body.Position = originalPosition;
        }
    }

    private static (int StatusCode, string Title, string SafeMessage) MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Không tìm thấy tài nguyên",
                "Không tìm thấy dữ liệu được yêu cầu."),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "Dữ liệu không hợp lệ",
                "Dữ liệu yêu cầu không hợp lệ."),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Không có quyền truy cập",
                "Bạn không có quyền thực hiện thao tác này."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Lỗi hệ thống nội bộ",
                "Đã xảy ra lỗi phía máy chủ. Vui lòng thử lại sau.")
        };
    }

    private sealed class AttemptBucket
    {
        public object Sync { get; } = new();
        public Queue<DateTimeOffset> Failures { get; } = new();
        public int InFlight { get; set; }
        public DateTimeOffset? LockedUntilUtc { get; set; }
    }
}
