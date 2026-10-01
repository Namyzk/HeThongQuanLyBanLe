using Microsoft.Data.SqlClient;
using DAL.DataHelper;
using Models;
using System;
using System.Data;

namespace DAL
{
    public class NhaCungCap_DAL
    {
        
        public DataTable GetAll()
        {
            try
            {
                DataTable dt = Connect.ExecuteStoredProcedure( "dbo.sp_GetNhaCungCap" );

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Lỗi khi lấy danh sách: " + ex.Message
                );
            }
        }


        public DataTable GetById(string ma)
        {
            try
            {
                SqlParameter[] parameters = {  new SqlParameter("@MANCC", SqlDbType.Char, 15) 
                {
                        Value = ma?.Trim() ?? ""
                }
 };

                DataTable dt = Connect.ExecuteStoredProcedure( "dbo.sp_GetByIdNhaCungCap", parameters );

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy: " + ex.Message);
            }
        }

        public DataTable Create(Models.NhaCungCap model)
        {
            try
            {
                if (model == null)
                {
                    throw new Exception("Thông tin nhà cung cấp không được để trống.");
                }

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MANCC",   model.MaNCC?.Trim() ?? ""),

                    new SqlParameter("@TENNCC",model.TenNCC?.Trim() ?? ""),

                    new SqlParameter( "@DIACHI", model.DiaChi?.Trim() ?? ""),

                    new SqlParameter( "@SDT", model.SDT?.Trim() ?? ""),

                    new SqlParameter( "@EMAIL", model.EMAIL?.Trim() ?? "")
                };

                DataTable dt = Connect.ExecuteStoredProcedure("SP_THEMNCC", parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi thêm: " + ex.Message);
            }
        }



        public DataTable Update(Models.NhaCungCap model)
        {
            try
            {
                if (model == null)
                {
                    throw new Exception("Thông tin nhà cung cấp không được để trống.");
                }

                SqlParameter[] parameters =
                {
                    new SqlParameter(  "@MANCC", model.MaNCC?.Trim() ?? ""),
                    new SqlParameter( "@TENNCC", model.TenNCC?.Trim() ?? ""),
                    new SqlParameter(  "@DIACHI", model.DiaChi?.Trim() ?? ""),
                    new SqlParameter( "@SDT", model.SDT?.Trim() ?? ""),
                    new SqlParameter( "@EMAIL", model.EMAIL?.Trim() ?? "")
                };

                DataTable dt = Connect.ExecuteStoredProcedure("SP_SUANCC",   parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi sửa: " + ex.Message);
            }
        }


        public DataTable Delete(string ma)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@MANCC", ma)
                };

                DataTable dt = Connect.ExecuteStoredProcedure("SP_XOANCC",  parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi xoá: " + ex.Message);
            }
        }
    }
}