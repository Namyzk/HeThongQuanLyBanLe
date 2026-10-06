using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Linq;

namespace API_KeToan1.Controllers
{
    [Authorize(Roles = "Admin,KeToan")]
    [Route("api/BaoCaoThongKe")]
    [ApiController]
    public class BaoCaoThongKe_Controller : ControllerBase
    {
        private readonly DanhMuc_BLL _dmBll;
        private readonly SanPham_BLL _spBll;
        private readonly KhuyenMai_BLL _kmBll;
        private readonly PhieuNhapKho_BLL _pnkBll;
        private readonly ChiTietNhap_BLL _ctnBll;
        private readonly HoaDonBan_BLL _hdbBll;
        private readonly ChiTietBan_BLL _ctbBll;

        public BaoCaoThongKe_Controller(DanhMuc_BLL _dmBll, SanPham_BLL _spBll, KhuyenMai_BLL _kmBll, PhieuNhapKho_BLL _pnkBll, ChiTietNhap_BLL _ctnBll, HoaDonBan_BLL _hdbBll, ChiTietBan_BLL _ctbBll)
        {
            this._dmBll = _dmBll;
            this._spBll = _spBll;
            this._kmBll = _kmBll;
            this._pnkBll = _pnkBll;
            this._ctnBll = _ctnBll;
            this._hdbBll = _hdbBll;
            this._ctbBll = _ctbBll;
        }

       
        private IActionResult? ValidateMa(string? ma, string tenTruong, out string maTrimmed)
        {
            maTrimmed = string.Empty;
            if (string.IsNullOrWhiteSpace(ma))
                return BadRequest(new { success = false, message = $"{tenTruong} không được để trống." });

            maTrimmed = ma.Trim();
            if (maTrimmed.Length > 15)
                return BadRequest(new { success = false, message = $"{tenTruong} không được vượt quá 15 ký tự." });

            return null;
        }

      
        [HttpGet("get-all-danhmuc")]
        public IActionResult GetAll_DM()
        {
            try
            {
                var list = _dmBll.LayTatCa();
                return Ok(new { success = true, count = list?.Count ?? 0, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byID-danhmuc")]
        public IActionResult Get_DM_ByID([FromQuery] string? madanhmuc)
        {
            if (ValidateMa(madanhmuc, "Mã danh mục", out string cleanMa) is IActionResult error)
                return error;

            try
            {
                var danhmuc = _dmBll.LayTheoID(cleanMa);
                if (danhmuc == null || danhmuc.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy danh mục có mã '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin danh mục thành công.", data = danhmuc });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-all-sanpham")]
        public IActionResult GetAll_SP()
        {
            try
            {
                var data = _spBll.LayTatCa();
                return Ok(new { success = true, count = data?.Count ?? 0, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-sanpham-by-id")]
        public IActionResult Get_SP_ByID([FromQuery] string? id)
        {
            if (ValidateMa(id, "Mã sản phẩm", out string cleanId) is IActionResult error)
                return error;

            try
            {
                var result = _spBll.LayTheoID(cleanId);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm có mã '{cleanId}'." });

                return Ok(new { success = true, message = "Lấy thông tin sản phẩm thành công.", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

       
        [HttpGet("get-all-khuyenmai")]
        public IActionResult GetAll_KM()
        {
            try
            {
                var dt = _kmBll.getAll();
                var data = dt.AsEnumerable().Select(row => new
                {
                    MAKM = row["MAKM"]?.ToString()?.Trim(),
                    TENKM = row["TENKM"]?.ToString()?.Trim(),
                    MASP = row["MASP"]?.ToString()?.Trim(),
                    NGAYBATDAU = row["NGAYBATDAU"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYBATDAU"]).ToString("yyyy-MM-dd"),
                    NGAYKETTHUC = row["NGAYKETTHUC"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYKETTHUC"]).ToString("yyyy-MM-dd")
                }).ToList();

                return Ok(new { success = true, count = data.Count, message = "Lấy danh sách khuyến mại thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-khuyenmai")]
        public IActionResult Get_KM_ById([FromQuery] string? ma)
        {
            if (ValidateMa(ma, "Mã khuyến mại", out string cleanMa) is IActionResult error)
                return error;

            try
            {
                var dt = _kmBll.GetById(cleanMa);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy mã khuyến mại: {cleanMa}" });

                var data = dt.AsEnumerable().Select(row => new
                {
                    MAKM = row["MAKM"]?.ToString()?.Trim(),
                    TENKM = row["TENKM"]?.ToString()?.Trim(),
                    MASP = row["MASP"]?.ToString()?.Trim(),
                    NGAYBATDAU = row["NGAYBATDAU"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYBATDAU"]).ToString("yyyy-MM-dd"),
                    NGAYKETTHUC = row["NGAYKETTHUC"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYKETTHUC"]).ToString("yyyy-MM-dd")
                }).ToList();

                return Ok(new { success = true, message = "Lấy thông tin khuyến mại thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

       
        [HttpGet("get-all-phieunhapkho")]
        public IActionResult GetAllPhieuNhapKho()
        {
            try
            {
                var data = _pnkBll.LayTatCa();
                return Ok(new { success = true, count = data?.Count ?? 0, message = "Lấy danh sách phiếu nhập kho thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-phieunhapkho")]
        public IActionResult GetByIdPhieuNhapKho([FromQuery] string? maphieunhap)
        {
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", out string cleanMa) is IActionResult error)
                return error;

            try
            {
                var data = _pnkBll.LayTheoID(cleanMa);
                if (data == null || data.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy phiếu nhập kho '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin phiếu nhập kho thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

     
        [HttpGet("get-all-chitietnhap")]
        public IActionResult GetAll_CTN()
        {
            try
            {
                var data = _ctnBll.LayTatCa()
                    .Select(x => new
                    {
                        MAPHIEUNHAP = x.MAPHIEUNHAP?.Trim(),
                        MASP = x.MASP?.Trim(),
                        SOLUONG = x.SOLUONG,
                        DONGIANHAP = x.DONGIANHAP,
                        THANHTIEN = x.THANHTIEN
                    }).ToList();

                return Ok(new { success = true, count = data.Count, message = "Lấy danh sách chi tiết nhập thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byphieu-chitietnhap")]
        public IActionResult GetByPhieu([FromQuery] string? maphieunhap)
        {
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", out string cleanMa) is IActionResult error)
                return error;

            try
            {
                var list = _ctnBll.LayTheoPhieu(cleanMa);
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy chi tiết nhập kho của phiếu: {cleanMa}" });

                var data = list.Select(x => new
                {
                    MAPHIEUNHAP = x.MAPHIEUNHAP?.Trim(),
                    MASP = x.MASP?.Trim(),
                    SOLUONG = x.SOLUONG,
                    DONGIANHAP = x.DONGIANHAP,
                    THANHTIEN = x.THANHTIEN
                }).ToList();

                return Ok(new { success = true, count = data.Count, message = "Lấy chi tiết theo phiếu thành công.", data });
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
                var data = _hdbBll.LayTatCa();
                return Ok(new { success = true, count = data?.Count ?? 0, message = "Lấy danh sách hóa đơn thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-hoadonban-by-id")]
        public IActionResult Get_HDB_ByID([FromQuery] string? maHoaDon)
        {
            if (ValidateMa(maHoaDon, "Mã hóa đơn", out string cleanMa) is IActionResult error)
                return error;

            try
            {
                var data = _hdbBll.LayTheoID(cleanMa);
                if (data == null || data.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy hóa đơn có mã '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin hóa đơn thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-all-chitietban")]
        public IActionResult GetAll_CTB()
        {
            try
            {
                var data = _ctbBll.LayTatCa();
                return Ok(new { success = true, count = data?.Count ?? 0, message = "Lấy danh sách chi tiết bán thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-chitietban-by-IDhoadon")]
        public IActionResult Get_CTB_ByHoaDon([FromQuery] string? maHDB)
        {
            if (ValidateMa(maHDB, "Mã hóa đơn", out string cleanMa) is IActionResult error)
                return error;

            try
            {
                var data = _ctbBll.LayTheoHoaDon(cleanMa);
                if (data == null || data.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy chi tiết của hóa đơn '{cleanMa}'." });

                return Ok(new { success = true, count = data.Count, message = "Lấy chi tiết hóa đơn thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}





