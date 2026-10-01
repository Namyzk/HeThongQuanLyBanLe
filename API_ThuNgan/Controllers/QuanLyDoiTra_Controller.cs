using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace API_ThuNgan.Controllers
{
    [Authorize]
    //[AllowAnonymous]
    [Route("api/QuanLyDoiTra")]
    [ApiController]
    public class QuanLyDoiTra_Controller : ControllerBase
    {
        private readonly HoaDonBan_BLL hdb_bll;
        private readonly ChiTietBan_BLL ctb_bll;
        private readonly KhachHang_BLL kh_bll;
        private readonly DanhMuc_BLL dm_bll;
        private readonly SanPham_BLL sp_bll;
        private readonly ThanhToan_BLL tt_bll;

        public QuanLyDoiTra_Controller(IConfiguration configuration)
        {
            hdb_bll = new HoaDonBan_BLL();
            ctb_bll = new ChiTietBan_BLL();
            kh_bll = new KhachHang_BLL();
            dm_bll = new DanhMuc_BLL();
            sp_bll = new SanPham_BLL();
            tt_bll = new ThanhToan_BLL();
        }

       
        // GET ALL
        [Route("get-all-hoadonban")]
        [HttpGet]
        public IActionResult GetAll_HoaDon()
        {
            try
            {
                var result = hdb_bll.LayTatCa();

                return Ok(new
                {
                    success = true,message = "Lấy danh sách hóa đơn thành công.", data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,message = "Lỗi khi lấy danh sách hóa đơn: " + ex.Message
                });
            }
        }

        // GET BY ID
        [Route("get-hoadonban-by-id")]
        [HttpGet]
        public IActionResult GetByID_HoaDon( [FromQuery] string? maHoaDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHoaDon))
                    return BadRequest(new
                    {
                        success = false,   message = "Mã hóa đơn không được để trống."
                    });

                maHoaDon = maHoaDon.Trim();

                if (maHoaDon.Length > 15)
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });

                var result = hdb_bll.LayTheoID(maHoaDon);

                if (result == null || result.Count == 0)
                    return NotFound(new
                    {
                        success = false, message = "Không tìm thấy hóa đơn."
                    });

                return Ok(new
                {
                    success = true,  message = "Lấy hóa đơn thành công.",  data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi lấy hóa đơn: " + ex.Message
                });
            }
        }

        
        // INSERT
        [Route("insert-chitietban")]
        [HttpPost]
        public IActionResult CreateChiTiet([FromBody] ChiTietBan model)
        {
            try
            {
              
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Dữ liệu chi tiết bán không được để trống."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.MAHDBAN))
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được để trống."
                    });
                }

                model.MAHDBAN = model.MAHDBAN.Trim();

                if (model.MAHDBAN.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.MASP))
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã sản phẩm không được để trống."
                    });
                }

                model.MASP = model.MASP.Trim();

                if (model.MASP.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });
                }

                if (model.SOLUONG <= 0)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Số lượng phải lớn hơn 0."
                    });
                }

                if (model.DONGIA <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,message = "Đơn giá phải lớn hơn 0."
                    });
                }

                decimal tongTienTinhLai = model.SOLUONG * model.DONGIA;

                if (model.TONGTIEN != tongTienTinhLai)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Tổng tiền không hợp lệ. " +  "Tổng tiền phải bằng Số lượng × Đơn giá."
                    });
                }

                string result = ctb_bll.ThemMoi(model);

                if (result != null)
                {
                    return BadRequest(new
                    {
                        success = false, message = result
                    });
                }

                return StatusCode(201, new
                {
                    success = true, message = "Thêm chi tiết bán thành công."
                });
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (error.Contains("FK__CT_HDB__MAHDBAN"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn " + model.MAHDBAN +  " không tồn tại."
                    });
                }

                if (error.Contains("FK__CT_HDB__MASP"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã sản phẩm " +  model.MASP + " không tồn tại."
                    });
                }

              
                if (error.Contains("PRIMARY KEY") || error.Contains("PRIMARY KEY constraint") || error.Contains("duplicate key"))
                {
                    return Conflict(new
                    {
                        success = false, message = "Chi tiết sản phẩm này đã tồn tại trong hóa đơn."
                    });
                }

                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi thêm chi tiết bán: " + error
                });
            }
        }

       
        [Route("update-hoadonban")]
        [HttpPut]
        public IActionResult UpdateHoaDon([FromBody] HoaDonBan model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new
                    {
                        success = false, message = "Dữ liệu hóa đơn không được để trống."
                    });

                if (string.IsNullOrWhiteSpace(model.MAHDBAN))
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được để trống."
                    });

                model.MAHDBAN = model.MAHDBAN.Trim();

                if (model.MAHDBAN.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });

                if (model.listjson_chitietban == null ||model.listjson_chitietban.Count == 0)
                    return BadRequest(new
                    {
                        success = false,  message = "Hóa đơn phải có ít nhất một chi tiết bán."
                    });

                foreach (var ct in model.listjson_chitietban)
                {
                    if (ct == null)
                        return BadRequest(new
                        {
                            success = false, message = "Chi tiết hóa đơn không được để trống."
                        });

                    if (string.IsNullOrWhiteSpace(ct.MASP))
                        return BadRequest(new
                        {
                            success = false, message = "Mã sản phẩm trong chi tiết không được để trống."
                        });

                    ct.MASP = ct.MASP.Trim();

                    if (ct.MASP.Length > 15)
                        return BadRequest(new
                        {
                            success = false, message = "Mã sản phẩm không được vượt quá 15 ký tự."
                        });

                    if (ct.SOLUONG <= 0)
                        return BadRequest(new
                        {
                            success = false, message = "Số lượng sản phẩm phải lớn hơn 0."
                        });

                    if (ct.DONGIA <= 0)
                        return BadRequest(new
                        {
                            success = false,
                            message = "Đơn giá sản phẩm phải lớn hơn 0."
                        });
                }

                bool result = hdb_bll.Sua(model);

                if (!result)
                    return NotFound(new
                    {
                        success = false, message = "Không tìm thấy hóa đơn hoặc cập nhật thất bại."
                    });

                return Ok(new
                {
                    success = true,   message = "Cập nhật hóa đơn thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi cập nhật hóa đơn: " + ex.Message
                });
            }
        }

        [Route("delete-hoadonban")]
        [HttpDelete]
        public IActionResult DeleteHoaDon([FromQuery] string? maHoaDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHoaDon))
                    return BadRequest(new
                    {
                        success = false,message = "Mã hóa đơn không được để trống."
                    });

                maHoaDon = maHoaDon.Trim();

                if (maHoaDon.Length > 15)
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });

                bool result = hdb_bll.Xoa(maHoaDon);

                if (!result)
                    return NotFound(new
                    {
                        success = false,  message = "Không tìm thấy hóa đơn."
                    });

                return Ok(new
                {
                    success = true, message = "Xóa hóa đơn thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi xóa hóa đơn: " + ex.Message
                });
            }
        }


        // GET ALL
        [Route("get-all-chitietban")]
        [HttpGet]
        public IActionResult GetAll_ChiTiet()
        {
            try
            {
                var result = ctb_bll.LayTatCa();

                return Ok(new
                {
                    success = true,  message = "Lấy danh sách chi tiết bán thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi lấy danh sách chi tiết bán: " + ex.Message
                });
            }
        }

        // GET BY HÓA ĐƠN
        [Route("get-chitietban-by-IDhoadon")]
        [HttpGet]
        public IActionResult GetByHoaDon(
            [FromQuery] string? maHDB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDB))
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được để trống."
                    });

                maHDB = maHDB.Trim();

                if (maHDB.Length > 15)
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });

                var result = ctb_bll.LayTheoHoaDon(maHDB);

                if (result == null || result.Count == 0)
                    return NotFound(new
                    {
                        success = false,  message = "Không tìm thấy chi tiết bán của hóa đơn."
                    });

                return Ok(new
                {
                    success = true,  message = "Lấy chi tiết bán thành công.", data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi lấy chi tiết bán: " + ex.Message
                });
            }
        }

        // INSERT
        [Route("insert-hoadonban")]
        [HttpPost]
        public IActionResult CreateHoaDon([FromBody] ChiTietBan model)
        {
            try
            {
                
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false,  message = "Dữ liệu chi tiết bán không được để trống."
                    });
                }

              
                if (string.IsNullOrWhiteSpace(model.MAHDBAN))
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn không được để trống."
                    });
                }

                model.MAHDBAN = model.MAHDBAN.Trim();

                if (model.MAHDBAN.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.MASP))
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã sản phẩm không được để trống."
                    });
                }

                model.MASP = model.MASP.Trim();

                if (model.MASP.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,  message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });
                }

                if (model.SOLUONG <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Số lượng phải lớn hơn 0."
                    });
                }

                if (model.DONGIA <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Đơn giá phải lớn hơn 0."
                    });
                }

                decimal tongTienTinhLai = model.SOLUONG * model.DONGIA;

                if (model.TONGTIEN != tongTienTinhLai)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tổng tiền không hợp lệ. Tổng tiền phải bằng Số lượng × Đơn giá."
                    });
                }

                string result = ctb_bll.ThemMoi(model);

                if (result != "success")
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Chi tiết bán đã tồn tại hoặc dữ liệu không hợp lệ."
                    });
                }

                
                return StatusCode(201, new
                {
                    success = true,
                    message = "Thêm chi tiết bán thành công."
                });
            }
            catch (Exception ex)
            {
                string error = ex.Message;

                if (error.Contains("FK__CT_HDB__MAHDBAN"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn " + model.MAHDBAN + " không tồn tại."
                    });
                }

                if (error.Contains("FK__CT_HDB__MASP"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã sản phẩm " + model.MASP + " không tồn tại."
                    });
                }

              
                if (error.Contains("PRIMARY KEY") || error.Contains("PRIMARY KEY constraint") || error.Contains("duplicate key"))
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Chi tiết sản phẩm này đã tồn tại trong hóa đơn."
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm chi tiết bán: " + error
                });
            }
        }

        // UPDATE
        [Route("update-chitietban")]
        [HttpPut]
        public IActionResult UpdateChiTiet([FromBody] ChiTietBan model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new
                    {
                        success = false, message = "Dữ liệu chi tiết bán không được để trống."
                    });

                if (string.IsNullOrWhiteSpace(model.MAHDBAN))
                    return BadRequest(new
                    {
                        success = false,message = "Mã hóa đơn không được để trống."
                    });

                model.MAHDBAN = model.MAHDBAN.Trim();

                if (model.MAHDBAN.Length > 15)
                    return BadRequest(new
                    {
                        success = false,   message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });

                if (string.IsNullOrWhiteSpace(model.MASP))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã sản phẩm không được để trống."
                    });

                model.MASP = model.MASP.Trim();

                if (model.MASP.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });

                if (model.SOLUONG <= 0)
                    return BadRequest(new
                    {
                        success = false,  message = "Số lượng phải lớn hơn 0."
                    });

                if (model.DONGIA <= 0)
                    return BadRequest(new
                    {
                        success = false,  message = "Đơn giá phải lớn hơn 0."
                    });

                bool result = ctb_bll.Sua(model);

                if (!result)
                    return NotFound(new
                    {
                        success = false,   message = "Không tìm thấy chi tiết bán."
                    });

                return Ok(new
                {
                    success = true,  message = "Cập nhật chi tiết bán thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi cập nhật chi tiết bán: " + ex.Message
                });
            }
        }

        // DELETE
        [Route("delete-chitietban")]
        [HttpDelete]
        public IActionResult DeleteChiTiet( [FromQuery] string? maHDB,  [FromQuery] string? maSP)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDB))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn không được để trống."
                    });

                if (string.IsNullOrWhiteSpace(maSP))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã sản phẩm không được để trống."
                    });

                maHDB = maHDB.Trim();
                maSP = maSP.Trim();

                if (maHDB.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });

                if (maSP.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });

                bool result = ctb_bll.Xoa(maHDB, maSP);

                if (!result)
                    return NotFound(new
                    {
                        success = false,   message = "Không tìm thấy chi tiết bán."
                    });

                return Ok(new
                {
                    success = true,  message = "Xóa chi tiết bán thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi xóa chi tiết bán: " + ex.Message
                });
            }
        }

        // GET ALL
        [Route("get-all-thanhtoan")]
        [HttpGet]
        public IActionResult GetAll_ThanhToan()
        {
            try
            {
                DataTable dt = tt_bll.getAll();

                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, object> item =   new Dictionary<string, object>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        if (row[column] == DBNull.Value)
                            item[column.ColumnName] = null;
                        else
                            item[column.ColumnName] = row[column];
                    }

                    data.Add(item);
                }

                return Ok(new
                {
                    success = true,  message = "Lấy danh sách thanh toán thành công.",  data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi lấy danh sách thanh toán: " + ex.Message
                });
            }
        }

        // GET BY ID
        [Route("get-byId-thanhtoan")]
        [HttpGet]
        public IActionResult Get_ThanhToan_ById(
            [FromQuery] string? ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                    return BadRequest(new
                    {
                        success = false, message = "Mã thanh toán không được để trống."
                    });

                ma = ma.Trim();

                if (ma.Length > 15)
                    return BadRequest(new
                    {
                        success = false, message = "Mã thanh toán không được vượt quá 15 ký tự."
                    });

                DataTable dt = tt_bll.GetById(ma);

                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new
                    {
                        success = false,  message = "Không tìm thấy thanh toán."
                    });

                List<Dictionary<string, object>> data =  new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, object> item =  new Dictionary<string, object>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        if (row[column] == DBNull.Value)
                            item[column.ColumnName] = null;
                        else
                            item[column.ColumnName] = row[column];
                    }

                    data.Add(item);
                }

                return Ok(new
                {
                    success = true,message = "Lấy thông tin thanh toán thành công.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi lấy thông tin thanh toán: " + ex.Message
                });
            }
        }

        // INSERT
        [Route("insert-thanhtoan")]
        [HttpPost]
        public IActionResult CreateThanhToan( [FromBody] Models.ThanhToan model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new
                    {
                        success = false,   message = "Dữ liệu thanh toán không được để trống."
                    });

                // MÃ THANH TOÁN
                if (string.IsNullOrWhiteSpace(model.MaThanhToan))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã thanh toán không được để trống."
                    });

                model.MaThanhToan = model.MaThanhToan.Trim();

                if (model.MaThanhToan.Length > 15)
                    return BadRequest(new
                    {
                        success = false, message = "Mã thanh toán không được vượt quá 15 ký tự."
                    });

                // MÃ HÓA ĐƠN
                if (string.IsNullOrWhiteSpace(model.MaHDBan))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn bán không được để trống."
                    });

                model.MaHDBan = model.MaHDBan.Trim();

                if (model.MaHDBan.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn bán không được vượt quá 15 ký tự."
                    });

                // PHƯƠNG THỨC
                if (string.IsNullOrWhiteSpace(model.PhuongThuc))
                    return BadRequest(new
                    {
                        success = false,
                        message = "Phương thức thanh toán không được để trống."
                    });

                model.PhuongThuc = model.PhuongThuc.Trim();

                if (model.PhuongThuc.Length > 50)
                    return BadRequest(new
                    {
                        success = false,  message = "Phương thức thanh toán không được vượt quá 50 ký tự."
                    });

                // SỐ TIỀN
                if (model.SoTienThanhToan <= 0)
                    return BadRequest(new
                    {
                        success = false,  message = "Số tiền thanh toán phải lớn hơn 0."
                    });

                // TRẠNG THÁI
                if (string.IsNullOrWhiteSpace(model.TrangThai))
                    return BadRequest(new
                    {
                        success = false,   message = "Trạng thái thanh toán không được để trống."
                    });

                model.TrangThai = model.TrangThai.Trim();

                if (model.TrangThai.Length > 50)
                    return BadRequest(new
                    {
                        success = false,  message = "Trạng thái thanh toán không được vượt quá 50 ký tự."
                    });

                // KIỂM TRA MÃ THANH TOÁN TRÙNG
                DataTable dt = tt_bll.GetById(model.MaThanhToan);

                if (dt != null && dt.Rows.Count > 0)
                    return Conflict(new
                    {
                        success = false,  message = "Mã thanh toán đã tồn tại."
                    });

                tt_bll.Create(model);

                return StatusCode(201, new
                {
                    success = true, message = "Thêm thông tin thanh toán thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi thêm thanh toán: " + ex.Message
                });
            }
        }

        // UPDATE
        [Route("update-thanhtoan")]
        [HttpPut]
        public IActionResult UpdateThanhToan(
            [FromBody] Models.ThanhToan model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new
                    {
                        success = false,  message = "Dữ liệu thanh toán không được để trống."
                    });

                // MÃ THANH TOÁN
                if (string.IsNullOrWhiteSpace(model.MaThanhToan))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã thanh toán không được để trống."
                    });

                model.MaThanhToan = model.MaThanhToan.Trim();

                if (model.MaThanhToan.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã thanh toán không được vượt quá 15 ký tự."
                    });

                // PHƯƠNG THỨC
                if (string.IsNullOrWhiteSpace(model.PhuongThuc))
                    return BadRequest(new
                    {
                        success = false, message = "Phương thức thanh toán không được để trống."
                    });

                model.PhuongThuc = model.PhuongThuc.Trim();

                if (model.PhuongThuc.Length > 50)
                    return BadRequest(new
                    {
                        success = false,  message = "Phương thức thanh toán không được vượt quá 50 ký tự."
                    });

                // TRẠNG THÁI
                if (string.IsNullOrWhiteSpace(model.TrangThai))
                    return BadRequest(new
                    {
                        success = false, message = "Trạng thái thanh toán không được để trống."
                    });

                model.TrangThai = model.TrangThai.Trim();

                if (model.TrangThai.Length > 50)
                    return BadRequest(new
                    {
                        success = false,  message = "Trạng thái thanh toán không được vượt quá 50 ký tự."
                    });

                // KIỂM TRA TỒN TẠI
                DataTable dt = tt_bll.GetById(model.MaThanhToan);

                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new
                    {
                        success = false,   message = "Không tồn tại thanh toán có mã này."
                    });

                tt_bll.Update(model);

                return Ok(new
                {
                    success = true, message = "Thay đổi thông tin thanh toán thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi cập nhật thanh toán: " + ex.Message
                });
            }
        }

        // DELETE
        [Route("del-thanhtoan")]
        [HttpDelete]
        public IActionResult DeleteThanhToan([FromQuery] string? ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                    return BadRequest(new
                    {
                        success = false, message = "Mã thanh toán không được để trống."
                    });

                ma = ma.Trim();

                if (ma.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã thanh toán không được vượt quá 15 ký tự."
                    });

                DataTable dt = tt_bll.GetById(ma);

                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new
                    {
                        success = false, message = "Không có thông tin thanh toán có mã này."
                    });

                tt_bll.Delete(ma);

                return Ok(new
                {
                    success = true, message = "Xóa thông tin thanh toán thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi xóa thanh toán: " + ex.Message
                });
            }
        }



        [Route("reset-sotienthanhtoan-by-mahdban")]
        [HttpPost]
        public IActionResult ResetSoTienThanhToanByHoaDon( [FromQuery] string? maHDBan, [FromQuery] decimal soTienMoi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDBan))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn bán không được để trống."
                    });

                maHDBan = maHDBan.Trim();

                if (maHDBan.Length > 15)
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn bán không được vượt quá 15 ký tự."
                    });

                // Kiểm tra có truyền tham số hay không
                if (!Request.Query.ContainsKey("soTienMoi"))
                    return BadRequest(new
                    {
                        success = false, message = "Số tiền mới không được để trống."
                    });

                if (soTienMoi < 0)
                    return BadRequest(new
                    {
                        success = false,  message = "Số tiền mới không được nhỏ hơn 0."
                    });

                DataTable dt =  tt_bll.ResetSoTienByHoaDon( maHDBan, soTienMoi);

                List<Dictionary<string, object>> data =  new List<Dictionary<string, object>>();

                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        Dictionary<string, object> item = new Dictionary<string, object>();

                        foreach (DataColumn column in dt.Columns)
                        {
                            if (row[column] == DBNull.Value)
                                item[column.ColumnName] = null;
                            else
                                item[column.ColumnName] = row[column];
                        }

                        data.Add(item);
                    }
                }

                return Ok(new
                {
                    success = true, message = "Đã reset số tiền thanh toán theo hóa đơn.", data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi reset số tiền thanh toán: " + ex.Message
                });
            }
        }

        [Route("reset-tongtienhang-by-mahdban")]
        [HttpPost]
        public IActionResult ResetTongTienHangByHoaDon([FromQuery] string? maHDBan, [FromQuery] decimal tongTienMoi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDBan))
                    return BadRequest(new
                    {
                        success = false,  message = "Mã hóa đơn bán không được để trống."
                    });

                maHDBan = maHDBan.Trim();

                if (maHDBan.Length > 15)
                    return BadRequest(new
                    {
                        success = false, message = "Mã hóa đơn bán không được vượt quá 15 ký tự."
                    });

                if (!Request.Query.ContainsKey("tongTienMoi"))
                    return BadRequest(new
                    {
                        success = false, message = "Tổng tiền mới không được để trống."
                    });

                if (tongTienMoi < 0)
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tổng tiền hàng không được nhỏ hơn 0."
                    });

                bool result =hdb_bll.ResetTongTienHangByHoaDon( maHDBan,  tongTienMoi);

                if (!result)
                    return NotFound(new
                    {
                        success = false, message = "Không tìm thấy hóa đơn hoặc cập nhật thất bại."
                    });

                return Ok(new
                {
                    success = true, message = "Đã cập nhật tổng tiền hàng cho hóa đơn."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, message = "Lỗi khi cập nhật tổng tiền hàng: " + ex.Message
                });
            }
        }
    }
}