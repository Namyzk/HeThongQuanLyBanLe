IF DB_ID(N'QUANLYBANLE') IS NULL
    CREATE DATABASE QUANLYBANLE;
GO
USE QUANLYBANLE;
GO
select * from Auditlog
-- Audit log chỉ ghi các thao tác có khả năng thay đổi dữ liệu qua API.
-- Khi chạy lần đầu: tạo bảng; nếu bảng đã tồn tại: chỉ bổ sung các cột còn thiếu.
IF OBJECT_ID(N'dbo.AuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditLog
    (
        AuditLogId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLog PRIMARY KEY,
        OccurredAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_AuditLog_OccurredAtUtc DEFAULT SYSUTCDATETIME(),
        ServiceName NVARCHAR(80) NOT NULL,
        HttpMethod NVARCHAR(12) NOT NULL,
        RequestPath NVARCHAR(1000) NOT NULL,
        StatusCode SMALLINT NOT NULL,
        UserId NVARCHAR(100) NULL,
        UserName NVARCHAR(100) NULL,
        RoleName NVARCHAR(50) NULL,
        ClientIp NVARCHAR(45) NULL,
        TraceId NVARCHAR(100) NOT NULL,
        ElapsedMilliseconds BIGINT NOT NULL,
        OperationType NVARCHAR(20) NOT NULL CONSTRAINT DF_AuditLog_OperationType DEFAULT N'UNKNOWN',
        EntityName NVARCHAR(128) NOT NULL CONSTRAINT DF_AuditLog_EntityName DEFAULT N'UNKNOWN',
        RecordKey NVARCHAR(200) NULL,
        Result NVARCHAR(20) NOT NULL CONSTRAINT DF_AuditLog_Result DEFAULT N'UNKNOWN'
    );
END;
GO

IF COL_LENGTH(N'dbo.AuditLog', N'OperationType') IS NULL
    ALTER TABLE dbo.AuditLog ADD OperationType NVARCHAR(20) NOT NULL
        CONSTRAINT DF_AuditLog_OperationType DEFAULT N'UNKNOWN' WITH VALUES;
GO
IF COL_LENGTH(N'dbo.AuditLog', N'EntityName') IS NULL
    ALTER TABLE dbo.AuditLog ADD EntityName NVARCHAR(128) NOT NULL
        CONSTRAINT DF_AuditLog_EntityName DEFAULT N'UNKNOWN' WITH VALUES;
GO
IF COL_LENGTH(N'dbo.AuditLog', N'RecordKey') IS NULL
    ALTER TABLE dbo.AuditLog ADD RecordKey NVARCHAR(200) NULL;
GO
IF COL_LENGTH(N'dbo.AuditLog', N'Result') IS NULL
    ALTER TABLE dbo.AuditLog ADD Result NVARCHAR(20) NOT NULL
        CONSTRAINT DF_AuditLog_Result DEFAULT N'UNKNOWN' WITH VALUES;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_OccurredAtUtc' AND object_id = OBJECT_ID(N'dbo.AuditLog'))
    CREATE INDEX IX_AuditLog_OccurredAtUtc ON dbo.AuditLog (OccurredAtUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_UserId_OccurredAtUtc' AND object_id = OBJECT_ID(N'dbo.AuditLog'))
    CREATE INDEX IX_AuditLog_UserId_OccurredAtUtc ON dbo.AuditLog (UserId, OccurredAtUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AuditLog_EntityName_OccurredAtUtc' AND object_id = OBJECT_ID(N'dbo.AuditLog'))
    CREATE INDEX IX_AuditLog_EntityName_OccurredAtUtc ON dbo.AuditLog (EntityName, OccurredAtUtc DESC);
GO
-- Nếu QUANLYBANLE đã tồn tại, hãy chỉ chạy phần AuditLog phía trên (từ IF OBJECT_ID đến các index).
-- Phần dưới là script khởi tạo schema mới và không dùng để chạy lại trên database đã có bảng.
-- Xem các thay đổi gần nhất:
-- SELECT TOP (100) OccurredAtUtc, UserName, RoleName, OperationType, EntityName,
--        RecordKey, Result, StatusCode, ServiceName, RequestPath
-- FROM dbo.AuditLog WHERE OperationType <> N'UNKNOWN' ORDER BY AuditLogId DESC;
CREATE TABLE DANHMUC (
    MADANHMUC CHAR(15) PRIMARY KEY,
    TENDANHMUC NVARCHAR(200),
    MOTA NVARCHAR(1000)
)

CREATE TABLE SANPHAM (
    MASP CHAR(15) PRIMARY KEY,
    TENSP NVARCHAR(300),
    MAVACH NVARCHAR(128),
    MOTA NVARCHAR(MAX),
    MADANHMUC CHAR(15),
    DONGIA FLOAT DEFAULT 0,
    THUOCTINH NVARCHAR(MAX),
    THUEVAT FLOAT DEFAULT 0,
    SOLUONGTON INT DEFAULT 0,
    CONSTRAINT FK_SANPHAM_DANHMUC FOREIGN KEY (MADANHMUC) REFERENCES DANHMUC(MADANHMUC)
)

CREATE TABLE KHUYENMAI (
    MAKM CHAR(15) PRIMARY KEY,
    TENKM NVARCHAR(300),
    MASP CHAR(15),
    NGAYBATDAU DATE,
    NGAYKETTHUC DATE,
    CONSTRAINT FK_KHUYENMAI_SANPHAM FOREIGN KEY (MASP) REFERENCES SANPHAM(MASP),
    CONSTRAINT CK_KHUYENMAI_DATE CHECK (NGAYKETTHUC >= NGAYBATDAU)
)

CREATE TABLE NHACUNGCAP (
    MANCC CHAR(15) PRIMARY KEY,
    TENNCC NVARCHAR(300),
    DIACHI NVARCHAR(500),
    SDT VARCHAR(12),
    EMAIL NVARCHAR(320)
)

CREATE TABLE NHANVIEN
(
	MANV CHAR(15) PRIMARY KEY,
	TENNV NVARCHAR(50),
	SDT CHAR(10),
	DIACHI NVARCHAR(100)
)

CREATE TABLE PHIEUNHAPKHO
(
	MAPHIEUNHAP CHAR(15) PRIMARY KEY,
	MASP CHAR(15) FOREIGN KEY REFERENCES SANPHAM(MASP),
	MANCC CHAR(15) FOREIGN KEY REFERENCES NHACUNGCAP(MANCC),
	MANV CHAR(15) FOREIGN KEY REFERENCES NHANVIEN(MANV),
	NGAYLAP DATETIME,
	THUEVAT FLOAT
)

CREATE TABLE CHITIETNHAP
(
	MAPHIEUNHAP CHAR(15) FOREIGN KEY REFERENCES PHIEUNHAPKHO(MAPHIEUNHAP),
	MASP CHAR(15) FOREIGN KEY REFERENCES SANPHAM(MASP),
	SOLUONG INT,
	DONGIANHAP FLOAT,
	THANHTIEN FLOAT,
	NGAYNHAPKHO DATETIME,
	PRIMARY KEY(MAPHIEUNHAP, MASP)
)

CREATE TABLE TAIKHOAN(
	MATAIKHOAN CHAR(15) PRIMARY KEY,
	USERNAME CHAR(20),
	PASS CHAR(20),
	QUYEN INT
)
GO

-- Refresh token được lưu dưới dạng SHA-256 hash, không lưu token thô.
-- Với database đã tồn tại, chỉ cần chạy riêng khối IF này sau khi có TAIKHOAN.
IF OBJECT_ID(N'dbo.RefreshToken', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RefreshToken
    (
        RefreshTokenHash CHAR(64) NOT NULL CONSTRAINT PK_RefreshToken PRIMARY KEY,
        AccountId CHAR(15) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_RefreshToken_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        ExpiresAtUtc DATETIME2(3) NOT NULL,
        RevokedAtUtc DATETIME2(3) NULL,
        ReplacedByHash CHAR(64) NULL,
        CONSTRAINT FK_RefreshToken_TaiKhoan FOREIGN KEY (AccountId)
            REFERENCES dbo.TAIKHOAN(MATAIKHOAN)
    );

    CREATE INDEX IX_RefreshToken_AccountId_ExpiresAtUtc
        ON dbo.RefreshToken(AccountId, ExpiresAtUtc);
END;
GO

CREATE TABLE KHACHHANG(
	MAKH CHAR(15) PRIMARY KEY,
	TENKH NVARCHAR(100),
	SDT CHAR(10),
	DIACHI NVARCHAR(300)
)
GO

CREATE TABLE HOADONBAN(
	MAHDBAN CHAR(15) PRIMARY KEY,
	MANV CHAR(15) FOREIGN KEY REFERENCES NHANVIEN(MANV),
	MAKH CHAR(15) FOREIGN KEY REFERENCES KHACHHANG(MAKH),
	NGAYLAP DATETIME,
	TONGTIENHANG FLOAT,
	THUEVAT FLOAT,
	GIAMGIA FLOAT
)
GO

	CREATE TABLE CT_HDB(
	MAHDBAN CHAR(15) FOREIGN KEY REFERENCES HOADONBAN(MAHDBAN),
	MASP CHAR(15) FOREIGN KEY REFERENCES SANPHAM(MASP),
	SOLUONG INT,
	DONGIA FLOAT,
	TONGTIEN FLOAT,
	PRIMARY KEY (MAHDBAN, MASP)
)
GO

CREATE TABLE THANHTOAN(
	MATHANHTOAN CHAR(15) PRIMARY KEY,
	MAHDBAN CHAR(15) FOREIGN KEY REFERENCES HOADONBAN(MAHDBAN),
	PHUONGTHUC NVARCHAR(50),
	SOTIENTHANHTOAN FLOAT,
	NGAYTHANHTOAN DATETIME,
	TRANGTHAI NVARCHAR(50)
	)
GO

-- Thông tin đối soát PayOS. Với database đã tồn tại, chỉ chạy riêng khối này.
IF COL_LENGTH(N'dbo.THANHTOAN', N'PAYOS_ORDER_CODE') IS NULL
    ALTER TABLE dbo.THANHTOAN ADD PAYOS_ORDER_CODE BIGINT NULL;
GO
IF COL_LENGTH(N'dbo.THANHTOAN', N'PAYOS_PAYMENT_LINK_ID') IS NULL
    ALTER TABLE dbo.THANHTOAN ADD PAYOS_PAYMENT_LINK_ID NVARCHAR(100) NULL;
GO
IF COL_LENGTH(N'dbo.THANHTOAN', N'PAYOS_CHECKOUT_URL') IS NULL
    ALTER TABLE dbo.THANHTOAN ADD PAYOS_CHECKOUT_URL NVARCHAR(1000) NULL;
GO
IF COL_LENGTH(N'dbo.THANHTOAN', N'PAYOS_QR_CODE') IS NULL
    ALTER TABLE dbo.THANHTOAN ADD PAYOS_QR_CODE NVARCHAR(MAX) NULL;
GO
IF COL_LENGTH(N'dbo.THANHTOAN', N'PAYOS_TRANSACTION_REFERENCE') IS NULL
    ALTER TABLE dbo.THANHTOAN ADD PAYOS_TRANSACTION_REFERENCE NVARCHAR(100) NULL;
GO
IF COL_LENGTH(N'dbo.THANHTOAN', N'PAYOS_CREATED_AT_UTC') IS NULL
    ALTER TABLE dbo.THANHTOAN ADD PAYOS_CREATED_AT_UTC DATETIME2(3) NULL;
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = N'UX_THANHTOAN_PAYOS_ORDER_CODE'
      AND object_id = OBJECT_ID(N'dbo.THANHTOAN')
)
    CREATE UNIQUE INDEX UX_THANHTOAN_PAYOS_ORDER_CODE
        ON dbo.THANHTOAN(PAYOS_ORDER_CODE)
        WHERE PAYOS_ORDER_CODE IS NOT NULL;
GO




    USE QUANLYBANLE
GO

-- XÓA TOÀN BỘ DỮ LIỆU CŨ THEO THỨ TỰ RÀNG BUỘC


-- 1. BẢNG DANHMUC (20 BẢN GHI)
INSERT INTO DANHMUC (MADANHMUC, TENDANHMUC, MOTA) VALUES
('DM001', N'Thiết Bị Nấu Nướng', N'Nồi cơm, bếp từ, nồi chiên không dầu, lò vi sóng'),
('DM002', N'Thiết Bị Xay Ép & Pha Chế', N'Máy xay sinh tố, máy ép chậm, ấm siêu tốc, máy làm sữa hạt'),
('DM003', N'Dụng Cụ Nấu Inox & Chống Dính', N'Chảo chống dính, bộ nồi inox, xửng hấp, chảo vân đá'),
('DM004', N'Dao Kéo & Dụng Cụ Sơ Chế', N'Bộ dao nhà bếp, thớt gỗ, thớt nhựa kháng khuẩn, kéo cắt gà'),
('DM005', N'Đồ Dùng Bàn Ăn & Phòng Bếp', N'Bát đĩa sứ, bộ đũa muỗng inox, khay đựng gia vị'),
('DM006', N'Hộp & Hũ Bảo Quản Thực Phẩm', N'Hộp thuỷ tinh chịu nhiệt, hộp nhựa BPA Free, hũ đựng gia vị'),
('DM007', N'Bình & Ly Đựng Nước', N'Bình giữ nhiệt inox, bình nước thể thao, ly thuỷ tinh chia vạch'),
('DM008', N'Làm Mát & Thông Gió Gia Đình', N'Quạt đứng, quạt bàn, quạt lửng, quạt treo tường'),
('DM009', N'Thiết Bị Chăm Sóc Quần Áo', N'Bàn là hơi nước, bàn là khô, máy sấy quần áo mini'),
('DM010', N'Thiết Bị Vệ Sinh Nhà Cửa', N'Máy hút bụi cầm tay, máy hút bụi giường đệm, robot hút bụi'),
('DM011', N'Dụng Cụ Vệ Sinh Thông Minh', N'Bộ lau nhà 360, chổi gạt nước sàn, cọ rửa chén đa năng'),
('DM012', N'Hoá Phẩm Rửa Bát & Nhà Bếp', N'Nước rửa chén hữu cơ, viên rửa bát chén, nước tẩy dầu mỡ'),
('DM013', N'Hoá Phẩm Giặt Xả & Vải Vóc', N'Nước giặt xả đậm đặc, nước làm mềm vải hương hoa'),
('DM014', N'Hoá Phẩm Tẩy Rửa Sàn & Vệ Sinh', N'Nước lau sàn hương sả chanh, nước tẩy bồn cầu diệt khuẩn'),
('DM015', N'Đồ Dùng Phòng Tắm & Giặt Giũ', N'Kệ để đồ nhà tắm dán tường, giỏ đựng quần áo, thảm lau chân'),
('DM016', N'Vật Dụng Tiện Ích Phòng Khách', N'Đồng hồ treo tường, thùng rác thông minh, móc treo đồ'),
('DM017', N'Đồ Dùng Nhựa Gia Dụng Tiện Lợi', N'Chậu nhựa đa năng, rổ quay rau sống, xô đựng nước'),
('DM018', N'Màng Bọc & Túi Tiện Ích Bếp', N'Màng bọc thực phẩm PE, giấy nến nướng bánh, túi rác tự hủy'),
('DM019', N'Thiết Bị Lọc Nước & Phụ Kiện', N'Bình lọc nước gia đình, lõi lọc nước thô, vòi tăng áp'),
('DM020', N'Sản Phẩm Khử Mùi & Diệt Côn Trùng', N'Xịt phòng tự nhiên, sáp thơm phòng ngủ, bình xịt muỗi thảo mộc');
GO

-- 2. BẢNG SANPHAM (20 BẢN GHI)
INSERT INTO SANPHAM (MASP, TENSP, MAVACH, MOTA, MADANHMUC, DONGIA, THUOCTINH, THUEVAT, SOLUONGTON) VALUES
('SP001', N'Nồi cơm điện Sunhouse 1.8L', '893600100001', N'Lòng nồi chống dính, mâm nhiệt lớn nấu cơm dẻo ngon', 'DM001', 650000, N'Dung tích 1.8L, 700W, Màu đỏ mận', 10, 50),
('SP002', N'Nồi chiên không dầu Lock&Lock 5.2L', '893600100002', N'Công nghệ chiên chân không hạn chế 80% dầu mỡ', 'DM001', 1850000, N'Dung tích 5.2L, 1800W, Vỏ đen nhám', 10, 35),
('SP003', N'Ấm siêu tốc Philips 1.5L HD9306', '893600100003', N'Thân ấm làm từ thép không gỉ SUS304 bền đẹp', 'DM002', 380000, N'Dung tích 1.5L, 1800W, Bạc sáng', 10, 80),
('SP004', N'Máy xay sinh tố đa năng Tefal', '893600100004', N'Cối thủy tinh chịu lực, lưỡi dao 6 cánh sắc bén', 'DM002', 720000, N'Cối 1.5L + cối nhỏ xay thịt, 600W', 10, 40),
('SP005', N'Chảo chống dính đáy từ Tefal 28cm', '893600100005', N'Lớp phủ Titanium bền chắc, báo nhiệt thông minh Thermo-Signal', 'DM003', 460000, N'Đường kính 28cm, Dùng cho mọi loại bếp', 10, 60),
('SP006', N'Bộ 3 nồi inox 5 đáy Kangaroo KG866', '893600100006', N'Bộ 3 kích thước 16-20-24cm tiện dụng, giữ nhiệt lâu', 'DM003', 890000, N'Inox 5 đáy, Nắp vung kính cường lực', 10, 25),
('SP007', N'Bộ dao kéo làm bếp 6 món Lock&Lock', '893600100007', N'Chất liệu thép cacbon cao cấp phủ sứ chống dính', 'DM004', 320000, N'Bộ 5 dao + 1 kéo cắt đa năng, Cán êm tay', 10, 45),
('SP008', N'Thớt gỗ kháng khuẩn Trường Sơn', '893600100008', N'Gỗ nghiến tự nhiên xử lý mối mọt chống cong vênh', 'DM004', 165000, N'Kích thước 30x20x2cm, Gỗ tự nhiên', 10, 70),
('SP009', N'Bộ 10 bát ăn cơm sứ Minh Long', '893600100009', N'Men sứ bóng mịn tráng nano chống bám dầu mỡ', 'DM005', 290000, N'Đường kính 11.5cm, Men trắng viền chỉ vàng', 10, 50),
('SP010', N'Set 3 hộp thủy tinh Lock&Lock chịu nhiệt', '893600100010', N'Thủy tinh Borosilicate dùng an toàn lò nướng và vi sóng', 'DM006', 270000, N'Gồm 3 hộp: 400ml, 650ml, 950ml, Nắp khóa 4 khớp', 10, 90),
('SP011', N'Bình giữ nhiệt Inox Lock&Lock 500ml', '893600100011', N'Ruột bình inox 316 giữ lạnh 24h và giữ nóng 12h', 'DM007', 285000, N'Dung tích 500ml, Màu xám kim loại', 10, 65),
('SP012', N'Quạt đứng Senko chuyển hướng điện', '893600100012', N'Động cơ đồng êm ái, nan lồng đan khít an toàn trẻ nhỏ', 'DM008', 430000, N'Sải cánh 40cm, Đen viền cam thời trang', 10, 40),
('SP013', N'Bàn là hơi nước cầm tay Tefal', '893600100013', N'Làm phẳng quần áo nhanh chóng trên móc treo', 'DM009', 690000, N'Công suất 1400W, Bình nước tháo rời 200ml', 10, 30),
('SP014', N'Máy hút bụi cầm tay Deerma DX700', '893600100014', N'Lực hút xoáy 15000Pa hút sạch bụi mịn và tóc', 'DM010', 580000, N'Bộ lọc HEPA 3 lớp, Dây nguồn dài 4.5m', 10, 35),
('SP015', N'Bộ lau nhà 360 độ lồng inox Lock&Lock', '893600100015', N'Thùng vắt inox trợ lực nhẹ nhàng, kèm 2 bông lau cotton', 'DM011', 390000, N'Bông lau sợi microfiber xoay 360 độ', 10, 55),
('SP016', N'Nước rửa chén Sunlight Thiên Nhiên 3.6kg', '893600100016', N'Chiết xuất lô hội và muối khoáng an toàn da tay', 'DM012', 125000, N'Can lớn 3.6kg, Hương bưởi lô hội', 8, 120),
('SP017', N'Nước giặt xả OMO Matic cửa trên 3.6kg', '893600100017', N'Xoáy bay vết bẩn cứng đầu, lưu hương tinh dầu hoa', 'DM013', 195000, N'Túi 3.6kg nắp vặn tiện lợi', 8, 110),
('SP018', N'Nước lau sàn Sunlight Tinh Dầu Quế 3.6kg', '893600100018', N'Sạch bóng sàn nhà, xua đuổi côn trùng hiệu quả', 'DM014', 98000, N'Can 3.6kg, Hương quế ấm nồng', 8, 130),
('SP019', N'Kệ góc nhà tắm dán tường inox 304', '893600100019', N'Keo dán siêu dính chịu tải 10kg, không cần khoan đục', 'DM015', 75000, N'Inox 304 không gỉ sét, Kích thước 22x22cm', 10, 85),
('SP020', N'Thùng rác thông minh đạp chân 12L', '893600100020', N'Nắp đóng giảm chấn êm ái, chống mùi hôi bay ra ngoài', 'DM016', 210000, N'Vỏ inox xước mờ sang trọng, Dung tích 12L', 10, 50);
GO

-- 3. BẢNG NHACUNGCAP (20 BẢN GHI)
INSERT INTO NHACUNGCAP (MANCC, TENNCC, DIACHI, SDT, EMAIL) VALUES
('NCC001', N'Công ty Cổ phần Tập đoàn Sunhouse', N'Cụm CN Ngọc Hồi, Thanh Trì, Hà Nội', '02437366666', 'contact@sunhouse.com.vn'),
('NCC002', N'Công ty TNHH Lock&Lock Việt Nam', N'Tầng 9, Bitexco, Bến Nghé, Quận 1, TP. HCM', '02854135756', 'sales@locknlock.com'),
('NCC003', N'Công ty TNHH Senko Electric Việt Nam', N'KCN Tân Tạo, Tân Tạo A, Bình Tân, TP. HCM', '02838770123', 'cskh@senko.vn'),
('NCC004', N'Tập đoàn Điện gia dụng Kangaroo', N'Tòa nhà Ocean Park, Cầu Giấy, Hà Nội', '02436281699', 'info@kangaroo.vn'),
('NCC005', N'Công ty TNHH Đồ Dùng Nhà Bếp Tefal VN', N'Nguyễn Cơ Thạch, An Lợi Đông, TP. Thủ Đức', '02839978899', 'support@tefal.vn'),
('NCC006', N'Công ty TNHH Philips Electronics VN', N'Tầng 12, Tòa nhà Kumho, Bến Nghé, Quận 1', '02838247000', 'service@philips.com.vn'),
('NCC007', N'Tổng Công ty Gốm Sứ Minh Long I', N'Đại lộ Bình Dương, Thuận An, Bình Dương', '02743668899', 'sales@minhlong.com'),
('NCC008', N'Công ty TNHH Unilever Việt Nam', N'KCN Tây Bắc Củ Chi, Củ Chi, TP. HCM', '02854135600', 'unilever.cskh@unilever.com'),
('NCC009', N'Công ty CP Bột Giặt LIX', N'Số 3, Đường số 2, Linh Trung, TP. Thủ Đức', '02838966803', 'lixco@lixco.com'),
('NCC010', N'Công ty Nhựa Gia Dụng Duy Tân', N'298 Hồ Học Lãm, An Lạc, Bình Tân, TP. HCM', '02838762222', 'info@duytan.com'),
('NCC011', N'Công ty TNHH Sản Xuất Đồ Gỗ Trường Sơn', N'Thạch Thất, Hà Nội', '02433842111', 'truongsongovn@gmail.com'),
('NCC012', N'Công ty Thiết Bị Gia Dụng Elmich VN', N'Cầu Giấy, Hà Nội', '02435134567', 'cskh@elmich.vn'),
('NCC013', N'Công ty TNHH Deerma SmartHome VN', N'Tân Bình, TP. HCM', '0903123456', 'deerma.vn@gmail.com'),
('NCC014', N'Công ty Gia Dụng Inox Tiến Thọ', N'Hoàng Mai, Hà Nội', '0912445566', 'inox.tientho@gmail.com'),
('NCC015', N'Công ty CP Mỹ Hảo Hóa Phẩm', N'Bình Chánh, TP. HCM', '02838753288', 'myhao@myhao.com.vn'),
('NCC016', N'Công ty Thiết Bị Nhà Bếp Bluestone', N'Quận 7, TP. HCM', '02854129999', 'info@bluestone.com.vn'),
('NCC017', N'Công ty TNHH Đồ Dùng Tiện Ích Mori', N'Nam Từ Liêm, Hà Nội', '0983112233', 'morivietnam@gmail.com'),
('NCC018', N'Công ty Nhựa Hiệp Thành', N'Quận 6, TP. HCM', '02838556677', 'sales@hiepthanhplastic.com'),
('NCC019', N'Công ty Hóa Mỹ Phẩm Vì Dân', N'Hải Phòng', '02253888999', 'vidanco@vico.vn'),
('NCC020', N'Công ty Cổ phần Thủy Tinh Phú Sơn', N'Gia Lâm, Hà Nội', '02438765432', 'phuson.glass@gmail.com');
GO

-- 4. BẢNG NHANVIEN (20 BẢN GHI)
INSERT INTO NHANVIEN (MANV, TENNV, SDT, DIACHI) VALUES
('NV001', N'Trần Thu Hà', '0912345601', N'Cầu Giấy, Hà Nội'),
('NV002', N'Lê Minh Hoàng', '0912345602', N'Đống Đa, Hà Nội'),
('NV003', N'Vũ Thị Lan', '0912345603', N'Hai Bà Trưng, Hà Nội'),
('NV004', N'Nguyễn Tuấn Anh', '0912345604', N'Thanh Xuân, Hà Nội'),
('NV005', N'Phạm Phương Thảo', '0912345605', N'Ba Đình, Hà Nội'),
('NV006', N'Đỗ Quốc Bảo', '0912345606', N'Hà Đông, Hà Nội'),
('NV007', N'Hoàng Yến Nhi', '0912345607', N'Bắc Từ Liêm, Hà Nội'),
('NV008', N'Bùi Đức Thắng', '0912345608', N'Hoàng Mai, Hà Nội'),
('NV009', N'Đinh Thùy Linh', '0912345609', N'Long Biên, Hà Nội'),
('NV010', N'Ngô Văn Nam', '0912345610', N'Tây Hồ, Hà Nội'),
('NV011', N'Lê Thị Hồng', '0912345611', N'Hoàn Kiếm, Hà Nội'),
('NV012', N'Trịnh Quang Hùng', '0912345612', N'Nam Từ Liêm, Hà Nội'),
('NV013', N'Võ Mỹ Duyên', '0912345613', N'Gia Lâm, Hà Nội'),
('NV014', N'Lý Minh Khôi', '0912345614', N'Thanh Trì, Hà Nội'),
('NV015', N'Dương Ánh Nguyệt', '0912345615', N'Hoài Đức, Hà Nội'),
('NV016', N'Phan Thanh Tùng', '0912345616', N'Cầu Giấy, Hà Nội'),
('NV017', N'Mai Trúc Quỳnh', '0912345617', N'Thanh Xuân, Hà Nội'),
('NV018', N'Tạ Quang Huy', '0912345618', N'Đống Đa, Hà Nội'),
('NV019', N'Cao Bích Ngọc', '0912345619', N'Ba Đình, Hà Nội'),
('NV020', N'Đoàn Văn Hậu', '0912345620', N'Hà Đông, Hà Nội');
GO

-- 5. BẢNG TAIKHOAN (20 BẢN GHI)
INSERT INTO TAIKHOAN (MATAIKHOAN, USERNAME, PASS, QUYEN) VALUES
('TK001', 'admin', 'admin123', 1),
('TK002', 'nv_ha', 'pass002', 2),
('TK003', 'nv_hoang', 'pass003', 2),
('TK004', 'nv_lan', 'pass004', 2),
('TK005', 'nv_tuananh', 'pass005', 2),
('TK006', 'nv_thao', 'pass006', 2),
('TK007', 'nv_bao', 'pass007', 2),
('TK008', 'nv_nhi', 'pass008', 2),
('TK009', 'nv_thang', 'pass009', 2),
('TK010', 'nv_linh', 'pass010', 2),
('TK011', 'nv_nam', 'pass011', 2),
('TK012', 'nv_hong', 'pass012', 2),
('TK013', 'nv_hung', 'pass013', 2),
('TK014', 'nv_duyen', 'pass014', 2),
('TK015', 'nv_khoi', 'pass015', 2),
('TK016', 'nv_nguyet', 'pass016', 2),
('TK017', 'nv_tung', 'pass017', 2),
('TK018', 'nv_quynh', 'pass018', 2),
('TK019', 'nv_huy', 'pass019', 2),
('TK020', 'nv_ngoc', 'pass020', 2);
GO

-- 6. BẢNG KHACHHANG (20 BẢN GHI)
INSERT INTO KHACHHANG (MAKH, TENKH, SDT, DIACHI) VALUES
('KH001', N'Nguyễn Hồng Hạnh', '0981112201', N'Nguyễn Trãi, Thanh Xuân, Hà Nội'),
('KH002', N'Phạm Quang Dũng', '0981112202', N'Kim Mã, Ba Đình, Hà Nội'),
('KH003', N'Đỗ Mai Phương', '0981112203', N'Lạc Long Quân, Tây Hồ, Hà Nội'),
('KH004', N'Trần Đình Trọng', '0981112204', N'Trần Phú, Hà Đông, Hà Nội'),
('KH005', N'Lê Tuyết Mai', '0981112205', N'Giải Phóng, Hoàng Mai, Hà Nội'),
('KH006', N'Vũ Hoàng Anh', '0981112206', N'Xuân Thủy, Cầu Giấy, Hà Nội'),
('KH007', N'Ngô Bảo Châu', '0981112207', N'Chùa Bộc, Đống Đa, Hà Nội'),
('KH008', N'Bùi Bích Phương', '0981112208', N'Nguyễn Văn Cừ, Long Biên, Hà Nội'),
('KH009', N'Đinh Tiến Dũng', '0981112209', N'Hoàng Hoa Thám, Ba Đình, Hà Nội'),
('KH010', N'Phạm Hương Tràm', '0981112210', N'Lê Văn Lương, Cầu Giấy, Hà Nội'),
('KH011', N'Hoàng Hải Yến', '0981112211', N'Minh Khai, Hai Bà Trưng, Hà Nội'),
('KH012', N'Đặng Văn Lâm', '0981112212', N'Đại Cồ Việt, Hai Bà Trưng, Hà Nội'),
('KH013', N'Trịnh Thăng Bình', '0981112213', N'Võ Chí Công, Tây Hồ, Hà Nội'),
('KH014', N'Phan Mạnh Quỳnh', '0981112214', N'Hồ Tùng Mậu, Cầu Giấy, Hà Nội'),
('KH015', N'Võ Hoàng Yến', '0981112215', N'Ngọc Hồi, Thanh Trì, Hà Nội'),
('KH016', N'Dương Triệu Vũ', '0981112216', N'Nguyễn Chí Thanh, Đống Đa, Hà Nội'),
('KH017', N'Tạ Bích Loan', '0981112217', N'Trần Duy Hưng, Cầu Giấy, Hà Nội'),
('KH018', N'Nguyễn Quang Hải', '0981112218', N'Phố Huế, Hai Bà Trưng, Hà Nội'),
('KH019', N'Đoàn Triệu Lương', '0981112219', N'Đội Cấn, Ba Đình, Hà Nội'),
('KH020', N'Lưu Hương Giang', '0981112220', N'Thái Hà, Đống Đa, Hà Nội');
GO

-- 7. BẢNG KHUYENMAI (20 BẢN GHI)
INSERT INTO KHUYENMAI (MAKM, TENKM, MASP, NGAYBATDAU, NGAYKETTHUC) VALUES
('KM001', N'Tuần Lễ Vàng Nồi Cơm Sunhouse', 'SP001', '2026-10-01', '2026-10-15'),
('KM002', N'Ăn Khỏe Sống Xanh Cùng Lock&Lock', 'SP002', '2026-10-01', '2026-10-10'),
('KM003', N'Ưu Đãi Ấm Siêu Tốc Philips', 'SP003', '2026-10-05', '2026-10-20'),
('KM004', N'Rộn Ràng Sinh Tố Tefal Mùa Thu', 'SP004', '2026-10-01', '2026-10-25'),
('KM005', N'Chảo Chống Dính Giá Sốc', 'SP005', '2026-10-10', '2026-10-31'),
('KM006', N'Bếp Ấm Gia Đình Bộ Nồi Kangaroo', 'SP006', '2026-10-05', '2026-10-15'),
('KM007', N'Trọn Bộ Dao Bếp Tiết Kiệm', 'SP007', '2026-10-01', '2026-10-31'),
('KM008', N'Mua Thớt Xịn Tặng Quà Nhỏ', 'SP008', '2026-10-15', '2026-10-30'),
('KM009', N'Tôn Vinh Bữa Cơm Bát Đĩa Minh Long', 'SP009', '2026-10-01', '2026-10-20'),
('KM010', N'Combo Hộp Thuỷ Tinh Lock&Lock', 'SP010', '2026-10-08', '2026-10-22'),
('KM011', N'Bình Giữ Nhiệt Phong Cách Trẻ', 'SP011', '2026-10-01', '2026-10-31'),
('KM012', N'Thổi Bay Oi Nóng Quạt Senko', 'SP012', '2026-10-01', '2026-10-15'),
('KM013', N'Áo Đẹp Tinh Tươm Cùng Bàn Là Tefal', 'SP013', '2026-10-12', '2026-10-28'),
('KM014', N'Nhà Sạch Bụi Mịn Máy Hút Deerma', 'SP014', '2026-10-05', '2026-10-20'),
('KM015', N'Lau Nhà Rảnh Tay Lock&Lock 360', 'SP015', '2026-10-01', '2026-10-15'),
('KM016', N'Can Lớn Siêu Rẻ Rửa Chén Sunlight', 'SP016', '2026-10-01', '2026-10-31'),
('KM017', N'Sạch Tinh Tươm Cùng Nước Giặt OMO', 'SP017', '2026-10-10', '2026-10-30'),
('KM018', N'Hương Quế Ấm Nồng Sàn Nhà Sunlight', 'SP018', '2026-10-01', '2026-10-31'),
('KM019', N'Tiện Nghi Phòng Tắm Kệ Inox', 'SP019', '2026-10-05', '2026-10-25'),
('KM020', N'Nhà Đẹp Sang Trọng Thùng Rác Inox', 'SP020', '2026-10-01', '2026-10-31');
GO

-- 8. BẢNG PHIEUNHAPKHO (20 BẢN GHI)
INSERT INTO PHIEUNHAPKHO (MAPHIEUNHAP, MASP, MANCC, MANV, NGAYLAP, THUEVAT) VALUES
('PN001', 'SP001', 'NCC001', 'NV001', '2026-09-01 08:00:00', 10),
('PN002', 'SP002', 'NCC002', 'NV002', '2026-09-02 08:30:00', 10),
('PN003', 'SP003', 'NCC006', 'NV003', '2026-09-03 09:00:00', 10),
('PN004', 'SP004', 'NCC005', 'NV004', '2026-09-04 09:30:00', 10),
('PN005', 'SP005', 'NCC005', 'NV005', '2026-09-05 10:00:00', 10),
('PN006', 'SP006', 'NCC004', 'NV006', '2026-09-06 10:30:00', 10),
('PN007', 'SP007', 'NCC002', 'NV007', '2026-09-07 11:00:00', 10),
('PN008', 'SP008', 'NCC011', 'NV008', '2026-09-08 13:30:00', 10),
('PN009', 'SP009', 'NCC007', 'NV009', '2026-09-09 14:00:00', 10),
('PN010', 'SP010', 'NCC002', 'NV010', '2026-09-10 14:30:00', 10),
('PN011', 'SP011', 'NCC002', 'NV011', '2026-09-11 15:00:00', 10),
('PN012', 'SP012', 'NCC003', 'NV012', '2026-09-12 15:30:00', 10),
('PN013', 'SP013', 'NCC005', 'NV013', '2026-09-13 16:00:00', 10),
('PN014', 'SP014', 'NCC013', 'NV014', '2026-09-14 08:30:00', 10),
('PN015', 'SP015', 'NCC002', 'NV015', '2026-09-15 09:00:00', 10),
('PN016', 'SP016', 'NCC008', 'NV016', '2026-09-16 09:30:00', 8),
('PN017', 'SP017', 'NCC008', 'NV017', '2026-09-17 10:00:00', 8),
('PN018', 'SP018', 'NCC008', 'NV018', '2026-09-18 10:30:00', 8),
('PN019', 'SP019', 'NCC014', 'NV019', '2026-09-19 11:00:00', 10),
('PN020', 'SP020', 'NCC010', 'NV020', '2026-09-20 14:00:00', 10);
GO

-- 9. BẢNG CHITIETNHAP (20 BẢN GHI)
INSERT INTO CHITIETNHAP (MAPHIEUNHAP, MASP, SOLUONG, DONGIANHAP, THANHTIEN, NGAYNHAPKHO) VALUES
('PN001', 'SP001', 50, 480000, 24000000, '2026-09-01 09:00:00'),
('PN002', 'SP002', 35, 1400000, 49000000, '2026-09-02 09:30:00'),
('PN003', 'SP003', 80, 280000, 22400000, '2026-09-03 10:00:00'),
('PN004', 'SP004', 40, 550000, 22000000, '2026-09-04 10:30:00'),
('PN005', 'SP005', 60, 340000, 20400000, '2026-09-05 11:00:00'),
('PN006', 'SP006', 25, 680000, 17000000, '2026-09-06 11:30:00'),
('PN007', 'SP007', 45, 230000, 10350000, '2026-09-07 13:30:00'),
('PN008', 'SP008', 70, 110000, 7700000, '2026-09-08 14:00:00'),
('PN009', 'SP009', 50, 200000, 10000000, '2026-09-09 14:30:00'),
('PN010', 'SP010', 90, 190000, 17100000, '2026-09-10 15:00:00'),
('PN011', 'SP011', 65, 200000, 13000000, '2026-09-11 15:30:00'),
('PN012', 'SP012', 40, 310000, 12400000, '2026-09-12 16:00:00'),
('PN013', 'SP013', 30, 500000, 15000000, '2026-09-13 16:30:00'),
('PN014', 'SP014', 35, 420000, 14700000, '2026-09-14 09:30:00'),
('PN015', 'SP015', 55, 270000, 14850000, '2026-09-15 10:00:00'),
('PN016', 'SP016', 120, 95000, 11400000, '2026-09-16 10:30:00'),
('PN017', 'SP017', 110, 150000, 16500000, '2026-09-17 11:00:00'),
('PN018', 'SP018', 130, 72000, 9360000, '2026-09-18 11:30:00'),
('PN019', 'SP019', 85, 50000, 4250000, '2026-09-19 13:30:00'),
('PN020', 'SP020', 50, 150000, 7500000, '2026-09-20 15:00:00');
GO

-- 10. BẢNG HOADONBAN (20 BẢN GHI)
-- TONGTIENHANG = Giá sản phẩm + Thuế VAT - Giảm giá
INSERT INTO HOADONBAN (MAHDBAN, MANV, MAKH, NGAYLAP, TONGTIENHANG, THUEVAT, GIAMGIA) VALUES
('HD001', 'NV001', 'KH001', '2026-09-25 08:30:00', 715000, 65000, 0),
('HD002', 'NV002', 'KH002', '2026-09-25 09:15:00', 1985000, 185000, 50000),
('HD003', 'NV003', 'KH003', '2026-09-25 10:00:00', 418000, 38000, 0),
('HD004', 'NV004', 'KH004', '2026-09-25 11:00:00', 762000, 72000, 30000),
('HD005', 'NV005', 'KH005', '2026-09-25 14:30:00', 506000, 46000, 0),
('HD006', 'NV006', 'KH006', '2026-09-25 15:20:00', 929000, 89000, 50000),
('HD007', 'NV007', 'KH007', '2026-09-25 16:45:00', 352000, 32000, 0),
('HD008', 'NV008', 'KH008', '2026-09-26 08:45:00', 181500, 16500, 0),
('HD009', 'NV009', 'KH009', '2026-09-26 09:30:00', 319000, 29000, 0),
('HD010', 'NV010', 'KH010', '2026-09-26 10:15:00', 277000, 27000, 20000),
('HD011', 'NV011', 'KH011', '2026-09-26 11:30:00', 313500, 28500, 0),
('HD012', 'NV012', 'KH012', '2026-09-26 14:00:00', 443000, 43000, 30000),
('HD013', 'NV013', 'KH013', '2026-09-26 15:10:00', 709000, 69000, 50000),
('HD014', 'NV014', 'KH014', '2026-09-26 16:20:00', 638000, 58000, 0),
('HD015', 'NV015', 'KH015', '2026-09-27 08:30:00', 399000, 39000, 30000),
('HD016', 'NV016', 'KH016', '2026-09-27 09:40:00', 135000, 10000, 0),
('HD017', 'NV017', 'KH017', '2026-09-27 10:50:00', 190600, 15600, 20000),
('HD018', 'NV018', 'KH018', '2026-09-27 14:15:00', 105840, 7840, 0),
('HD019', 'NV019', 'KH019', '2026-09-27 15:30:00', 82500, 7500, 0),
('HD020', 'NV020', 'KH020', '2026-09-27 17:00:00', 231000, 21000, 0);
GO

-- 11. BẢNG CT_HDB (20 BẢN GHI)
INSERT INTO CT_HDB (MAHDBAN, MASP, SOLUONG, DONGIA, TONGTIEN) VALUES
('HD001', 'SP001', 1, 650000, 650000),
('HD002', 'SP002', 1, 1850000, 1850000),
('HD003', 'SP003', 1, 380000, 380000),
('HD004', 'SP004', 1, 720000, 720000),
('HD005', 'SP005', 1, 460000, 460000),
('HD006', 'SP006', 1, 890000, 890000),
('HD007', 'SP007', 1, 320000, 320000),
('HD008', 'SP008', 1, 165000, 165000),
('HD009', 'SP009', 1, 290000, 290000),
('HD010', 'SP010', 1, 270000, 270000),
('HD011', 'SP011', 1, 285000, 285000),
('HD012', 'SP012', 1, 430000, 430000),
('HD013', 'SP013', 1, 690000, 690000),
('HD014', 'SP014', 1, 580000, 580000),
('HD015', 'SP015', 1, 390000, 390000),
('HD016', 'SP016', 1, 125000, 125000),
('HD017', 'SP017', 1, 195000, 195000),
('HD018', 'SP018', 1, 98000, 98000),
('HD019', 'SP019', 1, 75000, 75000),
('HD020', 'SP020', 1, 210000, 210000);
GO

-- 12. BẢNG THANHTOAN (20 BẢN GHI)
INSERT INTO THANHTOAN (MATHANHTOAN, MAHDBAN, PHUONGTHUC, SOTIENTHANHTOAN, NGAYTHANHTOAN, TRANGTHAI) VALUES
('TT001', 'HD001', N'Tiền mặt', 715000, '2026-09-25 08:35:00', N'Đã thanh toán'),
('TT002', 'HD002', N'Chuyển khoản QR', 1985000, '2026-09-25 09:20:00', N'Đã thanh toán'),
('TT003', 'HD003', N'Quẹt thẻ POS', 418000, '2026-09-25 10:05:00', N'Đã thanh toán'),
('TT004', 'HD004', N'Chuyển khoản QR', 762000, '2026-09-25 11:05:00', N'Đã thanh toán'),
('TT005', 'HD005', N'Tiền mặt', 506000, '2026-09-25 14:35:00', N'Đã thanh toán'),
('TT006', 'HD006', N'Chuyển khoản QR', 929000, '2026-09-25 15:25:00', N'Đã thanh toán'),
('TT007', 'HD007', N'Quẹt thẻ POS', 352000, '2026-09-25 16:50:00', N'Đã thanh toán'),
('TT008', 'HD008', N'Tiền mặt', 181500, '2026-09-26 08:50:00', N'Đã thanh toán'),
('TT009', 'HD009', N'Chuyển khoản QR', 319000, '2026-09-26 09:35:00', N'Đã thanh toán'),
('TT010', 'HD010', N'Tiền mặt', 277000, '2026-09-26 10:20:00', N'Đã thanh toán'),
('TT011', 'HD011', N'Quẹt thẻ POS', 313500, '2026-09-26 11:35:00', N'Đã thanh toán'),
('TT012', 'HD012', N'Chuyển khoản QR', 443000, '2026-09-26 14:05:00', N'Đã thanh toán'),
('TT013', 'HD013', N'Tiền mặt', 709000, '2026-09-26 15:15:00', N'Đã thanh toán'),
('TT014', 'HD014', N'Chuyển khoản QR', 638000, '2026-09-26 16:25:00', N'Đã thanh toán'),
('TT015', 'HD015', N'Tiền mặt', 399000, '2026-09-27 08:35:00', N'Đã thanh toán'),
('TT016', 'HD016', N'Chuyển khoản QR', 135000, '2026-09-27 09:45:00', N'Đã thanh toán'),
('TT017', 'HD017', N'Quẹt thẻ POS', 190600, '2026-09-27 10:55:00', N'Đã thanh toán'),
('TT018', 'HD018', N'Tiền mặt', 105840, '2026-09-27 14:20:00', N'Đã thanh toán'),
('TT019', 'HD019', N'Chuyển khoản QR', 82500, '2026-09-27 15:35:00', N'Đã thanh toán'),
('TT020', 'HD020', N'Tiền mặt', 231000, '2026-09-27 17:05:00', N'Đã thanh toán');
GO




SELECT * FROM DANHMUC;
SELECT * FROM SANPHAM;
SELECT * FROM KHUYENMAI;
SELECT * FROM NHACUNGCAP;
SELECT * FROM NHANVIEN;
SELECT * FROM PHIEUNHAPKHO;
SELECT * FROM CHITIETNHAP;
SELECT * FROM TAIKHOAN;
SELECT * FROM KHACHHANG;
SELECT * FROM HOADONBAN;
SELECT * FROM CT_HDB;
SELECT * FROM THANHTOAN;

USE QUANLYBANLE;
GO

USE QUANLYBANLE;
GO
---------------------------------------------------------

CREATE PROCEDURE dbo.sp_GetKhachHang
AS
BEGIN
    SET NOCOUNT ON;

    SELECT   MAKH, TENKH, SDT, DIACHI
    FROM dbo.KHACHHANG;
END
GO
-------------------------------------------------------------

CREATE PROCEDURE dbo.sp_GetThanhToan
AS
BEGIN
    SET NOCOUNT ON;

    SELECT  MATHANHTOAN,  MAHDBAN, PHUONGTHUC,   SOTIENTHANHTOAN, NGAYTHANHTOAN,TRANGTHAI
    FROM dbo.THANHTOAN;
END
GO
-------------------------------------------------------------
CREATE  PROCEDURE dbo.sp_GetNhaCungCap
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MANCC,    TENNCC,    DIACHI,    SDT,  EMAIL
    FROM dbo.NHACUNGCAP;
END
GO

-------------------------------------------------------------------


CREATE  PROCEDURE dbo.sp_GetByIdNhaCungCap
    @MANCC CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MANCC, TENNCC, DIACHI,SDT,   EMAIL
    FROM dbo.NHACUNGCAP
    WHERE RTRIM(MANCC) = RTRIM(@MANCC);
END
GO
----------------------------------------------
CREATE PROCEDURE dbo.SP_SUATT
    @MaThanhToan CHAR(15),
    @PhuongThuc NVARCHAR(50),
    @TrangThai NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.THANHTOAN
        WHERE RTRIM(MATHANHTOAN) = RTRIM(@MaThanhToan)
    )
    BEGIN
        RETURN -1;
    END;

    UPDATE dbo.THANHTOAN
    SET
        PHUONGTHUC = @PhuongThuc,
        TRANGTHAI = @TrangThai
    WHERE RTRIM(MATHANHTOAN) = RTRIM(@MaThanhToan);

    RETURN 0;
END
GO
-------------------------------------

CREATE OR ALTER PROCEDURE dbo.sp_GetByIdThanhToan
    @MATHANHTOAN CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MATHANHTOAN,   MAHDBAN,     PHUONGTHUC,   SOTIENTHANHTOAN,     NGAYTHANHTOAN,    TRANGTHAI
    FROM dbo.THANHTOAN
    WHERE RTRIM(MATHANHTOAN) = RTRIM(@MATHANHTOAN);
END
GO






SELECT
    H.MAHDBAN,
    H.MANV,
    H.MAKH,
    H.NGAYLAP,
    H.TONGTIENHANG,
    H.THUEVAT,
    H.GIAMGIA,
    T.MATHANHTOAN,
    T.PHUONGTHUC,
    T.SOTIENTHANHTOAN,
    T.NGAYTHANHTOAN,
    T.TRANGTHAI
FROM dbo.HOADONBAN H
LEFT JOIN dbo.THANHTOAN T
    ON RTRIM(H.MAHDBAN) = RTRIM(T.MAHDBAN)
WHERE
    T.MAHDBAN IS NULL
    OR T.TRANGTHAI <> N'Đã thanh toán';
--------------------------------------------------------
USE QUANLYBANLE;
GO

CREATE  PROCEDURE dbo.sp_GetByIDKhachHang
    @MAKH CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        MAKH,
        TENKH,
        SDT,
        DIACHI
    FROM dbo.KHACHHANG
    WHERE RTRIM(MAKH) = RTRIM(@MAKH);
END;
GO
EXEC dbo.sp_GetByIDKhachHang @MAKH = 'KH001';


--------------------------------------------------------

 SELECT
    H.MAHDBAN,
    T.MATHANHTOAN,
    T.TRANGTHAI,
    T.PHUONGTHUC
FROM dbo.HOADONBAN H
LEFT JOIN dbo.THANHTOAN T
    ON RTRIM(H.MAHDBAN) = RTRIM(T.MAHDBAN)
WHERE RTRIM(H.MAHDBAN) = 'HD001';
---------------------------------------------------------------

CREATE PROCEDURE dbo.SP_CAPNHAT_TRANGTHAI_THANHTOANTHANHCONG
    @MAHDBAN CHAR(15),
    @PHUONGTHUC NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.HOADONBAN
        WHERE RTRIM(MAHDBAN) = RTRIM(@MAHDBAN)
    )
    BEGIN
        RETURN -1;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.THANHTOAN
        WHERE RTRIM(MAHDBAN) = RTRIM(@MAHDBAN)
    )
    BEGIN
        RETURN -2;
    END;

    UPDATE dbo.THANHTOAN
    SET
        TRANGTHAI = N'Đã thanh toán',
        PHUONGTHUC = @PHUONGTHUC,
        SOTIENTHANHTOAN =
        (
            SELECT TOP 1
                ISNULL(TONGTIENHANG, 0)
                + ISNULL(THUEVAT, 0)
                - ISNULL(GIAMGIA, 0)
            FROM dbo.HOADONBAN
            WHERE RTRIM(MAHDBAN) = RTRIM(@MAHDBAN)
        ),
        NGAYTHANHTOAN = GETDATE()
    WHERE RTRIM(MAHDBAN) = RTRIM(@MAHDBAN);

    RETURN 0;
