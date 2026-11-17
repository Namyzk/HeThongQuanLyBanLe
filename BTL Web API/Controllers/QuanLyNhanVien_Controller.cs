using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;

namespace API_Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/QuanLyNhanVien")]
    [ApiController]
    public class QuanLyNhanVien_Controller : ControllerBase
    {
        private readonly NhanVien_BLL _bll;

        public QuanLyNhanVien_Controller(NhanVien_BLL _bll)
        {
            this._bll = _bll;
        }

        
        private static object MapNhanVien(NhanVien x) => new
        {
            MANV = x.MANV?.Trim(),
            TENNV = x.TENNV?.Trim(),
            SDT = x.SDT?.Trim(),
            DIACHI = x.DIACHI?.Trim(),
            EMAIL = x.EMAIL?.Trim()
        };

        [Route("get-all-nhanvien")]
        [HttpGet]
        public IActionResult GetAllNhanVien()
        {
            try
            {
                var data = _bll.LayTatCa().Select(MapNhanVien).ToList();
                return Ok(new { success = true, count = data.Count, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

        [Route("get-byid-nhanvien")]
        [HttpGet]
        public IActionResult GetByIdNhanVien([FromQuery] string manv)
        {
            if (string.IsNullOrWhiteSpace(manv))
                return BadRequest(new { success = false, message = "Mã nhân viên không được để trống." });

            try
            {
                var list = _bll.LayTheoID(manv.Trim());
                if (list == null || list.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy nhân viên có mã '{manv}'." });

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin nhân viên thành công.",
                    data = MapNhanVien(list[0]) // Trả về 1 object chi tiết trực tiếp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

        [Route("create-nhanvien")]
        [HttpPost]
        public IActionResult CreateNhanVien([FromBody] NhanVien nv)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.MANV))
                return BadRequest(new { success = false, message = "Dữ liệu nhân viên và mã nhân viên không được để trống." });

            try
            {
                bool ok = _bll.ThemMoi(nv);
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể thêm nhân viên." });

                return Ok(new { success = true, message = "Thêm nhân viên thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống." });
            }
        }


        [Route("update-nhanvien")]
        [HttpPost]
        public IActionResult UpdateNhanVien([FromBody] NhanVien nv)
        {
            if (nv == null || string.IsNullOrWhiteSpace(nv.MANV))
                return BadRequest(new { success = false, message = "Dữ liệu nhân viên và mã nhân viên không được để trống." });

            try
            {
                bool ok = _bll.CapNhat(nv);
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể cập nhật nhân viên." });

                return Ok(new { success = true, message = "Cập nhật nhân viên thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi hệ thống." });
            }
        }


        [Route("delete-nhanvien")]
        [HttpDelete]
        public IActionResult DeleteNhanVien([FromQuery] string manv)
        {
            if (string.IsNullOrWhiteSpace(manv))
                return BadRequest(new { success = false, message = "Mã nhân viên không được để trống." });

            try
            {
                bool ok = _bll.Xoa(manv.Trim());
                if (!ok)
                    return BadRequest(new { success = false, message = "Không thể xoá nhân viên." });

                return Ok(new { success = true, message = "Xoá nhân viên thành công." });
            }
            catch (Exception ex)
            {
                // Bắt lỗi ràng buộc khóa ngoại tham chiếu sang Phiếu Nhập hoặc Hóa Đơn
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("FK_"))
                {
                    return BadRequest(new
                    {
                        success = false, message = "Không thể xóa nhân viên này vì đã có dữ liệu Phiếu nhập kho hoặc Hóa đơn bán hàng liên quan!"
                    });
                }

                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

      

        [Route("upload")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload( IFormFile? file,[FromForm] string? manv = null, [FromForm] string? oldFilePath = null,[FromServices] IWebHostEnvironment? env = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Vui lòng chọn tệp ảnh đại diện hợp lệ." });
                }

          
                manv ??= Request.Query["manv"].FirstOrDefault() ?? Request.Query["maNV"].FirstOrDefault();
                oldFilePath ??= Request.Query["oldFilePath"].FirstOrDefault();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    return BadRequest(new { success = false, message = "Định dạng tệp không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .gif, .webp." });
                }

                string picRoot = GetPicRootDirectory(env);
                string uploadFolder = Path.Combine(picRoot, "nhanvien");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

          
                XoaAnhCu(manv, oldFilePath, uploadFolder, picRoot);

                
                string cleanId = !string.IsNullOrWhiteSpace(manv) ? manv.Trim().Replace("-", "_").Replace("%", "") : "nv";
                string cleanOriginal = Path.GetFileNameWithoutExtension(file.FileName).Replace("-", "_").Replace("%", "").Replace(" ", "_");
                string safeFileName = $"{cleanId}_{cleanOriginal}_{DateTime.UtcNow.Ticks}{ext}";
                string filePath = $"Pic/nhanvien/{safeFileName}";
                string fullPath = Path.Combine(uploadFolder, safeFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return Ok(new
                {
                    success = true,   message = "Tải ảnh đại diện nhân viên thành công.",
                    filePath, fullPath,  fileName = safeFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Không thể upload tệp: " + ex.Message });
            }
        }

        [Route("xem-anh/{fileName}")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult XemAnh([FromRoute] string fileName, [FromServices] IWebHostEnvironment? env = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return BadRequest(new { success = false, message = "Tên tệp ảnh không hợp lệ." });

                string safeName = Path.GetFileName(fileName);
                string picRoot = GetPicRootDirectory(env);

               
                string fullPath = Path.Combine(picRoot, "nhanvien", safeName);
                if (!System.IO.File.Exists(fullPath))
                {
                    fullPath = Path.Combine(picRoot, safeName);
                }

                if (!System.IO.File.Exists(fullPath))
                    return NotFound(new { success = false, message = "Không tìm thấy tệp ảnh đại diện nhân viên." });

                string ext = Path.GetExtension(fullPath).ToLowerInvariant();
                string contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                return PhysicalFile(fullPath, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Không thể truy xuất tệp: " + ex.Message });
            }
        }

        [Route("xem-anh-nv/{manv}")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult XemAnhTheoMaNV([FromRoute] string manv, [FromServices] IWebHostEnvironment? env = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(manv))
                    return BadRequest(new { success = false, message = "Mã nhân viên không hợp lệ." });

                string cleanMa = manv.Trim().Replace("-", "_").Replace("%", "");
                string picRoot = GetPicRootDirectory(env);
                string uploadFolder = Path.Combine(picRoot, "nhanvien");

                string? file = null;
                if (Directory.Exists(uploadFolder))
                {
                    file = Directory.GetFiles(uploadFolder, $"{cleanMa}_*.*")
                        .Concat(Directory.GetFiles(uploadFolder, $"{cleanMa}.*"))
                        .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                        .FirstOrDefault();
                }

                if (file == null && Directory.Exists(picRoot))
                {
                    file = Directory.GetFiles(picRoot, $"{cleanMa}_*.*")
                        .Concat(Directory.GetFiles(picRoot, $"{cleanMa}.*"))
                        .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                        .FirstOrDefault();
                }

                if (file == null || !System.IO.File.Exists(file))
                    return NotFound(new { success = false, message = $"Không tìm thấy ảnh của nhân viên '{manv}'." });

                string ext = Path.GetExtension(file).ToLowerInvariant();
                string contentType = ext switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                return PhysicalFile(file, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi truy xuất ảnh nhân viên: " + ex.Message });
            }
        }

        [Route("get-anh-by-manv")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAnhByMaNV([FromQuery] string? manv, [FromServices] IWebHostEnvironment? env = null)
        {
            if (string.IsNullOrWhiteSpace(manv))
                return BadRequest(new { success = false, message = "Mã nhân viên không được để trống." });

            string cleanMa = manv.Trim().Replace("-", "_").Replace("%", "");
            string picRoot = GetPicRootDirectory(env);
            string uploadFolder = Path.Combine(picRoot, "nhanvien");

            string? file = null;
            if (Directory.Exists(uploadFolder))
            {
                file = Directory.GetFiles(uploadFolder, $"{cleanMa}_*.*")
                    .Concat(Directory.GetFiles(uploadFolder, $"{cleanMa}.*"))
                    .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                    .FirstOrDefault();
            }

            if (file == null && Directory.Exists(picRoot))
            {
                file = Directory.GetFiles(picRoot, $"{cleanMa}_*.*")
                    .Concat(Directory.GetFiles(picRoot, $"{cleanMa}.*"))
                    .OrderByDescending(System.IO.File.GetLastWriteTimeUtc)
                    .FirstOrDefault();
            }

            if (file == null || !System.IO.File.Exists(file))
                return NotFound(new { success = false, message = $"Không tìm thấy ảnh của nhân viên '{manv}'." });

            string fileName = Path.GetFileName(file);
            string filePath = file.StartsWith(uploadFolder, StringComparison.OrdinalIgnoreCase)  ? $"Pic/nhanvien/{fileName}" : $"Pic/{fileName}";

            return Ok(new
            {
                success = true, manv = manv.Trim(),
                fileName, filePath, fullPath = file
            });
        }

        [Route("xoa-anh")]
        [HttpDelete]
        public IActionResult XoaAnh([FromQuery] string? manv, [FromQuery] string? filePath, [FromServices] IWebHostEnvironment? env = null)
        {
            string picRoot = GetPicRootDirectory(env);
            string uploadFolder = Path.Combine(picRoot, "nhanvien");
            bool deleted = XoaAnhCu(manv, filePath, uploadFolder, picRoot);

            if (deleted)
                return Ok(new { success = true, message = "Đã xóa ảnh đại diện thành công." });

            return NotFound(new { success = false, message = "Không tìm thấy ảnh để xóa." });
        }

        private static string GetPicRootDirectory(IWebHostEnvironment? env)
        {
            const string explicitPath = @"D:\WebApi\Pic";
            if (Directory.Exists(explicitPath))
                return explicitPath;

            string current = env?.ContentRootPath ?? Directory.GetCurrentDirectory();
            DirectoryInfo? dir = new DirectoryInfo(current);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "Pic");
                if (Directory.Exists(candidate) || dir.Name.Equals("WebApi", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Directory.Exists(candidate))
                        Directory.CreateDirectory(candidate);
                    return candidate;
                }
                dir = dir.Parent;
            }

            if (!Directory.Exists(explicitPath))
                Directory.CreateDirectory(explicitPath);

            return explicitPath;
        }

        private static bool XoaAnhCu(string? maDinhDanh, string? oldFilePath, string uploadFolder, string picRoot)
        {
            bool deleted = false;
            try
            {
                var folders = new[] { uploadFolder, picRoot };
                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder)) continue;

                    if (!string.IsNullOrWhiteSpace(maDinhDanh))
                    {
                        string cleanId = maDinhDanh.Trim().Replace("-", "_").Replace("%", "");
                        var oldFiles = Directory.GetFiles(folder, $"{cleanId}_*.*")
                            .Concat(Directory.GetFiles(folder, $"{cleanId}.*"));
                        foreach (var f in oldFiles)
                        {
                            if (System.IO.File.Exists(f))
                            {
                                System.IO.File.Delete(f);
                                deleted = true;
                            }
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(oldFilePath))
                    {
                        string oldFileName = Path.GetFileName(oldFilePath.Trim());
                        string oldFullPath = Path.Combine(folder, oldFileName);
                        if (System.IO.File.Exists(oldFullPath))
                        {
                            System.IO.File.Delete(oldFullPath);
                            deleted = true;
                        }
                    }
                }
            }
            catch
            {
                // Bỏ qua lỗi xóa tệp cũ để không làm gián đoạn tiến trình chính
            }
            return deleted;
        }
    }
}


