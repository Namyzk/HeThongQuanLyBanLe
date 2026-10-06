using System;
using System.Collections.Generic;
using System.Data;
using BLL;
using DAL.DataHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Models;

namespace API_Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    
    [Route("api/QuanLyKhuyenMai")]

    [ApiController]
    public class QuanLyKhuyenMai_Controller : ControllerBase
    {
        private readonly KhuyenMai_BLL _bll;

        public QuanLyKhuyenMai_Controller(KhuyenMai_BLL _bll)
        {
            this._bll = _bll;
        }

        
        private static object MapKhuyenMai(DataRow row) => new
        {
            MAKM = row["MAKM"]?.ToString()?.Trim(),
            TENKM = row["TENKM"]?.ToString()?.Trim(),
            MASP = row["MASP"]?.ToString()?.Trim(),
            NGAYBATDAU = row["NGAYBATDAU"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYBATDAU"]).ToString("yyyy-MM-dd"),
            NGAYKETTHUC = row["NGAYKETTHUC"] == DBNull.Value ? null : Convert.ToDateTime(row["NGAYKETTHUC"]).ToString("yyyy-MM-dd")
        };

        [HttpGet("test-db")]
        public IActionResult TestDatabase()
        {
            try
            {
                using var conn = Connect.GetConnection();
                conn.Open();
                return Ok(new { success = true, server = conn.DataSource, database = conn.Database, state = conn.State.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-all-khuyenmai")]
        public IActionResult GetAll()
        {
            try
            {
                DataTable dt = Connect.ExecuteStoredProcedure("dbo.sp_GetKhuyenMai");
                var list = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(MapKhuyenMai(row));
                }

                return Ok(new { success = true, count = list.Count, data = list });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpGet("get-byid-khuyenmai")]
        public IActionResult GetById([FromQuery] string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                return BadRequest(new { success = false, message = "Vui lòng nhập mã khuyến mại." });

            try
            {
                DataTable dt = _bll.GetById(ma.Trim());
                if (dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy mã khuyến mại: {ma}" });

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin khuyến mại thành công.",
                    data = MapKhuyenMai(dt.Rows[0]) 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("create-khuyenmai")]
        public IActionResult Create([FromBody] KhuyenMai model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MaKM))
                return BadRequest(new { success = false, message = "Dữ liệu khuyến mại và mã KM không được để trống." });

            try
            {
                _bll.Create(model);
                return Ok(new { success = true, message = "Thêm thông tin khuyến mại thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpPost("update-khuyenmai")]
        public IActionResult Update([FromBody] KhuyenMai model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MaKM))
                return BadRequest(new { success = false, message = "Dữ liệu khuyến mại và mã KM không được để trống." });

            try
            {
                DataTable check = _bll.GetById(model.MaKM.Trim());
                if (check.Rows.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy khuyến mại có mã '{model.MaKM}'." });

                _bll.Update(model);
                return Ok(new { success = true, message = "Thay đổi thông tin khuyến mại thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }

        [HttpDelete("del-khuyenmai")]
        public IActionResult Delete([FromQuery] string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                return BadRequest(new { success = false, message = "Mã khuyến mại không được để trống." });

            try
            {
                ma = ma.Trim();
                DataTable dt = _bll.GetById(ma);
                if (dt.Rows.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy khuyến mại có mã '{ma}'." });

                _bll.Delete(ma);
                return Ok(new { success = true, message = "Xoá thông tin khuyến mại thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống nội bộ." });
            }
        }
    }
}