END;
GO
﻿-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetKhachHang
AS
BEGIN
    SELECT MaKH, TenKH, SDT, DiaChi
    FROM KHACHHANG
    WHERE 
        MaKH IS NOT NULL AND MaKH <> ''
END
EXEC sp_GetKhachHang
-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROC sp_GetByIDKhachHang(@MAKH CHAR(15))
AS
BEGIN
    SELECT MaKH, TenKH, SDT, DiaChi
    FROM KHACHHANG
    WHERE 
        MaKH = @MaKH
END
EXEC sp_GetByIDKhachHang'KH006'
-------------------------------------------------------------------------------------------------
--XÓA THÔNG TIN CỦA KH CÓ MÃ BẤT KỲ
CREATE OR ALTER PROC SP_XOAKH(@MAKH CHAR(15))
AS
BEGIN
	IF(NOT EXISTS(SELECT * FROM KHACHHANG K WHERE K.MAKH = @MAKH)) 
		RETURN -1
	IF(EXISTS(SELECT * FROM KHACHHANG K WHERE K.MAKH = @MAKH))
	DELETE FROM KHACHHANG
	WHERE MAKH = @MAKH
END
EXEC SP_XOAKH'SV100'--KHÔNG XÓA ĐƯỢC
EXEC SP_XOAKH'KH005'--XÓA ĐƯỢC
-------------------------------------------------------------------------------------------------
--THÊM THÔNG TIN CỦA KH
CREATE OR ALTER PROC SP_THEMKH
	@MAKH CHAR(15),
	@TENKH NVARCHAR(100),
	@SDT CHAR(10),
	@DIACHI NVARCHAR(300)
