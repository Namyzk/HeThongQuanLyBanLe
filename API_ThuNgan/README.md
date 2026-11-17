# API Thu ngân

Gateway base URL: http://localhost:8080. Public prefix: /api/thungan. API cần xác thực yêu cầu Bearer token với role phù hợp (Admin/ThuNgan). JSON request dùng Content-Type: application/json.

## Bán hàng — /api/thungan/QuanLyBanHang

- GET /get-all-nhanvien-ban-hang
- GET /get-all-hoadonban
- GET /get-hoadonban-by-id?maHoaDon=HDTEST01
- POST /insert-hoadonban
- GET /get-all-chitietban
- GET /get-chitietban-by-IDhoadon?maHDB=HDTEST01
- GET /get-all-khachhang
- GET /get-khachhang-byid?maKH=KHTEST01
- POST /insert-khachhang
- GET /get-all-danhmuc
- GET /get-byID-danhmuc?madanhmuc=DMTEST01
- GET /get-all-sanpham
- GET /get-sanpham-by-id?id=SPTEST01
- PATCH /update-soluong-sanpham?maSP=SPTEST01&soLuongMoi=25
- GET /get-all-thanhtoan
- GET /get-thanhtoan-by-id?ma=TTTEST01
- POST /insert-thanhtoan

Body tạo hóa đơn kèm chi tiết:
{
  "MAHDBAN": "HDTEST01",
  "MANV": "NVTEST01",
  "MAKH": "KHTEST01",
  "NGAYLAP": "2026-10-23",
  "TONGTIENHANG": 50000,
  "THUEVAT": 5000,
  "GIAMGIA": 0,
  "listjson_chitietban": [
    {
      "MAHDBAN": "HDTEST01",
      "MASP": "SPTEST01",
      "SOLUONG": 2,
      "DONGIA": 25000,
      "TONGTIEN": 50000
    }
  ]
}

Body tạo khách hàng:
{
  "MaKH": "KHTEST01",
  "TenKH": "Khách hàng kiểm thử",
  "SDT": "0987654321",
  "DiaChi": "TP. Hồ Chí Minh"
}

## Đổi trả — /api/thungan/QuanLyDoiTra

- GET /get-all-hoadonban
- GET /get-hoadonban-by-id?maHoaDon=HDTEST01
- PUT /update-hoadonban — body cùng cấu trúc hóa đơn ở trên
- DELETE /delete-hoadonban?maHoaDon=HDTEST01
- POST /reset-tongtienhang-by-mahdban?maHDBan=HDTEST01&tongTienMoi=55000
- GET /get-all-chitietban
- GET /get-chitietban-by-IDhoadon?maHDB=HDTEST01
- POST /insert-chitietban
- PUT /update-chitietban
- DELETE /delete-chitietban?maHDB=HDTEST01&maSP=SPTEST01
- GET /get-all-thanhtoan
- GET /get-byId-thanhtoan?ma=TTTEST01
- POST /insert-thanhtoan
- PUT /update-thanhtoan
- DELETE /del-thanhtoan?ma=TTTEST01
- POST /reset-sotienthanhtoan-by-mahdban?maHDBan=HDTEST01&soTienMoi=55000

Body chi tiết bán:
{
  "MAHDBAN": "HDTEST01",
  "MASP": "SPTEST01",
  "SOLUONG": 2,
  "DONGIA": 25000,
  "TONGTIEN": 50000
}

Body thanh toán:
{
  "MaThanhToan": "TTTEST01",
  "MaHDBan": "HDTEST01",
  "PhuongThuc": "TienMat",
  "SoTienThanhToan": 55000,
  "NgayThanhToan": "2026-10-23T10:00:00",
  "TrangThai": "DaThanhToan"
}

## PayOS — /api/thungan/QuanLyBanHang/payos

- POST /create, body {"MaHoaDon":"HDTEST01"}
- GET /status?maHoaDon=HDTEST01
- POST /webhook: PayOS gửi payload có chữ ký; không giả lập bằng JSON tùy ý.

PayOS create cần hóa đơn tồn tại và backend đã được cấu hình PayOS. Tạo khách hàng/nhân viên/sản phẩm, nhập kho, hóa đơn, rồi thanh toán.
