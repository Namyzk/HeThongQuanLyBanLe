using DAL.DataHelper;
using Microsoft.Data.SqlClient;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAL
{
    public class TaiKhoan_DAL
    {
        public bool KiemTraUsernameTonTai(string username)
        {
            try
            {
                string sql = @"SELECT COUNT(*) AS SoLuong
                               FROM dbo.TAIKHOAN
                               WHERE RTRIM(USERNAME) = @USERNAME";

                SqlParameter[] p = { new SqlParameter("@USERNAME", username.Trim()) };
                DataTable dt = Connect.ExecuteQuery(sql, p);

                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["SoLuong"]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra tên đăng nhập: " + ex.Message);
            }
        }

        public bool KiemTraUsernameKhacTaiKhoan(string username, string maTK)
        {
            try
            {
                string sql = @"SELECT COUNT(*) AS SoLuong
                               FROM dbo.TAIKHOAN
                               WHERE RTRIM(USERNAME) = @USERNAME
                                 AND RTRIM(MATAIKHOAN) <> @MATAIKHOAN";

                SqlParameter[] p =
                {
                    new SqlParameter("@USERNAME", username.Trim()),
                    new SqlParameter("@MATAIKHOAN", maTK.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, p);
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["SoLuong"]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra tên đăng nhập: " + ex.Message);
            }
        }

        public bool KiemTraTonTai(string maTK)
        {
            try
            {
                string sql = "SELECT COUNT(*) AS SoLuong FROM dbo.TAIKHOAN WHERE RTRIM(MATAIKHOAN) = @MATAIKHOAN";
                SqlParameter[] p = { new SqlParameter("@MATAIKHOAN", maTK.Trim()) };
                var dt = Connect.ExecuteQuery(sql, p);
                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0]["SoLuong"]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi kiểm tra tồn tại tài khoản: " + ex.Message);
            }
        }

        public List<TaiKhoan> GetAll()
        {
            try
            {
                var list = new List<TaiKhoan>();
                string sql = @"SELECT RTRIM(MATAIKHOAN) AS MATAIKHOAN,
                                      RTRIM(USERNAME) AS USERNAME,
                                      RTRIM(PASS) AS PASS,
                                      QUYEN
                               FROM dbo.TAIKHOAN";
                var dt = Connect.ExecuteQuery(sql);

                foreach (DataRow r in dt.Rows)
                {
                    list.Add(new TaiKhoan
                    {
                        MATAIKHOAN = r["MATAIKHOAN"].ToString(),
                        USERNAME = r["USERNAME"].ToString(),
                        PASS = r["PASS"].ToString(),
                        QUYEN = r["QUYEN"] == DBNull.Value ? 0 : Convert.ToInt32(r["QUYEN"])
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy danh sách tài khoản: " + ex.Message);
            }
        }

        public List<TaiKhoan> GetByID(string maTK)
        {
            try
            {
                var list = new List<TaiKhoan>();
                string sql = @"SELECT RTRIM(MATAIKHOAN) AS MATAIKHOAN,
                                      RTRIM(USERNAME) AS USERNAME,
                                      RTRIM(PASS) AS PASS,
                                      QUYEN
                               FROM dbo.TAIKHOAN
                               WHERE RTRIM(MATAIKHOAN) = @MATAIKHOAN";
                SqlParameter[] p = { new SqlParameter("@MATAIKHOAN", maTK.Trim()) };
                var dt = Connect.ExecuteQuery(sql, p);

                foreach (DataRow r in dt.Rows)
                {
                    list.Add(new TaiKhoan
                    {
                        MATAIKHOAN = r["MATAIKHOAN"].ToString(),
                        USERNAME = r["USERNAME"].ToString(),
                        PASS = r["PASS"].ToString(),
                        QUYEN = r["QUYEN"] == DBNull.Value ? 0 : Convert.ToInt32(r["QUYEN"])
                    });
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy tài khoản theo mã: " + ex.Message);
            }
        }

        public List<TaiKhoan> Login(string username, string password)
        {
            try
            {
                var list = new List<TaiKhoan>();
                // Dùng RTRIM để triệt tiêu các khoảng trắng do kiểu dữ liệu CHAR(20) gây ra
                string sql = @"SELECT TOP 1 RTRIM(MATAIKHOAN) AS MATAIKHOAN,
                                            RTRIM(USERNAME) AS USERNAME,
                                            RTRIM(PASS) AS PASS,
                                            QUYEN
                               FROM dbo.TAIKHOAN
                               WHERE RTRIM(USERNAME) = @USERNAME AND RTRIM(PASS) = @PASS";

                SqlParameter[] p =
                {
                    new SqlParameter("@USERNAME", username.Trim()),
                    new SqlParameter("@PASS", password.Trim())
                };

                DataTable dt = Connect.ExecuteQuery(sql, p);
                foreach (DataRow r in dt.Rows)
                {
                    list.Add(new TaiKhoan
                    {
                        MATAIKHOAN = r["MATAIKHOAN"].ToString(),
                        USERNAME = r["USERNAME"].ToString(),
                        PASS = r["PASS"].ToString(),
                        QUYEN = r["QUYEN"] == DBNull.Value ? 0 : Convert.ToInt32(r["QUYEN"])
                    });
                }

                return list;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi đăng nhập: " + ex.Message);
            }
        }

        public int GetRoleByUsername(string username)
        {
            try
            {
                string sql = "SELECT TOP 1 QUYEN FROM dbo.TAIKHOAN WHERE RTRIM(USERNAME) = @USERNAME";
                SqlParameter[] p = { new SqlParameter("@USERNAME", username.Trim()) };

                DataTable dt = Connect.ExecuteQuery(sql, p);
                if (dt.Rows.Count > 0 && dt.Rows[0]["QUYEN"] != DBNull.Value)
                    return Convert.ToInt32(dt.Rows[0]["QUYEN"]);

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public bool Insert(TaiKhoan tk)
        {
            try
            {
                string sql = @"INSERT INTO dbo.TAIKHOAN (MATAIKHOAN, USERNAME, PASS, QUYEN)
                               VALUES (@MATAIKHOAN, @USERNAME, @PASS, @QUYEN)";
                SqlParameter[] p =
                {
                    new SqlParameter("@MATAIKHOAN", tk.MATAIKHOAN.Trim()),
                    new SqlParameter("@USERNAME",   tk.USERNAME.Trim()),
                    new SqlParameter("@PASS",       tk.PASS.Trim()),
                    new SqlParameter("@QUYEN",      tk.QUYEN)
                };
                int rows = Connect.ExecuteNonQuery(sql, p);
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm tài khoản: " + ex.Message);
            }
        }

        public bool Update(TaiKhoan tk)
        {
            try
            {
                if (tk == null || string.IsNullOrWhiteSpace(tk.MATAIKHOAN))
                    return false;

                string sql = @"UPDATE dbo.TAIKHOAN
                               SET USERNAME = @USERNAME,
                                   PASS = @PASS,
                                   QUYEN = @QUYEN
                               WHERE RTRIM(MATAIKHOAN) = @MATAIKHOAN";
                SqlParameter[] p =
                {
                    new SqlParameter("@MATAIKHOAN", tk.MATAIKHOAN.Trim()),
                    new SqlParameter("@USERNAME", tk.USERNAME.Trim()),
                    new SqlParameter("@PASS", tk.PASS.Trim()),
                    new SqlParameter("@QUYEN", tk.QUYEN)
                };
                int rows = Connect.ExecuteNonQuery(sql, p);
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi sửa tài khoản: " + ex.Message);
            }
        }

        public bool Delete(string maTK)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maTK))
                    return false;

                string sql = "DELETE FROM dbo.TAIKHOAN WHERE RTRIM(MATAIKHOAN) = @MATAIKHOAN";
                SqlParameter[] p = { new SqlParameter("@MATAIKHOAN", maTK.Trim()) };
                int rows = Connect.ExecuteNonQuery(sql, p);
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xoá tài khoản: " + ex.Message);
            }
        }
    }
}