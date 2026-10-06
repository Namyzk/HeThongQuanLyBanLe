using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace API_Login.Controllers
{
    [Route("api/Login")]
    [Authorize]
    [ApiController]
    public class Login_Controller : ControllerBase
    {
        private readonly TaiKhoan_BLL _bll;
        private readonly DAL.RefreshToken_DAL _refreshTokenDal;
        private readonly IConfiguration _config;
        private readonly ILogger<Login_Controller> _logger;

        
        public Login_Controller(TaiKhoan_BLL bll, DAL.RefreshToken_DAL refreshTokenDal, IConfiguration config, ILogger<Login_Controller> logger)
        {
            _bll = bll;
            _refreshTokenDal = refreshTokenDal;
            _config = config;
            _logger = logger;
        }
        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Pass { get; set; } = string.Empty;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Pass))
                {
                    return BadRequest(new { success = false, message = "Vui lòng nhập tên đăng nhập và mật khẩu!" });
                }

                var list = _bll.DangNhap(request.Username.Trim(), request.Pass);
                if (list == null || list.Count == 0)
                {
                    return Unauthorized(new { success = false, message = "Sai tên đăng nhập hoặc mật khẩu!" });
                }

                var user = list.First();

                string token = GenerateJwtToken(user);
                string refreshToken = TaoRefreshToken();
                DateTime refreshExpiresAtUtc = DateTime.UtcNow.AddDays(LaySoNgayRefreshToken());
                if (!_refreshTokenDal.Insert(HashRefreshToken(refreshToken), user.MATAIKHOAN.Trim(), refreshExpiresAtUtc))
                    throw new InvalidOperationException("Không thể lưu refresh token.");

                DateTime accessExpiresAtUtc = LayThoiDiemHetHanAccessToken();
                DatCookiePhien(token, refreshToken, accessExpiresAtUtc, refreshExpiresAtUtc);

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công!",
                    refreshTokenExpiresAtUtc = refreshExpiresAtUtc,
                    data = new
                    {
                        MaTaiKhoan = user.MATAIKHOAN.Trim(),
                        UserName = user.USERNAME.Trim(),
                        Quyen = user.QUYEN,
                        RoleName = LayTenRole(user.QUYEN)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Đăng nhập thất bại do lỗi hệ thống.");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            try
            {
                if (!Request.Cookies.TryGetValue("qlbl_refresh", out string? oldRefreshToken) ||
                    string.IsNullOrWhiteSpace(oldRefreshToken))
                    return Unauthorized(new { success = false, message = "Refresh token không hợp lệ hoặc đã hết hạn." });

                string newRefreshToken = TaoRefreshToken();
                DateTime refreshExpiresAtUtc = DateTime.UtcNow.AddDays(LaySoNgayRefreshToken());
                string? accountId = _refreshTokenDal.Rotate(
                    HashRefreshToken(oldRefreshToken.Trim()),
                    HashRefreshToken(newRefreshToken),
                    refreshExpiresAtUtc);

                if (string.IsNullOrWhiteSpace(accountId))
                    return Unauthorized(new { success = false, message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });

                var user = _bll.LayTheoID(accountId).FirstOrDefault();
                if (user == null)
                    return Unauthorized(new { success = false, message = "Tài khoản không còn tồn tại." });

                string accessToken = GenerateJwtToken(user);
                DateTime accessExpiresAtUtc = LayThoiDiemHetHanAccessToken();
                DatCookiePhien(accessToken, newRefreshToken, accessExpiresAtUtc, refreshExpiresAtUtc);

                return Ok(new
                {
                    success = true,
                    message = "Làm mới phiên đăng nhập thành công.",
                    refreshTokenExpiresAtUtc = refreshExpiresAtUtc,
                    data = new
                    {
                        MaTaiKhoan = user.MATAIKHOAN.Trim(),
                        UserName = user.USERNAME.Trim(),
                        Quyen = user.QUYEN,
                        RoleName = LayTenRole(user.QUYEN)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Làm mới phiên đăng nhập thất bại.");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [AllowAnonymous]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (Request.Cookies.TryGetValue("qlbl_refresh", out string? refreshToken) &&
                !string.IsNullOrWhiteSpace(refreshToken))
            {
                try
                {
                    _refreshTokenDal.Revoke(HashRefreshToken(refreshToken.Trim()));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thu hồi được refresh token khi đăng xuất.");
                }
            }

            XoaCookiePhien();
            return Ok(new { success = true, message = "Đã đăng xuất." });
        }

        private void DatCookiePhien(string accessToken, string refreshToken, DateTime accessExpiresAtUtc, DateTime refreshExpiresAtUtc)
        {
            Response.Headers["Cache-Control"] = "no-store";
            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = new DateTimeOffset(accessExpiresAtUtc),
                IsEssential = true
            };
            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/api/Login",
                Expires = new DateTimeOffset(refreshExpiresAtUtc),
                IsEssential = true
            };

            Response.Cookies.Append("qlbl_access", accessToken, accessOptions);
            Response.Cookies.Append("qlbl_refresh", refreshToken, refreshOptions);
        }

        private void XoaCookiePhien()
        {
            Response.Cookies.Delete("qlbl_access", new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Path = "/"
            });
            Response.Cookies.Delete("qlbl_refresh", new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, Path = "/api/Login"
            });
        }

        private int LaySoNgayRefreshToken()
        {
            return int.TryParse(_config["Jwt:RefreshTokenExpiresInDays"], out int days) && days > 0 && days <= 90
                ? days
                : 7;
        }

        private static string TaoRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string HashRefreshToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }

        private string GenerateJwtToken(TaiKhoan user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var keyString = jwtSettings["Key"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key.");
            if (Encoding.UTF8.GetByteCount(keyString) < 32)
                throw new InvalidOperationException("Jwt:Key phải có tối thiểu 32 byte.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Gán Role dạng chuỗi để dùng được với [Authorize(Roles = "...")]
            string roleName = LayTenRole(user.QUYEN);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MATAIKHOAN.Trim()),
                new Claim(ClaimTypes.Name, user.USERNAME.Trim()),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("QUYEN", user.QUYEN.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }.Concat(LayScopes(user.QUYEN).Select(scope => new Claim("scope", scope))).ToArray();

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "QUANLYBANLE_API",
                audience: jwtSettings["Audience"] ?? "QUANLYBANLE_CLIENT",
                claims: claims,
                expires: LayThoiDiemHetHanAccessToken(),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private DateTime LayThoiDiemHetHanAccessToken()
        {
            var jwtSettings = _config.GetSection("Jwt");
            double minutes = double.TryParse(jwtSettings["ExpiresInMinutes"], out double configured) && configured > 0
                ? configured
                : 15;
            return DateTime.UtcNow.AddMinutes(minutes);
        }

        private static string LayTenRole(int quyen) => quyen switch
        {
            1 => "Admin",
            2 => "ThuNgan",
            3 => "ThuKho",
            4 => "KeToan",
            _ => "Unknown"
        };

        private static string[] LayScopes(int quyen) => quyen switch
        {
            1 => new[] { "authenticated", "admin", "accountant", "sales", "warehouse", "warehouse.read" },
            2 => new[] { "authenticated", "sales", "warehouse.read" },
            3 => new[] { "authenticated", "warehouse", "warehouse.read" },
            // Kế toán truy cập API nghiệp vụ/báo cáo kế toán, không có quyền quản trị Admin.
            4 => new[] { "authenticated", "accountant" },
            _ => new[] { "authenticated" }
        };

        [HttpGet("get-role")]
        public IActionResult GetRole([FromQuery] string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    return BadRequest(new { success = false, message = "Thiếu tên đăng nhập!" });
                }

                int quyen = _bll.LayQuyen(username.Trim());
                if (quyen == 0)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy tài khoản hoặc chưa cấp quyền!" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy quyền thành công!",
                    data = new
                    {
                        UserName = username.Trim(),
                        Quyen = quyen,
                        RoleName = LayTenRole(quyen)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể lấy quyền tài khoản.");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