AS
BEGIN
	IF(EXISTS(SELECT * FROM KHACHHANG K WHERE K.MAKH = @MAKH)) 
		RETURN -1
	IF(NOT EXISTS(SELECT * FROM KHACHHANG K WHERE K.MAKH = @MAKH))
		INSERT INTO KHACHHANG VALUES (@MAKH, @TENKH, @SDT, @DIACHI)
END
EXEC SP_THEMKH'KH006', N'Đỗ Tiến Đạt', '0905555555', N'Hưng Yên'
EXEC SP_THEMKH'KH007', N'Đỗ Hữu Quốc Anh', '0905555555', N'Hưng Yên'
-------------------------------------------------------------------------------------------------
--SỬA THÔNG TIN CỦA KH CÓ MÃ BẤT KỲ
CREATE OR ALTER PROC SP_SUAKH(@MAKH CHAR(15),
								@TENKH NVARCHAR(100),
								@SDT CHAR(10),
								@DIACHI NVARCHAR(300))
AS
BEGIN
	IF(NOT EXISTS(SELECT * FROM KHACHHANG K WHERE K.MAKH = @MAKH)) 
		RETURN -1
	IF(EXISTS(SELECT * FROM KHACHHANG K WHERE K.MAKH = @MAKH))
	UPDATE KHACHHANG SET TENKH = @TENKH, SDT = @SDT, DIACHI = @DIACHI 
	WHERE MAKH = @MAKH
