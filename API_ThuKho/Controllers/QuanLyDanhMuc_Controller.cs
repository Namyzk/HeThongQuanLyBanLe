using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_ThuKho.Controllers
{
    [Authorize(Roles = "Admin,ThuKho")]
    [Route("api/QuanLyDanhMuc")]

    [ApiController]
    public class QuanLyDanhMuc_Controller : ControllerBase
    {
        private readonly DanhMuc_BLL _dmBll;

        public QuanLyDanhMuc_Controller(DanhMuc_BLL _dmBll)
        {
            this._dmBll = _dmBll;
        }

        [HttpGet("get-all-danhmuc")]
        public IActionResult GetAll()
        {
            try
            {
                var list = _dmBll.LayTatCa();
                return Ok(new
                {
                    success = true,
                    count = list?.Count ?? 0,
                    data = list ?? new System.Collections.Generic.List<DanhMuc>()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byID-danhmuc")]
        public IActionResult GetByID([FromQuery] string? madanhmuc)
        {
            if (string.IsNullOrWhiteSpace(madanhmuc))
                return BadRequest(new { success = false, message = "Mã danh mục không được để trống." });

            try
            {
                var list = _dmBll.LayTheoID(madanhmuc.Trim());
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy danh mục có mã '{madanhmuc}'." });

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin danh mục thành công.",
                    data = list[0] 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("insert-danhmuc")]
        public IActionResult Create([FromBody] DanhMuc? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu danh mục không được để trống." });

            try
            {
                string? error = _dmBll.ThemMoi(model);
                if (error != null)
                    return BadRequest(new { success = false, message = error });

                return Ok(new { success = true, message = "Thêm danh mục thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPut("update-danhmuc")]
        public IActionResult Update([FromBody] DanhMuc? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu danh mục không được để trống." });

            try
            {
                string? error = _dmBll.CapNhat(model);
                if (error != null)
                    return BadRequest(new { success = false, message = error });

                return Ok(new { success = true, message = "Cập nhật danh mục thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-danhmuc")]
        public IActionResult Delete([FromQuery] string? maDanhMuc)
        {
            if (string.IsNullOrWhiteSpace(maDanhMuc))
                return BadRequest(new { success = false, message = "Mã danh mục không được để trống." });

            try
            {
                string? error = _dmBll.Xoa(maDanhMuc.Trim());
                if (error != null)
                    return BadRequest(new { success = false, message = error });

                return Ok(new { success = true, message = "Xóa danh mục thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

