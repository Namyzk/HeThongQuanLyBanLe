
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_ThuNgan.Controllers
{
    [Authorize(Roles = "Admin,ThuNgan")]
    [Route("api/QuanLyDoiTra")]
    [ApiController]
    public class QuanLyDoiTra_Controller : ControllerBase
    {
        private readonly HoaDonBan_BLL _hdbBll;
        private readonly ChiTietBan_BLL _ctbBll;
        private readonly ThanhToan_BLL _ttBll;

        public QuanLyDoiTra_Controller(HoaDonBan_BLL _hdbBll, ChiTietBan_BLL _ctbBll, ThanhToan_BLL _ttBll)
        {
            this._hdbBll = _hdbBll;
            this._ctbBll = _ctbBll;
            this._ttBll = _ttBll;
        }

  
        private IActionResult? ValidateMa(string? ma, string tenTruong, int maxLen, out string cleanMa)
        {
            cleanMa = string.Empty;
            if (string.IsNullOrWhiteSpace(ma))
                return BadRequest(new { success = false, message = $"{tenTruong} không được để trống." });

            cleanMa = ma.Trim();
            if (cleanMa.Length > maxLen)
                return BadRequest(new { success = false, message = $"{tenTruong} không được vượt quá {maxLen} ký tự." });

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

       
        private IActionResult? ValidateChiTietBan(ChiTietBan? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu chi tiết bán không được để trống." });

            if (ValidateMa(model.MAHDBAN, "Mã hóa đơn", 15, out string cleanMaHD) is IActionResult errHD) return errHD;
            if (ValidateMa(model.MASP, "Mã sản phẩm", 15, out string cleanMaSP) is IActionResult errSP) return errSP;

            model.MAHDBAN = cleanMaHD;
            model.MASP = cleanMaSP;

            if (model.SOLUONG <= 0)
                return BadRequest(new { success = false, message = "Số lượng phải lớn hơn 0." });

            if (model.DONGIA <= 0)
                return BadRequest(new { success = false, message = "Đơn giá phải lớn hơn 0." });

            model.TONGTIEN = model.SOLUONG * model.DONGIA;

            return null;
        }

     
        private IActionResult HandleDbSqlException(Exception ex, ChiTietBan model)
        {
            string error = ex.Message;
            if (error.Contains("FK__CT_HDB__MAHDBAN", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = $"Mã hóa đơn '{model.MAHDBAN}' không tồn tại." });

            if (error.Contains("FK__CT_HDB__MASP", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = $"Mã sản phẩm '{model.MASP}' không tồn tại." });

            if (error.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) || error.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { success = false, message = "Chi tiết sản phẩm này đã tồn tại trong hóa đơn." });

            return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
        }

    
        [HttpGet("get-all-hoadonban")]
        public IActionResult GetAll_HoaDon()
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
        public IActionResult GetByID_HoaDon([FromQuery] string? maHoaDon)
        {
            if (ValidateMa(maHoaDon, "Mã hóa đơn", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                var result = _hdbBll.LayTheoID(cleanMa);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy hóa đơn có mã '{cleanMa}'." });

                return Ok(new { success = true, data = result[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPut("update-hoadonban")]
        public IActionResult UpdateHoaDon([FromBody] HoaDonBan? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu hóa đơn không được để trống." });

            if (ValidateMa(model.MAHDBAN, "Mã hóa đơn", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            model.MAHDBAN = cleanMa;

            if (model.listjson_chitietban == null || model.listjson_chitietban.Count == 0)
                return BadRequest(new { success = false, message = "Hóa đơn phải có ít nhất một chi tiết bán." });

            var maSanPhamDaGap = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ct in model.listjson_chitietban)
            {
                if (ct == null)
                    return BadRequest(new { success = false, message = "Chi tiết hóa đơn không được để trống." });

                if (ValidateMa(ct.MASP, "Mã sản phẩm", 15, out string cleanSP) is IActionResult errSP)
                    return errSP;

                ct.MASP = cleanSP;

                if (!maSanPhamDaGap.Add(cleanSP))
                    return BadRequest(new { success = false, message = $"Sản phẩm '{cleanSP}' bị lặp trong hóa đơn." });

                if (ct.SOLUONG <= 0 || ct.DONGIA <= 0)
                    return BadRequest(new { success = false, message = "Số lượng và đơn giá sản phẩm phải lớn hơn 0." });

                ct.TONGTIEN = ct.SOLUONG * ct.DONGIA;
            }

            try
            {
                bool result = _hdbBll.Sua(model);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn hoặc cập nhật thất bại." });

                return Ok(new { success = true, message = "Cập nhật hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-hoadonban")]
        public IActionResult DeleteHoaDon([FromQuery] string? maHoaDon)
        {
            if (ValidateMa(maHoaDon, "Mã hóa đơn", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                bool result = _hdbBll.Xoa(cleanMa);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn cần xóa." });

                return Ok(new { success = true, message = "Xóa hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("reset-tongtienhang-by-mahdban")]
        public IActionResult ResetTongTienHangByHoaDon([FromQuery] string? maHDBan, [FromQuery] decimal tongTienMoi)
        {
            if (ValidateMa(maHDBan, "Mã hóa đơn bán", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (tongTienMoi < 0)
                return BadRequest(new { success = false, message = "Tổng tiền hàng không được nhỏ hơn 0." });

            try
            {
                bool result = _hdbBll.ResetTongTienHangByHoaDon(cleanMa, tongTienMoi);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy hóa đơn hoặc cập nhật thất bại." });

                return Ok(new { success = true, message = "Đã cập nhật tổng tiền hàng cho hóa đơn." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

     

        [HttpGet("get-all-chitietban")]
        public IActionResult GetAll_ChiTiet()
        {
            try
            {
                var result = _ctbBll.LayTatCa();
                return Ok(new { success = true, count = result?.Count ?? 0, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-chitietban-by-IDhoadon")]
        public IActionResult GetByHoaDon([FromQuery] string? maHDB)
        {
            if (ValidateMa(maHDB, "Mã hóa đơn", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                var result = _ctbBll.LayTheoHoaDon(cleanMa);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy chi tiết bán của hóa đơn '{cleanMa}'." });

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("insert-chitietban")]
        public IActionResult CreateChiTiet([FromBody] ChiTietBan model)
        {
            if (ValidateChiTietBan(model) is IActionResult valErr)
                return valErr;

            try
            {
                string result = _ctbBll.ThemMoi(model);
                if (result != null && result != "success")
                    return BadRequest(new { success = false, message = result });

                return StatusCode(201, new { success = true, message = "Thêm chi tiết bán thành công." });
            }
            catch (Exception ex)
            {
                return HandleDbSqlException(ex, model);
            }
        }

        [HttpPut("update-chitietban")]
        public IActionResult UpdateChiTiet([FromBody] ChiTietBan model)
        {
            if (ValidateChiTietBan(model) is IActionResult valErr)
                return valErr;

            try
            {
                bool result = _ctbBll.Sua(model);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết bán để cập nhật." });

                return Ok(new { success = true, message = "Cập nhật chi tiết bán thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-chitietban")]
        public IActionResult DeleteChiTiet([FromQuery] string? maHDB, [FromQuery] string? maSP)
        {
            if (ValidateMa(maHDB, "Mã hóa đơn", 15, out string cleanHD) is IActionResult errHD) return errHD;
            if (ValidateMa(maSP, "Mã sản phẩm", 15, out string cleanSP) is IActionResult errSP) return errSP;

            try
            {
                bool result = _ctbBll.Xoa(cleanHD, cleanSP);
                if (!result)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết bán để xóa." });

                return Ok(new { success = true, message = "Xóa chi tiết bán thành công." });
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
        public IActionResult Get_ThanhToan_ById([FromQuery] string? ma)
        {
            if (ValidateMa(ma, "Mã thanh toán", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                DataTable dt = _ttBll.GetById(cleanMa);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy thanh toán có mã '{cleanMa}'." });

                return Ok(new { success = true, data = list[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("insert-thanhtoan")]
        public IActionResult CreateThanhToan([FromBody] ThanhToan? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu thanh toán không được để trống." });

            if (ValidateMa(model.MaThanhToan, "Mã thanh toán", 15, out string cleanMaTT) is IActionResult errTT) return errTT;
            if (ValidateMa(model.MaHDBan, "Mã hóa đơn", 15, out string cleanMaHD) is IActionResult errHD) return errHD;
            if (ValidateMa(model.PhuongThuc, "Phương thức thanh toán", 50, out string cleanPT) is IActionResult errPT) return errPT;
            if (ValidateMa(model.TrangThai, "Trạng thái thanh toán", 50, out string cleanTT) is IActionResult errStatus) return errStatus;
            if (!ThanhToan_BLL.TrangThaiHopLe(cleanTT))
                return BadRequest(new { success = false, message = "Trạng thái thanh toán không hợp lệ." });

            if (model.SoTienThanhToan <= 0)
                return BadRequest(new { success = false, message = "Số tiền thanh toán phải lớn hơn 0." });

            model.MaThanhToan = cleanMaTT;
            model.MaHDBan = cleanMaHD;
            model.PhuongThuc = cleanPT;
            model.TrangThai = cleanTT;

            try
            {
                DataTable dt = _ttBll.GetById(model.MaThanhToan);
                if (dt != null && dt.Rows.Count > 0)
                    return Conflict(new { success = false, message = "Mã thanh toán đã tồn tại." });

                _ttBll.Create(model);
                return StatusCode(201, new { success = true, message = "Thêm thông tin thanh toán thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPut("update-thanhtoan")]
        public IActionResult UpdateThanhToan([FromBody] ThanhToan? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu thanh toán không được để trống." });

            if (ValidateMa(model.MaThanhToan, "Mã thanh toán", 15, out string cleanMaTT) is IActionResult errTT) return errTT;
            if (ValidateMa(model.PhuongThuc, "Phương thức thanh toán", 50, out string cleanPT) is IActionResult errPT) return errPT;
            if (ValidateMa(model.TrangThai, "Trạng thái thanh toán", 50, out string cleanTT) is IActionResult errStatus) return errStatus;
            if (!ThanhToan_BLL.TrangThaiHopLe(cleanTT))
                return BadRequest(new { success = false, message = "Trạng thái thanh toán không hợp lệ." });

            model.MaThanhToan = cleanMaTT;
            model.PhuongThuc = cleanPT;
            model.TrangThai = cleanTT;

            if (model.SoTienThanhToan <= 0)
                return BadRequest(new { success = false, message = "Số tiền thanh toán phải lớn hơn 0." });
            if (model.NgayThanhToan == default)
                return BadRequest(new { success = false, message = "Ngày thanh toán không hợp lệ." });

            try
            {
                DataTable dt = _ttBll.GetById(model.MaThanhToan);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = "Không tồn tại thanh toán có mã này." });

                _ttBll.Update(model);
                return Ok(new { success = true, message = "Thay đổi thông tin thanh toán thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("del-thanhtoan")]
        public IActionResult DeleteThanhToan([FromQuery] string? ma)
        {
            if (ValidateMa(ma, "Mã thanh toán", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                DataTable dt = _ttBll.GetById(cleanMa);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = "Không tìm thấy thông tin thanh toán để xóa." });

                _ttBll.Delete(cleanMa);
                return Ok(new { success = true, message = "Xóa thông tin thanh toán thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("reset-sotienthanhtoan-by-mahdban")]
        public IActionResult ResetSoTienThanhToanByHoaDon([FromQuery] string? maHDBan, [FromQuery] decimal soTienMoi)
        {
            if (ValidateMa(maHDBan, "Mã hóa đơn bán", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (soTienMoi < 0)
                return BadRequest(new { success = false, message = "Số tiền mới không được nhỏ hơn 0." });

            try
            {
                DataTable dt = _ttBll.ResetSoTienByHoaDon(cleanMa, soTienMoi);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = "Không tìm thấy thanh toán của hóa đơn để cập nhật." });

                DataColumn? amountColumn = dt.Columns.Cast<DataColumn>()
                    .FirstOrDefault(column => string.Equals(column.ColumnName, "SOTIENTHANHTOAN", StringComparison.OrdinalIgnoreCase));
                if (amountColumn == null || dt.Rows.Cast<DataRow>().Any(row =>
                    row[amountColumn] == DBNull.Value ||
                    Math.Abs(Convert.ToDecimal(row[amountColumn]) - soTienMoi) >= 0.01m))
                {
                    return Conflict(new { success = false, message = "Không thể xác nhận số tiền thanh toán sau khi cập nhật." });
                }

                var list = ToDictionaryList(dt);

                return Ok(new { success = true, message = "Đã reset số tiền thanh toán theo hóa đơn.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

