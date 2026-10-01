using Microsoft.Data.SqlClient;
using DAL.DataHelper;
using Models;
using System;
using System.Data;

namespace DAL
{
    public class KhachHang_DAL
    {


        public DataTable getAllKH()
        {
            try
            {
                DataTable dt = Connect.ExecuteStoredProcedure( "dbo.sp_GetKhachHang" );

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy danh sách: " + ex.Message);
            }
        }




        public DataTable GetByIdKH(string makh)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAKH", makh.Trim())
                };

                return Connect.ExecuteStoredProcedure( "dbo.sp_GetByIDKhachHang", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy khách hàng: " + ex.Message);
            }
        }




        public DataTable CreateKH(Models.KhachHang kh)
        {
            try
            {
                if (kh == null)
                    throw new Exception(  "Thông tin khách hàng không được để trống.");

                SqlParameter[] parameters =
                {
                    new SqlParameter(  "@MAKH", kh.MaKH?.Trim() ?? ""),

                    new SqlParameter( "@TENKH", kh.TenKH?.Trim() ?? ""),

                    new SqlParameter( "@SDT", kh.SDT?.Trim() ?? ""),

                    new SqlParameter( "@DIACHI", kh.DiaChi?.Trim() ?? "")
                };

                DataTable dt = Connect.ExecuteStoredProcedure(  "SP_THEMKH",    parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi thêm: " + ex.Message);
            }
        }


        

        public DataTable UpdateByIdKH(Models.KhachHang kh)
        {
            try
            {
                if (kh == null)
                    throw new Exception( "Thông tin khách hàng không được để trống.");

                SqlParameter[] parameters =
                {
                    new SqlParameter(  "@MAKH", kh.MaKH?.Trim() ?? ""),
                    new SqlParameter( "@TENKH", kh.TenKH?.Trim() ?? ""),
                    new SqlParameter( "@SDT", kh.SDT?.Trim() ?? ""),
                    new SqlParameter( "@DIACHI", kh.DiaChi?.Trim() ?? "")
                };

                DataTable dt = Connect.ExecuteStoredProcedure(  "SP_SUAKH",    parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi sửa: " + ex.Message);
            }
        }

        public DataTable DeleteByIdKH(string makh)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAKH", makh)
                };

                DataTable dt = Connect.ExecuteStoredProcedure(  "SP_XOAKH",    parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi xoá: " + ex.Message);
            }
        }


    }
}