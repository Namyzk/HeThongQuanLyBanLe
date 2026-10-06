using DAL.DataHelper;
using API_GateWay.Middleware;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.IO.Compression;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

Connect.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddOcelot();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Cấu hình Cors:AllowedOrigins là bắt buộc ngoài môi trường Development.");

    allowedOrigins = new[]
    {
        "http://localhost:3000",
        "http://localhost:5173",
        "https://localhost:3001",
        "https://localhost:5173",
        "http://localhost:5500",
        "http://localhost:5501",
        "http://127.0.0.1:5500",
        "http://127.0.0.1:5501"
    };
}

builder.Services.AddCors(options => options.AddPolicy("GatewayCors", policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "application/problem+json" });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    options.Level = CompressionLevel.Fastest);

var jwt = builder.Configuration.GetSection("Jwt");
var secretKey = jwt["Key"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key.");
if (Encoding.UTF8.GetByteCount(secretKey) < 32)
    throw new InvalidOperationException("Jwt:Key phải có tối thiểu 32 byte.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"] ?? "QUANLYBANLE_API",
            ValidateAudience = true,
            ValidAudience = jwt["Audience"] ?? "QUANLYBANLE_CLIENT",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

app.UseResponseCompression();
app.UseCors("GatewayCors");
app.UseMiddleware<GlobalExceptionMiddleware>();

.
app.Use(async (context, next) =>
{
    string path = context.Request.Path.Value ?? string.Empty;
    bool isRefreshOrLogout = path.Equals("/api/Login/refresh", StringComparison.OrdinalIgnoreCase) ||
                             path.Equals("/api/Login/logout", StringComparison.OrdinalIgnoreCase);
    bool isUnsafeMethod = !HttpMethods.IsGet(context.Request.Method) &&
                          !HttpMethods.IsHead(context.Request.Method) &&
                          !HttpMethods.IsOptions(context.Request.Method);
    bool hasSessionCookie = context.Request.Cookies.ContainsKey("qlbl_access") ||
                            context.Request.Cookies.ContainsKey("qlbl_refresh");

    if (hasSessionCookie && isUnsafeMethod)
    {
        string origin = context.Request.Headers["Origin"].ToString();
        if (string.IsNullOrWhiteSpace(origin) ||
            !allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { success = false, message = "Origin không được phép." });
            return;
        }
    }

    if (!context.Request.Headers.ContainsKey("Authorization") &&
        context.Request.Cookies.TryGetValue("qlbl_access", out string? accessToken) &&
        !string.IsNullOrWhiteSpace(accessToken))
    {
        context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
    }

    if (isRefreshOrLogout && context.Request.Cookies.TryGetValue("qlbl_refresh", out string? refreshToken))
        context.Request.Headers["Cookie"] = $"qlbl_refresh={refreshToken}";
    else
        context.Request.Headers.Remove("Cookie");

   
    context.Request.Headers.Remove("Origin");

    await next();
});
app.UseAuthentication();
app.UseRateLimiter();
await app.UseOcelot();
await app.RunAsync();

public partial class Program
{
}

