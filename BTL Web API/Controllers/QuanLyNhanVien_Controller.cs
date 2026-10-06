using System;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/QuanLyNhanVien")]
    [ApiController]
    public class QuanLyNhanVien_Controller : ControllerBase
    {
        private readonly NhanVien_BLL _bll;

        public QuanLyNhanVien_Controller(NhanVien_BLL _bll)
        {
            this._bll = _bll;
        }

        
        private static object MapNhanVien(NhanVien x) => new
        {
            MANV = x.MANV?.Trim(),
            TENNV = x.TENNV?.Trim(),
            SDT = x.SDT?.Trim(),
            DIACHI = x.DIACHI?.Trim()
        };

        [HttpGet("get-all-nhanvien")]
        public IActionResult GetAllNhanVien()
        {
            try
            {
                var data = _bll.LayTatCa().Select(MapNhanVien).ToList();
                return Ok(new { success = true, count = data.Count, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        
        [HttpGet("get-byid-nhanvien")]
        public IActionResult GetByIdNhanVien([FromQuery] string manv)
        {
            if (string.IsNullOrWhiteSpace(manv))
                return BadRequest(new { success = false, message = "Mã nhân viên không được để trống." });

            try
            {
                var list = _bll.LayTheoID(manv.Trim());
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy nhân viên có mã '{manv}'." });

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin nhân viên thành công.",
                    data = MapNhanVien(list[0]) // Trả về 1 object chi tiết trực tiếp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("create-nhanvien")]
        public IActionResult CreateNhanVien([FromBody] NhanVien nv)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.MANV))
                return BadRequest(new { success = false, message = "Dữ liệu nhân viên và mã nhân viên không được để trống." });

            try
            {
                bool ok = _bll.ThemMoi(nv);
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể thêm nhân viên." });

                return Ok(new { success = true, message = "Thêm nhân viên thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("update-nhanvien")]
        public IActionResult UpdateNhanVien([FromBody] NhanVien nv)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.MANV))
                return BadRequest(new { success = false, message = "Dữ liệu nhân viên và mã nhân viên không được để trống." });

            try
            {
                bool ok = _bll.CapNhat(nv);
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể cập nhật nhân viên." });

                return Ok(new { success = true, message = "Cập nhật nhân viên thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-nhanvien")]
        public IActionResult DeleteNhanVien([FromQuery] string manv)
        {
            if (string.IsNullOrWhiteSpace(manv))
                return BadRequest(new { success = false, message = "Mã nhân viên không được để trống." });

            try
            {
                bool ok = _bll.Xoa(manv.Trim());
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể xoá nhân viên." });

                return Ok(new { success = true, message = "Xoá nhân viên thành công." });
            }
            catch (Exception ex)
            {
                // Bắt lỗi ràng buộc khóa ngoại tham chiếu sang Phiếu Nhập hoặc Hóa Đơn
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("FK_"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không thể xóa nhân viên này vì đã có dữ liệu Phiếu nhập kho hoặc Hóa đơn bán hàng liên quan!"
                    });
                }

                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}


