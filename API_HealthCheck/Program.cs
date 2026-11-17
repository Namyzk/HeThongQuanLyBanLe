using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services
    .AddHealthChecks()
    .AddCheck("application", () => HealthCheckResult.Healthy("Health Check API đang hoạt động."), tags: ["live"])
    .AddCheck("database", new SqlServerHealthCheck(connectionString), tags: ["ready"]);

var app = builder.Build();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = WriteHealthResponseAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponseAsync
});

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthResponseAsync
});

app.Run();

static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json; charset=utf-8";
    context.Response.StatusCode = report.Status == HealthStatus.Healthy
        ? StatusCodes.Status200OK
        : StatusCodes.Status503ServiceUnavailable;

    var body = new
    {
        status = report.Status.ToString(),
        checkedAtUtc = DateTimeOffset.UtcNow,
        totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
            description = entry.Value.Description
        })
    };

    return JsonSerializer.SerializeAsync(context.Response.Body, body);
}

sealed class SqlServerHealthCheck(string? connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy("Chưa cấu hình ConnectionStrings:DefaultConnection.");
        }

        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.ConnectTimeout = Math.Min(builder.ConnectTimeout, 3);

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 3;
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("Kết nối cơ sở dữ liệu hoạt động.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Không đưa chi tiết kết nối hoặc thông tin xác thực ra endpoint công khai.
            return HealthCheckResult.Unhealthy("Không thể kết nối hoặc truy vấn cơ sở dữ liệu.");
        }
    }
}
