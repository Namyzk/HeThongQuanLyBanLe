using BLL;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace _API_ThuNgan.Controllers
{
    [Authorize]
   // [AllowAnonymous]
    [Route("api/QuanLyBanHang")]
    [ApiController]
    public class QuanLyBanHang_Controller : ControllerBase
    {
        private readonly HoaDonBan_BLL hdb_bll;
        private readonly ChiTietBan_BLL ctb_bll;
        private readonly KhachHang_BLL KH_BLL;
        private readonly DanhMuc_BLL dm_bll;
        private readonly SanPham_BLL sp_bll;
        private readonly ThanhToan_BLL tt_bll;

        public QuanLyBanHang_Controller(IConfiguration configuration)
        {
            hdb_bll = new HoaDonBan_BLL();
            ctb_bll = new ChiTietBan_BLL();
            KH_BLL = new KhachHang_BLL();
            dm_bll = new DanhMuc_BLL();
            sp_bll = new SanPham_BLL();
            tt_bll = new ThanhToan_BLL();
        }


        [Route("get-all-hoadonban")]
        [HttpGet]
        public IActionResult GetAll_HoaDon()
        {
            try
            {
                var result = hdb_bll.LayTatCa();

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
                    message = "Lỗi khi lấy danh sách hóa đơn: " + ex.Message
                });
            }
        }

        [Route("get-hoadonban-by-id")]
        [HttpGet]
        public IActionResult GetByID_HoaDon([FromQuery] string? maHoaDon)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHoaDon))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được để trống."
                    });
                }

                maHoaDon = maHoaDon.Trim();

                if (maHoaDon.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });
                }

                var result = hdb_bll.LayTheoID(maHoaDon);

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Không tìm thấy hóa đơn có mã '{maHoaDon}'."
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
                    message = "Lỗi khi lấy hóa đơn: " + ex.Message
                });
            }
        }


        
        [Route("insert-hoadonban")]
        [HttpPost]
        public IActionResult CreateHoaDon( [FromBody] HoaDonBan model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu hóa đơn không được để trống."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.MAHDBAN))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được để trống."
                    });
                }

                model.MAHDBAN = model.MAHDBAN.Trim();

                if (model.MAHDBAN.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });
                }

                if (model.listjson_chitietban == null ||
                    model.listjson_chitietban.Count == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Hóa đơn phải có ít nhất một sản phẩm."
                    });
                }

                foreach (var ct in model.listjson_chitietban)
                {
                    if (ct == null)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Chi tiết hóa đơn không được để trống."
                        });
                    }

                    if (string.IsNullOrWhiteSpace(ct.MASP))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Mã sản phẩm trong chi tiết hóa đơn không được để trống."
                        });
                    }

                    ct.MASP = ct.MASP.Trim();

                    if (ct.MASP.Length > 15)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Mã sản phẩm không được vượt quá 15 ký tự."
                        });
                    }

                    if (ct.SOLUONG <= 0)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Số lượng sản phẩm phải lớn hơn 0."
                        });
                    }

                    if (ct.DONGIA <= 0)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Đơn giá sản phẩm phải lớn hơn 0."
                        });
                    }
                }

                bool kq = hdb_bll.ThemMoi(model);

                if (!kq)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Không thể thêm hóa đơn. Có thể mã hóa đơn đã tồn tại hoặc dữ liệu không hợp lệ."
                    });
                }

                return StatusCode(201, new
                {
                    success = true,
                    message = "Thêm hóa đơn thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm hóa đơn: " + ex.Message
                });
            }
        }

        [Route("get-all-chitietban")]
        [HttpGet]
        public IActionResult GetAll_ChiTiet()
        {
            try
            {
                var result = ctb_bll.LayTatCa();

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
                    message = "Lỗi khi lấy chi tiết bán: " + ex.Message
                });
            }
        }

        [Route("get-chitietban-by-IDhoadon")]
        [HttpGet]
        public IActionResult GetByHoaDon([FromQuery] string? maHDB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDB))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được để trống."
                    });
                }

                maHDB = maHDB.Trim();

                if (maHDB.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được vượt quá 15 ký tự."
                    });
                }

                var result = ctb_bll.LayTheoHoaDon(maHDB);

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy chi tiết của hóa đơn."
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
                    message = "Lỗi khi lấy chi tiết bán: " + ex.Message
                });
            }
        }

        [Route("get-all-khachhang")]
        [HttpGet]
        public IActionResult GetAllKH()
        {
            try
            {
                DataTable dt = KH_BLL.getAllKH();

                var data = dt.AsEnumerable()
                    .Select(row => dt.Columns
                        .Cast<DataColumn>()
                        .ToDictionary(
                            col => col.ColumnName,
                            col => row[col] == DBNull.Value ? null : row[col]
                        ))
                    .ToList();

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách khách hàng thành công.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy danh sách khách hàng: " + ex.Message
                });
            }
        }


      
        [Route("get-byid-khachhang")]
        [HttpGet]
        public IActionResult GetByIdKH([FromQuery] string maKH)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maKH))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã khách hàng không được để trống."
                    });
                }

                maKH = maKH.Trim();

                if (maKH.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã khách hàng không được vượt quá 15 ký tự."
                    });
                }

                DataTable dt = KH_BLL.GetByIdKH(maKH);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy khách hàng."
                    });
                }

                List<Dictionary<string, object>> data =  new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, object> item =  new Dictionary<string, object>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        if (row[column] == DBNull.Value)
                        {
                            item[column.ColumnName] = null;
                        }
                        else
                        {
                            item[column.ColumnName] = row[column];
                        }
                    }

                    data.Add(item);
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin khách hàng thành công.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy khách hàng: " + ex.Message
                });
            }
        }


        [Route("insert-khachhang")]
        [HttpPost]
        public IActionResult CreateKhachHang( [FromBody] KhachHang kh)
        {
            try
            {
                if (kh == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu khách hàng không được để trống."
                    });
                }

                if (string.IsNullOrWhiteSpace(kh.MaKH))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã khách hàng không được để trống."
                    });
                }

                kh.MaKH = kh.MaKH.Trim();

                if (kh.MaKH.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã khách hàng không được vượt quá 15 ký tự."
                    });
                }

                if (string.IsNullOrWhiteSpace(kh.TenKH))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên khách hàng không được để trống."
                    });
                }

                kh.TenKH = kh.TenKH.Trim();

                if (kh.TenKH.Length > 100)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên khách hàng không được vượt quá 100 ký tự."
                    });
                }

                if (!string.IsNullOrWhiteSpace(kh.SDT))
                {
                    kh.SDT = kh.SDT.Trim();

                    if (kh.SDT.Length != 10)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Số điện thoại phải có đúng 10 ký tự."
                        });
                    }

                    if (!kh.SDT.All(char.IsDigit))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Số điện thoại chỉ được chứa chữ số."
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(kh.DiaChi))
                {
                    kh.DiaChi = kh.DiaChi.Trim();

                    if (kh.DiaChi.Length > 300)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Địa chỉ không được vượt quá 300 ký tự."
                        });
                    }
                }

                DataTable check = KH_BLL.GetByIdKH(kh.MaKH);

                if (check != null && check.Rows.Count > 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        message = "Mã khách hàng đã tồn tại."
                    });
                }

                KH_BLL.CreateKH(kh);

                return StatusCode(201, new
                {
                    success = true,
                    message = "Thêm khách hàng thành công."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("không được để trống") ||
                    ex.Message.Contains("không được vượt quá") ||
                    ex.Message.Contains("phải có đúng") ||
                    ex.Message.Contains("chỉ được chứa"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi thêm khách hàng: " + ex.Message
                });
            }
        }

        [Route("get-all-danhmuc")]
        [HttpGet]
        public IActionResult GetAll_DanhMuc()
        {
            try
            {
                var result = dm_bll.LayTatCa();

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
                    message = "Lỗi khi lấy danh sách danh mục: " + ex.Message
                });
            }
        }

        [Route("get-byID-danhmuc")]
        [HttpGet]
        public IActionResult GetByID_DanhMuc([FromQuery] string? madanhmuc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(madanhmuc))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã danh mục không được để trống."
                    });
                }

                madanhmuc = madanhmuc.Trim();

                if (madanhmuc.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã danh mục không được vượt quá 15 ký tự."
                    });
                }

                var result = dm_bll.LayTheoID(madanhmuc);

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy danh mục."
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
                    message = "Lỗi khi lấy danh mục: " + ex.Message
                });
            }
        }

        [Route("get-all-sanpham")]
        [HttpGet]
        public IActionResult GetAll_SanPham()
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


        [Route("get-sanpham-by-id")]
        [HttpGet]
        public IActionResult GetByID_SanPham([FromQuery] string? id)
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

                id = id.Trim();

                if (id.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });
                }

                var result = sp_bll.LayTheoID(id);

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm."
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

        [Route("update-soluong-sanpham")]
        [HttpPatch]
        public IActionResult UpdateSoLuong( [FromQuery] string maSP, [FromQuery] int soLuongMoi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maSP))
                {
                    return BadRequest(new
                    {
                        success = false,message = "Mã sản phẩm không được để trống."
                    });
                }

                maSP = maSP.Trim();

                if (maSP.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã sản phẩm không được vượt quá 15 ký tự."
                    });
                }

                
                if (!Request.Query.ContainsKey("soLuongMoi"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Số lượng mới không được để trống."
                    });
                }

               
                if (soLuongMoi < 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Số lượng tồn không được nhỏ hơn 0."
                    });
                }

                string error = sp_bll.SuaSoLuong(maSP, soLuongMoi);

                if (error != null)
                {
                    if (error.Contains("Không tìm thấy") ||error.Contains("không tồn tại"))
                    {
                        return NotFound(new
                        {
                            success = false, message = error
                        });
                    }

                    return BadRequest(new
                    {
                        success = false,  message = error
                    });
                }

                return Ok(new
                {
                    success = true, message = "Cập nhật số lượng sản phẩm thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi cập nhật số lượng sản phẩm: " + ex.Message
                });
            }
        }

        [Route("get-all-thanhtoan")]
        [HttpGet]
        public IActionResult GetAllThanhToan()
        {
            try
            {
                DataTable dt = tt_bll.getAll();

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false, message = "Không tìm thấy dữ liệu thanh toán."
                    });
                }

                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, object> item =
                        new Dictionary<string, object>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        if (row[column] == DBNull.Value)
                        {
                            item[column.ColumnName] = null;
                        }
                        else
                        {
                            item[column.ColumnName] = row[column];
                        }
                    }

                    data.Add(item);
                }

                return Ok(new
                {
                    success = true, message = "Lấy danh sách thanh toán thành công.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi lấy danh sách thanh toán: " + ex.Message
                });
            }
        }


        [Route("get-thanhtoan-by-id")]
        [HttpGet]
        public IActionResult GetThanhToanById( [FromQuery] string? maThanhToan)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maThanhToan))
                {
                    return BadRequest(new
                    {
                        success = false,message = "Mã thanh toán không được để trống."
                    });
                }

                maThanhToan = maThanhToan.Trim();

                if (maThanhToan.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã thanh toán không được vượt quá 15 ký tự."
                    });
                }

                DataTable dt = tt_bll.GetById(maThanhToan);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thanh toán."
                    });
                }

                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    Dictionary<string, object> item =  new Dictionary<string, object>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        if (row[column] == DBNull.Value)
                        {
                            item[column.ColumnName] = null;
                        }
                        else
                        {
                            item[column.ColumnName] = row[column];
                        }
                    }

                    data.Add(item);
                }

                return Ok(new
                {
                    success = true, message = "Lấy thông tin thanh toán thành công.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi lấy thanh toán: " + ex.Message
                });
            }
        }

        [Route("insert-thanhtoan")]
        [HttpPost]
        public IActionResult CreateThanhToan([FromBody] Models.ThanhToan model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Dữ liệu thanh toán không được để trống."
                    });
                }

                if (string.IsNullOrWhiteSpace(model.MaThanhToan))
                {
                    return BadRequest(new
                    {
                        success = false,  message = "Mã thanh toán không được để trống."
                    });
                }

                model.MaThanhToan = model.MaThanhToan.Trim();

                if (model.MaThanhToan.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Mã thanh toán không được vượt quá 15 ký tự."
                    });
                }

                DataTable dt = tt_bll.GetById(model.MaThanhToan);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return Conflict(new
                    {
                        success = false,  message = "Đã tồn tại thanh toán có mã này."
                    });
                }

                tt_bll.Create(model);

                return StatusCode(201, new
                {
                    success = true, message = "Thêm thông tin thanh toán thành công."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("không được để trống") || ex.Message.Contains("không được vượt quá"))
                {
                    return BadRequest(new
                    {
                        success = false, message = ex.Message
                    });
                }

                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi khi thêm thanh toán: " + ex.Message
                });
            }
        }
    }
}