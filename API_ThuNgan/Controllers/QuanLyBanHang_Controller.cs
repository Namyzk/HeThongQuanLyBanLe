using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace _API_ThuNgan.Controllers
{
    [Authorize(Roles = "Admin,ThuNgan")]
    [Route("api/QuanLyBanHang")]
    [ApiController]
    public class QuanLyBanHang_Controller : ControllerBase
    {
        private readonly HoaDonBan_BLL _hdbBll;
        private readonly ChiTietBan_BLL _ctbBll;
        private readonly KhachHang_BLL _khBll;
        private readonly DanhMuc_BLL _dmBll;
        private readonly SanPham_BLL _spBll;
        private readonly ThanhToan_BLL _ttBll;
        private readonly NhanVien_BLL _nvBll;
        private readonly ILogger<QuanLyBanHang_Controller> _logger;

        public QuanLyBanHang_Controller(HoaDonBan_BLL _hdbBll, ChiTietBan_BLL _ctbBll, KhachHang_BLL _khBll, DanhMuc_BLL _dmBll, SanPham_BLL _spBll, ThanhToan_BLL _ttBll, NhanVien_BLL _nvBll, ILogger<QuanLyBanHang_Controller> logger)
        {
            this._hdbBll = _hdbBll;
            this._ctbBll = _ctbBll;
            this._khBll = _khBll;
            this._dmBll = _dmBll;
            this._spBll = _spBll;
            this._ttBll = _ttBll;
            this._nvBll = _nvBll;
            _logger = logger;
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

       

        [HttpGet("get-all-nhanvien-ban-hang")]
        public IActionResult GetNhanVienBanHang()
        {
            try
            {
                var result = _nvBll.LayTatCa()
                    .Select(nv => new { nv.MANV, nv.TENNV })
                    .ToList();
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tải nhân viên cho màn hình bán hàng.");
                return StatusCode(500, new { success = false, message = "Không thể tải danh sách nhân viên." });
            }
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

        [HttpPost("insert-hoadonban")]
        public IActionResult CreateHoaDon([FromBody] HoaDonBan? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu hóa đơn không được để trống." });

            if (ValidateMa(model.MAHDBAN, "Mã hóa đơn", 15, out string cleanMaHD) is IActionResult valErr)
                return valErr;

            model.MAHDBAN = cleanMaHD;

            if (ValidateMa(model.MANV, "Mã nhân viên", 15, out string cleanMaNV) is IActionResult nvErr)
                return nvErr;

            model.MANV = cleanMaNV;
            if (!_nvBll.LayTheoID(cleanMaNV).Any())
                return BadRequest(new { success = false, message = $"Không tìm thấy nhân viên có mã '{cleanMaNV}'. Hãy chọn mã nhân viên trong danh sách." });

            if (model.listjson_chitietban == null || model.listjson_chitietban.Count == 0)
                return BadRequest(new { success = false, message = "Hóa đơn phải có ít nhất một sản phẩm." });

            var maSanPhamDaGap = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var ct in model.listjson_chitietban)
            {
                if (ct == null)
                    return BadRequest(new { success = false, message = "Chi tiết hóa đơn không được để trống." });

                if (ValidateMa(ct.MASP, "Mã sản phẩm trong chi tiết", 15, out string cleanMaSP) is IActionResult ctErr)
                    return ctErr;

                ct.MASP = cleanMaSP;

                if (!maSanPhamDaGap.Add(cleanMaSP))
                    return BadRequest(new { success = false, message = $"Sản phẩm '{cleanMaSP}' bị lặp trong hóa đơn." });

                if (ct.SOLUONG <= 0)
                    return BadRequest(new { success = false, message = "Số lượng sản phẩm phải lớn hơn 0." });

                if (ct.DONGIA <= 0)
                    return BadRequest(new { success = false, message = "Đơn giá sản phẩm phải lớn hơn 0." });
            }

            try
            {
                bool kq = _hdbBll.ThemMoi(model);
                if (!kq)
                    return Conflict(new { success = false, message = "Không thể thêm hóa đơn (mã đã tồn tại hoặc dữ liệu không hợp lệ)." });

                return StatusCode(201, new { success = true, message = "Thêm hóa đơn thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không thể tạo hóa đơn {MaHoaDon} cho nhân viên {MaNhanVien}.", model.MAHDBAN, model.MANV);
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
                    return NotFound(new { success = false, message = $"Không tìm thấy chi tiết của hóa đơn '{cleanMa}'." });

                return Ok(new { success = true, count = result.Count, data = result });
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
                return Ok(new { success = true, count = list.Count, message = "Lấy danh sách khách hàng thành công.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-khachhang")]
        public IActionResult GetByIdKH([FromQuery] string? maKH)
        {
            if (ValidateMa(maKH, "Mã khách hàng", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                DataTable dt = _khBll.GetByIdKH(cleanMa);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy khách hàng có mã '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin khách hàng thành công.", data = list[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("insert-khachhang")]
        public IActionResult CreateKhachHang([FromBody] KhachHang? kh)
        {
            if (kh == null)
                return BadRequest(new { success = false, message = "Dữ liệu khách hàng không được để trống." });

            if (ValidateMa(kh.MaKH, "Mã khách hàng", 15, out string cleanMa) is IActionResult errMa) return errMa;
            if (ValidateMa(kh.TenKH, "Tên khách hàng", 100, out string cleanTen) is IActionResult errTen) return errTen;

            kh.MaKH = cleanMa;
            kh.TenKH = cleanTen;

            if (!string.IsNullOrWhiteSpace(kh.SDT))
            {
                kh.SDT = kh.SDT.Trim();
                if (kh.SDT.Length != 10 || !kh.SDT.All(char.IsDigit))
                    return BadRequest(new { success = false, message = "Số điện thoại phải có đúng 10 chữ số." });
            }

            if (!string.IsNullOrWhiteSpace(kh.DiaChi))
            {
                kh.DiaChi = kh.DiaChi.Trim();
                if (kh.DiaChi.Length > 300)
                    return BadRequest(new { success = false, message = "Địa chỉ không được vượt quá 300 ký tự." });
            }

            try
            {
                DataTable check = _khBll.GetByIdKH(kh.MaKH);
                if (check != null && check.Rows.Count > 0)
                    return Conflict(new { success = false, message = "Mã khách hàng đã tồn tại." });

                _khBll.CreateKH(kh);
                return StatusCode(201, new { success = true, message = "Thêm khách hàng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

      
        [HttpGet("get-all-danhmuc")]
        public IActionResult GetAll_DanhMuc()
        {
            try
            {
                var result = _dmBll.LayTatCa();
                return Ok(new { success = true, count = result?.Count ?? 0, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byID-danhmuc")]
        public IActionResult GetByID_DanhMuc([FromQuery] string? madanhmuc)
        {
            if (ValidateMa(madanhmuc, "Mã danh mục", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                var result = _dmBll.LayTheoID(cleanMa);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy danh mục '{cleanMa}'." });

                return Ok(new { success = true, data = result[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-all-sanpham")]
        public IActionResult GetAll_SanPham()
        {
            try
            {
                var result = _spBll.LayTatCa();
                return Ok(new { success = true, count = result?.Count ?? 0, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-sanpham-by-id")]
        public IActionResult GetByID_SanPham([FromQuery] string? id)
        {
            if (ValidateMa(id, "Mã sản phẩm", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                var result = _spBll.LayTheoID(cleanMa);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm '{cleanMa}'." });

                return Ok(new { success = true, data = result[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPatch("update-soluong-sanpham")]
        public IActionResult UpdateSoLuong([FromQuery] string? maSP, [FromQuery] int soLuongMoi)
        {
            if (ValidateMa(maSP, "Mã sản phẩm", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (soLuongMoi < 0)
                return BadRequest(new { success = false, message = "Số lượng tồn không được nhỏ hơn 0." });

            try
            {
                string? error = _spBll.SuaSoLuong(cleanMa, soLuongMoi);
                if (error != null)
                {
                    if (error.StartsWith("Lỗi hệ thống", StringComparison.OrdinalIgnoreCase))
                        return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });

                    if (error.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase) || error.Contains("không tồn tại", StringComparison.OrdinalIgnoreCase))
                        return NotFound(new { success = false, message = error });

                    return BadRequest(new { success = false, message = error });
                }

                return Ok(new { success = true, message = "Cập nhật số lượng sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

   

        [HttpGet("get-all-thanhtoan")]
        public IActionResult GetAllThanhToan()
        {
            try
            {
                DataTable dt = _ttBll.getAll();
                var list = ToDictionaryList(dt);
                return Ok(new { success = true, count = list.Count, message = "Lấy danh sách thanh toán thành công.", data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-thanhtoan-by-id")]
        public IActionResult GetThanhToanById([FromQuery] string? maThanhToan)
        {
            if (ValidateMa(maThanhToan, "Mã thanh toán", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                DataTable dt = _ttBll.GetById(cleanMa);
                var list = ToDictionaryList(dt);

                if (list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy thanh toán có mã '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin thanh toán thành công.", data = list[0] });
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

            if (ValidateMa(model.MaThanhToan, "Mã thanh toán", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (ValidateMa(model.MaHDBan, "Mã hóa đơn", 15, out string cleanMaHD) is IActionResult errHD) return errHD;
            if (ValidateMa(model.PhuongThuc, "Phương thức thanh toán", 50, out string cleanPT) is IActionResult errPT) return errPT;
            if (ValidateMa(model.TrangThai, "Trạng thái thanh toán", 50, out string cleanTT) is IActionResult errTT) return errTT;
            if (string.Equals(cleanPT, "PayOS", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Thanh toán PayOS phải được tạo qua luồng QR và xác nhận bằng webhook." });
            if (!ThanhToan_BLL.TrangThaiHopLe(cleanTT))
                return BadRequest(new { success = false, message = "Trạng thái thanh toán không hợp lệ." });
            if (model.SoTienThanhToan <= 0)
                return BadRequest(new { success = false, message = "Số tiền thanh toán phải lớn hơn 0." });

            model.MaThanhToan = cleanMa;
            model.MaHDBan = cleanMaHD;
            model.PhuongThuc = cleanPT;
            model.TrangThai = cleanTT;

            try
            {
                DataTable dt = _ttBll.GetById(model.MaThanhToan);
                if (dt != null && dt.Rows.Count > 0)
                    return Conflict(new { success = false, message = "Đã tồn tại thanh toán có mã này." });

                _ttBll.Create(model);
                return StatusCode(201, new { success = true, message = "Thêm thông tin thanh toán thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

