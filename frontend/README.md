# HỆ THỐNG QUẢN LÝ BÁN LẺ & KHO

**HỆ THỐNG QUẢN LÝ BÁN LẺ & KHO** được xây dựng theo kiến trúc **Microservices (.NET 8 Web API + Ocelot API Gateway + SQL Server)** kết hợp với giao diện **Frontend (HTML5, Modular CSS3 & AngularJS 1.x)**, đóng gói và vận hành đồng bộ qua **Docker Compose** (Apache Reverse Proxy + 6 container dịch vụ .NET).

---

## Mục lục
1. [Kiến trúc tổng thể hệ thống](#1-kiến-trúc-tổng-thể-hệ-thống)
2. [Công nghệ sử dụng](#2-công-nghệ-sử-dụng)
3. [Cấu trúc thư mục dự án (Backend & Frontend)](#3-cấu-trúc-thư-mục-dự-án-backend--frontend)
4. [Thiết kế Cơ sở dữ liệu (SQL Server)](#4-thiết-kế-cơ-sở-dữ-liệu-sql-server)
5. [Phân quyền RBAC & Cơ chế Bảo mật](#5-phân-quyền-rbac--cơ-chế-bảo-mật)
6. [Các công thức nghiệp vụ cốt lõi](#6-các-công-thức-nghiệp-vụ-cốt-lõi)
7. [Danh sách API theo từng Microservice](#7-danh-sách-api-theo-từng-microservice)
8. [Hướng dẫn cài đặt & Triển khai (Docker / Local)](#8-hướng-dẫn-cài-đặt--triển-khai-docker--local)
9. [Tài khoản đăng nhập mẫu theo vai trò](#9-tài-khoản-đăng-nhập-mẫu-theo-vai-trò)

---

## 1. Kiến trúc tổng thể hệ thống

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        TRÌNH DUYỆT NGƯỜI DÙNG (Browser)                      │
│                      http://localhost:8080/trang/*.html                      │
└───────────────────────────────────────┬──────────────────────────────────────┘
                                        │ HTTP (Port 8080)
                                        ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                 CONTAINER: frontend (Apache HTTPD 2.4)                       │
│   • Phục vụ tĩnh HTML / CSS / AngularJS (/trang/*, /css/*, /js/*)            │
│   • Reverse Proxy chuyển tiếp toàn bộ /api/* sang http://api-gateway:8080    │
└───────────────────────────────────────┬──────────────────────────────────────┘
                                        │ Internal Docker Network
                                        ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                 CONTAINER: api-gateway (Ocelot API Gateway)                  │
│   • Xác thực JWT từ HttpOnly Cookie (qlbl_access / qlbl_refresh)             │
│   • Kiểm tra Origin CSRF cho các phương thức thay đổi dữ liệu                │
│   • GlobalExceptionMiddleware: Bắt lỗi toàn cục + Chống Brute-Force Login    │
│   • RateLimiter: Chặn spam khi chưa đăng nhập, mở giới hạn sau khi xác thực  │
│   • Định tuyến & Kiểm tra Scope phân quyền theo vai trò (ocelot.json)        │
└──────┬───────────────┬────────────────┬────────────────┬─────────────────────┘
       │               │                │                │
       ▼               ▼                ▼                ▼
┌─────────────┐ ┌─────────────┐ ┌──────────────┐ ┌──────────────┐ ┌────────────┐
│  api-login  │ │  api-admin  │ │  api-thukho  │ │ api-thungan  │ │ api-ketoan │
│  (Xác thực) │ │ (Quản trị)  │ │ (Kho & Hàng) │ │ (Bán hàng)   │ │ (Kế toán)  │
└──────┬──────┘ └──────┬──────┘ └──────┬───────┘ └──────┬───────┘ └─────┬──────┘
       └───────────────┴───────────────┴────────┬───────┴───────────────┘
                                                │ ADO.NET (Microsoft.Data.SqlClient)
                                                ▼
                                 ┌──────────────────────────────┐
                                 │   SQL SERVER (QUANLYBANLE)   │
                                 └──────────────────────────────┘
```

---

## 2. Công nghệ sử dụng

### Backend (`D:\WebApi`)
- **Nền tảng**: ASP.NET Core 8.0 Web API (`net8.0`).
- **Kiến trúc**: Microservices định tuyến qua **Ocelot API Gateway**, mỗi service tách biệt theo mô hình 3 lớp (**Controller $\rightarrow$ BLL $\rightarrow$ DAL**) dùng chung thư viện `Models`, `BLL`, `DAL`.
- **Truy cập dữ liệu**: `Microsoft.Data.SqlClient` (ADO.NET thuần với `SqlTransaction` đảm bảo tính toàn vẹn dữ liệu và Stored Procedures).
- **Bảo mật & Xác thực**:
  - **JWT (JSON Web Token)** lưu trong **HttpOnly Cookie** (`qlbl_access` thời hạn 15 phút, `qlbl_refresh` lưu dưới dạng băm `SHA-256` trong bảng `RefreshToken`).
  - **Mật khẩu**: Băm an toàn bằng thuật toán **PBKDF2**.
  - **Rate Limiting & Brute-Force Protection**: Tích hợp tại `API_GateWay`.
- **Tích hợp thanh toán**: Cổng thanh toán **PayOS QR** (Tạo mã QR VietQR động, đối soát tự động qua Polling & Webhook).
- **Ghi vết vận hành**: Bảng `AuditLog` tự động ghi nhận các thao tác thay đổi dữ liệu (`POST`, `PUT`, `DELETE`, `PATCH`).

### Frontend (`D:\FontEnd`)
- **Framework**: **AngularJS 1.x** (`qlblApp`) kết hợp kiến trúc Multi-Page (`frontend/trang/*.html`) và hệ thống dịch vụ dùng chung (`ApiService`, `AuthService`, `SidebarHeaderService`, `ToastService`, `ModalService`).
- **Giao diện (UI/UX)**:
  - Thiết kế theo tông màu chủ đạo **Trắng (`#ffffff`) & Xanh Dương Hoàng Gia (`#2563eb` / `#1d4ed8`)**.
  - Hệ thống CSS phân tách theo module (`chung.css`, `bo-cuc.css`, `thanh-phan.css`, `bieu-mau.css`, `hien-thi-di-dong.css` và `css/trang/*.css`).
  - Hỗ trợ giao diện Responsive trên Desktop, Tablet và Mobile.

---

## 3. Cấu trúc thư mục dự án (Backend & Frontend)

### 3.1. Thư mục Backend (`D:\WebApi`)
```text
D:\WebApi\
├── API_GateWay\                    # Cổng Ocelot API Gateway trung tâm
│   ├── Middleware\
│   │   └── GlobalExceptionMiddleware.cs # Bắt ngoại lệ toàn cục & Chống Brute-Force Login (429)
│   ├── ocelot.json                 # Cấu hình định tuyến & phân quyền Scope cho các service
│   └── Program.cs                  # Cấu hình JWT Cookie, CORS, RateLimiter, Ocelot
├── API_Login\                      # Service Đăng nhập, Làm mới Token (Refresh), Đăng xuất
├── BTL Web API\ (API_Admin)        # Service Quản trị: Tài khoản, Nhân viên, Khuyến mãi
├── API_ThuKho\                     # Service Thủ kho: Danh mục, Sản phẩm, Tồn kho, Nhập kho, NCC
├── API_ThuNgan\                    # Service Thu ngân: Bán hàng POS, Hóa đơn, Đổi trả, Khách hàng, PayOS
├── API_KeToan1\                    # Service Kế toán: Báo cáo thống kê, Quản lý công nợ
├── A\ (BLL)                        # Tầng nghiệp vụ (Business Logic Layer) dùng chung
├── DAL\                            # Tầng truy xuất dữ liệu (Data Access Layer) & DataHelper
├── Model\                          # Các lớp thực thể (Entities / DTOs)
├── Pic\                            # Thư mục lưu trữ ảnh Sản phẩm & Nhân viên
├── SQL\
│   └── QuanLyBanLe.sql             # Script khởi tạo CSDL, Bảng, Ràng buộc, Dữ liệu mẫu
├── Dockerfile                      # Multi-stage Dockerfile dùng chung cho các .NET Services
└── docker-compose.yml              # Khởi chạy đồng bộ toàn bộ Backend + Frontend
```

### 3.2. Thư mục Frontend (`D:\FontEnd`)
```text
D:\FontEnd\
├── README.md                       # Tài liệu tổng quan toàn hệ thống
└── frontend\
    ├── Dockerfile                  # Cấu hình Apache HTTPD & Reverse Proxy /api/*
    ├── angular.min.js              # Thư viện lõi AngularJS
    ├── index.html                  # Điểm vào chuyển hướng mặc định
    ├── css\
    │   ├── chung.css               # Biến CSS (:root), Reset, Typography, Tông màu Trắng - Xanh
    │   ├── bo-cuc.css              # Bố cục Sidebar, Header, Khung trang
    │   ├── thanh-phan.css          # Card, Button, Table, Modal, Toast, Badge
    │   ├── bieu-mau.css            # Form, Input, Select, Validation
    │   ├── hien-thi-di-dong.css    # Responsive cho màn hình nhỏ
    │   └── trang\                  # CSS chuyên biệt cho từng trang (ban-hang.css, quan-ly-hoa-don.css,...)
    ├── js\
    │   ├── angular-app.js          # Khởi tạo module qlblApp, APP_CONFIG, ApiService, AuthService,...
    │   └── trang\                  # AngularJS Controllers cho 15 trang nghiệp vụ
    └── trang\                      # Các trang giao diện HTML
        ├── dang-nhap.html          # Màn hình đăng nhập
        ├── tong-quan.html          # Dashboard KPI theo vai trò
        ├── quan-ly-tai-khoan.html  # Quản lý tài khoản & phân quyền
        ├── quan-ly-nhan-vien.html  # Quản lý hồ sơ nhân viên & ảnh
        ├── quan-ly-danh-muc.html   # Quản lý danh mục hàng hóa
        ├── quan-ly-san-pham.html   # Quản lý sản phẩm, thuế VAT, ảnh & xem chi tiết 2 cột
        ├── quan-ly-ton-kho.html    # Theo dõi và điều chỉnh tồn kho
        ├── quan-ly-phieu-nhap.html # Lập phiếu nhập kho & cập nhật tồn kho tự động
        ├── quan-ly-nha-cung-cap.html # Quản lý đối tác nhà cung cấp
        ├── quan-ly-khuyen-mai.html # Quản lý chương trình khuyến mãi theo sản phẩm & thời hạn
        ├── ban-hang.html           # Màn hình POS bán hàng, tính thuế/KM tự động, QR PayOS
        ├── quan-ly-hoa-don.html    # Quản lý lịch sử hóa đơn, xem chi tiết 960px & in hóa đơn
        ├── doi-tra.html            # Nghiệp vụ trả 1 món, trả hết hoặc đổi sang sản phẩm khác
        ├── quan-ly-khach-hang.html # Quản lý thông tin khách hàng
        ├── quan-ly-thanh-toan.html # Lịch sử giao dịch thanh toán
        ├── quan-ly-cong-no.html    # Đối soát công nợ & thu nợ hóa đơn
        └── bao-cao-thong-ke.html   # Báo cáo doanh thu, chi phí nhập, lợi nhuận gộp, Top SP, xuất CSV
```

---

## 4. Thiết kế Cơ sở dữ liệu (SQL Server)

Cơ sở dữ liệu **`QUANLYBANLE`** bao gồm **12 bảng nghiệp vụ và bảo mật**:

| STT | Tên bảng | Khóa chính | Mô tả chức năng & Ràng buộc chính |
| :---: | :--- | :--- | :--- |
| 1 | `DANHMUC` | `MADANHMUC` | Danh mục phân loại hàng hóa. |
| 2 | `SANPHAM` | `MASP` | Thông tin sản phẩm, `DONGIA`, `THUEVAT` (% thuế riêng từng SP), `SOLUONGTON`, `FK_SANPHAM_DANHMUC`. |
| 3 | `KHUYENMAI` | `MAKM` | Chương trình khuyến mãi gắn với `MASP` (`FK_KHUYENMAI_SANPHAM`), kiểm tra `CK_KHUYENMAI_DATE (NGAYKETTHUC >= NGAYBATDAU)`. |
| 4 | `NHACUNGCAP` | `MANCC` | Thông tin nhà cung cấp (`TENNCC`, `SDT`, `DIACHI`, `EMAIL`). |
| 5 | `NHANVIEN` | `MANV` | Thông tin nhân viên (`TENNV`, `SDT`, `DIACHI`, `EMAIL`). |
| 6 | `TAIKHOAN` | `MATAIKHOAN` | Tài khoản đăng nhập (`USERNAME`, `PASS` băm PBKDF2, `QUYEN`: `1=Admin, 2=ThuNgan, 3=ThuKho, 4=KeToan`). |
| 7 | `RefreshToken` | `RefreshTokenHash` | Lưu mã băm SHA-256 của Refresh Token, hỗ trợ xoay vòng token và thu hồi khi khởi động lại Gateway. |
| 8 | `KHACHHANG` | `MAKH` | Hồ sơ khách hàng mua lẻ / thân thiết. |
| 9 | `PHIEUNHAPKHO` | `MAPHIEUNHAP` | Thông tin chung phiếu nhập kho (`MANCC`, `MANV`, `NGAYLAP`, `THUEVAT`). |
| 10 | `CHITIETNHAP` | `(MAPHIEUNHAP, MASP)` | Chi tiết từng mặt hàng nhập (`SOLUONG`, `DONGIANHAP`, `THANHTIEN`). |
| 11 | `HOADONBAN` | `MAHDBAN` | Hóa đơn bán hàng (`MANV`, `MAKH`, `NGAYLAP`, `TONGTIENHANG`, `THUEVAT`, `GIAMGIA`). |
| 12 | `CT_HDB` | `(MAHDBAN, MASP)` | Chi tiết các mặt hàng trong hóa đơn bán (`SOLUONG`, `DONGIA`, `TONGTIEN`). |
| 13 | `THANHTOAN` | `MATHANHTOAN` | Giao dịch thanh toán (`Tiền mặt`, `PayOS`, trạng thái `Đã thanh toán` / `Chưa thanh toán`, thông tin đối soát QR PayOS). |
| 14 | `AuditLog` | `AuditLogId` | Nhật ký kiểm toán tự động ghi lại các thao tác làm thay đổi dữ liệu qua API. |

---

## 5. Phân quyền RBAC & Cơ chế Bảo mật

### 5.1. Ma trận phân quyền 4 vai trò (RBAC)

| Vai trò | Mã quyền | Quyền hạn trên Giao diện (UI) & API Gateway (`ocelot.json`) |
| :--- | :---: | :--- |
| **Quản trị viên (`Admin`)** | `1` | Toàn quyền trên cả 4 phân hệ (`/api/admin`, `/api/thukho`, `/api/thungan`, `/api/ketoan`) và toàn bộ 14 menu chức năng. |
| **Thu ngân (`ThuNgan`)** | `2` | Truy cập POS Bán hàng, Hóa đơn, Đổi trả, Khách hàng, Thanh toán (`/api/thungan/*`) và quyền đọc Sản phẩm/Danh mục/Tồn kho (`/api/thukho/...`). Chặn `403` với Admin & Kế toán. |
| **Thủ kho (`ThuKho`)** | `3` | Truy cập Danh mục, Sản phẩm, Tồn kho, Phiếu nhập kho, Nhà cung cấp (`/api/thukho/*`). Chặn `403` với Admin, Bán hàng POS & Kế toán. |
| **Kế toán (`KeToan`)** | `4` | Truy cập Báo cáo & Thống kê, Quản lý Công nợ (`/api/ketoan/*`) và đối soát Hóa đơn/Thanh toán. Chặn `403` với Admin, POS & thao tác sửa Kho. |

### 5.2. Cơ chế chống Brute-Force & Rate Limiting (`API_GateWay`)
- **Chống dò mật khẩu (`GlobalExceptionMiddleware.cs`)**:
  - Áp dụng riêng cho `POST /api/Login/login`.
  - Theo dõi theo cặp `IP | USERNAME`. Nếu đăng nhập sai (`401 Unauthorized`) **từ 5 lần trở lên trong 3 phút**, hệ thống khóa tạm thời **3 phút** và trả về **`429 Too Many Requests`**. Khi đăng nhập đúng (`200 OK`), bộ đếm lỗi được xóa về `0`.
- **Giới hạn tần suất (`Program.cs`)**:
  - Khi **chưa đăng nhập** (`IsAuthenticated == false`): Giới hạn bằng `SlidingWindowRateLimiter` (`100 request / 1 phút / IP`).
  - Khi **đã đăng nhập** (`IsAuthenticated == true`): Mở giới hạn (`RateLimitPartition.GetNoLimiter("authenticated")`) để người dùng thao tác nghiệp vụ và tải ảnh sản phẩm mượt mà không bị chặn `429`.

---

## 6. Các công thức nghiệp vụ cốt lõi

1. **Nghiệp vụ Bán hàng & Tính thuế riêng từng sản phẩm (`ban-hang.js`, `HoaDonBan_DAL.cs`)**:
   - Với mỗi sản phẩm $i$ trong giỏ hàng:
     - $\text{Tiền chưa thuế SP}_i = \text{Số lượng}_i \times \text{Đơn giá bán}_i$ (`SOLUONG > 0`, `DONGIA > 0`)
     - $\text{Tiền thuế VAT riêng SP}_i = \text{Làm tròn}\left(\text{Tiền chưa thuế SP}_i \times \frac{\text{\% Thuế của SP}_i}{100}\right)$
     - $\text{Thành tiền SP}_i\text{ (đã gồm thuế)} = \text{Tiền chưa thuế SP}_i + \text{Tiền thuế VAT riêng SP}_i$
   - Tổng hợp toàn hóa đơn:
     - $\text{Tổng tiền hàng (chưa thuế)} = \sum \text{Tiền chưa thuế SP}_i$
     - $\text{Tổng thuế VAT} = \sum \text{Tiền thuế VAT riêng SP}_i$
     - $\text{Tổng tiền hàng (gồm thuế)} = \text{Tổng tiền hàng (chưa thuế)} + \text{Tổng thuế VAT}$
2. **Nghiệp vụ Áp dụng Khuyến mãi theo sản phẩm & thời hạn (`ban-hang.js`)**:
   - Chỉ áp dụng chương trình khuyến mãi gắn với `MASP` đang trong thời hạn hiệu lực ($\text{Ngày bắt đầu} \le \text{Hôm nay} \le \text{Ngày kết thúc}$). Khuyến mãi đã hết hạn bị bỏ qua hoàn toàn.
   - Tự động tính số tiền giảm khuyến mãi của từng sản phẩm đủ điều kiện và cộng vào trường `GIAMGIA`.
3. **Số tiền khách phải thanh toán (`PayOsPayment_DAL.cs`, `ban-hang.js`, `quan-ly-hoa-don.js`)**:
   $$\text{Số tiền phải trả} = \text{Tổng tiền hàng (chưa thuế)} + \text{Tổng thuế VAT} - \text{Giảm giá}$$
4. **Nghiệp vụ Nhập kho & Cập nhật tồn kho (`PhieuNhapKho_DAL.cs`)**:
   - $\text{Thành tiền nhập} = \text{Số lượng nhập} \times \text{Đơn giá nhập}$ (`SOLUONG > 0`, `DONGIANHAP >= 0`)
   - $\text{Tổng chi phí phiếu nhập} = \sum \text{Thành tiền nhập của các sản phẩm trong phiếu}$
   - $\text{Tồn kho mới} = \text{Tồn kho hiện tại} + \text{Số lượng nhập ròng}$ (khi sửa phiếu: $\text{SL nhập ròng} = \text{SL mới} - \text{SL cũ}$; khi xóa chi tiết nhập: trừ lại số lượng tương ứng khỏi tồn kho).
5. **Báo cáo Lợi nhuận gộp (`bao-cao-thong-ke.js`)**:
   $$\text{Lợi nhuận gộp ước tính} = \text{Tổng doanh thu bán hàng} - \text{Tổng chi phí nhập hàng}$$

---

## 7. Danh sách API theo từng Microservice

Tất cả request từ Frontend gọi qua cổng duy nhất `http://localhost:8080/api/...`:

- **Xác thực (`API_Login` — `/api/Login/...`)**:
  - `POST /api/Login/login`: Đăng nhập và cấp HttpOnly Cookie (`qlbl_access`, `qlbl_refresh`).
  - `POST /api/Login/refresh`: Làm mới phiên đăng nhập.
  - `POST /api/Login/logout`: Thu hồi Refresh Token và xóa Cookie.
- **Quản trị (`API_Admin` — `/api/admin/...`)**:
  - `QuanLyTaiKhoan`: `get-all-taikhoan`, `create-taikhoan`, `update-taikhoan`, `del-taikhoan`.
  - `QuanLyNhanVien`: `get-all-nhanvien`, `create-nhanvien`, `update-nhanvien`, `del-nhanvien`, `upload-anh-nv`, `xem-anh-nv/{maNV}`.
  - `QuanLyKhuyenMai`: `get-all-khuyenmai`, `get-byid-khuyenmai`, `create-khuyenmai`, `update-khuyenmai`, `del-khuyenmai`.
- **Thủ kho (`API_ThuKho` — `/api/thukho/...`)**:
  - `QuanLyDanhMuc`: `get-all-danhmuc`, `insert-danhmuc`, `update-danhmuc`, `delete-danhmuc`.
  - `QuanLySanPham`: `get-all-sanpham`, `get-sanpham-by-id`, `insert-sanpham`, `update-sanpham`, `delete-sanpham`, `upload-anh-sp`, `xem-anh-sp/{maSP}`.
  - `QuanLyTonKho`: `get-all-sanpham-tonkho`, `update-soluongton`.
  - `QuanLyNhapKho`: `get-all-phieunhapkho`, `insert-phieunhapkho`, `update-phieunhapkho`, `delete-phieunhapkho`, `get-all-nhacungcap`, `insert-nhacungcap`, `update-nhacungcap`, `delete-nhacungcap`.
- **Thu ngân (`API_ThuNgan` — `/api/thungan/...`)**:
  - `QuanLyBanHang`: `get-all-hoadonban`, `get-hoadonban-by-id`, `insert-hoadonban`, `update-hoadonban`, `delete-hoadonban`, `get-all-khachhang`, `insert-khachhang`, `update-khachhang`, `delete-khachhang`, `get-all-thanhtoan`, `insert-thanhtoan`, `payos/create`, `payos/status`, `payos/webhook`.
  - `QuanLyDoiTra`: `get-chitietban-by-IDhoadon`, `tra-1-san-pham`, `tra-het-san-pham`, `doi-san-pham`.
- **Kế toán (`API_KeToan1` — `/api/ketoan/...`)**:
  - `BaoCaoThongKe`: `get-all-hoadonban`, `get-all-chitietban`, `get-all-phieunhapkho`, `get-all-chitietnhap`, `get-all-sanpham`, `get-all-danhmuc`.
  - `QuanLyCongNo`: `get-all-thanhtoan`, `insert-thanhtoan`, `update-thanhtoan`, `delete-thanhtoan`.

---

## 8. Hướng dẫn cài đặt & Triển khai (Docker / Local)

### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
1. Mở **SQL Server Management Studio (SSMS)**.
2. Mở và chạy file script `D:\WebApi\SQL\QuanLyBanLe.sql` để tạo CSDL `QUANLYBANLE`, các bảng, Stored Procedures và dữ liệu mẫu (20 bản ghi cho mỗi bảng).

### Bước 2: Khởi chạy toàn bộ hệ thống bằng Docker Compose
Mở PowerShell tại `D:\WebApi` và chạy lệnh:
```powershell
cd D:\WebApi
docker compose up -d --build
```
Sau khi các container khởi chạy thành công, mở trình duyệt truy cập:
- **Giao diện Web & POS**: `http://localhost:8080/trang/dang-nhap.html`

---

## 9. Tài khoản đăng nhập mẫu theo vai trò

| Vai trò | Tên đăng nhập (`USERNAME`) | Mật khẩu (`PASS`) | Mã quyền (`QUYEN`) | Mô tả quyền hạn |
| :--- | :--- | :--- | :---: | :--- |
| **Quản trị viên (`Admin`)** | `admin` | `admin123` | `1` | Toàn quyền quản trị hệ thống |
| **Thu ngân (`ThuNgan`)** | `nv_ha` | `pass002` | `2` | Bán hàng POS, Hóa đơn, Đổi trả, Khách hàng |
| **Thủ kho (`ThuKho`)** | `nv_lan` | `pass004` | `3` | Quản lý Sản phẩm, Danh mục, Tồn kho, Nhập kho, NCC |
| **Kế toán (`KeToan`)** | `nv_hoang` | `pass003` | `4` | Báo cáo thống kê doanh thu/lợi nhuận, Quản lý Công nợ |
