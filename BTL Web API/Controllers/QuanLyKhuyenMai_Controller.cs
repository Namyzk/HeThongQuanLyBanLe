using BLL;
using DAL.DataHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace API_Admin.Controllers
{
    [Authorize]
    
    [Route("api/QuanLyKhuyenMai")]
    [ApiController]
    public class QuanLyKhuyenMai_Controller : ControllerBase
    {
        private readonly KhuyenMai_BLL _BLL;   // đổi TaiKhoan_BLL -> KhuyenMai_BLL

        public QuanLyKhuyenMai_Controller()
        {
            _BLL = new KhuyenMai_BLL();  // khởi tạo đúng BLL khuyến mãi
        }

        [HttpGet("test-db")]
        public IActionResult TestDatabase()
        {
            try
            {
                using SqlConnection conn = Connect.GetConnection();

                conn.Open();

                return Ok(new
                {
                    success = true,
                    server = conn.DataSource,
                    database = conn.Database,
                    state = conn.State.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        [Route("get-all-khuyenmai")]
        [HttpGet]
        public IActionResult getAll()
        {
            try
            {
                DataTable dt = Connect.ExecuteStoredProcedure(  "dbo.sp_GetKhuyenMai"
                );

                var data = dt.AsEnumerable().Select(row => dt.Columns .Cast<DataColumn>()
                .ToDictionary( column => column.ColumnName,  column =>
                {
                    if (row[column] == DBNull.Value) return null;
                    object value = row[column];
                    if (value is DateTime dateTime)
                    return dateTime.ToString("yyyy-MM-dd"); return value; }))
                    .ToList();

                return Ok(new
                {
                    success = true, count = data.Count, data = data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,message = ex.Message,  inner = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("get-byid-khuyenmai")]
        public IActionResult GetById(string ma)    
        {
            if (string.IsNullOrWhiteSpace(ma))
            {
                return BadRequest(new
                {
                    success = false, statucode = 400,
                    message = "Vui lòng nhập mã khuyến mại."
                });
            }

            try
            {
                DataTable dt = _BLL.GetById(ma);
                var list = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new
                    {
                        MAKM = row["MAKM"].ToString()?.Trim(),
                        TENKM = row["TENKM"],
                        MASP = row["MASP"].ToString()?.Trim(),
                        NGAYBATDAU = row["NGAYBATDAU"] == DBNull.Value ? null  : Convert.ToDateTime(row["NGAYBATDAU"]).ToString("yyyy-MM-dd"),
                        NGAYKETTHUC = row["NGAYKETTHUC"] == DBNull.Value  ? null : Convert.ToDateTime(row["NGAYKETTHUC"]).ToString("yyyy-MM-dd")
                    });
                }

                if (list.Count == 0)
                {
                    return NotFound(new
                    {
                        success = false, StatusCode = 404, message = "Không tìm thấy mã khuyến mại: " + ma
                    });
                }

                return Ok(new
                {
                    success = true, StatusCode = 200,  message = "Lấy thông tin khuyến mại thành công",
                    data = list
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, StatusCode = 500, message = "Lỗi không tìm thấy mã: " + ex.Message

                });
            }
        }

        [Route("del-khuyenmai")]
        [HttpDelete]
        public IActionResult Delete(string ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                {
                    return BadRequest(new
                    {
                        success = false, StatusCode = 400, message = "Mã khuyến mại không được để trống."
                    });
                }
                ma = ma.Trim();
                DataTable dt = _BLL.GetById(ma);
                if (dt.Rows.Count < 1)
                {
                    return NotFound(new
                    {
                        success = false, StatusCode = 404, message = "Không có thông tin khuyến mại có mã này."
                    });
                }

                _BLL.Delete(ma);

                return Ok(new
                {
                    success = true, StatusCode = 200, message = "Xoá thông tin khuyến mại thành công."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false, StatusCode = 500, message = "Lỗi: " + ex.Message
                });
            }
        }

        [Route("update-khuyenmai")]
        [HttpPost]
        public IActionResult Update([FromBody] Models.KhuyenMai model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false, StatusCode = 400, message = "Dữ liệu khuyến mại không được để trống."
                    });
                }

                DataTable check = _BLL.GetById(model.MaKM ?? "");

                if (check.Rows.Count < 1)
                {
                    return NotFound(new
                    {
                        success = false,StatusCode = 404, message = "Không có thông tin khuyến mại có mã này."
                    });
                }

                _BLL.Update(model);

                return Ok(new
                {
                    success = true, StatusCode = 200, message = "Thay đổi thông tin khuyến mại thành công."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false, StatusCode = 500, message = ex.Message
                });
            }
        }

        [Route("create-khuyenmai")]
        [HttpPost]
        public IActionResult Create([FromBody] Models.KhuyenMai model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new
                    {
                        success = false, StatusCode = 400, message = "Dữ liệu khuyến mại không được để trống."
                    });
                }

                DataTable dt = _BLL.Create(model);

                return Ok(new
                {
                    success = true, StatusCode = 200, message = "Thêm thông tin khuyến mại thành công"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false, StatusCode = 400, message = ex.Message

                });
            }
        }
    }
}
