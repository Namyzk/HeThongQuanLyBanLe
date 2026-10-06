using DAL;
using Models;
using System;
using System.Data;

namespace BLL
{
    public class KhachHang_BLL
    {
        private readonly KhachHang_DAL KH_DAL;

        public KhachHang_BLL(KhachHang_DAL KH_DAL)
        {
            this.KH_DAL = KH_DAL;
        }

        
        public DataTable getAllKH()
        {
            return KH_DAL.getAllKH();
        }


        public DataTable GetByIdKH(string? makh)
        {
            if (string.IsNullOrWhiteSpace(makh))
            {
                throw new Exception(  "Mã khách hàng không được để trống.");
            }

            makh = makh.Trim();

            // MAKH CHAR(15)
            if (makh.Length > 15)
            {
                throw new Exception( "Mã khách hàng không được vượt quá 15 ký tự.");
            }
            return KH_DAL.GetByIdKH(makh);
        }


        public DataTable DeleteByIdKH(string? makh)
        {
            if (string.IsNullOrWhiteSpace(makh))
            {
                throw new Exception( "Mã khách hàng không được để trống.");
            }

            makh = makh.Trim();

            if (makh.Length > 15)
            {
                throw new Exception( "Mã khách hàng không được vượt quá 15 ký tự.");
            }

            return KH_DAL.DeleteByIdKH(makh);
        }


        public DataTable UpdateByIdKH(Models.KhachHang kh)
        {
            ValidateKhachHang(kh);

            return KH_DAL.UpdateByIdKH(kh);
        }


        public DataTable CreateKH(Models.KhachHang kh)
        {
            ValidateKhachHang(kh);

            // Kiểm tra mã khách hàng đã tồn tại
            DataTable dt = KH_DAL.GetByIdKH(kh.MaKH!);

            if (dt != null && dt.Rows.Count > 0)
            {
                throw new Exception(  "Mã khách hàng đã tồn tại.");
            }

            return KH_DAL.CreateKH(kh);
        }


       
        private void ValidateKhachHang(Models.KhachHang kh)
        {
            if (kh == null)
            {
                throw new Exception( "Dữ liệu khách hàng không được để trống.");
            }


            if (string.IsNullOrWhiteSpace(kh.MaKH))
            {
                throw new Exception(  "Mã khách hàng không được để trống.");
            }

            kh.MaKH = kh.MaKH.Trim();

            if (kh.MaKH.Length > 15)
            {
                throw new Exception("Mã khách hàng không được vượt quá 15 ký tự.");
            }



            if (string.IsNullOrWhiteSpace(kh.TenKH))
            {
                throw new Exception(  "Tên khách hàng không được để trống.");
            }

            kh.TenKH = kh.TenKH.Trim();

            if (kh.TenKH.Length > 100)
            {
                throw new Exception(  "Tên khách hàng không được vượt quá 100 ký tự.");
            }

            if (!string.IsNullOrWhiteSpace(kh.SDT))
            {
                kh.SDT = kh.SDT.Trim();

                if (kh.SDT.Length != 10)
                {
                    throw new Exception( "Số điện thoại phải có đúng 10 ký tự.");
                }

                if (!long.TryParse(kh.SDT, out _))
                {
                    throw new Exception( "Số điện thoại chỉ được chứa chữ số.");
                }
            }



            if (!string.IsNullOrWhiteSpace(kh.DiaChi))
            {
                kh.DiaChi = kh.DiaChi.Trim();

                if (kh.DiaChi.Length > 300)
                {
                    throw new Exception( "Địa chỉ không được vượt quá 300 ký tự.");
                }
            }
        }
    }
}