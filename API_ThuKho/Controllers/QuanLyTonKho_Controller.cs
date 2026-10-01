using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_ThuKho.Controllers
{
    [Authorize]
    
    [Route("api/QuanLyTonKho")]
    [ApiController]
    public class QuanLyTonKho_Controller : ControllerBase
    {
        private readonly SanPham_BLL sp_bll;

        public QuanLyTonKho_Controller(IConfiguration configuration)
        {
            sp_bll = new SanPham_BLL();
        }

        [HttpGet("get-all-sanpham-tonkho")]
        public IActionResult GetAll()
        {
            try
            {
                var result = sp_bll.LayTatCa();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet("get-sanpham-tonkho-by-id")]
        public IActionResult GetByID(string id)
        {
            try
            {
                var result = sp_bll.LayTheoID(id);
                if (result == null || result.Count == 0)
                    return NotFound("Không tìm thấy sản phẩm.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        [Route("update-soluong-sanpham-tonkho")]
        [HttpPost]
        public IActionResult UpdateSoLuong([FromQuery] string maSP, [FromQuery] int soLuongMoi)
        {
            try
            {
                string result = sp_bll.SuaSoLuong(maSP, soLuongMoi);
                return string.IsNullOrEmpty(result) ? Ok("Cập nhật số lượng thành công") : BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}
