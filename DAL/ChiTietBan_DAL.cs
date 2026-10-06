using Microsoft.Data.SqlClient;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.DataHelper;

namespace DAL
{
    public class ChiTietBan_DAL
    {

        public List<ChiTietBan> GetAll()
        {
            try
            {
                var list = new List<ChiTietBan>();
                const string sql = @"SELECT  ct.MAHDBAN, ct.MASP,sp.TENSP AS TenSP,ct.SOLUONG,ct.DONGIA,ct.TONGTIEN
                    FROM CT_HDB ct
                    LEFT JOIN SANPHAM sp ON sp.MASP = ct.MASP";

                var dt = Connect.ExecuteQuery(sql);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietBan
                    {
                        MAHDBAN = row["MAHDBAN"]?.ToString(),
                        MASP = row["MASP"]?.ToString(),
                        TenSP = row["TenSP"]?.ToString(), // <-- thêm
                        SOLUONG = Convert.ToInt32(row["SOLUONG"]),
                        DONGIA = Convert.ToDecimal(row["DONGIA"]),
                        TONGTIEN = Convert.ToDecimal(row["TONGTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public List<ChiTietBan> GetByHoaDon(string maHDB)
        {
            try
            {
                var list = new List<ChiTietBan>();
                const string sql = @"SELECT  ct.MAHDBAN, ct.MASP,sp.TENSP AS TenSP,ct.SOLUONG,ct.DONGIA,ct.TONGTIEN
                    FROM CT_HDB ct
                    LEFT JOIN SANPHAM sp ON sp.MASP = ct.MASP
                    WHERE ct.MAHDBAN = @MAHDBAN";

                SqlParameter[] parameters = {  new SqlParameter("@MAHDBAN", maHDB) };

                var dt = Connect.ExecuteQuery(sql, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietBan
                    {
                        MAHDBAN = row["MAHDBAN"]?.ToString(),
                        MASP = row["MASP"]?.ToString(),
                        TenSP = row["TenSP"]?.ToString(), // <-- thêm
                        SOLUONG = Convert.ToInt32(row["SOLUONG"]),
                        DONGIA = Convert.ToDecimal(row["DONGIA"]),
                        TONGTIEN = Convert.ToDecimal(row["TONGTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }
        public bool KiemTraTonTai(string maHDB, string maSP)
        {
            try
            {
                const string sql = @"SELECT COUNT(*) AS SoLuong
                FROM CT_HDB
                WHERE RTRIM(MAHDBAN) = @MAHDBAN
                  AND RTRIM(MASP) = @MASP";

                SqlParameter[] parameters =
                {
            new SqlParameter("@MAHDBAN", maHDB.Trim()),
            new SqlParameter("@MASP", maSP.Trim())
        };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                if (dt.Rows.Count > 0)
                {
                    int count = Convert.ToInt32(dt.Rows[0]["SoLuong"]);
                    return count > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public bool Insert(ChiTietBan ct)
        {
            try
            {
                ct.TONGTIEN = ct.SOLUONG * ct.DONGIA;
                const string sql = @"INSERT INTO CT_HDB ( MAHDBAN,MASP, SOLUONG, DONGIA, TONGTIEN )
                VALUES( @MAHDBAN, @MASP,@SOLUONG, @DONGIA,  @TONGTIEN  )";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAHDBAN", ct.MAHDBAN.Trim()),
                    new SqlParameter("@MASP", ct.MASP.Trim()),
                    new SqlParameter("@SOLUONG", ct.SOLUONG),
                    new SqlParameter("@DONGIA", ct.DONGIA),
                    new SqlParameter("@TONGTIEN", ct.TONGTIEN)
                };

                return Connect.ExecuteInTransaction((connection, transaction) =>
                {
                    int rows = ExecuteNonQuery(connection, transaction, sql, parameters);
                    if (rows == 0) return false;
                    RecalculateInvoiceTotal(connection, transaction, ct.MAHDBAN);
                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm chi tiết bán: " + ex.Message);
            }
        }

        public bool Update(ChiTietBan ct)
        {
            try
            {
                ct.TONGTIEN = ct.SOLUONG * ct.DONGIA;
                if (!KiemTraTonTai(ct.MAHDBAN, ct.MASP))
                    return false;

                const string sql = @" UPDATE CT_HDB
                    SET SOLUONG = @SOLUONG,
                        DONGIA  = @DONGIA,
                        TONGTIEN= @TONGTIEN
                    WHERE MAHDBAN = @MAHDBAN AND MASP = @MASP";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAHDBAN", ct.MAHDBAN),
                    new SqlParameter("@MASP", ct.MASP),
                    new SqlParameter("@SOLUONG", ct.SOLUONG),
                    new SqlParameter("@DONGIA", ct.DONGIA),
                    new SqlParameter("@TONGTIEN", ct.TONGTIEN)
                };

                return Connect.ExecuteInTransaction((connection, transaction) =>
                {
                    int rows = ExecuteNonQuery(connection, transaction, sql, parameters);
                    if (rows == 0) return false;
                    RecalculateInvoiceTotal(connection, transaction, ct.MAHDBAN);
                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public bool Delete(string maHDB, string maSP)
        {
            try
            {
                if (!KiemTraTonTai(maHDB, maSP))
                    return false;

                const string sql = "DELETE FROM CT_HDB WHERE MAHDBAN = @MAHDBAN AND MASP = @MASP";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAHDBAN", maHDB),
                    new SqlParameter("@MASP", maSP)
                };

                return Connect.ExecuteInTransaction((connection, transaction) =>
                {
                    int rows = ExecuteNonQuery(connection, transaction, sql, parameters);
                    if (rows == 0) return false;
                    RecalculateInvoiceTotal(connection, transaction, maHDB);
                    return true;
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi: " + ex.Message);
            }
        }

        public bool KiemTraHoaDonTonTai(string maHDB)
        {
            try
            {
                const string sql = @"SELECT COUNT(*)
                    FROM dbo.HOADONBAN
                    WHERE MAHDBAN = @MAHDBAN";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAHDBAN", SqlDbType.Char, 15)
                {
                Value = maHDB.Trim()
                } };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                if (dt.Rows.Count == 0)
                    return false;

                return Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool KiemTraSanPhamTonTai(string maSP)
        {
            try
            {
                const string sql = @"SELECT COUNT(*)
                    FROM dbo.SANPHAM
                    WHERE MASP = @MASP";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MASP", SqlDbType.Char, 15)
                {
                Value = maSP.Trim()
                }
        };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                if (dt.Rows.Count == 0)
                    return false;

                return Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch
            {
                return false;
            }
        }

        private static void RecalculateInvoiceTotal(SqlConnection connection, SqlTransaction transaction, string maHDB)
        {
            const string sql = @"UPDATE H
                                 SET TONGTIENHANG = CASE
                                     WHEN COALESCE(S.TONG, 0) + COALESCE(H.THUEVAT, 0) - COALESCE(H.GIAMGIA, 0) < 0 THEN 0
                                     ELSE COALESCE(S.TONG, 0) + COALESCE(H.THUEVAT, 0) - COALESCE(H.GIAMGIA, 0)
                                 END
                                 FROM dbo.HOADONBAN H
                                 OUTER APPLY (SELECT SUM(TONGTIEN) AS TONG
                                              FROM dbo.CT_HDB
                                              WHERE RTRIM(MAHDBAN) = RTRIM(H.MAHDBAN)) S
                                 WHERE RTRIM(H.MAHDBAN) = @MAHDBAN";
            int rows = ExecuteNonQuery(connection, transaction, sql,
                new SqlParameter("@MAHDBAN", SqlDbType.Char, 15) { Value = maHDB.Trim() });
            if (rows == 0)
                throw new InvalidOperationException("Không tìm thấy hóa đơn để cập nhật tổng tiền.");
        }

        private static int ExecuteNonQuery(SqlConnection connection, SqlTransaction transaction, string sql, params SqlParameter[] parameters)
        {
            using SqlCommand command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddRange(parameters);
            return command.ExecuteNonQuery();
        }

    }
}
