using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API_Login.Controllers
{
    [Route("api/Login")]
    [ApiController]
    public class Login_Controller : ControllerBase
    {
        private readonly TaiKhoan_BLL _bll;
        private readonly IConfiguration _config;

        // DI Configuration chuẩn của ASP.NET Core
        public Login_Controller(IConfiguration config)
        {
            _bll = new TaiKhoan_BLL();
            _config = config;
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

                var list = _bll.DangNhap(request.Username.Trim(), request.Pass.Trim());
                if (list == null || list.Count == 0)
                {
                    return Unauthorized(new { success = false, message = "Sai tên đăng nhập hoặc mật khẩu!" });
                }

                var user = list.First();

                // Tạo chuỗi JWT Token
                string token = GenerateJwtToken(user);

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công!",
                    token,
                    data = new
                    {
                        MaTaiKhoan = user.MATAIKHOAN.Trim(),
                        UserName = user.USERNAME.Trim(),
                        Quyen = user.QUYEN,
                        RoleName = user.QUYEN == 1 ? "Admin" : (user.QUYEN == 2 ? "ThuNgan" : "ThuKho")
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        private string GenerateJwtToken(TaiKhoan user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var keyString = jwtSettings["Key"] ?? "QUANLYBANLE_SUPER_SECRET_KEY_2026_MIN_32_CHARS_LONG";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Gán Role dạng chuỗi để dùng được với [Authorize(Roles = "...")]
            string roleName = user.QUYEN == 1 ? "Admin" : (user.QUYEN == 2 ? "ThuNgan" : "ThuKho");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MATAIKHOAN.Trim()),
                new Claim(ClaimTypes.Name, user.USERNAME.Trim()),
                new Claim(ClaimTypes.Role, roleName),
                new Claim("QUYEN", user.QUYEN.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            double minutes = 480; // Mặc định 8 tiếng
            if (double.TryParse(jwtSettings["ExpiresInMinutes"], out double exp))
            {
                minutes = exp;
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "QUANLYBANLE_API",
                audience: jwtSettings["Audience"] ?? "QUANLYBANLE_CLIENT",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

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
                        RoleName = quyen == 1 ? "Admin" : (quyen == 2 ? "ThuNgan" : "ThuKho")
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}