END
EXEC SP_SUAKH'KH006', N'Đỗ Hữu Quốc Anh', '0905555553', N'Hà Nội'
EXEC SP_SUAKH'KH007', N'Đỗ Tiến Đạt', '0905555554', N'Thái Nguyên'
-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_LAY_HOADON_CHUA_THANHTOAN_THEO_TENKH
    @TENKH NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT   HDB.MAHDBAN,  HDB.MAKH,   KH.TENKH,  KH.SDT,   HDB.NGAYLAP,  HDB.TONGTIENHANG,   HDB.THUEVAT,  HDB.GIAMGIA, 
    TT.MATHANHTOAN,   TT.PHUONGTHUC,   TT.SOTIENTHANHTOAN,    TT.NGAYTHANHTOAN,    TT.TRANGTHAI
    FROM dbo.HOADONBAN HDB

    INNER JOIN dbo.KHACHHANG KH
        ON HDB.MAKH = KH.MAKH

    LEFT JOIN dbo.THANHTOAN TT
        ON HDB.MAHDBAN = TT.MAHDBAN

    WHERE KH.TENKH LIKE N'%' + @TENKH + '%'
      AND (
            TT.MAHDBAN IS NULL
            OR TT.TRANGTHAI IS NULL
            OR TT.TRANGTHAI <> N'Đã thanh toán'
          )

    ORDER BY HDB.NGAYLAP DESC;
