using System.Data;
using DAL.DataHelper;
using Models;
using System;
using Microsoft.Data.SqlClient; 
namespace DAL
{
    public class ThanhToan_DAL
    {
       
        public DataTable getAll()
        {
            try
            {
                DataTable dt = Connect.ExecuteStoredProcedure(  "dbo.sp_GetThanhToan" );

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi lấy danh sách: " + ex.Message
                );
            }
        }


        public DataTable GetById(string ma)
        {
            try
            {
                SqlParameter[] parameters =
                {
                     new SqlParameter("@MATHANHTOAN", SqlDbType.Char, 15)
                {
                Value = ma.Trim()
            }
        };

                DataTable dt = Connect.ExecuteStoredProcedure(  "dbo.sp_GetByIdThanhToan",     parameters
                );

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy thông tin thanh toán: " + ex.Message
                );
            }
        }
        public bool KiemTraHoaDonTonTai(string maHDBan)
        {
            try
            {
                string sql = @"
                    SELECT COUNT(*)
                    FROM dbo.HOADONBAN
                    WHERE RTRIM(MAHDBAN) = RTRIM(@MAHDBAN)";

                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAHDBAN", SqlDbType.Char, 15)
                    {
                        Value = maHDBan.Trim()
                    }
                };
                DataTable dt = Connect.ExecuteQuery(sql, parameters);

                return dt.Rows.Count > 0 && Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi kiểm tra hóa đơn: " + ex.Message
                );
            }
        }

        public DataTable Create(Models.ThanhToan model)
        {
            try
            {
                SqlParameter[] parameters =
                {
                     new SqlParameter("@MaThanhToan", SqlDbType.Char, 15)
                {
                Value = model.MaThanhToan?.Trim() ?? ""  },

                new SqlParameter("@MaHDBan", SqlDbType.Char, 15)
                {
                    Value = model.MaHDBan?.Trim() ?? ""
                },

                new SqlParameter("@PhuongThuc", SqlDbType.NVarChar, 50)
                {
                    Value = model.PhuongThuc?.Trim() ?? ""
                },

                new SqlParameter("@SoTienThanhToan", SqlDbType.Float)
                {
                      Value = model.SoTienThanhToan
                },
                new SqlParameter("@NgayThanhToan", SqlDbType.DateTime)
                {
                      Value = model.NgayThanhToan
                },

                new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50)
                {
                      Value = model.TrangThai?.Trim() ?? ""
                }};

                return Connect.ExecuteStoredProcedure( "dbo.SP_THEMTT",  parameters  );
            }
            catch (Exception ex)
            {
                throw new Exception(   "Lỗi khi thêm: " + ex.Message
                );
            }
        }

        public DataTable Update(Models.ThanhToan model)
        {
            try
            {
                SqlParameter[] parameters =
                {
                new SqlParameter("@MaThanhToan", SqlDbType.Char, 15)
                {
                    Value = model.MaThanhToan?.Trim() ?? ""
                },
                new SqlParameter("@PhuongThuc", SqlDbType.NVarChar, 50)
                {
                    Value = model.PhuongThuc?.Trim() ?? ""
                },
                new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50)
                {
                    Value = model.TrangThai?.Trim() ?? ""
                } };

                return Connect.ExecuteStoredProcedure(  "dbo.SP_SUATT",    parameters  );
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi sửa: " + ex.Message);
            }
        }
       
        public DataTable Delete(string ma)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MAThanhToan",  ma)
                };

                DataTable dt = Connect.ExecuteStoredProcedure("SP_XOAtt", parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi xoá: " + ex.Message);
            }
        }


        public DataTable GetHoaDonChuaThanhToan()
        {
            try
            {
                string sql = @"
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
                    OR T.TRANGTHAI <> N'Đã thanh toán'
            ";

                return Connect.ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi lấy danh sách hóa đơn chưa thanh toán: " + ex.Message);
            }
        }

        public DataTable GetHoaDonChuaThanhToanTheoTen(string tenKh)
        {
            try
            {
                SqlParameter[] parameters =
                {
                new SqlParameter("@TENKH", SqlDbType.NVarChar, 100)
                {
                    Value = tenKh.Trim()
                } };

                return Connect.ExecuteStoredProcedure("dbo.SP_LAY_HOADON_CHUA_THANHTOAN_THEO_TENKH", parameters );
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy danh sách hóa đơn chưa thanh toán theo tên khách hàng: " + ex.Message);
            }
        }


        public int UpdateTrangThaiThanhToan(string maHDBan,string phuongThuc)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MAHDBAN", SqlDbType.Char, 15)
                {
                     Value = maHDBan.Trim()
                },

                new SqlParameter( "@PHUONGTHUC", SqlDbType.NVarChar, 50)
                {
                    Value = phuongThuc.Trim()
                }};

                int result =  Connect.ExecuteStoredProcedureReturnValue("dbo.SP_CAPNHAT_TRANGTHAI_THANHTOANTHANHCONG",   parameters);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi cập nhật trạng thái thanh toán: "  + ex.Message);
            }
        }



        public DataTable GetByHoaDon(string maHDBan)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter(  "@MAHDBAN",    maHDBan?.Trim() ?? "")
                };

                DataTable dt = Connect.ExecuteStoredProcedure(   "SP_GET_THANHTOAN_BY_MAHDBAN",   parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy danh sách thanh toán theo hóa đơn: "+ ex.Message);
            }
        }


      
        public DataTable ResetSoTienByHoaDon(string maHDBan, decimal soTienMoi = 0)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MAHDBAN",  maHDBan?.Trim() ?? ""),
                    new SqlParameter( "@SoTienMoi", soTienMoi)
                };

                Connect.ExecuteStoredProcedureNonQuery(  "SP_RESET_SOTIENTHANHTOAN_BY_MAHDBAN",  parameters);

                // Lấy lại dữ liệu sau khi reset
                return GetByHoaDon(maHDBan);
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi reset số tiền thanh toán: "   + ex.Message);
            }
        }
    }
}