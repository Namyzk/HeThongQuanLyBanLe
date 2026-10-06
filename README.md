# QuanLyBanLe Backend

## Cấu hình khi chạy

Không đưa khóa ký JWT, thông tin PayOS hoặc connection string máy cá nhân lên Git. Cấu hình các giá trị này bằng User Secrets khi phát triển hoặc biến môi trường khi triển khai.

Các API và Gateway cần dùng chung các giá trị JWT:

- `Jwt__Key`: khóa ký tối thiểu 32 byte.
- `Jwt__Issuer`: cùng issuer giữa Login, Gateway và API con.
- `Jwt__Audience`: cùng audience giữa Login, Gateway và API con.

Các API kết nối SQL Server cần `ConnectionStrings__DefaultConnection`. API Thu Ngân cần thêm `PayOS__ClientId`, `PayOS__ApiKey` và `PayOS__ChecksumKey` để tạo giao dịch PayOS.

## Gateway và frontend

Frontend dùng cookie HttpOnly và phải gọi Gateway qua HTTPS (`https://localhost:7144`). Cấu hình các origin frontend được phép trong `Cors__AllowedOrigins` của Gateway. Cookie không hoạt động qua HTTP vì được đánh dấu `Secure`.

Ocelot routes nằm trong `API_GateWay/ocelot.json`.