END
GO
-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetNhaCungCap
AS
BEGIN
    SELECT MANCC, TENNCC, DIACHI, SDT, EMAIL
    FROM NHACUNGCAP
    WHERE 
        MANCC IS NOT NULL AND MANCC <> ''
END
EXEC sp_GetNhaCungCap
-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetByIDNhaCungCap(@MANCC CHAR(15))
AS
BEGIN
    SELECT MaNCC, TENNCC, DiaChi, SDT, EMAIL
    FROM NHACUNGCAP
    WHERE 
        MANCC = @MANCC
END
EXEC sp_GetByIDNhaCungCap'NCC001'
-------------------------------------------------------------------------------------------------
--XÓA THÔNG TIN CỦA NCC CÓ MÃ BẤT KỲ
USE [QUANLYBANLE]
GO

CREATE  PROCEDURE [dbo].[SP_XOANCC]
    @MANCC VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    -- Xóa nhà cung cấp theo mã
    DELETE FROM dbo.NHACUNGCAP
    WHERE RTRIM(MANCC) = RTRIM(@MANCC);
END
GO
-------------------------------------------------------------------------------------------------
--THÊM THÔNG TIN CỦA NCC
CREATE OR ALTER PROC SP_THEMNCC
	@MANCC CHAR(15),
	@TENNCC NVARCHAR(300),
	@DIACHI NVARCHAR(500),
	@SDT VARCHAR(12),
	@EMAIL NVARCHAR(320)
