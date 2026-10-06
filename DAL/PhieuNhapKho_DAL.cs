using Microsoft.Data.SqlClient;
using DAL.DataHelper;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class PhieuNhapKho_DAL
    {
        

        public bool KiemTraTonTai(string maPN)
        {
            try
            {
                string sql = @" SELECT COUNT(*) AS SoLuong
                    FROM PHIEUNHAPKHO
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP";

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MAPHIEUNHAP", (object?)maPN?.Trim() ?? DBNull.Value)
                };

                DataTable dt = Connect.ExecuteQuery( sql,  parameters);

                if (dt.Rows.Count > 0)
                {
                    int count = Convert.ToInt32( dt.Rows[0]["SoLuong"]);

                    return count > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi kiểm tra tồn tại phiếu nhập: "  + ex.Message);
            }
        }



        public List<PhieuNhapKho> GetAll()
        {
            try
            {
                string sql = @" SELECT  MAPHIEUNHAP,    MANCC,     MANV,
                        NGAYLAP
                    FROM PHIEUNHAPKHO";

                DataTable dt = Connect.ExecuteQuery(sql);

                List<PhieuNhapKho> list =
                    new List<PhieuNhapKho>();

                foreach (DataRow row in dt.Rows)
                {
                    string maPN = row["MAPHIEUNHAP"]? .ToString()? .Trim() ?? "";

                    list.Add(new PhieuNhapKho
                    {
                        MAPHIEUNHAP = maPN,
                        MANCC =   row["MANCC"]? .ToString()? .Trim(),
                        MANV =  row["MANV"]? .ToString()? .Trim(),
                        NGAYLAP = row["NGAYLAP"] == DBNull.Value  ? DateOnly.MinValue
                        : DateOnly.FromDateTime(Convert.ToDateTime(   row["NGAYLAP"])),
                        listjson_chitietnhap =  GetChiTietByMa(maPN)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy danh sách phiếu nhập: "   + ex.Message);
            }
        }


        public List<PhieuNhapKho> GetByID(string maPN)
        {
            try
            {
                string sql = @" SELECT  MAPHIEUNHAP, MANCC,     MANV,       NGAYLAP
                    FROM PHIEUNHAPKHO
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP";

                SqlParameter[] parameters =
                {
                    new SqlParameter(   "@MAPHIEUNHAP",   (object?)maPN?.Trim() ?? DBNull.Value)
                };

                DataTable dt = Connect.ExecuteQuery(  sql,  parameters);

                List<PhieuNhapKho> list =   new List<PhieuNhapKho>();

                foreach (DataRow row in dt.Rows)
                {
                    string ma =  row["MAPHIEUNHAP"]?   .ToString()?  .Trim() ?? "";

                    list.Add(new PhieuNhapKho
                    {
                        MAPHIEUNHAP = ma,

                        MANCC = row["MANCC"]?  .ToString()?.Trim(),

                        MANV =  row["MANV"]?   .ToString()? .Trim(),

                        NGAYLAP = row["NGAYLAP"] == DBNull.Value ? DateOnly.MinValue : DateOnly.FromDateTime(Convert.ToDateTime( row["NGAYLAP"])),

                        listjson_chitietnhap =   GetChiTietByMa(ma)
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi lấy phiếu nhập theo mã: "   + ex.Message);
            }
        }



        private List<ChiTietNhap> GetChiTietByMa( string maPN)
        {
            try
            {
                string sql = @" SELECT   MAPHIEUNHAP, MASP,  SOLUONG,    DONGIANHAP,  THANHTIEN
                    FROM CHITIETNHAP
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP";

                SqlParameter[] parameters =
                {
                    new SqlParameter(  "@MAPHIEUNHAP",   (object?)maPN?.Trim() ?? DBNull.Value)
                };

                DataTable dt = Connect.ExecuteQuery(  sql,  parameters);

                List<ChiTietNhap> list =  new List<ChiTietNhap>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietNhap
                    {
                        MAPHIEUNHAP =   row["MAPHIEUNHAP"]?  .ToString()?  .Trim(),
                        MASP =  row["MASP"]? .ToString()?  .Trim(),
                        SOLUONG =  row["SOLUONG"] == DBNull.Value   ? 0  : Convert.ToInt32(  row["SOLUONG"]),
                        DONGIANHAP =  row["DONGIANHAP"] == DBNull.Value   ? 0   : Convert.ToDecimal(  row["DONGIANHAP"]),
                        THANHTIEN =   row["THANHTIEN"] == DBNull.Value      ? 0    : Convert.ToDecimal(   row["THANHTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy chi tiết phiếu nhập: " + ex.Message);
            }
        }

        public bool Insert(PhieuNhapKho pnk)
        {
            try
            {
                if (pnk == null)
                    return false;

                if (KiemTraTonTai(pnk.MAPHIEUNHAP))
                    return false;

                decimal tongTienHang = 0;

                if (pnk.listjson_chitietnhap != null && pnk.listjson_chitietnhap.Count > 0)
                {
                    foreach (var ct in pnk.listjson_chitietnhap)
                    {
                        ct.THANHTIEN = ct.SOLUONG * ct.DONGIANHAP;
                        tongTienHang += ct.THANHTIEN;
                    }
                }

                string sql = @" INSERT INTO PHIEUNHAPKHO (MAPHIEUNHAP, MANCC, MANV, NGAYLAP)
                VALUES (@MAPHIEUNHAP, @MANCC, @MANV, @NGAYLAP)";

                // Xử lý chuyển đổi DateOnly sang DateTime an toàn cho SQL Server
                object ngayLapValue = pnk.NGAYLAP.ToDateTime(TimeOnly.MinValue);

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAPHIEUNHAP", (object?)pnk.MAPHIEUNHAP?.Trim() ?? DBNull.Value),
                    new SqlParameter("@MANCC", (object?)pnk.MANCC ?? DBNull.Value),
                    new SqlParameter("@MANV", (object?)pnk.MANV ?? DBNull.Value),
                    new SqlParameter("@NGAYLAP", ngayLapValue)
                };

                return Connect.ExecuteInTransaction((connection, transaction) =>
                {
                    int rows = ExecuteNonQuery(connection, transaction, sql, parameters);
                    if (rows == 0) return false;

                    foreach (var ct in pnk.listjson_chitietnhap ?? new List<ChiTietNhap>())
                    {
                        const string sqlCT = @"INSERT INTO CHITIETNHAP (MAPHIEUNHAP, MASP, SOLUONG, DONGIANHAP, THANHTIEN)
                                               VALUES (@MAPHIEUNHAP, @MASP, @SOLUONG, @DONGIANHAP, @THANHTIEN)";
                        SqlParameter[] parametersCT =
                        {
                            new SqlParameter("@MAPHIEUNHAP", pnk.MAPHIEUNHAP),
                            new SqlParameter("@MASP", ct.MASP),
                            new SqlParameter("@SOLUONG", ct.SOLUONG),
                            new SqlParameter("@DONGIANHAP", ct.DONGIANHAP),
                            new SqlParameter("@THANHTIEN", ct.THANHTIEN)
                        };
                        ExecuteNonQuery(connection, transaction, sqlCT, parametersCT);
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thêm phiếu nhập: " + ex.Message);
            }
        }

        public bool Update(PhieuNhapKho pnk)
        {
            try
            {
                if (pnk == null)
                    return false;

                if (!KiemTraTonTai(pnk.MAPHIEUNHAP))
                    return false;


                decimal tongTienHang = 0;

                if (pnk.listjson_chitietnhap != null && pnk.listjson_chitietnhap.Count > 0)
                {
                    foreach ( var ct in pnk.listjson_chitietnhap)
                    {
                        ct.THANHTIEN  = ct.SOLUONG *ct.DONGIANHAP;

                        tongTienHang += ct.THANHTIEN;
                    }
                }

                decimal tongSauTinh = tongTienHang;

                if (tongSauTinh < 0)tongSauTinh = 0;


                string sql = @" UPDATE PHIEUNHAPKHO
                    SET
                        MANCC = @MANCC,
                        MANV = @MANV,
                        NGAYLAP = @NGAYLAP
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP";

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MANCC",(object?)pnk.MANCC ?? DBNull.Value),
                    new SqlParameter( "@MANV",(object?)pnk.MANV ?? DBNull.Value),
                    new SqlParameter( "@NGAYLAP",  pnk.NGAYLAP),
                    new SqlParameter( "@MAPHIEUNHAP", (object?)pnk.MAPHIEUNHAP?.Trim()    ?? DBNull.Value)
                };

                return Connect.ExecuteInTransaction((connection, transaction) =>
                {
                    int rows = ExecuteNonQuery(connection, transaction, sql, parameters);
                    if (rows == 0) return false;

                    ExecuteNonQuery(connection, transaction,
                        "DELETE FROM CHITIETNHAP WHERE MAPHIEUNHAP = @MAPHIEUNHAP",
                        new SqlParameter("@MAPHIEUNHAP", pnk.MAPHIEUNHAP));

                    foreach (var ct in pnk.listjson_chitietnhap ?? new List<ChiTietNhap>())
                    {
                        const string sqlCT = @"INSERT INTO CHITIETNHAP (MAPHIEUNHAP, MASP, SOLUONG, DONGIANHAP, THANHTIEN)
                                               VALUES (@MAPHIEUNHAP, @MASP, @SOLUONG, @DONGIANHAP, @THANHTIEN)";
                        SqlParameter[] parametersCT =
                        {
                            new SqlParameter("@MAPHIEUNHAP", pnk.MAPHIEUNHAP),
                            new SqlParameter("@MASP", ct.MASP),
                            new SqlParameter("@SOLUONG", ct.SOLUONG),
                            new SqlParameter("@DONGIANHAP", ct.DONGIANHAP),
                            new SqlParameter("@THANHTIEN", ct.THANHTIEN)
                        };
                        ExecuteNonQuery(connection, transaction, sqlCT, parametersCT);
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi cập nhật phiếu nhập: "+ ex.Message);
            }
        }

        public bool Delete(string maPN)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maPN))
                    return false;
                maPN = maPN.Trim();

                if (!KiemTraTonTai(maPN))
                    return false;

                return Connect.ExecuteInTransaction((connection, transaction) =>
                {
                    ExecuteNonQuery(connection, transaction,
                        "DELETE FROM CHITIETNHAP WHERE MAPHIEUNHAP = @MAPHIEUNHAP",
                        new SqlParameter("@MAPHIEUNHAP", maPN));
                    int rows = ExecuteNonQuery(connection, transaction,
                        "DELETE FROM PHIEUNHAPKHO WHERE MAPHIEUNHAP = @MAPHIEUNHAP",
                        new SqlParameter("@MAPHIEUNHAP", maPN));
                    return rows > 0;
                });
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi xoá phiếu nhập: "  + ex.Message);
            }
        }



        public bool KiemTraNhaCungCapTonTai(string mancc)
        {
            try
            {
                string sql = @"SELECT COUNT(*)
                FROM NHACUNGCAP
                WHERE MANCC = @MANCC";

                SqlParameter[] parameters =
                { new SqlParameter("@MANCC", mancc.Trim()) };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                return dt.Rows.Count > 0 &&   Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi kiểm tra nhà cung cấp: " + ex.Message);
            }
        }



        public bool KiemTraNhanVienTonTai(string manv)
        {
            try
            {
                string sql = @" SELECT COUNT(*)
                FROM NHANVIEN
                WHERE MANV = @MANV";

                SqlParameter[] parameters =
                {
                     new SqlParameter("@MANV", manv.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                return dt.Rows.Count > 0 &&   Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra nhân viên: " + ex.Message);
            }
        }


        public bool KiemTraSanPhamTonTai(string masp)
        {
            try
            {
                string sql = @"  SELECT COUNT(*)
                    FROM SANPHAM
                    WHERE MASP = @MASP";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MASP", masp.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                return dt.Rows.Count > 0 &&   Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi kiểm tra sản phẩm: " + ex.Message);
            }
        }

        private static int ExecuteNonQuery(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            using SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddRange(parameters);
            return command.ExecuteNonQuery();
        }
    }
}
