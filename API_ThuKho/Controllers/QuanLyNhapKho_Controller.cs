using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Data;

namespace API_ThuKho.Controllers
{
    [Authorize]
   
    [Route("api/QuanLyNhapKho")]
    [ApiController]
    public class QuanLyNhapKho_Controller : ControllerBase
    {
        private readonly PhieuNhapKho_BLL _bll;
        private readonly ChiTietNhap_BLL ctn_bll;
        private readonly NhaCungCap_BLL NCC_BLL;
        public QuanLyNhapKho_Controller(IConfiguration configuration)
        {
            _bll = new PhieuNhapKho_BLL();
            ctn_bll = new ChiTietNhap_BLL();
            NCC_BLL = new NhaCungCap_BLL();
        }

        //PhieuNhapKho
        [HttpGet("get-all-phieunhapkho")]
        public IActionResult GetAllPhieuNhapKho()
        {
            try
            {
                var data = _bll.LayTatCa();
                return Ok(new { success = true, message = "Lấy danh sách phiếu nhập kho thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpGet("get-byid-phieunhapkho")]
        public IActionResult GetByIdPhieuNhapKho([FromQuery] string? maphieunhap)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maphieunhap))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã phiếu nhập không được để trống."
                    });
                }

                maphieunhap = maphieunhap.Trim();