AS
BEGIN
	IF(EXISTS(SELECT * FROM NHACUNGCAP WHERE MANCC = @MANCC)) 
		RETURN -1
	IF(NOT EXISTS(SELECT * FROM NHACUNGCAP WHERE MANCC = @MANCC))
		INSERT INTO NHACUNGCAP VALUES (@MANCC, @TENNCC, @DIACHI, @SDT, @EMAIL)
END
EXEC SP_THEMNCC'NCC005', N'Việt Tiến', N'Hà Nội', '0905678901', 'viettien@gmail.com'
EXEC SP_THEMNCC'NCC006', N'Việt Tiến 2', N'Hà Nội', '0905678901', 'viettien@gmail.com'
-------------------------------------------------------------------------------------------------
--SỬA THÔNG TIN CỦA NCC CÓ MÃ BẤT KỲ
CREATE OR ALTER PROC SP_SUANCC(@MANCC CHAR(15),
	@TENNCC NVARCHAR(300),
	@DIACHI NVARCHAR(500),
	@SDT VARCHAR(12),
	@EMAIL NVARCHAR(320))
AS
BEGIN
	IF(NOT EXISTS(SELECT * FROM NHACUNGCAP WHERE MANCC = @MANCC)) 
		RETURN -1
	IF(EXISTS(SELECT * FROM NHACUNGCAP WHERE MANCC = @MANCC))
	UPDATE NHACUNGCAP SET TENNCC = @TENNCC, SDT = @SDT, DIACHI = @DIACHI, EMAIL = @EMAIL 
	WHERE MANCC = @MANCC
