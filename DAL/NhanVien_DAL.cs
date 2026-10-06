using Microsoft.Data.SqlClient;
using DAL.DataHelper;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class NhanVien_DAL
    {
       
      
        public bool KiemTraTonTai(string manv)
        {
            try
            {
                string sql = @" SELECT COUNT(*) AS SoLuong   FROM NHANVIEN   WHERE MANV = @MANV";
                SqlParameter[] parameters = { new SqlParameter("@MANV", manv)   };
                DataTable dt = Connect.ExecuteQuery(  sql,  parameters);

                if (dt.Rows.Count > 0)
                {
                    int count = Convert.ToInt32( dt.Rows[0]["SoLuong"]);
                    return count > 0;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi kiểm tra tồn tại nhân viên: "     + ex.Message);
            }
        }

        public bool KiemTraSDTKhacNhanVien(string sdt, string manv)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SDT", sdt),
                    new SqlParameter("@MANV", manv)
                };
                DataTable dt = Connect.ExecuteStoredProcedure(  "dbo.SP_KIEMTRASDTKHAC",    parameters);
                return dt.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi kiểm tra số điện thoại: " + ex.Message);
            }
        }

        public bool KiemTraSDTTonTai(string sdt)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@SDT", sdt)
                };

                DataTable dt = Connect.ExecuteStoredProcedure("dbo.SP_KIEMTRASDT",  parameters);
                return dt.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi kiểm tra số điện thoại: " + ex.Message);
            }
        }
       
        // LẤY TẤT CẢ NHÂN VIÊN

        public List<NhanVien> GetAll()
        {
            try
            {
                List<NhanVien> list = new List<NhanVien>();

                string sql = @" SELECT MANV, TENNV, SDT, DIACHI FROM NHANVIEN";
                DataTable dt = Connect.ExecuteQuery(sql);
                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new NhanVien
                    {
                        MANV = row["MANV"]?.ToString()?.Trim(),
                        TENNV = row["TENNV"]?.ToString()?.Trim(),
                        SDT = row["SDT"]?.ToString()?.Trim(),
                        DIACHI = row["DIACHI"]?.ToString()?.Trim()
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy danh sách nhân viên: "   + ex.Message);
            }
        }


       
        // LẤY NHÂN VIÊN THEO MÃ
        

        public List<NhanVien> GetByID(string manv)
        {
            try
            {
                List<NhanVien> list = new List<NhanVien>();
                string sql = @" SELECT MANV, TENNV, SDT, DIACHI FROM NHANVIEN WHERE MANV = @MANV";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MANV", manv)
                };

                DataTable dt = Connect.ExecuteQuery( sql,  parameters);

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new NhanVien
                    {
                        MANV = row["MANV"]?.ToString()?.Trim(),
                        TENNV = row["TENNV"]?.ToString()?.Trim(),
                        SDT = row["SDT"]?.ToString()?.Trim(),
                        DIACHI = row["DIACHI"]?.ToString()?.Trim()
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi lấy nhân viên theo mã: "  + ex.Message);
            }
        }

        // THÊM NHÂN VIÊN
       

        public bool Insert(NhanVien nv)
        {
            try
            {
                if (nv == null)  return false;

                string sql = @" INSERT INTO NHANVIEN  ( MANV, TENNV, SDT, DIACHI)
                    VALUES ( @MANV,  @TENNV,   @SDT, @DIACHI  )";

                SqlParameter[] parameters =
                {
                    new SqlParameter(  "@MANV",   nv.MANV?.Trim() ?? ""),
                    new SqlParameter( "@TENNV",    nv.TENNV?.Trim() ?? ""),
                    new SqlParameter(  "@SDT",   nv.SDT?.Trim() ?? ""),
                    new SqlParameter(  "@DIACHI",   nv.DIACHI?.Trim() ?? "")
                };
                int rows = Connect.ExecuteNonQuery( sql, parameters);
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi thêm nhân viên: "   + ex.Message);
            }
        }

       
        // SỬA NHÂN VIÊN
        

        public bool Update(NhanVien nv)
        {
            try
            {
                if (nv == null || string.IsNullOrWhiteSpace(nv.MANV))
                {
                    return false;
                }

                if (!KiemTraTonTai(nv.MANV))
                {
                    return false;
                }

                string sql = @"  UPDATE NHANVIEN
                    SET
                        TENNV = @TENNV,
                        SDT = @SDT,
                        DIACHI = @DIACHI
                    WHERE MANV = @MANV";

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MANV", nv.MANV.Trim()),
                    new SqlParameter( "@TENNV", nv.TENNV?.Trim() ?? ""),
                    new SqlParameter( "@SDT", nv.SDT?.Trim() ?? ""),
                    new SqlParameter( "@DIACHI", nv.DIACHI?.Trim() ?? "")
                };

                int rows = Connect.ExecuteNonQuery( sql, parameters);

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi sửa nhân viên: "   + ex.Message);
            }
        }

       
        // XOÁ NHÂN VIÊN
       

        public bool Delete(string manv)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(manv))
                {
                    return false;
                }

                manv = manv.Trim();

                if (!KiemTraTonTai(manv))
                {
                    return false;
                }

                string sql = @"  DELETE FROM NHANVIEN  WHERE MANV = @MANV";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MANV", manv)
                };

                int rows = Connect.ExecuteNonQuery( sql,  parameters);

                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi xoá nhân viên: "   + ex.Message);
            }
        }
    }
}