                if (maphieunhap.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã phiếu nhập không được vượt quá 15 ký tự."
                    });
                }

                var list = _bll.LayTheoID(maphieunhap);

                if (list == null || list.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy phiếu nhập có mã '{maphieunhap}'."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin phiếu nhập kho thành công",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        [HttpPost("create-phieunhapkho")]
        public IActionResult CreatePhieuNhapKho( [FromBody] PhieuNhapKho pnk)
        {
            try
            {
                string error = _bll.ThemMoi(pnk);

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
                    message = "Thêm phiếu nhập kho thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        [HttpPost("update-phieunhapkho")]
        public IActionResult UpdatePhieuNhapKho([FromBody] PhieuNhapKho pnk)
        {
            try
            {
                string error = _bll.CapNhat(pnk);

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
                    message = "Cập nhật phiếu nhập kho thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }
        [HttpDelete("delete-phieunhapkho")]
        public IActionResult DeletePhieuNhapKho( [FromQuery] string maphieunhap)
        {
            try
            {
                string error = _bll.Xoa(maphieunhap);

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
                    message = "Xoá phiếu nhập kho thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }



        //ChiTietNhap

        // 🔹 Lấy tất cả
        [HttpGet("get-all-chitietnhap")]
        public IActionResult GetAll()
        {
            try
            {
                var data = ctn_bll.LayTatCa()
                               .Select(x => new {
                                   MAPHIEUNHAP = x.MAPHIEUNHAP?.Trim(),
                                   MASP = x.MASP?.Trim(),
                                   SOLUONG = x.SOLUONG,
                                   DONGIANHAP = x.DONGIANHAP,
                                   THANHTIEN = x.THANHTIEN,
                               })
                               .ToList();

                return Ok(new { success = true, message = "Lấy danh sách chi tiết nhập thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // 🔹 Lấy theo phiếu
        [HttpGet("get-byphieu-chitietnhap")]
        public IActionResult GetByPhieu([FromQuery] string? maphieunhap)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maphieunhap))
                    return Ok(new { success = false, message = "Thiếu mã phiếu nhập" });

                var list = ctn_bll.LayTheoPhieu(maphieunhap);
                var data = list.Select(x => new {
                    MAPHIEUNHAP = x.MAPHIEUNHAP?.Trim(),
                    MASP = x.MASP?.Trim(),
                    SOLUONG = x.SOLUONG,
                    DONGIANHAP = x.DONGIANHAP,
                    THANHTIEN = x.THANHTIEN,
                })
                               .ToList();

                return Ok(new { success = true, message = "Lấy chi tiết theo phiếu thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        //  Lấy theo (MAPHIEUNHAP, MASP)
        [HttpGet("get-byid-chitietnhap")]
        public IActionResult GetById([FromQuery] string maphieunhap, [FromQuery] string masp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maphieunhap) || string.IsNullOrWhiteSpace(masp))
                    return Ok(new { success = false, message = "Thiếu mã phiếu nhập hoặc mã sản phẩm" });

                var list = ctn_bll.LayTheoID(maphieunhap, masp);
                if (list == null || list.Count == 0)
                    return Ok(new { success = false, message = "Không tìm thấy chi tiết nhập" });

                var x = list.First();
                var data = new
                {
                    MAPHIEUNHAP = x.MAPHIEUNHAP?.Trim(),
                    MASP = x.MASP?.Trim(),
                    SOLUONG = x.SOLUONG,
                    DONGIANHAP = x.DONGIANHAP,
                    THANHTIEN = x.THANHTIEN
                };

                return Ok(new { success = true, message = "Lấy chi tiết nhập thành công", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        //  Tạo mới
        [HttpPost("create-chitietnhap")]
        public IActionResult Create([FromBody] ChiTietNhap ct)
        {
            try
            {
                string error = ctn_bll.ThemMoi(ct);

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

                    if (error.Contains("không tồn tại"))
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

                return StatusCode(201, new
                {
                    success = true,
                    message = "Thêm chi tiết nhập thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        //  Cập nhật
        [HttpPost("update-chitietnhap")]
        public IActionResult Update([FromBody] ChiTietNhap ct)
        {
            try
            {
                string error = ctn_bll.CapNhat(ct);

                if (error != null)
                {
                    if (error.Contains("Không tìm thấy"))
                    {
                        return NotFound(new
                        {
                            success = false, message = error
                        });
                    }

                    if (error.Contains("không tồn tại"))
                    {
                        return NotFound(new { success = false, message = error });
                    }

                    return BadRequest(new
                    { success = false,   message = error });
                }

                return Ok(new  {  success = true, message = "Cập nhật chi tiết nhập thành công"  });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false,   message = "Lỗi hệ thống: " + ex.Message  });
            }
        }

        //  Xoá
        [HttpDelete("delete-chitietnhap")]
        public IActionResult Delete([FromQuery] string maphieunhap,[FromQuery] string masp)
        {
            try
            {
                string error = ctn_bll.Xoa(
                    maphieunhap,
                    masp);

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
                    message = "Xoá chi tiết nhập thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        //NhaCungCap
        [Route("get-all-nhacungcap")]
        [HttpGet]
        public IActionResult getAllNCC()
        {
            try
            {
                DataTable dt = NCC_BLL.GetAll();
                var list = new List<object>();
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        MANCC = row["MANCC"].ToString().Trim(),
                        TENNCC = row["TENNCC"],
                        DIACHI = row["DIACHI"],
                        SDT = row["SDT"],
                        EMAIL = row["EMAIL"]
                    });
                }
                return Ok(new { success = true, message = "Lấy danh sách nhà cung cấp thành công", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi: " + ex.Message });
            }
        }


        [Route("get-byid-nhacungcap")]
        [HttpGet]
        public IActionResult GetById([FromQuery] string ma)
        {
            try
            {
                string error = NCC_BLL.ValidateMa(ma);

                if (error != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = error
                    });
                }

                DataTable dt = NCC_BLL.GetById(ma.Trim());

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy nhà cung cấp có mã '{ma.Trim()}'."
                    });
                }

                var list = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        MANCC = row["MANCC"]?.ToString()?.Trim(),
                        TENNCC = row["TENNCC"],
                        DIACHI = row["DIACHI"],
                        SDT = row["SDT"],
                        EMAIL = row["EMAIL"]
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin nhà cung cấp thành công",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }
        [Route("del-nhacungcap")]
        [HttpDelete]
        public IActionResult Delete(
          [FromQuery] string ma)
        {
            try
            {
                string error = NCC_BLL.Delete(ma);

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
                    message = "Xoá thông tin nhà cung cấp thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        [Route("update-nhacungcap")]
        [HttpPost]
        public IActionResult Update( [FromBody] Models.NhaCungCap model)
        {
            try
            {
                string error = NCC_BLL.Update(model);

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
                    message = "Thay đổi thông tin nhà cung cấp thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

        [Route("create-nhacungcap")]
        [HttpPost]
        public IActionResult Create( [FromBody] Models.NhaCungCap model)
        {
            try
            {
                string error = NCC_BLL.Create(model);

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
                    message = "Thêm thông tin nhà cung cấp thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi hệ thống: " + ex.Message
                });
            }
        }

    }
}
