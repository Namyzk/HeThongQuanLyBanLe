using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_ThuKho.Controllers
{
    [Authorize]
 
    [Route("api/QuanLyDanhMuc")]
    [ApiController]
    public class QuanLyDanhMuc_Controller : ControllerBase
    {
        private readonly DanhMuc_BLL dm_bll;

        public QuanLyDanhMuc_Controller(IConfiguration configuration)
        {
            dm_bll = new DanhMuc_BLL();
        }
        [Route("get-all-danhmuc")]
        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                var list = dm_bll.LayTatCa();

                if (list == null || !list.Any())
                {
                    return NoContent();
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [Route("get-byID-danhmuc")]
        [HttpGet]
        public IActionResult GetByID(string madanhmuc)
        {
            try
            {
                var danhmuc = dm_bll.LayTheoID(madanhmuc);

                if (danhmuc == null || danhmuc.Count == 0)
                    return NotFound("Không tìm thấy danh mục.");

                return Ok(danhmuc);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [Route("insert-danhmuc")]
        [HttpPost]
        public IActionResult Create([FromBody] Models.DanhMuc model)
        {
            try
            {
                string error = dm_bll.ThemMoi(model);

                if (error != null)
                {
                    return BadRequest(new
                    {
                        success = false, StatusCode = 400,  message = "Không thể thêm danh mục" 
                    });
                }

                return Ok(new
                {
                    success = true, StatusCode = 200,  message = "Thêm danh mục thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, StatusCode = 500, message = "Lỗi: " + ex.Message
                });
            }
        }

        [Route("update-danhmuc")]
        [HttpPut]
        public IActionResult Update([FromBody] Models.DanhMuc model)
        {
            try
            {
                string error = dm_bll.CapNhat(model);

                if (error != null)
                {
                    return BadRequest(new
                    {
                        success = false, StatusCode = 400, message = "Không thể cập nhật danh mục"

                    });
                }

                return Ok(new
                {
                    success = true, StatusCode = 200, message = "Cập nhật danh mục thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, StatusCode = 500,
                    message = "Lỗi: " + ex.Message
                });
            }
        }

        [Route("delete-danhmuc")]
        [HttpDelete]
        public IActionResult Delete(string maDanhMuc)
        {
            try
            {
                string error = dm_bll.Xoa(maDanhMuc);

                if (error != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                return Ok(new
                {
                    success = true, StatusCode = 200,  message = "Xóa danh mục thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,   message = "Lỗi: " + ex.Message
                });
            }
        }
    }
}

