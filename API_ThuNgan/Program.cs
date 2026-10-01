using DAL.DataHelper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============ CORS DEV============
const string PermissiveDevCors = "PermissiveDevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(PermissiveDevCors, policy =>
        policy
            .SetIsOriginAllowed(origin => true)
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

// ============ JWT ============
var jwt = builder.Configuration.GetSection("Jwt");
string secretKey = jwt["Key"] ?? "QUANLYBANLE_SUPER_SECRET_KEY_2026_MIN_32_CHARS_LONG";
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false; // DEV
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"] ?? "QUANLYBANLE_API",
        ValidAudience = jwt["Audience"] ?? "QUANLYBANLE_CLIENT",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ❶ CORS phải đứng TRƯỚC auth/authorization & MapControllers
app.UseCors(PermissiveDevCors);

// Preflight cho proxy/hosting
app.Use(async (ctx, next) =>
{
    if (string.Equals(ctx.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        var origin = ctx.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin) || origin == "null")
        {
            ctx.Response.Headers["Access-Control-Allow-Origin"] = string.IsNullOrEmpty(origin) ? "null" : origin;
            ctx.Response.Headers["Vary"] = "Origin";
        }
        ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET,POST,PUT,PATCH,DELETE,OPTIONS";
        ctx.Response.Headers["Access-Control-Allow-Headers"] = "Authorization,Content-Type,Accept";
        ctx.Response.StatusCode = StatusCodes.Status204NoContent;
        return;
    }
    await next();
});
Connect.ConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

// ❷ Xác thực và Ủy quyền
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();