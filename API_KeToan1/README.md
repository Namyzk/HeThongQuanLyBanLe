# API Kế toán

Gateway base URL: http://localhost:8080. Public prefix: /api/ketoan. API có phân quyền cần Bearer token và role Admin hoặc KeToan.

## Công nợ — /api/ketoan/QuanLyCongNo

- GET /search-khachhang-chuathanhtoan?tenKh=Nguyen
- GET /get-hoadon-chuathanhtoan
- PUT /update-trangthai-thanhtoan?maHDBan=HDTEST01&phuongThuc=TienMat
- GET /get-all-thanhtoan
- GET /get-byId-thanhtoan?mathanhtoan=TTTEST01
- POST /update-thanhtoan
- GET /get-all-hoadonban
- GET /get-hoadonban-by-id?maHoaDon=HDTEST01
- GET /get-all-nhacungcap
- GET /get-byid-nhacungcap?ma=NCCTEST01
- GET /get-all-khachhang
- GET /get-byid-khachhang?makh=KHTEST01

Body cho POST update-thanhtoan:
{
  "MaThanhToan": "TTTEST01",
  "MaHDBan": "HDTEST01",
  "PhuongThuc": "TienMat",
  "SoTienThanhToan": 55000,
  "NgayThanhToan": "2026-10-23T10:00:00",
  "TrangThai": "DaThanhToan"
}

## Báo cáo thống kê — /api/ketoan/BaoCaoThongKe

Tất cả endpoint là GET và không có body:

- /get-all-danhmuc
- /get-byID-danhmuc?madanhmuc=DMTEST01
- /get-all-sanpham
- /get-sanpham-by-id?id=SPTEST01
- /get-all-khuyenmai
- /get-byid-khuyenmai?ma=KMTEST01
- /get-all-phieunhapkho
- /get-byid-phieunhapkho?maphieunhap=PNTEST01
- /get-all-chitietnhap
- /get-byphieu-chitietnhap?maphieunhap=PNTEST01
- /get-all-hoadonban
- /get-hoadonban-by-id?maHoaDon=HDTEST01
- /get-all-chitietban
- /get-chitietban-by-IDhoadon?maHDB=HDTEST01

Các API báo cáo yêu cầu role Admin hoặc KeToan. Dữ liệu tra cứu phải được tạo từ API Thủ kho/Thu ngân trước.
