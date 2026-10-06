using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API_GateWay.Middleware;


public sealed class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

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

    private static (int StatusCode, string Title, string SafeMessage) MapException(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => ( StatusCodes.Status404NotFound, "Không tìm thấy tài nguyên",
            "Không tìm thấy dữ liệu được yêu cầu."),
            ArgumentException => (  StatusCodes.Status400BadRequest,  "Dữ liệu không hợp lệ", "Dữ liệu yêu cầu không hợp lệ."),
            UnauthorizedAccessException => ( StatusCodes.Status403Forbidden, "Không có quyền truy cập",   "Bạn không có quyền thực hiện thao tác này."), => (
                StatusCodes.Status500InternalServerError,
                "Lỗi hệ thống nội bộ",
                "Đã xảy ra lỗi phía máy chủ. Vui lòng thử lại sau.")
        };
    }
}
