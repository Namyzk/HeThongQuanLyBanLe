# Health Check API

Đây là dịch vụ độc lập, không chỉnh sửa hoặc thay thế các API hiện có.

## Endpoint

- `GET /health/live`: kiểm tra tiến trình Health Check API có hoạt động.
- `GET /health/ready`: kiểm tra khả năng kết nối và truy vấn SQL Server.
- `GET /health`: tổng hợp cả trạng thái ứng dụng và cơ sở dữ liệu.

Endpoint trả HTTP `200` khi các bước kiểm tra được yêu cầu đều đạt; trả HTTP `503` nếu có kiểm tra không đạt. Phản hồi JSON không tiết lộ chuỗi kết nối hay chi tiết lỗi nhạy cảm.

## Chạy riêng bằng Docker Compose

```powershell
docker compose -f docker-compose.healthcheck.yml up --build -d
```

Truy cập `http://localhost:8081/health`.
