
using System;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/QuanLyTaiKhoan")]
    [ApiController]
    public class QuanLyTaiKhoan_Controller : ControllerBase
    {
        private readonly TaiKhoan_BLL _bll;
        public QuanLyTaiKhoan_Controller(TaiKhoan_BLL _bll)
        {
            this._bll = _bll;
        }

     
        private static object MapTaiKhoan(TaiKhoan x) => new
        {
            MaTaiKhoan = x.MATAIKHOAN?.Trim(),
            UserName = x.USERNAME?.Trim(),
            Quyen = x.QUYEN,
            RoleName = x.QUYEN switch
            {
                1 => "Admin",
                2 => "ThuNgan",
                3 => "ThuKho",
                4 => "KeToan",
                _ => "Unknown"
            }
        };

        // 1. Lấy tất cả tài khoản
        [HttpGet("get-all-taikhoan")]
        public IActionResult GetAllTaiKhoan()
        {
            try
            {
                var data = _bll.LayTatCa().Select(MapTaiKhoan).ToList();
                return Ok(new { success = true, count = data.Count, message = "Lấy danh sách tài khoản thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        // 2. Lấy tài khoản theo ID
        [HttpGet("get-byid-taikhoan")]
        public IActionResult GetByIdTaiKhoan([FromQuery] string mataikhoan)
        {
            if (string.IsNullOrWhiteSpace(mataikhoan))
                return BadRequest(new { success = false, message = "Mã tài khoản không được để trống." });

            try
            {
                var list = _bll.LayTheoID(mataikhoan.Trim());
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy tài khoản có mã '{mataikhoan}'." });

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin tài khoản thành công.",
                    data = MapTaiKhoan(list[0]) // Trả về object đơn
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        // 3. Thêm tài khoản
        [HttpPost("create-taikhoan")]
        public IActionResult CreateTaiKhoan([FromBody] TaiKhoan tk)
        {
            if (tk == null || string.IsNullOrWhiteSpace(tk.MATAIKHOAN) ||
                string.IsNullOrWhiteSpace(tk.USERNAME) || string.IsNullOrWhiteSpace(tk.PASS))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ Mã tài khoản, Tên đăng nhập và Mật khẩu." });
            }

            try
            {
                bool ok = _bll.ThemMoi(tk);
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể thêm tài khoản (mã hoặc username đã tồn tại)." });

                return Ok(new { success = true, message = "Thêm tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        // 4. Cập nhật tài khoản
        [HttpPost("update-byID-taikhoan")]
        public IActionResult UpdateTaiKhoan([FromBody] TaiKhoan tk)
        {
            if (tk == null || string.IsNullOrWhiteSpace(tk.MATAIKHOAN) ||
                string.IsNullOrWhiteSpace(tk.USERNAME) || string.IsNullOrWhiteSpace(tk.PASS))
            {
                return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ Mã tài khoản, Tên đăng nhập và Mật khẩu." });
            }

            try
            {
                bool ok = _bll.CapNhat(tk);
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể cập nhật (không tìm thấy tài khoản hoặc dữ liệu không hợp lệ)." });

                return Ok(new { success = true, message = "Cập nhật tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        // 5. Xóa tài khoản
        [HttpDelete("del-byID-taikhoan")]
        public IActionResult DeleteTaiKhoan([FromQuery] string mataikhoan)
        {
            if (string.IsNullOrWhiteSpace(mataikhoan))
                return BadRequest(new { success = false, message = "Mã tài khoản không được để trống." });

            try
            {
                bool ok = _bll.Xoa(mataikhoan.Trim());
                if (!ok)
                    return NotFound(new { success = false, message = $"Không tìm thấy tài khoản có mã '{mataikhoan}' để xóa." });

                return Ok(new { success = true, message = "Xóa tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        // 6. Đăng nhập (Dành cho việc gọi trực tiếp trong phân hệ Admin)
    } 
}

