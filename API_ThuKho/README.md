# API Thủ kho

Gateway base URL: http://localhost:8080. Public prefix: /api/thukho. Endpoint bảo vệ cần Bearer token và role được API yêu cầu. Dùng Content-Type: application/json cho JSON body.

## Danh mục — /api/thukho/QuanLyDanhMuc

- GET /get-all-danhmuc
- GET /get-byID-danhmuc?madanhmuc=DMTEST01
- POST /insert-danhmuc
- POST /update-danhmuc
- DELETE /delete-danhmuc?maDanhMuc=DMTEST01

Body create/update:
{
  "MADANHMUC": "DMTEST01",
  "TENDANHMUC": "Đồ dùng kiểm thử",
  "MOTA": "Danh mục dữ liệu mẫu"
}

## Nhà cung cấp — /api/thukho/QuanLyNhapKho

- GET /get-all-nhacungcap
- GET /get-byid-nhacungcap?ma=NCCTEST01
- POST /create-nhacungcap
- POST /update-nhacungcap
- DELETE /del-nhacungcap?ma=NCCTEST01
- GET /get-all-nhanvien

Body create/update:
{
  "MaNCC": "NCCTEST01",
  "TenNCC": "Nhà cung cấp kiểm thử",
  "DiaChi": "Hà Nội",
  "SDT": "0912345678",
  "EMAIL": "test@example.com"
}

## Phiếu nhập và chi tiết nhập — /api/thukho/QuanLyNhapKho

- GET /get-all-phieunhapkho
- GET /get-byid-phieunhapkho?maphieunhap=PNTEST01
- POST /create-phieunhapkho
- PUT /update-phieunhapkho
- DELETE /delete-phieunhapkho?maphieunhap=PNTEST01
- GET /get-all-chitietnhap
- GET /get-byphieu-chitietnhap?maphieunhap=PNTEST01
- GET /get-byid-chitietnhap?maphieunhap=PNTEST01&masp=SPTEST01
- POST /create-chitietnhap
- POST /update-chitietnhap
- DELETE /delete-chitietnhap?maphieunhap=PNTEST01&masp=SPTEST01

Body create phiếu nhập (đã gồm chi tiết):
{
  "MAPHIEUNHAP": "PNTEST01",
  "MANCC": "NCCTEST01",
  "MANV": "NVTEST01",
  "NGAYLAP": "2026-10-23",
  "listjson_chitietnhap": [
    {
      "MAPHIEUNHAP": "PNTEST01",
      "MASP": "SPTEST01",
      "SOLUONG": 20,
      "DONGIANHAP": 18000,
      "THANHTIEN": 360000
    }
  ]
}

Body create/update chi tiết riêng:
{
  "MAPHIEUNHAP": "PNTEST01",
  "MASP": "SPTEST01",
  "SOLUONG": 20,
  "DONGIANHAP": 18000,
  "THANHTIEN": 360000
}

## Sản phẩm — /api/thukho/QuanLySanPham

- GET /get-all-sanpham
- GET /get-sanpham-by-id?id=SPTEST01
- POST /create-sanpham
- POST /update-sanpham
- POST /update-soluong-sanpham?maSP=SPTEST01&soLuongMoi=25
- DELETE /delete-sanpham?maSP=SPTEST01
- POST /upload: multipart/form-data
- GET /xem-anh/{fileName}
- GET /xem-anh-sp/{maSP}
- GET /get-anh-by-masp?maSP=SPTEST01
- DELETE /xoa-anh?maSP=SPTEST01&filePath=...

Body create/update:
{
  "MASP": "SPTEST01",
  "TENSP": "Sản phẩm kiểm thử",
  "MAVACH": "8930000000001",
  "MOTA": "Dữ liệu kiểm thử",
  "MADANHMUC": "DMTEST01",
  "DONGIA": 25000,
  "THUOCTINH": "Cái",
  "THUE": 0.1,
  "SOLUONGTON": 0
}

## Tồn kho — /api/thukho/QuanLyTonKho

- GET /get-all-sanpham-tonkho
- GET /get-sanpham-tonkho-by-id?id=SPTEST01
- POST /update-soluong-sanpham-tonkho?maSP=SPTEST01&soLuongMoi=25

Tạo danh mục, nhà cung cấp, nhân viên, sản phẩm trước khi tạo phiếu nhập. Phiếu nhập có thể làm tăng tồn kho theo trigger database.
