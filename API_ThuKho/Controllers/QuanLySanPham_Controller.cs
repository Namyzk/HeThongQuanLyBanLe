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

namespace API_ThuKho.Controllers
{
    [Authorize]
    [Route("api/QuanLySanPham")]
    [ApiController]
    public class QuanLySanPham_Controller : ControllerBase
    {
        private readonly SanPham_BLL _spBll;

        public QuanLySanPham_Controller(SanPham_BLL _spBll)
        {
            this._spBll = _spBll;
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

       
        private IActionResult? ValidateMa(string? ma, out string cleanMa)
        {
            cleanMa = string.Empty;
            if (string.IsNullOrWhiteSpace(ma))
                return BadRequest(new { success = false, message = "Mã sản phẩm không được để trống." });

            cleanMa = ma.Trim();
            if (cleanMa.Length > 15)
                return BadRequest(new { success = false, message = "Mã sản phẩm không được vượt quá 15 ký tự." });

            return null;
        }

        [Route("get-all-sanpham")]
        [HttpGet]
        [Authorize(Roles = "Admin,ThuKho,ThuNgan")]
        public IActionResult GetAll()
        {
            try
            {
                var result = _spBll.LayTatCa();
                return Ok(new
                {
                    success = true, count = result?.Count ?? 0,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

        [Route("get-sanpham-by-id")]
        [HttpGet]
        [Authorize(Roles = "Admin,ThuKho")]
        public IActionResult GetByID([FromQuery] string? id)
        {
            if (ValidateMa(id, out string cleanId) is IActionResult valErr)
                return valErr;

            try
            {
                var result = _spBll.LayTheoID(cleanId);
                if (result == null || result.Count == 0)
                    return NotFound(new { success = false, message = $"Không tìm thấy sản phẩm có mã '{cleanId}'." });

                return Ok(new
                {
                    success = true,
                    data = result[0]
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }


        //[Route("insert-sanpham")]
        [Route("create-sanpham")]
        [HttpPost]
        [Authorize(Roles = "Admin,ThuKho")]
        public IActionResult Create([FromBody] SanPham? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu sản phẩm không được để trống." });

            try
            {
                string? bllErr = _spBll.ThemMoi(model);
                if (bllErr != null) return HandleBllError(bllErr);

                return StatusCode(201, new { success = true, message = "Thêm sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

        [Route("update-sanpham")]
        [HttpPost]
        [Authorize(Roles = "Admin,ThuKho")]
        //[HttpPut]
        public IActionResult Update([FromBody] SanPham? model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu sản phẩm không được để trống." });

            try
            {
                string? bllErr = _spBll.Sua(model);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Cập nhật sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

        [Route("update-soluong-sanpham")]
        [HttpPost]
        [Authorize(Roles = "Admin,ThuKho")]
        public IActionResult UpdateSoLuong([FromQuery] string? maSP, [FromQuery] int soLuongMoi)
        {
            if (ValidateMa(maSP, out string cleanMa) is IActionResult valErr)
                return valErr;

            if (soLuongMoi < 0)
                return BadRequest(new { success = false, message = "Số lượng sản phẩm không được nhỏ hơn 0." });

            try
            {
                string? bllErr = _spBll.SuaSoLuong(cleanMa, soLuongMoi);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Cập nhật số lượng sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

        [Route("delete-sanpham")]
        [HttpDelete]
        [Authorize(Roles = "Admin,ThuKho")]
        public IActionResult Delete([FromQuery] string? maSP)
        {
            if (ValidateMa(maSP, out string cleanMa) is IActionResult valErr)
                return valErr;

            try
            {
                string? bllErr = _spBll.Xoa(cleanMa);
                if (bllErr != null) return HandleBllError(bllErr);

                return Ok(new { success = true, message = "Xóa sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống." });
            }
        }

       

        [Route("upload")]
        [HttpPost]
        [Authorize(Roles = "Admin,ThuKho")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(
            IFormFile? file,
            [FromForm] string? maSP = null,
            [FromForm] string? oldFilePath = null,
            [FromServices] IWebHostEnvironment? env = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Vui lòng chọn tệp ảnh hợp lệ." });
                }

              
                maSP ??= Request.Query["maSP"].FirstOrDefault();
                oldFilePath ??= Request.Query["oldFilePath"].FirstOrDefault();

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    return BadRequest(new { success = false, message = "Định dạng tệp không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .gif, .webp." });
                }

                string picRoot = GetPicRootDirectory(env);
                string uploadFolder = Path.Combine(picRoot, "sanpham");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                // Khi thay đổi ảnh: Xoá ảnh cũ ngay lập tức nếu tồn tại
                XoaAnhCu(maSP, oldFilePath, uploadFolder, picRoot);

                // Tạo tên tệp mới chuẩn hoá và tránh trùng lặp
                string cleanId = !string.IsNullOrWhiteSpace(maSP) ? maSP.Trim().Replace("-", "_").Replace("%", "") : "sp";
                string cleanOriginal = Path.GetFileNameWithoutExtension(file.FileName).Replace("-", "_").Replace("%", "").Replace(" ", "_");
                string safeFileName = $"{cleanId}_{cleanOriginal}_{DateTime.UtcNow.Ticks}{ext}";
                string filePath = $"Pic/sanpham/{safeFileName}";
                string fullPath = Path.Combine(uploadFolder, safeFileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return Ok(new
                {
                    success = true, message = "Tải ảnh sản phẩm thành công.",
                    filePath,
                    fullPath,
                    fileName = safeFileName
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

             
                string fullPath = Path.Combine(picRoot, "sanpham", safeName);
                if (!System.IO.File.Exists(fullPath))
                {
                    fullPath = Path.Combine(picRoot, safeName);
                }

                if (!System.IO.File.Exists(fullPath))
                    return NotFound(new { success = false, message = "Không tìm thấy tệp ảnh sản phẩm." });

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

        [Route("xem-anh-sp/{maSP}")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult XemAnhTheoMaSP([FromRoute] string maSP, [FromServices] IWebHostEnvironment? env = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maSP))
                    return BadRequest(new { success = false, message = "Mã sản phẩm không hợp lệ." });

                string cleanMa = maSP.Trim().Replace("-", "_").Replace("%", "");
                string picRoot = GetPicRootDirectory(env);
                string uploadFolder = Path.Combine(picRoot, "sanpham");

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
                    return NotFound(new { success = false, message = $"Không tìm thấy ảnh của sản phẩm '{maSP}'." });

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
                return StatusCode(500, new { success = false, message = "Lỗi khi truy xuất ảnh sản phẩm: " + ex.Message });
            }
        }

        [Route("get-anh-by-masp")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAnhByMaSP([FromQuery] string? maSP, [FromServices] IWebHostEnvironment? env = null)
        {
            if (string.IsNullOrWhiteSpace(maSP))
                return BadRequest(new { success = false, message = "Mã sản phẩm không được để trống." });

            string cleanMa = maSP.Trim().Replace("-", "_").Replace("%", "");
            string picRoot = GetPicRootDirectory(env);
            string uploadFolder = Path.Combine(picRoot, "sanpham");

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
                return NotFound(new { success = false, message = $"Không tìm thấy ảnh của sản phẩm '{maSP}'." });

            string fileName = Path.GetFileName(file);
            string filePath = file.StartsWith(uploadFolder, StringComparison.OrdinalIgnoreCase)
                ? $"Pic/sanpham/{fileName}"
                : $"Pic/{fileName}";

            return Ok(new
            {
                success = true,
                maSP = maSP.Trim(),
                fileName,
                filePath,
                fullPath = file
            });
        }

        [Route("xoa-anh")]
        [HttpDelete]
        [Authorize(Roles = "Admin,ThuKho")]
        public IActionResult XoaAnh([FromQuery] string? maSP, [FromQuery] string? filePath, [FromServices] IWebHostEnvironment? env = null)
        {
            string picRoot = GetPicRootDirectory(env);
            string uploadFolder = Path.Combine(picRoot, "sanpham");
            bool deleted = XoaAnhCu(maSP, filePath, uploadFolder, picRoot);

            if (deleted)
                return Ok(new { success = true, message = "Đã xóa ảnh sản phẩm thành công." });

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
                
            }
            return deleted;
        }
    }
}

