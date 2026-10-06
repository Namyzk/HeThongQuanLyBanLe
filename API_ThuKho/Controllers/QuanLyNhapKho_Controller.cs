using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_ThuKho.Controllers
{
    [Authorize(Roles = "Admin,ThuKho")]
    [Route("api/QuanLyNhapKho")]
    [ApiController]
    public class QuanLyNhapKho_Controller : ControllerBase
    {
        private readonly PhieuNhapKho_BLL _pnkBll;
        private readonly ChiTietNhap_BLL _ctnBll;
        private readonly NhaCungCap_BLL _nccBll;

        public QuanLyNhapKho_Controller(PhieuNhapKho_BLL _pnkBll, ChiTietNhap_BLL _ctnBll, NhaCungCap_BLL _nccBll)
        {
            this._pnkBll = _pnkBll;
            this._ctnBll = _ctnBll;
            this._nccBll = _nccBll;
        }

       
        private IActionResult HandleBllError(string bllError)
        {
            if (bllError.StartsWith("Lỗi hệ thống", StringComparison.OrdinalIgnoreCase))
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });

            if (bllError.Contains("đã tồn tại", StringComparison.OrdinalIgnoreCase))
                return Conflict(new { success = false, message = bllError });

            if (bllError.Contains("Không tìm thấy", StringComparison.OrdinalIgnoreCase) ||
                bllError.Contains("không tồn tại", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { success = false, message = bllError });

            return BadRequest(new { success = false, message = bllError });
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

       
        private static object MapNhaCungCap(DataRow row) => new
        {
            MANCC = row["MANCC"]?.ToString()?.Trim(),
            TENNCC = row["TENNCC"]?.ToString()?.Trim(),
            DIACHI = row["DIACHI"]?.ToString()?.Trim(),
            SDT = row["SDT"]?.ToString()?.Trim(),
            EMAIL = row["EMAIL"]?.ToString()?.Trim()
        };

       
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
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                var list = _pnkBll.LayTheoID(cleanMa);
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy phiếu nhập có mã '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin phiếu nhập kho thành công.", data = list[0] });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("create-phieunhapkho")]
        public IActionResult CreatePhieuNhapKho([FromBody] PhieuNhapKho pnk)
        {
            if (pnk == null)
                return BadRequest(new { success = false, message = "Dữ liệu phiếu nhập không được để trống." });

            try
            {
                string? bllErr = _pnkBll.ThemMoi(pnk);
                if (bllErr != null) return HandleBllError(bllErr);

                return StatusCode(201, new { success = true, message = "Thêm phiếu nhập kho thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("update-phieunhapkho")]
        public IActionResult UpdatePhieuNhapKho([FromBody] PhieuNhapKho pnk)
        {
            if (pnk == null)
                return BadRequest(new { success = false, message = "Dữ liệu phiếu nhập không được để trống." });

            try
            {
                string? bllErr = _pnkBll.CapNhat(pnk);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Cập nhật phiếu nhập kho thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-phieunhapkho")]
        public IActionResult DeletePhieuNhapKho([FromQuery] string? maphieunhap)
        {
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                string? bllErr = _pnkBll.Xoa(cleanMa);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Xoá phiếu nhập kho thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        

        [HttpGet("get-all-chitietnhap")]
        public IActionResult GetAllChiTietNhap()
        {
            try
            {
                var data = _ctnBll.LayTatCa().Select(x => new
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
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                var list = _ctnBll.LayTheoPhieu(cleanMa);
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy chi tiết nhập của phiếu '{cleanMa}'." });

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

        [HttpGet("get-byid-chitietnhap")]
        public IActionResult GetById([FromQuery] string? maphieunhap, [FromQuery] string? masp)
        {
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", 15, out string cleanPhieu) is IActionResult valErr1) return valErr1;
            if (ValidateMa(masp, "Mã sản phẩm", 15, out string cleanSp) is IActionResult valErr2) return valErr2;

            try
            {
                var list = _ctnBll.LayTheoID(cleanPhieu, cleanSp);
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = "Không tìm thấy chi tiết nhập kho phù hợp." });

                var x = list[0];
                var data = new
                {
                    MAPHIEUNHAP = x.MAPHIEUNHAP?.Trim(),
                    MASP = x.MASP?.Trim(),
                    SOLUONG = x.SOLUONG,
                    DONGIANHAP = x.DONGIANHAP,
                    THANHTIEN = x.THANHTIEN
                };

                return Ok(new { success = true, message = "Lấy chi tiết nhập thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("create-chitietnhap")]
        public IActionResult Create([FromBody] ChiTietNhap ct)
        {
            if (ct == null)
                return BadRequest(new { success = false, message = "Dữ liệu chi tiết nhập không được để trống." });

            try
            {
                string? bllErr = _ctnBll.ThemMoi(ct);
                if (bllErr != null) return HandleBllError(bllErr);

                return StatusCode(201, new { success = true, message = "Thêm chi tiết nhập thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("update-chitietnhap")]
        public IActionResult Update([FromBody] ChiTietNhap ct)
        {
            if (ct == null)
                return BadRequest(new { success = false, message = "Dữ liệu chi tiết nhập không được để trống." });

            try
            {
                string? bllErr = _ctnBll.CapNhat(ct);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Cập nhật chi tiết nhập thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("delete-chitietnhap")]
        public IActionResult Delete([FromQuery] string? maphieunhap, [FromQuery] string? masp)
        {
            if (ValidateMa(maphieunhap, "Mã phiếu nhập", 15, out string cleanPhieu) is IActionResult valErr1) return valErr1;
            if (ValidateMa(masp, "Mã sản phẩm", 15, out string cleanSp) is IActionResult valErr2) return valErr2;

            try
            {
                string? bllErr = _ctnBll.Xoa(cleanPhieu, cleanSp);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Xoá chi tiết nhập thành công." });
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
                var data = dt.AsEnumerable().Select(MapNhaCungCap).ToList();
                return Ok(new { success = true, count = data.Count, message = "Lấy danh sách nhà cung cấp thành công.", data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-nhacungcap")]
        public IActionResult GetByIdNCC([FromQuery] string? ma)
        {
            if (ValidateMa(ma, "Mã nhà cung cấp", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                string? valBllErr = _nccBll.ValidateMa(cleanMa);
                if (valBllErr != null) return BadRequest(new { success = false, message = valBllErr });

                DataTable dt = _nccBll.GetById(cleanMa);
                if (dt == null || dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy nhà cung cấp có mã '{cleanMa}'." });

                return Ok(new { success = true, message = "Lấy thông tin nhà cung cấp thành công.", data = MapNhaCungCap(dt.Rows[0]) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("del-nhacungcap")]
        public IActionResult DeleteNCC([FromQuery] string? ma)
        {
            if (ValidateMa(ma, "Mã nhà cung cấp", 15, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                string? bllErr = _nccBll.Delete(cleanMa);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Xoá thông tin nhà cung cấp thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("update-nhacungcap")]
        public IActionResult UpdateNCC([FromBody] NhaCungCap model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu nhà cung cấp không được để trống." });

            try
            {
                string? bllErr = _nccBll.Update(model);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Thay đổi thông tin nhà cung cấp thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("create-nhacungcap")]
        public IActionResult CreateNCC([FromBody] NhaCungCap model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu nhà cung cấp không được để trống." });

            try
            {
                string? bllErr = _nccBll.Create(model);
                if (bllErr != null) return HandleBllError(bllErr);

                return StatusCode(201, new { success = true, message = "Thêm thông tin nhà cung cấp thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