END
EXEC SP_SUANCC'NCC006', N'Việt Tiến 2', N'Hà Nội', '0905678901', 'viettien@gmail.com'
EXEC SP_SUANCC'NCC007', N'Việt Tiến 2', N'Hà Nội', '0905678901', 'viettien@gmail.com'


----------------------------------------------------------------------------------------------


CREATE PROCEDURE dbo.SP_THEMKM
    @MAKM CHAR(15),
    @TENKM NVARCHAR(300),
    @MASP CHAR(15),
    @NGAYBATDAU DATE,
    @NGAYKETTHUC DATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra mã khuyến mãi đã tồn tại
    IF EXISTS
    (
        SELECT 1
        FROM dbo.KHUYENMAI
        WHERE MAKM = @MAKM
    )
    BEGIN
        RETURN -1;
    END;

    -- Thêm khuyến mãi
    INSERT INTO dbo.KHUYENMAI ( MAKM, TENKM, MASP, NGAYBATDAU,   NGAYKETTHUC)
    VALUES ( @MAKM,  @TENKM,   @MASP,   @NGAYBATDAU,    @NGAYKETTHUC  );

    RETURN 0;
END;
GO

-----------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_GetByIDKM
    @MAKM CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        RTRIM(MAKM) AS MAKM,
        TENKM,
        RTRIM(MASP) AS MASP,
        NGAYBATDAU,
        NGAYKETTHUC
    FROM dbo.KHUYENMAI
    WHERE MAKM = @MAKM;
END
GO

------------------------------------------------------


CREATE PROCEDURE dbo.SP_XOAKM
    @MAKM CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.KHUYENMAI
        WHERE MAKM = @MAKM
    )
    BEGIN
        RETURN -1;
    END;

    DELETE FROM dbo.KHUYENMAI
    WHERE MAKM = @MAKM;

    RETURN 0;
END
GO



USE QUANLYBANLE;
GO


-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetThanhToan
AS
BEGIN
    SELECT * FROM THANHTOAN
    WHERE 
        MATHANHTOAN IS NOT NULL AND MATHANHTOAN <> ''
END
EXEC sp_GetThanhToan
-------------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_GetByIDTT(@MATHANHTOAN CHAR(15))
AS
BEGIN
    SELECT * FROM THANHTOAN WHERE 
        MATHANHTOAN = @MATHANHTOAN
END
EXEC sp_GetByIDTT'TT007'
-------------------------------------------------------------------------------------------------
--XÓA THÔNG TIN CỦA TT CÓ MÃ BẤT KỲ
CREATE OR ALTER PROC SP_XOATT(@MATHANHTOAN CHAR(15))
AS
BEGIN
	IF(NOT EXISTS(SELECT * FROM THANHTOAN WHERE MATHANHTOAN= @MATHANHTOAN)) 
		RETURN -1
	IF(EXISTS(SELECT * FROM THANHTOAN WHERE MATHANHTOAN= @MATHANHTOAN))
	DELETE FROM THANHTOAN
	WHERE MATHANHTOAN = @MATHANHTOAN
END
EXEC SP_XOATT'TT100'
-------------------------------------------------------------------------------------------------
--THÊM THÔNG TIN CỦA TT
CREATE OR ALTER PROC SP_THEMTT
	@MATHANHTOAN CHAR(15),
	@MAHDBAN CHAR(15),
	@PHUONGTHUC NVARCHAR(50),
	@SOTIENTHANHTOAN FLOAT,
	@NGAYTHANHTOAN DATETIME,
	@TRANGTHAI NVARCHAR(50)
AS
BEGIN
	IF(EXISTS(SELECT * FROM THANHTOAN WHERE MATHANHTOAN= @MATHANHTOAN)) 
		RETURN -1
	IF(NOT EXISTS(SELECT * FROM THANHTOAN WHERE MATHANHTOAN= @MATHANHTOAN))
		INSERT INTO THANHTOAN VALUES (@MATHANHTOAN, @MAHDBAN, @PHUONGTHUC, @SOTIENTHANHTOAN, @NGAYTHANHTOAN, @TRANGTHAI)
END
EXEC SP_THEMTT'TT005', 'HD005', N'Chuyển khoản', 1400000, '2025-01-14', N'Chưa thanh toán'
-------------------------------------------------------------------------------------------------
--SỬA PHƯƠNG THỨC TT, TRẠNG THÁI CỦA TT CÓ MÃ BẤT KỲ
CREATE OR ALTER PROC SP_SUATT(
	@MATHANHTOAN CHAR(15),
	@PHUONGTHUC NVARCHAR(50),
	@TRANGTHAI NVARCHAR(50))
AS
BEGIN
	IF(NOT EXISTS(SELECT * FROM THANHTOAN WHERE MATHANHTOAN= @MATHANHTOAN)) 
		RETURN -1
	IF(EXISTS(SELECT * FROM THANHTOAN WHERE MATHANHTOAN= @MATHANHTOAN))
	UPDATE THANHTOAN SET PHUONGTHUC = @PHUONGTHUC, TRANGTHAI = @TRANGTHAI
	WHERE MATHANHTOAN = @MATHANHTOAN
END
EXEC SP_SUATT'TT007', N'Chuyển khoản', N'Đã thanh toán'

------------------------------------------------------------
CREATE PROCEDURE SP_CAPNHAT_TRANGTHAI_THANHTOANTHANHCONG
    @MAHDBAN NVARCHAR(10),
    @PHUONGTHUC NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra xem hóa đơn có tồn tại trong bảng THANHTOAN không
    IF EXISTS (SELECT 1 FROM THANHTOAN WHERE MAHDBAN = @MAHDBAN)
    BEGIN
        UPDATE THANHTOAN
        SET 
            TRANGTHAI = N'Đã thanh toán',
            PHUONGTHUC = @PHUONGTHUC,
            SOTIENTHANHTOAN = (
                SELECT TOP 1 (TONGTIENHANG + THUEVAT - GIAMGIA)
                FROM HOADONBAN
                WHERE MAHDBAN = @MAHDBAN
            ),
            NGAYTHANHTOAN = GETDATE()
        WHERE MAHDBAN = @MAHDBAN;

        PRINT N' Cập nhật trạng thái, phương thức, số tiền và ngày thanh toán thành công!';
    END
    ELSE
    BEGIN
        PRINT N' Hóa đơn không tồn tại trong bảng THANHTOAN!';
    END
