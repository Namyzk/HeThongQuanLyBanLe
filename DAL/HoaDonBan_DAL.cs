using Microsoft.Data.SqlClient;
using Models;
using DAL.DataHelper;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class HoaDonBan_DAL
    {
        // 1. Chỉ giữ lại 1 hàm kiểm tra sự tồn tại của Hóa đơn bán
        public bool KiemTraTonTai(string maHDB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDB))
                    return false;

                const string sql = @"
                    SELECT COUNT(*) AS SoLuong
                    FROM HOADONBAN
                    WHERE RTRIM(MAHDBAN) = @MAHDBAN";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAHDBAN", maHDB.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0]["SoLuong"]) > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra tồn tại: " + ex.Message);
            }
        }

        // (Tùy chọn) Nếu cần kiểm tra chi tiết, tách thành hàm riêng biệt rõ ràng
        public bool KiemTraTonTaiChiTiet(string maHDB, string maSP)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM CT_HDB
                WHERE RTRIM(MAHDBAN) = @MAHDBAN
                  AND RTRIM(MASP) = @MASP";

            SqlParameter[] parameters =
            {
                new SqlParameter("@MAHDBAN", maHDB.Trim()),
                new SqlParameter("@MASP", maSP.Trim())
            };

            DataTable dt = Connect.ExecuteQuery(sql, parameters);
            return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public List<HoaDonBan> GetAll()
        {
            try
            {
                string sql = @"SELECT MAHDBAN, MANV, MAKH, NGAYLAP, TONGTIENHANG, THUEVAT, GIAMGIA FROM HOADONBAN";
                DataTable dt = Connect.ExecuteQuery(sql);

                List<HoaDonBan> list = new List<HoaDonBan>();

                foreach (DataRow row in dt.Rows)
                {
                    string maHDB = row["MAHDBAN"]?.ToString()?.Trim() ?? "";

                    DateOnly? ngay = null;
                    if (row["NGAYLAP"] != DBNull.Value)
                    {
                        ngay = DateOnly.FromDateTime(Convert.ToDateTime(row["NGAYLAP"]));
                    }

                    list.Add(new HoaDonBan
                    {
                        MAHDBAN = maHDB,
                        MANV = row["MANV"] == DBNull.Value ? null : row["MANV"]?.ToString()?.Trim(),
                        MAKH = row["MAKH"] == DBNull.Value ? null : row["MAKH"]?.ToString()?.Trim(),
                        NGAYLAP = ngay,
                        TONGTIENHANG = row["TONGTIENHANG"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TONGTIENHANG"]),
                        THUEVAT = row["THUEVAT"] == DBNull.Value ? 0 : Convert.ToDecimal(row["THUEVAT"]),
                        GIAMGIA = row["GIAMGIA"] == DBNull.Value ? 0 : Convert.ToDecimal(row["GIAMGIA"]),
                        listjson_chitietban = GetChiTietByMa(maHDB)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public List<HoaDonBan> GetByID(string maHDB)
        {
            try
            {
                string sql = @"SELECT MAHDBAN, MANV, MAKH, NGAYLAP, TONGTIENHANG, THUEVAT, GIAMGIA 
                               FROM HOADONBAN WHERE MAHDBAN = @MAHDBAN";

                SqlParameter[] parameters = { new SqlParameter("@MAHDBAN", maHDB) };
                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                List<HoaDonBan> list = new List<HoaDonBan>();

                foreach (DataRow row in dt.Rows)
                {
                    string maHDBRow = row["MAHDBAN"]?.ToString()?.Trim() ?? "";

                    DateOnly? ngay = null;
                    if (row["NGAYLAP"] != DBNull.Value)
                    {
                        ngay = DateOnly.FromDateTime(Convert.ToDateTime(row["NGAYLAP"]));
                    }

                    list.Add(new HoaDonBan
                    {
                        MAHDBAN = maHDBRow,
                        MANV = row["MANV"] == DBNull.Value ? null : row["MANV"]?.ToString()?.Trim(),
                        MAKH = row["MAKH"] == DBNull.Value ? null : row["MAKH"]?.ToString()?.Trim(),
                        NGAYLAP = ngay,
                        TONGTIENHANG = row["TONGTIENHANG"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TONGTIENHANG"]),
                        THUEVAT = row["THUEVAT"] == DBNull.Value ? 0 : Convert.ToDecimal(row["THUEVAT"]),
                        GIAMGIA = row["GIAMGIA"] == DBNull.Value ? 0 : Convert.ToDecimal(row["GIAMGIA"]),
                        listjson_chitietban = GetChiTietByMa(maHDBRow)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        private List<ChiTietBan> GetChiTietByMa(string maHDB)
        {
            try
            {
                string sql = @"SELECT CT.MAHDBAN, CT.MASP, SP.TENSP, CT.SOLUONG, CT.DONGIA, CT.TONGTIEN
                               FROM dbo.CT_HDB CT
                               LEFT JOIN dbo.SANPHAM SP ON RTRIM(CT.MASP) = RTRIM(SP.MASP)
                               WHERE RTRIM(CT.MAHDBAN) = @MAHDBAN";

                SqlParameter[] parameters = { new SqlParameter("@MAHDBAN", maHDB.Trim()) };
                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                List<ChiTietBan> list = new List<ChiTietBan>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietBan
                    {
                        MAHDBAN = row["MAHDBAN"]?.ToString()?.Trim() ?? "",
                        MASP = row["MASP"]?.ToString()?.Trim() ?? "",
                        TenSP = row["TENSP"] == DBNull.Value ? null : row["TENSP"]?.ToString()?.Trim(),
                        SOLUONG = row["SOLUONG"] == DBNull.Value ? 0 : Convert.ToInt32(row["SOLUONG"]),
                        DONGIA = row["DONGIA"] == DBNull.Value ? 0 : Convert.ToDecimal(row["DONGIA"]),
                        TONGTIEN = row["TONGTIEN"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TONGTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy chi tiết bán theo hóa đơn: " + ex.Message);
            }
        }

        public bool Insert(HoaDonBan hd)
        {
            try
            {
                if (hd == null) return false;

                decimal tongTienSanPham = 0;
                if (hd.listjson_chitietban != null && hd.listjson_chitietban.Count > 0)
                {
                    foreach (var ct in hd.listjson_chitietban)
                    {
                        ct.TONGTIEN = ct.SOLUONG * ct.DONGIA;
                        tongTienSanPham += ct.TONGTIEN;
                    }
                }

                decimal tongTienSauTinh = tongTienSanPham + hd.THUEVAT - hd.GIAMGIA;
                if (tongTienSauTinh < 0) tongTienSauTinh = 0;

                string sqlHD = @"
                    INSERT INTO HOADONBAN (MAHDBAN, MANV, MAKH, NGAYLAP, TONGTIENHANG, THUEVAT, GIAMGIA)
                    VALUES (@MAHDBAN, @MANV, @MAKH, @NGAYLAP, @TONGTIENHANG, @THUEVAT, @GIAMGIA)";

                // Chuyển DateOnly thành DateTime để truyền vào Sql Server an toàn
                object ngayLapValue = hd.NGAYLAP.HasValue
                    ? hd.NGAYLAP.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.Today;

                SqlParameter[] pHD =
                {
                    new SqlParameter("@MAHDBAN", hd.MAHDBAN.Trim()),
                    new SqlParameter("@MANV", (object?)hd.MANV ?? DBNull.Value),
                    new SqlParameter("@MAKH", (object?)hd.MAKH ?? DBNull.Value),
                    new SqlParameter("@NGAYLAP", ngayLapValue),
                    new SqlParameter("@TONGTIENHANG", tongTienSauTinh),
                    new SqlParameter("@THUEVAT", hd.THUEVAT),
                    new SqlParameter("@GIAMGIA", hd.GIAMGIA)
                };

                int rows = Connect.ExecuteNonQuery(sqlHD, pHD);

                if (rows > 0 && hd.listjson_chitietban != null)
                {
                    foreach (var ct in hd.listjson_chitietban)
                    {
                        string sqlCT = @"
                            INSERT INTO CT_HDB (MAHDBAN, MASP, SOLUONG, DONGIA, TONGTIEN)
                            VALUES (@MAHDBAN, @MASP, @SOLUONG, @DONGIA, @TONGTIEN)";

                        SqlParameter[] pCT =
                        {
                            new SqlParameter("@MAHDBAN", hd.MAHDBAN.Trim()),
                            new SqlParameter("@MASP", ct.MASP.Trim()),
                            new SqlParameter("@SOLUONG", ct.SOLUONG),
                            new SqlParameter("@DONGIA", ct.DONGIA),
                            new SqlParameter("@TONGTIEN", ct.TONGTIEN)
                        };

                        Connect.ExecuteNonQuery(sqlCT, pCT);
                    }
                }

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm hóa đơn bán: " + ex.Message);
            }
        }

        public bool Update(HoaDonBan hd)
        {
            try
            {
                if (hd == null || !KiemTraTonTai(hd.MAHDBAN))
                    return false;

                // Xóa chi tiết cũ
                string sqlDeleteCT = @"DELETE FROM CT_HDB WHERE MAHDBAN = @MAHDBAN";
                Connect.ExecuteNonQuery(sqlDeleteCT, new SqlParameter[] { new SqlParameter("@MAHDBAN", hd.MAHDBAN) });

                // Tính lại tổng tiền
                decimal tongTienSanPham = 0;
                if (hd.listjson_chitietban != null && hd.listjson_chitietban.Count > 0)
                {
                    foreach (var ct in hd.listjson_chitietban)
                    {
                        ct.TONGTIEN = ct.SOLUONG * ct.DONGIA;
                        tongTienSanPham += ct.TONGTIEN;
                    }
                }

                decimal tongTienSauTinh = tongTienSanPham + hd.THUEVAT - hd.GIAMGIA;
                if (tongTienSauTinh < 0) tongTienSauTinh = 0;

                string sql = @"UPDATE HOADONBAN
                               SET MANV = @MANV,
                                   MAKH = @MAKH,
                                   NGAYLAP = @NGAYLAP,
                                   TONGTIENHANG = @TONGTIENHANG,
                                   THUEVAT = @THUEVAT,
                                   GIAMGIA = @GIAMGIA
                               WHERE MAHDBAN = @MAHDBAN";

                object ngayLapValue = hd.NGAYLAP.HasValue
                    ? hd.NGAYLAP.Value.ToDateTime(TimeOnly.MinValue)
                    : DateTime.Today;

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MANV", (object?)hd.MANV ?? DBNull.Value),
                    new SqlParameter("@MAKH", (object?)hd.MAKH ?? DBNull.Value),
                    new SqlParameter("@NGAYLAP", ngayLapValue),
                    new SqlParameter("@TONGTIENHANG", tongTienSauTinh),
                    new SqlParameter("@THUEVAT", hd.THUEVAT),
                    new SqlParameter("@GIAMGIA", hd.GIAMGIA),
                    new SqlParameter("@MAHDBAN", hd.MAHDBAN)
                };

                int rows = Connect.ExecuteNonQuery(sql, parameters);

                if (rows > 0 && hd.listjson_chitietban != null)
                {
                    foreach (var ct in hd.listjson_chitietban)
                    {
                        string sqlCT = @"INSERT INTO CT_HDB (MAHDBAN, MASP, SOLUONG, DONGIA, TONGTIEN)
                                         VALUES (@MAHDBAN, @MASP, @SOLUONG, @DONGIA, @TONGTIEN)";

                        SqlParameter[] p =
                        {
                            new SqlParameter("@MAHDBAN", hd.MAHDBAN),
                            new SqlParameter("@MASP", ct.MASP),
                            new SqlParameter("@SOLUONG", ct.SOLUONG),
                            new SqlParameter("@DONGIA", ct.DONGIA),
                            new SqlParameter("@TONGTIEN", ct.TONGTIEN)
                        };

                        Connect.ExecuteNonQuery(sqlCT, p);
                    }
                }

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public bool Delete(string maHDB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDB) || !KiemTraTonTai(maHDB))
                    return false;

                string sqlCT = @"DELETE FROM CT_HDB WHERE MAHDBAN = @MAHDBAN";
                Connect.ExecuteNonQuery(sqlCT, new SqlParameter[] { new SqlParameter("@MAHDBAN", maHDB) });

                string sql = @"DELETE FROM HOADONBAN WHERE MAHDBAN = @MAHDBAN";
                int rows = Connect.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@MAHDBAN", maHDB) });

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public bool ResetTongTienHang(string maHDBan, decimal tongTienMoi)
        {
            try
            {
                const string sql = @"UPDATE HOADONBAN SET TONGTIENHANG = @TongTienMoi WHERE MAHDBAN = @MaHDBan";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@TongTienMoi", tongTienMoi),
                    new SqlParameter("@MaHDBan", maHDBan)
                };

                return Connect.ExecuteNonQuery(sql, parameters) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi cập nhật TONGTIENHANG: " + ex.Message);
            }
        }
    }
}