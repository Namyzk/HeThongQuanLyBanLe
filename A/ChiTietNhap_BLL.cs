using DAL;
using Models;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class ChiTietNhap_BLL
    {
        private readonly ChiTietNhap_DAL ctn_dal;

        public ChiTietNhap_BLL(ChiTietNhap_DAL ctn_dal)
        {
            this.ctn_dal = ctn_dal;
        }


        public List<ChiTietNhap> LayTatCa()
        {
            var list = ctn_dal.GetAll();

            return (list == null || list.Count == 0) ? new List<ChiTietNhap>() : list;
        }

        public List<ChiTietNhap> LayTheoPhieu(string maphieunhap)
        {
            if (string.IsNullOrWhiteSpace(maphieunhap))
                return new List<ChiTietNhap>();

            maphieunhap = maphieunhap.Trim();

            return ctn_dal.GetByPhieu(maphieunhap);
        }


        public List<ChiTietNhap> LayTheoID( string maphieunhap,string masp)
        {
            if (string.IsNullOrWhiteSpace(maphieunhap) ||
                string.IsNullOrWhiteSpace(masp))
                return null;

            maphieunhap = maphieunhap.Trim();
            masp = masp.Trim();

            if (!ctn_dal.KiemTraTonTai(maphieunhap, masp))
                return null;

            return ctn_dal.GetById(maphieunhap, masp);
        }


        private string ValidateChiTiet(  ChiTietNhap ctn,   bool isUpdate = false)
        {
            if (ctn == null)
                return "Thông tin chi tiết nhập không được để trống.";

            // MÃ PHIẾU
            if (string.IsNullOrWhiteSpace(ctn.MAPHIEUNHAP))
                return "Mã phiếu nhập không được để trống.";

            ctn.MAPHIEUNHAP = ctn.MAPHIEUNHAP.Trim();

            if (ctn.MAPHIEUNHAP.Length > 15)
                return "Mã phiếu nhập không được vượt quá 15 ký tự.";

            // MÃ SẢN PHẨM
            if (string.IsNullOrWhiteSpace(ctn.MASP))
                return "Mã sản phẩm không được để trống.";

            ctn.MASP = ctn.MASP.Trim();

            if (ctn.MASP.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            // KIỂM TRA PHIẾU
            if (!ctn_dal.KiemTraPhieuNhapTonTai(ctn.MAPHIEUNHAP))
            {
                return $"Phiếu nhập có mã '{ctn.MAPHIEUNHAP}' không tồn tại.";
            }

            // KIỂM TRA SẢN PHẨM
            if (!ctn_dal.KiemTraSanPhamTonTai(ctn.MASP))
            {
                return $"Sản phẩm có mã '{ctn.MASP}' không tồn tại.";
            }

            // SỐ LƯỢNG
            if (ctn.SOLUONG <= 0)
                return "Số lượng nhập phải lớn hơn 0.";

            // ĐƠN GIÁ
            if (ctn.DONGIANHAP < 0)
                return "Đơn giá nhập không được nhỏ hơn 0.";

            // THÀNH TIỀN
            ctn.THANHTIEN =
                ctn.SOLUONG * ctn.DONGIANHAP;

            if (ctn.THANHTIEN < 0)
                return "Thành tiền không hợp lệ.";

            // KIỂM TRA TRÙNG
            if (!isUpdate &&
                ctn_dal.KiemTraTonTai(  ctn.MAPHIEUNHAP,    ctn.MASP))
            {
                return $"Sản phẩm '{ctn.MASP}' đã tồn tại trong phiếu nhập '{ctn.MAPHIEUNHAP}'.";
            }

            return null;
        }



        public string ThemMoi(ChiTietNhap ctn)
        {
            string error = ValidateChiTiet(ctn, false);

            if (error != null)
                return error;

            try
            {
                return ctn_dal.Insert(ctn)  ? null   : "Không thể thêm chi tiết nhập.";
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }


    
        public string CapNhat(ChiTietNhap ctn)
        {
            string error = ValidateChiTiet(ctn, true);

            if (error != null)
                return error;

            if (!ctn_dal.KiemTraTonTai( ctn.MAPHIEUNHAP,  ctn.MASP))
            {
                return $"Không tìm thấy chi tiết sản phẩm '{ctn.MASP}' trong phiếu nhập '{ctn.MAPHIEUNHAP}'.";
            }

            try
            {
                return ctn_dal.Update(ctn)  ? null     : "Không thể cập nhật chi tiết nhập.";
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }


       

        public string Xoa( string maphieunhap,  string masp)
        {
            if (string.IsNullOrWhiteSpace(maphieunhap))
                return "Mã phiếu nhập không được để trống.";

            if (string.IsNullOrWhiteSpace(masp))
                return "Mã sản phẩm không được để trống.";

            maphieunhap = maphieunhap.Trim();
            masp = masp.Trim();

            if (maphieunhap.Length > 15)
                return "Mã phiếu nhập không được vượt quá 15 ký tự.";

            if (masp.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            if (!ctn_dal.KiemTraTonTai(
                maphieunhap,
                masp))
            {
                return $"Không tìm thấy chi tiết sản phẩm '{masp}' trong phiếu nhập '{maphieunhap}'.";
            }

            try
            {
                return ctn_dal.Delete( maphieunhap,  masp)  ? null  : "Không thể xoá chi tiết nhập.";
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }
    }
}
