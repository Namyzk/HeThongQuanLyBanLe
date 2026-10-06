using Microsoft.Data.SqlClient;
using DAL.DataHelper;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class ChiTietNhap_DAL
    {


        public bool KiemTraTonTai(string maphieunhap, string masp)
        {
            try
            {
                string sql = @" SELECT COUNT(*) AS SoLuong
                    FROM CHITIETNHAP
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP
                      AND MASP = @MASP";

                SqlParameter[] p =
                {
                    new SqlParameter("@MAPHIEUNHAP", maphieunhap),
                    new SqlParameter("@MASP", masp)
                };

                DataTable dt = Connect.ExecuteQuery(sql, p);

                if (dt.Rows.Count > 0)
                {
                    int count = Convert.ToInt32(dt.Rows[0]["SoLuong"]);
                    return count > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra tồn tại chi tiết nhập: " + ex.Message);
            }
        }



        public List<ChiTietNhap> GetAll()
        {
            try
            {
                var list = new List<ChiTietNhap>();

                string sql = @" SELECT   MAPHIEUNHAP, MASP, SOLUONG, DONGIANHAP,   THANHTIEN
                    FROM CHITIETNHAP";

                DataTable dt = Connect.ExecuteQuery(sql);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietNhap
                    {
                        MAPHIEUNHAP = row["MAPHIEUNHAP"] == DBNull.Value? ""
                            : row["MAPHIEUNHAP"].ToString()?.Trim() ?? "",

                        MASP = row["MASP"] == DBNull.Value ? ""
                            : row["MASP"].ToString()?.Trim() ?? "",

                        SOLUONG = row["SOLUONG"] == DBNull.Value ? 0
                            : Convert.ToInt32(row["SOLUONG"]),

                        DONGIANHAP = row["DONGIANHAP"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["DONGIANHAP"]),

                        THANHTIEN = row["THANHTIEN"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["THANHTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi lấy danh sách chi tiết nhập: " + ex.Message);
            }
        }



        public List<ChiTietNhap> GetByPhieu(string maphieunhap)
        {
            try
            {
                var list = new List<ChiTietNhap>();

                string sql = @"SELECT   MAPHIEUNHAP,  MASP, SOLUONG,DONGIANHAP,  THANHTIEN
                    FROM CHITIETNHAP
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP";

                SqlParameter[] p =
                {
                    new SqlParameter("@MAPHIEUNHAP", maphieunhap)
                };

                DataTable dt = Connect.ExecuteQuery(sql, p);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietNhap
                    {
                        MAPHIEUNHAP = row["MAPHIEUNHAP"] == DBNull.Value ? ""
                            : row["MAPHIEUNHAP"].ToString()?.Trim() ?? "",

                        MASP = row["MASP"] == DBNull.Value ? ""
                            : row["MASP"].ToString()?.Trim() ?? "",

                        SOLUONG = row["SOLUONG"] == DBNull.Value  ? 0
                            : Convert.ToInt32(row["SOLUONG"]),

                        DONGIANHAP = row["DONGIANHAP"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["DONGIANHAP"]),

                        THANHTIEN = row["THANHTIEN"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["THANHTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi lấy chi tiết theo phiếu nhập: " + ex.Message);
            }
        }


        public List<ChiTietNhap> GetById(string maphieunhap, string masp)
        {
            try
            {
                var list = new List<ChiTietNhap>();

                string sql = @"SELECT  MAPHIEUNHAP,  MASP, SOLUONG, DONGIANHAP, THANHTIEN
                    FROM CHITIETNHAP
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP
                      AND MASP = @MASP";

                SqlParameter[] p =
                {
                    new SqlParameter("@MAPHIEUNHAP", maphieunhap),
                    new SqlParameter("@MASP", masp)
                };

                DataTable dt = Connect.ExecuteQuery(sql, p);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new ChiTietNhap
                    {
                        MAPHIEUNHAP = row["MAPHIEUNHAP"] == DBNull.Value ? ""
                            : row["MAPHIEUNHAP"].ToString()?.Trim() ?? "",

                        MASP = row["MASP"] == DBNull.Value  ? ""
                            : row["MASP"].ToString()?.Trim() ?? "",

                        SOLUONG = row["SOLUONG"] == DBNull.Value  ? 0
                            : Convert.ToInt32(row["SOLUONG"]),

                        DONGIANHAP = row["DONGIANHAP"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["DONGIANHAP"]),

                        THANHTIEN = row["THANHTIEN"] == DBNull.Value ? 0
                            : Convert.ToDecimal(row["THANHTIEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi lấy chi tiết nhập theo khóa: " + ex.Message);
            }
        }



        public bool Insert(ChiTietNhap ctn)
        {
            try
            {
                if (ctn == null)
                    return false;

                if (string.IsNullOrWhiteSpace(ctn.MAPHIEUNHAP))
                    return false;

                if (string.IsNullOrWhiteSpace(ctn.MASP))
                    return false;

                string sql = @" INSERT INTO CHITIETNHAP  ( MAPHIEUNHAP, MASP, SOLUONG, DONGIANHAP,  THANHTIEN  )
                    VALUES ( @MAPHIEUNHAP, @MASP,  @SOLUONG, @DONGIANHAP,  (@SOLUONG * @DONGIANHAP) )";

                SqlParameter[] p =
                {
                    new SqlParameter("@MAPHIEUNHAP", ctn.MAPHIEUNHAP),
                    new SqlParameter("@MASP", ctn.MASP),
                    new SqlParameter("@SOLUONG", ctn.SOLUONG),
                    new SqlParameter("@DONGIANHAP", ctn.DONGIANHAP)
                };

                int rows = Connect.ExecuteNonQuery(sql, p);

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi thêm chi tiết nhập: " + ex.Message);
            }
        }


        public bool Update(ChiTietNhap ctn)
        {
            try
            {
                if (ctn == null)
                    return false;

                if (string.IsNullOrWhiteSpace(ctn.MAPHIEUNHAP))
                    return false;

                if (string.IsNullOrWhiteSpace(ctn.MASP))
                    return false;

                if (!KiemTraTonTai(ctn.MAPHIEUNHAP, ctn.MASP))
                    return false;

                string sql = @"UPDATE CHITIETNHAP
                    SET 
                        SOLUONG = @SOLUONG,
                        DONGIANHAP = @DONGIANHAP,
                        THANHTIEN = (@SOLUONG * @DONGIANHAP)
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP
                      AND MASP = @MASP";

                SqlParameter[] p =
                {
                    new SqlParameter("@MAPHIEUNHAP", ctn.MAPHIEUNHAP),
                    new SqlParameter("@MASP", ctn.MASP),
                    new SqlParameter("@SOLUONG", ctn.SOLUONG),
                    new SqlParameter("@DONGIANHAP", ctn.DONGIANHAP)
                };

                int rows = Connect.ExecuteNonQuery(sql, p);

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi cập nhật chi tiết nhập: " + ex.Message);
            }
        }


        public bool Delete(string maphieunhap, string masp)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maphieunhap))
                    return false;

                if (string.IsNullOrWhiteSpace(masp))
                    return false;

                if (!KiemTraTonTai(maphieunhap, masp))
                    return false;

                string sql = @"DELETE FROM CHITIETNHAP
                    WHERE MAPHIEUNHAP = @MAPHIEUNHAP
                      AND MASP = @MASP";

                SqlParameter[] p =
                {
                    new SqlParameter("@MAPHIEUNHAP", maphieunhap),
                    new SqlParameter("@MASP", masp)
                };

                int rows = Connect.ExecuteNonQuery(sql, p);

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi xóa chi tiết nhập: " + ex.Message);
            }
        }


        public bool KiemTraPhieuNhapTonTai(string maphieunhap)
        {
            try
            {
                string sql = @"SELECT COUNT(*)
                FROM PHIEUNHAPKHO
                WHERE MAPHIEUNHAP = @MAPHIEUNHAP";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAPHIEUNHAP", maphieunhap.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                return dt.Rows.Count > 0 &&Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra phiếu nhập: " + ex.Message);
            }
        }


        public bool KiemTraSanPhamTonTai(string masp)
        {
            try
            {
                string sql = @"SELECT COUNT(*) FROM SANPHAM
                        WHERE MASP = @MASP";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MASP", masp.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                return dt.Rows.Count > 0 &&
                       Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi kiểm tra sản phẩm: " + ex.Message);
            }
        }
    }
}