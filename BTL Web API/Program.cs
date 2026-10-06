using DAL.Middleware;
using DAL;
using BLL;
using DAL.DataHelper;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<AuditLog_DAL>();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        var key = jwt["Key"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key.");
        if (System.Text.Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("Jwt:Key phải có tối thiểu 32 byte.");

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
        };
    });
builder.Services.AddAuthorization();

// ============ CORS DEV ============
const string PermissiveDevCors = "PermissiveDevCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    if (!builder.Environment.IsDevelopment())
        throw new InvalidOperationException("Cấu hình Cors:AllowedOrigins là bắt buộc ngoài môi trường Development.");
    allowedOrigins = new[] { 
        "http://localhost:3000", 
        "http://localhost:5173", 
        "https://localhost:3001", 
        "https://localhost:5173" 
    };
}
builder.Services.AddCors(options =>
{
    options.AddPolicy(PermissiveDevCors, policy =>
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

// ============ Controllers / Swagger ============
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Cấu hình Swagger có nút Authorize (ổ khóa) để test Token
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "QUANLYBANLE API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập trực tiếp chuỗi token (không cần chữ Bearer)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DI registrations
builder.Services.AddScoped<ChiTietBan_DAL>();
builder.Services.AddScoped<ChiTietNhap_DAL>();
builder.Services.AddScoped<DanhMuc_DAL>();
builder.Services.AddScoped<HoaDonBan_DAL>();
builder.Services.AddScoped<KhachHang_DAL>();
builder.Services.AddScoped<KhuyenMai_DAL>();
builder.Services.AddScoped<NhaCungCap_DAL>();
builder.Services.AddScoped<NhanVien_DAL>();
builder.Services.AddScoped<PhieuNhapKho_DAL>();
builder.Services.AddScoped<SanPham_DAL>();
builder.Services.AddScoped<TaiKhoan_DAL>();
builder.Services.AddScoped<ThanhToan_DAL>();
builder.Services.AddScoped<ChiTietBan_BLL>();
builder.Services.AddScoped<ChiTietNhap_BLL>();
builder.Services.AddScoped<DanhMuc_BLL>();
builder.Services.AddScoped<HoaDonBan_BLL>();
builder.Services.AddScoped<KhachHang_BLL>();
builder.Services.AddScoped<KhuyenMai_BLL>();
builder.Services.AddScoped<NhaCungCap_BLL>();
builder.Services.AddScoped<NhanVien_BLL>();
builder.Services.AddScoped<PhieuNhapKho_BLL>();
builder.Services.AddScoped<SanPham_BLL>();
builder.Services.AddScoped<TaiKhoan_BLL>();
builder.Services.AddScoped<ThanhToan_BLL>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var remoteIp = context.Connection.RemoteIpAddress;
    if (remoteIp != null)
    {
        if (remoteIp.IsIPv4MappedToIPv6)
            remoteIp = remoteIp.MapToIPv4();

        if (!System.Net.IPAddress.IsLoopback(remoteIp))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("API này chỉ nhận request từ Gateway nội bộ.");
            return;
        }
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//  CORS phải đứng TRƯỚC auth/authorization & MapControllers
app.UseCors(PermissiveDevCors);

Connect.ConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

//  Xác thực và Ủy quyền

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditChangeMiddleware>("API_Admin");

app.MapControllers();

app.Run();

