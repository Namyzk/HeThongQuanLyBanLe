
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_KeToan1.Controllers
{
    [Authorize(Roles = "Admin,KeToan")]
    [Route("api/QuanLyCongNo")]
    [ApiController]
    public class QuanLyCongNo_Controller : ControllerBase
    {
        private readonly NhaCungCap_BLL _nccBll;
        private readonly KhachHang_BLL _khBll;
        private readonly ThanhToan_BLL _ttBll;
        private readonly HoaDonBan_BLL _hdbBll;

        public QuanLyCongNo_Controller(NhaCungCap_BLL _nccBll, KhachHang_BLL _khBll, ThanhToan_BLL _ttBll, HoaDonBan_BLL _hdbBll)
        {
            this._nccBll = _nccBll;
            this._khBll = _khBll;
            this._ttBll = _ttBll;
            this._hdbBll = _hdbBll;
        }

    
        private IActionResult? ValidateInput(string? value, string fieldName, int maxLength, out string cleanValue)
        {
            cleanValue = string.Empty;
            if (string.IsNullOrWhiteSpace(value))
                return BadRequest(new { success = false, message = $"{fieldName} không được để trống." });

            cleanValue = value.Trim();
            if (cleanValue.Length > maxLength)
                return BadRequest(new { success = false, message = $"{fieldName} không được vượt quá {maxLength} ký tự." });

            return null;
        }

      
        private static List<Dictionary<string, object?>> ToDictionaryList(DataTable? dt)
        {
            if (dt == null || dt.Rows.Count == 0) return new List<Dictionary<string, object?>>();

            return dt.AsEnumerable().Select(row =>
                dt.Columns.Cast<DataColumn>().ToDictionary(
                    col => col.ColumnName,
                    col => row[col] == DBNull.Value ? null : (row[col] is string str ? str.Trim() : row[col])
                )
            ).ToList();
        }

        // ==================== 1. CÔNG NỢ & THANH TOÁN ====================

        [HttpGet("search-khachhang-chuathanhtoan")]
        public IActionResult SearchKhachHangChuaThanhToan([FromQuery] string? tenKh)
        {
            if (ValidateInput(tenKh, "Tên khách hàng", 100, out string cleanTenKh) is IActionResult error)
                return error;

            try
            {
                DataTable dt = _ttBll.GetHoaDonChuaThanhToanTheoTen(cleanTenKh);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy hóa đơn chưa thanh toán của khách: '{cleanTenKh}'" });

                return Ok(new { success = true, message = "Lấy danh sách thành công.", count = list.Count, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-hoadon-chuathanhtoan")]
        public IActionResult GetHoaDonChuaThanhToan()
        {
            try
            {
                DataTable dt = _ttBll.GetHoaDonChuaThanhToan();
                var list = ToDictionaryList(dt);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPut("update-trangthai-thanhtoan")]
        public IActionResult UpdateTrangThaiThanhToan([FromQuery] string? maHDBan, [FromQuery] string? phuongThuc)
        {
            if (ValidateInput(maHDBan, "Mã hóa đơn", 15, out string cleanMaHDB) is IActionResult err1) return err1;
            if (ValidateInput(phuongThuc, "Phương thức thanh toán", 50, out string cleanPhuongThuc) is IActionResult err2) return err2;

            try
            {
                int result = _ttBll.UpdateTrangThaiThanhToan(cleanMaHDB, cleanPhuongThuc);

                return result switch
                {
                    -1 => NotFound(new { success = false, message = $"Không tìm thấy hóa đơn: {cleanMaHDB}" }),
                    -2 => NotFound(new { success = false, message = "Hóa đơn tồn tại nhưng chưa có thông tin thanh toán." }),
                    0 => Ok(new { success = true, message = "Cập nhật trạng thái thanh toán thành công.", maHDBan = cleanMaHDB, phuongThuc = cleanPhuongThuc }),
                    _ => StatusCode(500, new { success = false, message = "Cập nhật trạng thái thanh toán thất bại." })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-all-thanhtoan")]
        public IActionResult GetAll_ThanhToan()
        {
            try
            {
                DataTable dt = _ttBll.getAll();
                var list = ToDictionaryList(dt);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byId-thanhtoan")]
        public IActionResult GetByIdThanhToan([FromQuery] string? ma)
        {
            if (ValidateInput(ma, "Mã thanh toán", 15, out string cleanMa) is IActionResult error)
                return error;

            try
            {
                DataTable dt = _ttBll.GetById(cleanMa);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy thanh toán có mã: {cleanMa}" });

                return Ok(new { success = true, data = list[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("update-thanhtoan")]
        public IActionResult Update([FromBody] ThanhToan? model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MaThanhToan))
                return BadRequest(new { success = false, message = "Dữ liệu và mã thanh toán không được để trống." });

            string maTT = model.MaThanhToan.Trim();
            if (maTT.Length > 15)
                return BadRequest(new { success = false, message = "Mã thanh toán không được vượt quá 15 ký tự." });

            try
            {
                DataTable dt = _ttBll.GetById(maTT);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = $"Không tồn tại thanh toán có mã: {maTT}" });

                model.MaThanhToan = maTT;
                _ttBll.Update(model);
                return Ok(new { success = true, message = "Thay đổi thông tin thanh toán thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

       

        [HttpGet("get-all-hoadonban")]
        public IActionResult GetAll_HDB()
        {
            try
            {
                var result = _hdbBll.LayTatCa();
                return Ok(new { success = true, count = result?.Count ?? 0, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-hoadonban-by-id")]
        public IActionResult Get_HDB_ByID([FromQuery] string? maHoaDon)
        {
            if (ValidateInput(maHoaDon, "Mã hóa đơn", 15, out string cleanMaHD) is IActionResult error)
                return error;

            try
            {
                var result = _hdbBll.LayTheoID(cleanMaHD);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy hóa đơn: {cleanMaHD}" });

                return Ok(new { success = true, data = result[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

      
        [HttpGet("get-all-nhacungcap")]
        public IActionResult GetAllNCC()
        {
            try
            {
                DataTable dt = _nccBll.GetAll();
                var list = ToDictionaryList(dt);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-nhacungcap")]
        public IActionResult Get_NCC_ById([FromQuery] string? ma)
        {
            if (ValidateInput(ma, "Mã nhà cung cấp", 15, out string cleanMa) is IActionResult error)
                return error;

            try
            {
                DataTable dt = _nccBll.GetById(cleanMa);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy nhà cung cấp: {cleanMa}" });

                return Ok(new { success = true, data = list[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

      

        [HttpGet("get-all-khachhang")]
        public IActionResult GetAllKH()
        {
            try
            {
                DataTable dt = _khBll.getAllKH();
                var list = ToDictionaryList(dt);
                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-khachhang")]
        public IActionResult GetByIdKH([FromQuery] string? makh)
        {
            if (ValidateInput(makh, "Mã khách hàng", 15, out string cleanMa) is IActionResult error)
                return error;

            try
            {
                DataTable dt = _khBll.GetByIdKH(cleanMa);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy khách hàng có mã: {cleanMa}" });

                return Ok(new { success = true, data = list[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

