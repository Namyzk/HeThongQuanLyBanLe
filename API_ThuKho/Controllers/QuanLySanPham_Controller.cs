using System;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;
using Microsoft.AspNetCore.Authorization;

namespace API_ThuKho.Controllers
{
    [Authorize(Roles = "Admin,ThuKho")]
    [Route("api/QuanLySanPham")]
    [ApiController]
    public class QuanLySanPham_Controller : ControllerBase
    {
        private readonly SanPham_BLL _spBll;

        public QuanLySanPham_Controller(SanPham_BLL _spBll)
        {
            this._spBll = _spBll;
        }

      
        private IActionResult HandleBllError(string bllError)
        {
            if (bllError.StartsWith("Lỗi hệ thống", StringComparison.OrdinalIgnoreCase))
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });

            if (bllError.Contains("đã tồn tại", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { success = false, message = bllError });

            if (bllError.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase) ||
                bllError.Contains("không tồn tại", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { success = false, message = bllError });

            return BadRequest(new { success = false, message = bllError });
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

        [HttpGet("get-all-sanpham")]
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

        [HttpGet("get-sanpham-by-id")]
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
                    data = result[0]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("insert-sanpham")]
        public IActionResult Create([FromBody] SanPham? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu sản phẩm không được để trống." });

            try
            {
                string? bllErr = _spBll.ThemMoi(model);
                if (bllErr != null) return HandleBllError(bllErr);

                return StatusCode(201, new { success = true, message = "Thêm sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPut("update-sanpham")]
        public IActionResult Update([FromBody] SanPham? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu sản phẩm không được để trống." });

            try
            {
                string? bllErr = _spBll.Sua(model);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Cập nhật sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPatch("update-soluong-sanpham")]
        public IActionResult UpdateSoLuong([FromQuery] string? maSP, [FromQuery] int soLuongMoi)
        {
            if (ValidateMa(maSP, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (soLuongMoi < 0)
                return BadRequest(new { success = false, message = "Số lượng sản phẩm không được nhỏ hơn 0." });

            try
            {
                string? bllErr = _spBll.SuaSoLuong(cleanMa, soLuongMoi);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Cập nhật số lượng sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-sanpham")]
        public IActionResult Delete([FromQuery] string? maSP)
        {
            if (ValidateMa(maSP, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                string? bllErr = _spBll.Xoa(cleanMa);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Xóa sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

