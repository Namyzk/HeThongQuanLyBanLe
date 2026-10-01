using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace API_KeToan1.Controllers
{
    [Authorize]
    
    [Route("api/QuanLyCongNo")]
    [ApiController]
    public class QuanLyCongNo_Controller : ControllerBase
    {
        private readonly NhaCungCap_BLL NCC_BLL;
        private readonly KhachHang_BLL KH_BLL;
        private readonly ThanhToan_BLL TT_BLL;
        private readonly HoaDonBan_BLL hdb_bll;


        public QuanLyCongNo_Controller()
        {
            NCC_BLL = new NhaCungCap_BLL();
            KH_BLL = new KhachHang_BLL();
            TT_BLL = new ThanhToan_BLL();
            hdb_bll = new HoaDonBan_BLL();

        }



        [HttpGet("search-khachhang-chuathanhtoan")]
        public IActionResult SearchKhachHangChuaThanhToan( [FromQuery] string? tenKh)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(tenKh))
                {
                    return BadRequest(new
                    {
                        success = false, message = "Tên khách hàng không được để trống."
                    });
                }

                tenKh = tenKh.Trim();

                if (tenKh.Length > 100)
                {
                    return BadRequest(new
                    {
                        success = false, message = "Tên khách hàng không được vượt quá 100 ký tự."
                    });
                }

                // Gọi BLL
                DataTable dt = TT_BLL.GetHoaDonChuaThanhToanTheoTen(tenKh);

             
                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,  message = "Không tìm thấy khách hàng hoặc khách hàng không có hóa đơn chưa thanh toán.",   tenKh = tenKh
                    });
                }

                
                var list = new List<Dictionary<string, object>>();

                foreach (DataRow row in dt.Rows)
                {
                    var dict = new Dictionary<string, object>();

                    foreach (DataColumn col in dt.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value  ? null: row[col];
                    }

                    list.Add(dict);
                }

                return Ok(new
                {
                    success = true, message = "Lấy danh sách hóa đơn chưa thanh toán theo tên khách hàng thành công.",
                    count = list.Count, data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,  message = "Lỗi: " + ex.Message
                });
            }
        }




        [HttpPut("update-trangthai-thanhtoan")]
        public IActionResult UpdateTrangThaiThanhToan(
    [FromQuery] string? maHDBan,
    [FromQuery] string? phuongThuc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDBan))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã hóa đơn không được để trống."
                    });
                }

                if (string.IsNullOrWhiteSpace(phuongThuc))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Phương thức thanh toán không được để trống."
                    });
                }

                maHDBan = maHDBan.Trim();
                phuongThuc = phuongThuc.Trim();

                int result = TT_BLL.UpdateTrangThaiThanhToan(
                    maHDBan,
                    phuongThuc);

                if (result == -1)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy hóa đơn: " + maHDBan
                    });
                }

                if (result == -2)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Hóa đơn tồn tại nhưng chưa có thông tin thanh toán."
                    });
                }

                if (result != 0)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Cập nhật trạng thái thanh toán thất bại."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Cập nhật trạng thái thanh toán thành công.",
                    maHDBan = maHDBan,
                    phuongThuc = phuongThuc
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }




        [HttpGet("get-hoadon-chuathanhtoan")]
        public IActionResult GetHoaDonChuaThanhToan()
        {
            try
            {
                DataTable dt = TT_BLL.GetHoaDonChuaThanhToan();

                var list = ChuyenDataTableThanhDictionary(dt);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách hóa đơn chưa thanh toán thành công.",
                    count = list.Count,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }



        [HttpGet("get-all-hoadonban")]
        public IActionResult GetAll_HDB()
        {
            try
            {
                var result = hdb_bll.LayTatCa();

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không có hóa đơn bán nào."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách hóa đơn bán thành công.",
                    count = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }



        [HttpGet("get-hoadonban-by-id")]
        public IActionResult Get_HDB_ByID(
            [FromQuery] string? maHoaDon)
        {
            try
            {
                // Kiểm tra mã
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

                var result =
                    hdb_bll.LayTheoID(maHoaDon);

                if (result == null || result.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Không tìm thấy hóa đơn có mã: " + maHoaDon
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin hóa đơn thành công.",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }


        private List<Dictionary<string, object>>
            ChuyenDataTableThanhDictionary(DataTable dt)
        {
            var list =
                new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict =
                    new Dictionary<string, object>();

                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] =
                        row[col] == DBNull.Value
                            ? null
                            : row[col];
                }

                list.Add(dict);
            }

            return list;
        }


        

        [HttpGet("get-all-thanhtoan")]
        public IActionResult GetAll_ThanhToan()
        {
            try
            {
                DataTable dt =
                    TT_BLL.getAll();

                var list =
                    ChuyenThanhList(dt);

                if (list.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không có dữ liệu thanh toán."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Lấy danh sách thanh toán thành công.",
                    count = list.Count,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }


        [HttpGet("get-byId-thanhtoan")]
        public IActionResult GetByIdThanhToan([FromQuery] string? ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã thanh toán không được để trống."
                    });
                }

                ma = ma.Trim();

                DataTable dt = TT_BLL.GetById(ma);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thanh toán với mã: " + ma
                    });
                }

                var list = ChuyenDataTableThanhDictionary(dt);

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin thanh toán thành công.",
                    data = list[0]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }

        [HttpPost("update-thanhtoan")]
        public IActionResult Update(
            [FromBody] Models.ThanhToan? model)
        {
            try
            {
                // Model null
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu thanh toán không được để trống."
                    });
                }

                // Mã thanh toán
                if (string.IsNullOrWhiteSpace(model.MaThanhToan))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã thanh toán không được để trống."
                    });
                }

                model.MaThanhToan =
                    model.MaThanhToan.Trim();

                if (model.MaThanhToan.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Mã thanh toán không được vượt quá 15 ký tự."
                    });
                }

                // Kiểm tra tồn tại
                DataTable dt =
                    TT_BLL.GetById(model.MaThanhToan);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Không tồn tại thanh toán có mã: "
                            + model.MaThanhToan
                    });
                }

                // Update
                TT_BLL.Update(model);

                return Ok(new
                {
                    success = true,
                    message =
                        "Thay đổi thông tin thanh toán thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }


      

        [HttpGet("get-all-nhacungcap")]
        public IActionResult GetAllNCC()
        {
            try
            {
                DataTable dt =
                    NCC_BLL.GetAll();

                var list =
                    new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        MANCC = row["MANCC"] == DBNull.Value
                            ? null
                            : row["MANCC"].ToString()?.Trim(),

                        TENNCC = row["TENNCC"] == DBNull.Value
                            ? null
                            : row["TENNCC"].ToString()?.Trim(),

                        DIACHI = row["DIACHI"] == DBNull.Value
                            ? null
                            : row["DIACHI"].ToString()?.Trim(),

                        SDT = row["SDT"] == DBNull.Value
                            ? null
                            : row["SDT"].ToString()?.Trim(),

                        EMAIL = row["EMAIL"] == DBNull.Value
                            ? null
                            : row["EMAIL"].ToString()?.Trim()
                    });
                }

                if (list.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không có nhà cung cấp nào."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Lấy danh sách nhà cung cấp thành công.",
                    count = list.Count,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }



        [HttpGet("get-byid-nhacungcap")]
        public IActionResult Get_NCC_ById(
    [FromQuery] string? ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã nhà cung cấp không được để trống."
                    });
                }

                ma = ma.Trim();

                if (ma.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã nhà cung cấp không được vượt quá 15 ký tự."
                    });
                }

                DataTable dt = NCC_BLL.GetById(ma);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy nhà cung cấp có mã: " + ma
                    });
                }

                DataRow row = dt.Rows[0];

                var data = new
                {
                    MANCC = row["MANCC"] == DBNull.Value
                        ? null
                        : row["MANCC"].ToString()?.Trim(),

                    TENNCC = row["TENNCC"] == DBNull.Value
                        ? null
                        : row["TENNCC"].ToString()?.Trim(),

                    DIACHI = row["DIACHI"] == DBNull.Value
                        ? null
                        : row["DIACHI"].ToString()?.Trim(),

                    SDT = row["SDT"] == DBNull.Value
                        ? null
                        : row["SDT"].ToString()?.Trim(),

                    EMAIL = row["EMAIL"] == DBNull.Value
                        ? null
                        : row["EMAIL"].ToString()?.Trim()
                };

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin nhà cung cấp thành công.",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }




        [HttpGet("get-all-khachhang")]
        public IActionResult GetAllKH()
        {
            try
            {
                DataTable dt =
                    KH_BLL.getAllKH();

                var list =
                    new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        MaKH = row["MaKH"] == DBNull.Value
                            ? null
                            : row["MaKH"].ToString()?.Trim(),

                        TenKH = row["TenKH"] == DBNull.Value
                            ? null
                            : row["TenKH"].ToString()?.Trim(),

                        SDT = row["SDT"] == DBNull.Value
                            ? null
                            : row["SDT"].ToString()?.Trim(),

                        DiaChi = row["DiaChi"] == DBNull.Value
                            ? null
                            : row["DiaChi"].ToString()?.Trim()
                    });
                }

                if (list.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không có khách hàng nào."
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Lấy danh sách khách hàng thành công.",
                    count = list.Count,
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }


       

        [HttpGet("get-byid-khachhang")]
        public IActionResult GetByIdKH(
            [FromQuery] string? makh)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(makh))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Mã khách hàng không được để trống."
                    });
                }

                makh = makh.Trim();

                if (makh.Length > 15)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message =
                            "Mã khách hàng không được vượt quá 15 ký tự."
                    });
                }

                DataTable dt =
                    KH_BLL.GetByIdKH(makh);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false,
                        message =
                            "Không tìm thấy khách hàng có mã: " + makh
                    });
                }

                var list =
                    new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        MaKH = row["MaKH"] == DBNull.Value
                            ? null
                            : row["MaKH"].ToString()?.Trim(),

                        TenKH = row["TenKH"] == DBNull.Value
                            ? null
                            : row["TenKH"].ToString()?.Trim(),

                        SDT = row["SDT"] == DBNull.Value
                            ? null
                            : row["SDT"].ToString()?.Trim(),

                        DiaChi = row["DiaChi"] == DBNull.Value
                            ? null
                            : row["DiaChi"].ToString()?.Trim()
                    });
                }

                return Ok(new
                {
                    success = true,
                    message =
                        "Lấy thông tin khách hàng thành công.",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }


       

        private List<object> ChuyenThanhList(DataTable dt)
        {
            var list =
                new List<object>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new
                {
                    MATHANHTOAN =
                        row["MATHANHTOAN"] == DBNull.Value
                            ? null
                            : row["MATHANHTOAN"].ToString()?.Trim(),

                    MAHDBan =
                        row["MAHDBan"] == DBNull.Value
                            ? null
                            : row["MAHDBan"].ToString()?.Trim(),

                    PhuongThuc =
                        row["PhuongThuc"] == DBNull.Value
                            ? null
                            : row["PhuongThuc"].ToString()?.Trim(),

                    SoTienThanhToan =
                        row["SoTienThanhToan"] == DBNull.Value
                            ? null
                            : row["SoTienThanhToan"],

                    NGAYTHANHTOAN =
                        row["NGAYTHANHTOAN"] == DBNull.Value
                            ? null
                            : row["NGAYTHANHTOAN"],

                    TrangThai =
                        row["TrangThai"] == DBNull.Value
                            ? null
                            : row["TrangThai"].ToString()?.Trim()
                });
            }

            return list;
        }
    }
}