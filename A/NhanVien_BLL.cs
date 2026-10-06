using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BLL
{
    public class NhanVien_BLL
    {
        private readonly NhanVien_DAL nv_dal;

        public NhanVien_BLL(NhanVien_DAL nv_dal)
        {
            this.nv_dal = nv_dal;
        }

        public List<NhanVien> LayTatCa()
        {
            var list = nv_dal.GetAll();

            return list ?? new List<NhanVien>();
        }

        public List<NhanVien> LayTheoID(string manv)
        {
            if (string.IsNullOrWhiteSpace(manv))
                throw new Exception("Mã nhân viên không được để trống.");

            manv = manv.Trim();

            if (manv.Length > 15)
                throw new Exception("Mã nhân viên không được vượt quá 15 ký tự.");

            if (!nv_dal.KiemTraTonTai(manv))
                return new List<NhanVien>();

            return nv_dal.GetByID(manv);
        }

        public bool ThemMoi(NhanVien nv)
        {
            ValidateNhanVien(nv);

            nv.MANV = nv.MANV.Trim();
            nv.TENNV = nv.TENNV.Trim();
            nv.SDT = nv.SDT.Trim();
            nv.DIACHI = nv.DIACHI.Trim();

            if (nv_dal.KiemTraTonTai(nv.MANV))
                throw new Exception("Mã nhân viên đã tồn tại.");

            if (nv_dal.KiemTraSDTTonTai(nv.SDT))
                throw new Exception("Số điện thoại đã tồn tại.");

            return nv_dal.Insert(nv);
        }

        public bool CapNhat(NhanVien nv)
        {
            ValidateNhanVien(nv);

            nv.MANV = nv.MANV.Trim();
            nv.TENNV = nv.TENNV.Trim();
            nv.SDT = nv.SDT.Trim();
            nv.DIACHI = nv.DIACHI.Trim();

            if (!nv_dal.KiemTraTonTai(nv.MANV))
                throw new Exception("Không tìm thấy nhân viên.");

            if (nv_dal.KiemTraSDTKhacNhanVien(nv.SDT, nv.MANV))
                throw new Exception("Số điện thoại đã tồn tại.");

            return nv_dal.Update(nv);
        }

        public bool Xoa(string manv)
        {
            if (string.IsNullOrWhiteSpace(manv))
                throw new Exception(
                    "Mã nhân viên không được để trống.");

            manv = manv.Trim();

            if (manv.Length > 15)
                throw new Exception(
                    "Mã nhân viên không được vượt quá 15 ký tự.");

            if (!nv_dal.KiemTraTonTai(manv))
                throw new Exception("Không tìm thấy nhân viên.");

            return nv_dal.Delete(manv);
        }

        private void ValidateNhanVien(NhanVien nv)
        {
            if (nv == null)
                throw new Exception(  "Dữ liệu nhân viên không được để trống.");

            // MANV
            if (string.IsNullOrWhiteSpace(nv.MANV))
                throw new Exception( "Mã nhân viên không được để trống.");

            nv.MANV = nv.MANV.Trim();

            if (nv.MANV.Length > 15)
                throw new Exception( "Mã nhân viên không được vượt quá 15 ký tự.");

            // TENNV
            if (string.IsNullOrWhiteSpace(nv.TENNV))
                throw new Exception(  "Tên nhân viên không được để trống.");

            nv.TENNV = nv.TENNV.Trim();

            if (nv.TENNV.Length > 50)
                throw new Exception( "Tên nhân viên không được vượt quá 50 ký tự.");

            // SDT
            if (string.IsNullOrWhiteSpace(nv.SDT))
                throw new Exception("Số điện thoại không được để trống.");

            nv.SDT = nv.SDT.Trim();

            if (!Regex.IsMatch(nv.SDT, @"^\d+$"))
                throw new Exception("Số điện thoại chỉ được chứa chữ số.");

            if (nv.SDT.Length != 10)
                throw new Exception( "Số điện thoại phải gồm 10 chữ số.");

            // DIACHI
            if (string.IsNullOrWhiteSpace(nv.DIACHI))
                throw new Exception(  "Địa chỉ không được để trống.");

            nv.DIACHI = nv.DIACHI.Trim();

            if (nv.DIACHI.Length > 100)
                throw new Exception( "Địa chỉ không được vượt quá 100 ký tự.");
        }
    }
}