END

EXEC SP_CAPNHAT_TRANGTHAI_THANHTOANTHANHCONG 
	@MAHDBAN = 'HD78271906',
    @PHUONGTHUC = N'Tiền mặt';

------------------------------------------------------------------

CREATE OR ALTER PROCEDURE SP_LAY_HOADON_CHUA_THANHTOAN
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        H.MAHDBAN AS MaHoaDon,
        K.TENKH AS TenKhachHang,
        K.SDT AS SoDienThoai,
        H.NGAYLAP AS NgayLap,
        (H.TONGTIENHANG + H.THUEVAT - H.GIAMGIA) AS SoTienPhaiTra,
        ISNULL((
            SELECT TOP (1) T2.TRANGTHAI
            FROM THANHTOAN T2
            WHERE T2.MAHDBAN = H.MAHDBAN
            ORDER BY CASE WHEN T2.TRANGTHAI = N'Đã thanh toán' THEN 0 ELSE 1 END,
                     T2.PAYOS_CREATED_AT_UTC DESC, T2.NGAYTHANHTOAN DESC
        ), N'Chưa thanh toán') AS TrangThai
    FROM HOADONBAN H
        INNER JOIN KHACHHANG K ON H.MAKH = K.MAKH
    WHERE NOT EXISTS (
        SELECT 1 FROM THANHTOAN T
        WHERE T.MAHDBAN = H.MAHDBAN AND T.TRANGTHAI = N'Đã thanh toán'
    );
END

EXEC SP_LAY_HOADON_CHUA_THANHTOAN


----------------------------------------------------------



CREATE OR ALTER PROCEDURE SP_LAY_HOADON_CHUA_THANHTOAN_THEO_TENKH
    @TenKhachHang NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON

    SELECT 
        H.MAHDBAN AS MaHoaDon,
        K.TENKH AS TenKhachHang,
        K.SDT AS SoDienThoai,
        H.NGAYLAP AS NgayLap,
        (H.TONGTIENHANG + H.THUEVAT - H.GIAMGIA) AS SoTienPhaiTra,
        ISNULL((
            SELECT TOP (1) T2.TRANGTHAI
            FROM THANHTOAN T2
            WHERE T2.MAHDBAN = H.MAHDBAN
            ORDER BY CASE WHEN T2.TRANGTHAI = N'Đã thanh toán' THEN 0 ELSE 1 END,
                     T2.PAYOS_CREATED_AT_UTC DESC, T2.NGAYTHANHTOAN DESC
        ), N'Chưa thanh toán') AS TrangThai
    FROM HOADONBAN H
        INNER JOIN KHACHHANG K ON H.MAKH = K.MAKH
    WHERE NOT EXISTS (
        SELECT 1 FROM THANHTOAN T
        WHERE T.MAHDBAN = H.MAHDBAN AND T.TRANGTHAI = N'Đã thanh toán'
    )
          AND K.TENKH LIKE N'%' + @TenKhachHang + N'%'
END

EXEC SP_LAY_HOADON_CHUA_THANHTOAN_THEO_TENKH @TenKhachHang = N'Lê Thị Lan'




--Tự động cập nhật số lượng bảng SanPham khi thêm, sửa, hoặc xóa ChiTietNhap.
CREATE OR ALTER TRIGGER TG_CapNhatTonKho_KhiNhapHang
ON dbo.ChiTietNhap  
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    --Tạo bảng tạm để chứa TẤT CẢ các thay đổi
    --   +SoLuong cho HÀNG MỚI (từ INSERT hoặc UPDATE)
    --   -SoLuong cho HÀNG CŨ (từ DELETE hoặc UPDATE)
    SELECT MaSP, SoLuong AS SoLuongThayDoi
    INTO #Changes
    FROM inserted

    UNION ALL

    SELECT MaSP, -SoLuong AS SoLuongThayDoi
    FROM deleted;

    --Gom nhóm các thay đổi theo từng MaSP (trường hợp 1 phiếu nhập thay đổi nhiều dòng của cùng 1 MaSP)
    SELECT MaSP, SUM(SoLuongThayDoi) AS NetChange
    INTO #NetChanges
    FROM #Changes
    WHERE MaSP IS NOT NULL
    GROUP BY MaSP;

    --Cập nhật các sản phẩm ĐÃ CÓ trong bảng
    UPDATE SANPHAM
    SET SOLUONGTON = SP.SOLUONGTON + nc.NetChange
    FROM SANPHAM SP
    JOIN #NetChanges nc ON SP.MaSP = nc.MaSP;

    --Dọn dẹp các bảng tạm
    DROP TABLE #Changes;
    DROP TABLE #NetChanges;
END;
------------------------------------------------------------------------------------

USE QUANLYBANLE;
GO

IF OBJECT_ID('dbo.SP_SUAKM', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.SP_SUAKM;
END
GO

CREATE PROCEDURE dbo.SP_SUAKM
    @MAKM CHAR(15),
    @TENKM NVARCHAR(300),
    @MASP CHAR(15),
    @NGAYBATDAU DATE,
    @NGAYKETTHUC DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.KHUYENMAI
        WHERE MAKM = @MAKM
    )
    BEGIN
        RETURN -1;
    END;

    UPDATE dbo.KHUYENMAI
    SET
        TENKM = @TENKM,
        MASP = @MASP,
        NGAYBATDAU = @NGAYBATDAU,
        NGAYKETTHUC = @NGAYKETTHUC
    WHERE MAKM = @MAKM;

    RETURN 0;
END
GO

---------------------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.SP_KIEMTRASDT
    @SDT CHAR(10)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MANV
    FROM dbo.NHANVIEN
    WHERE SDT = @SDT;
END
GO


-------------------------------

CREATE OR ALTER PROCEDURE dbo.SP_KIEMTRASDTKHAC
    @SDT CHAR(10),
    @MANV CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MANV
    FROM dbo.NHANVIEN
    WHERE SDT = @SDT
      AND MANV <> @MANV;
END
GO
-------------------------------------------
-- Kim tra user name
CREATE OR ALTER PROCEDURE dbo.SP_KIEMTRAUSERNAME
    @USERNAME CHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MATAIKHOAN
    FROM dbo.TAIKHOAN
    WHERE USERNAME = @USERNAME;
END
GO

-----------------
--Kiem tr user name của tk khac
CREATE OR ALTER PROCEDURE dbo.SP_KIEMTRAUSERNAMEKHAC
    @USERNAME CHAR(20),
    @MATAIKHOAN CHAR(15)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT MATAIKHOAN
    FROM dbo.TAIKHOAN
    WHERE USERNAME = @USERNAME
      AND MATAIKHOAN <> @MATAIKHOAN;
END
GO











USE QUANLYBANLE;
GO

CREATE PROCEDURE dbo.SP_THEMNCC
    @MANCC CHAR(15),
    @TENNCC NVARCHAR(300),
    @DIACHI NVARCHAR(500),
    @SDT VARCHAR(12),
    @EMAIL NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.NHACUNGCAP
    (
        MANCC,
        TENNCC,
        DIACHI,
        SDT,
        EMAIL
    )
    VALUES
    (
        @MANCC,
        @TENNCC,
        @DIACHI,
        @SDT,
        @EMAIL
    );
END;
GO

----------------
USE QUANLYBANLE;
GO

EXEC sp_helptext 'dbo.SP_SUANCC';


USE QUANLYBANLE;
GO

CREATE PROCEDURE dbo.SP_SUANCC
    @MANCC CHAR(15),
    @TENNCC NVARCHAR(300),
    @DIACHI NVARCHAR(500),
    @SDT VARCHAR(12),
    @EMAIL NVARCHAR(320)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.NHACUNGCAP
    SET
        TENNCC = @TENNCC,
        DIACHI = @DIACHI,
        SDT = @SDT,
        EMAIL = @EMAIL
    WHERE MANCC = @MANCC;

    SELECT *
    FROM dbo.NHACUNGCAP
    WHERE MANCC = @MANCC;
END;
GO




USE QUANLYBANLE;
GO

CREATE  PROCEDURE dbo.SP_THEMTT
    @MaThanhToan CHAR(15),
    @MaHDBan CHAR(15),
    @PhuongThuc NVARCHAR(50),
    @SoTienThanhToan FLOAT,
    @NgayThanhToan DATETIME,
    @TrangThai NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.THANHTOAN
    (
        MATHANHTOAN,
        MAHDBAN,
        PHUONGTHUC,
        SOTIENTHANHTOAN,
        NGAYTHANHTOAN,
        TRANGTHAI
    )
    VALUES
    (
        @MaThanhToan,
        @MaHDBan,
        @PhuongThuc,
        @SoTienThanhToan,
        @NgayThanhToan,
        @TrangThai
    );

    SELECT *
    FROM dbo.THANHTOAN
    WHERE MATHANHTOAN = @MaThanhToan;
END;
GO


USE QUANLYBANLE;
GO

SELECT MAHDBAN, MANV, MAKH, NGAYLAP, TONGTIENHANG
FROM dbo.HOADONBAN;





