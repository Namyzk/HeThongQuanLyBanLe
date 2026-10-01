using Microsoft.Data.SqlClient;
using DAL.DataHelper;
using Models;
using System;
using System.Data;

namespace DAL
{
    public class KhuyenMai_DAL
    {


        public DataTable getAll()
        {
            try
            {
                DataTable dt = Connect.ExecuteStoredProcedure( "sp_GetKhuyenMai");

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi lấy danh sách: " + ex.Message);
            }
        }




        public DataTable GetById(string ma)
        {
            try
            {
                SqlParameter[] parameters =  {   new SqlParameter("@MAKM", ma) };

                return Connect.ExecuteStoredProcedure(  "dbo.SP_GetByIDKM", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi lấy: " + ex.Message);
            }
        }
        public DataTable Delete(string ma)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@MAKM", ma)
                };

                return Connect.ExecuteStoredProcedure( "dbo.SP_XOAKM",parameters);
            }
            catch (Exception ex)
            {
                throw new Exception(  "Lỗi khi xoá: " + ex.Message);
            }
        }

        public DataTable CheckSanPham(string maSP)
        {
            try
            {
                SqlParameter[] parameters = { new SqlParameter("@MASP", maSP)  };

                return Connect.ExecuteStoredProcedure( "dbo.sp_CheckSanPham", parameters);
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi kiểm tra sản phẩm: " + ex.Message);
            }
        }


        public DataTable Create(Models.KhuyenMai model)
        {
            try
            {
                if (model == null)
                    throw new Exception("Thông tin khuyến mại không được để trống.");

                SqlParameter[] parameters =
                {
                    new SqlParameter( "@MAKM", model.MaKM?.Trim() ?? ""),
                    new SqlParameter("@TENKM", model.TenKM?.Trim() ?? ""),
                    new SqlParameter("@MASP", model.MaSP?.Trim() ?? ""),
                    new SqlParameter("@NGAYBATDAU", model.NgayBD),
                    new SqlParameter("@NGAYKETTHUC", model.NgayKT)
                };
                DataTable dt = Connect.ExecuteStoredProcedure(   "SP_THEMKM",  parameters);
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi thêm: " + ex.Message);
            }
        }


        public DataTable Update(Models.KhuyenMai model)
        {
            try
            {
                if (model == null)
                    throw new Exception( "Thông tin khuyến mại không được để trống.");

                SqlParameter[] parameters =
                {
                    new SqlParameter(   "@MAKM",  model.MaKM?.Trim() ?? ""),
                    new SqlParameter(  "@TENKM",  model.TenKM?.Trim() ?? ""),
                    new SqlParameter(  "@MASP",  model.MaSP?.Trim() ?? ""),
                    new SqlParameter( "@NGAYBATDAU",   model.NgayBD),
                    new SqlParameter("@NGAYKETTHUC",  model.NgayKT)
                };

                DataTable dt = Connect.ExecuteStoredProcedure( "SP_SUAKM", parameters);

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception( "Lỗi khi sửa: " + ex.Message);
            }
        }

    }
}