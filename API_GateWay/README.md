# API Gateway

Gateway trong Docker Compose publish cổng 8080. Dùng http://localhost:8080; Gateway không có API nghiệp vụ riêng mà chuyển tiếp request tới các project API.

## Prefix theo Ocelot

- Login: /api/Login/... → API_Login
- Admin: /api/admin/... → BTL Web API (downstream bỏ /api/admin)
- Kế toán: /api/ketoan/... → API_KeToan1 (downstream bỏ /api/ketoan)
- Thủ kho: /api/thukho/... → API_ThuKho (downstream bỏ /api/thukho)
- Thu ngân: /api/thungan/... → API_ThuNgan (downstream bỏ /api/thungan)

Ví dụ:
POST http://localhost:8080/api/thukho/QuanLyDanhMuc/insert-danhmuc
Content-Type: application/json
Authorization: Bearer <token>

{
  "MADANHMUC": "DMTEST01",
  "TENDANHMUC": "Đồ dùng kiểm thử",
  "MOTA": "Danh mục dữ liệu mẫu"
}

Đăng nhập công khai:
POST http://localhost:8080/api/Login/login

{
  "Username": "testadmin",
  "Pass": "Test123!"
}

## Xác thực

- Các API có bảo vệ cần header Authorization: Bearer <token>.
- Login, refresh và logout được cấu hình là route công khai.
- Quyền còn được kiểm tra bởi scope tại Gateway và role trong API đích.
- Upload ảnh phải dùng multipart/form-data.
- API backend trong Docker nghe nội bộ cổng 8080; từ máy host gọi qua Gateway.
