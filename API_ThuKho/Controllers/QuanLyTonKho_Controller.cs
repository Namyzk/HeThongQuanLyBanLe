using System;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API_ThuKho.Controllers
{
    [Authorize]
    [Route("api/QuanLyTonKho")]
    [ApiController]
    public class QuanLyTonKho_Controller : ControllerBase
    {
        private readonly SanPham_BLL _spBll;

        public QuanLyTonKho_Controller(SanPham_BLL _spBll)
        {
            this._spBll = _spBll;
        }

     
        private IActionResult? ValidateMa(string? ma, out string cleanMa)
        {
            cleanMa = string.Empty;
            if (string.IsNullOrWhiteSpace(ma))
                return BadRequest(new { success = false, message = "Mã sản phẩm không được để trống." });

            cleanMa = ma.Trim();
            if (cleanMa.Length > 15)
                return BadRequest(new { success = false, message = "Mã sản phẩm không được vượt quá 15 ký tự." });

            return null;
        }
        [Authorize(Roles = "Admin,ThuKho,ThuNgan")]
        [HttpGet("get-all-sanpham-tonkho")]
        public IActionResult GetAll()
        {
            try
            {
                var result = _spBll.LayTatCa();
                return Ok(new
                {
                    success = true,
                    count = result?.Count ?? 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [Authorize(Roles = "Admin,ThuKho")]
        [HttpGet("get-sanpham-tonkho-by-id")]
        public IActionResult GetByID([FromQuery] string? id)
        {
            if (ValidateMa(id, out string cleanId) is IActionResult valErr)
                return valErr;

            try
            {
                var result = _spBll.LayTheoID(cleanId);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm có mã '{cleanId}'." });

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin tồn kho sản phẩm thành công.",
                    data = result[0] 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
        [Authorize(Roles = "Admin,ThuKho")]
        [HttpPost("update-soluong-sanpham-tonkho")]
        public IActionResult UpdateSoLuong([FromQuery] string? maSP, [FromQuery] int soLuongMoi)
        {
            if (ValidateMa(maSP, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (soLuongMoi < 0)
                return BadRequest(new { success = false, message = "Số lượng tồn kho không được nhỏ hơn 0." });

            try
            {
                string? result = _spBll.SuaSoLuong(cleanMa, soLuongMoi);

                if (!string.IsNullOrEmpty(result))
                {
                    if (result.StartsWith("Lỗi hệ thống", StringComparison.OrdinalIgnoreCase))
                        return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });

                    if (result.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase))
                        return NotFound(new { success = false, message = result });

                    return BadRequest(new { success = false, message = result });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật số lượng tồn kho thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

