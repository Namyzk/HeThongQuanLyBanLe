using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace API_ThuKho.Controllers
{
    [Authorize]

    [Route("api/QuanLySanPham")]
    [ApiController]
    public class QuanLySanPham_Controller : ControllerBase
    {
        private readonly SanPham_BLL sp_bll;

        public QuanLySanPham_Controller(IConfiguration configuration)
        {
            sp_bll = new SanPham_BLL();
        }

        [HttpGet("get-all-sanpham")]
        public IActionResult GetAll()
        {
            try
            {
                var result = sp_bll.LayTatCa();

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách sản phẩm: " + ex.Message
                });
            }
        }

        [HttpGet("get-sanpham-by-id")]
        public IActionResult GetByID([FromQuery] string? id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã sản phẩm không được để trống."
                    });
                }

                if (id.Trim().Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });
                }

                var result = sp_bll.LayTheoID(id.Trim());

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy sản phẩm có mã '{id.Trim()}'."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy sản phẩm: " + ex.Message
                });
            }
        }

        [HttpPost("insert-sanpham")]
        public IActionResult Create([FromBody] Models.SanPham model)
        {
            try
            {
                string error = sp_bll.ThemMoi(model);

                if (error != null)
                {
                    if (error.Contains("đã tồn tại"))
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = error
                        });
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                return StatusCode(201, new
                {
                    success = true,
                    message = "Thêm sản phẩm thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm sản phẩm: " + ex.Message
                });
            }
        }

        [HttpPut("update-sanpham")]
        public IActionResult Update([FromBody] Models.SanPham model)
        {
            try
            {
                string error = sp_bll.Sua(model);

                if (error != null)
                {
                    if (error.Contains("Không tìm thấy"))
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = error
                        });
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật sản phẩm thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật sản phẩm: " + ex.Message
                });
            }
        }

        [HttpPatch("update-soluong-sanpham")]
        public IActionResult UpdateSoLuong(
            [FromQuery] string maSP,
            [FromQuery] int soLuongMoi)
        {
            try
            {
                string error = sp_bll.SuaSoLuong(
                    maSP,
                    soLuongMoi);

                if (error != null)
                {
                    if (error.Contains("Không tìm thấy"))
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = error
                        });
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật số lượng sản phẩm thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật số lượng: " + ex.Message
                });
            }
        }

        [HttpDelete("delete-sanpham")]
        public IActionResult Delete([FromQuery] string maSP)
        {
            try
            {
                string error = sp_bll.Xoa(maSP);

                if (error != null)
                {
                    if (error.Contains("Không tìm thấy"))
                    {
                        return NotFound(new
                        {
                            success = false,
                            message = error
                        });
                    }

                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Xóa sản phẩm thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi xóa sản phẩm: " + ex.Message
                });
            }
        }
    }
}