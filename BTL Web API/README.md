# API Admin

Gateway base URL: http://localhost:8080. Public prefix: /api/admin. Các endpoint quản trị cần Bearer token; quyền quản lý nhân viên/tài khoản là Admin.

## Khuyến mãi — /api/admin/QuanLyKhuyenMai

- GET /get-all-khuyenmai
- GET /get-byid-khuyenmai?ma=KMTEST01
- GET /test-db
- POST /create-khuyenmai
- POST /update-khuyenmai
- DELETE /del-khuyenmai?ma=KMTEST01

Body cho create/update:
{
  "MaKM": "KMTEST01",
  "TenKM": "Khuyến mãi kiểm thử",
  "MaSP": "SPTEST01",
  "NgayBD": "2026-10-23",
  "NgayKT": "2026-11-23"
}

## Nhân viên — /api/admin/QuanLyNhanVien

- GET /get-all-nhanvien
- GET /get-byid-nhanvien?manv=NVTEST01
- POST /create-nhanvien
- POST /update-nhanvien
- DELETE /delete-nhanvien?manv=NVTEST01
- POST /upload: multipart/form-data gồm file, manv, tùy chọn oldFilePath
- GET /xem-anh/{fileName}
- GET /xem-anh-nv/{manv}
- GET /get-anh-by-manv?manv=NVTEST01
- DELETE /xoa-anh?manv=NVTEST01&filePath=...

Body cho create/update:
{
  "MANV": "NVTEST01",
  "TENNV": "Nhân viên kiểm thử",
  "SDT": "0901234567",
  "DIACHI": "Hà Nội"
}

## Tài khoản — /api/admin/QuanLyTaiKhoan

- GET /get-all-taikhoan
- GET /get-byid-taikhoan?mataikhoan=TKTEST01
- POST /create-taikhoan
- POST /update-byID-taikhoan
- DELETE /del-byID-taikhoan?mataikhoan=TKTEST01

Body cho create/update:
{
  "MATAIKHOAN": "TKTEST01",
  "USERNAME": "testadmin",
  "PASS": "Test123!",
  "QUYEN": 1
}

QUYEN: 1 Admin, 2 ThuNgan, 3 ThuKho, 4 KeToan. Update cần mã đã tồn tại; create cần mã/username mới. Upload ảnh dùng multipart/form-data, không phải JSON.
