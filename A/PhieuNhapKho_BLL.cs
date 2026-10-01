using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class PhieuNhapKho_BLL
    {
        private readonly PhieuNhapKho_DAL pnk_dal;

        public PhieuNhapKho_BLL()
        {
            pnk_dal = new PhieuNhapKho_DAL();
        }


        public List<PhieuNhapKho> LayTatCa()
        {
            var list = pnk_dal.GetAll();

            return (list == null || list.Count == 0) ? new List<PhieuNhapKho>() : list;
        }


        public List<PhieuNhapKho> LayTheoID(string maphieunhap)
        {
            if (string.IsNullOrWhiteSpace(maphieunhap))
                return null;

            maphieunhap = maphieunhap.Trim();

            if (!pnk_dal.KiemTraTonTai(maphieunhap))
                return null;

            return pnk_dal.GetByID(maphieunhap);
        }



        private string ValidatePhieuNhap( PhieuNhapKho pnk,  bool isUpdate = false)
        {
            if (pnk == null)
                return "Thông tin phiếu nhập không được để trống.";

            // MÃ PHIẾU
            if (string.IsNullOrWhiteSpace(pnk.MAPHIEUNHAP))
                return "Mã phiếu nhập không được để trống.";

            pnk.MAPHIEUNHAP = pnk.MAPHIEUNHAP.Trim();

            if (pnk.MAPHIEUNHAP.Length > 15)
                return "Mã phiếu nhập không được vượt quá 15 ký tự.";


            // NHÀ CUNG CẤP
            if (string.IsNullOrWhiteSpace(pnk.MANCC))
                return "Mã nhà cung cấp không được để trống.";

            pnk.MANCC = pnk.MANCC.Trim();

            if (pnk.MANCC.Length > 15)
                return "Mã nhà cung cấp không được vượt quá 15 ký tự.";

            if (!pnk_dal.KiemTraNhaCungCapTonTai(pnk.MANCC))
                return $"Nhà cung cấp có mã '{pnk.MANCC}' không tồn tại.";


            // NHÂN VIÊN
            if (string.IsNullOrWhiteSpace(pnk.MANV))
                return "Mã nhân viên không được để trống.";

            pnk.MANV = pnk.MANV.Trim();

            if (pnk.MANV.Length > 15)
                return "Mã nhân viên không được vượt quá 15 ký tự.";

            if (!pnk_dal.KiemTraNhanVienTonTai(pnk.MANV))
                return $"Nhân viên có mã '{pnk.MANV}' không tồn tại.";


            // NGÀY LẬP
            if (pnk.NGAYLAP == DateOnly.MinValue)
                return "Ngày lập phiếu nhập không hợp lệ.";

            if (pnk.NGAYLAP > DateOnly.FromDateTime(DateTime.Now))
                return "Ngày lập phiếu nhập không được lớn hơn ngày hiện tại.";


            // KIỂM TRA MÃ TRÙNG
            if (!isUpdate && pnk_dal.KiemTraTonTai(pnk.MAPHIEUNHAP))
            {
                return $"Mã phiếu nhập '{pnk.MAPHIEUNHAP}' đã tồn tại.";
            }


            // CHI TIẾT
            if (pnk.listjson_chitietnhap == null || pnk.listjson_chitietnhap.Count == 0)
            {
                return "Phiếu nhập phải có ít nhất một chi tiết sản phẩm.";
            }


            // KIỂM TRA TRÙNG SẢN PHẨM
            var danhSachMaSP = new HashSet<string>(  StringComparer.OrdinalIgnoreCase);

            foreach (var ct in pnk.listjson_chitietnhap)
            {
                if (ct == null)
                    return "Thông tin chi tiết nhập không được để trống.";

                // MASP
                if (string.IsNullOrWhiteSpace(ct.MASP))
                    return "Mã sản phẩm trong chi tiết nhập không được để trống.";

                ct.MASP = ct.MASP.Trim();

                if (ct.MASP.Length > 15)
                    return $"Mã sản phẩm '{ct.MASP}' không được vượt quá 15 ký tự.";

                // SẢN PHẨM CÓ TỒN TẠI
                if (!pnk_dal.KiemTraSanPhamTonTai(ct.MASP))
                    return $"Sản phẩm có mã '{ct.MASP}' không tồn tại.";

                // TRÙNG SẢN PHẨM
                if (!danhSachMaSP.Add(ct.MASP))
                {
                    return $"Sản phẩm '{ct.MASP}' bị trùng trong cùng một phiếu nhập.";
                }

                // SỐ LƯỢNG
                if (ct.SOLUONG <= 0)
                    return $"Số lượng sản phẩm '{ct.MASP}' phải lớn hơn 0.";

                // ĐƠN GIÁ
                if (ct.DONGIANHAP < 0)
                    return $"Đơn giá nhập của sản phẩm '{ct.MASP}' không được âm.";

                // TÍNH THÀNH TIỀN
                ct.THANHTIEN = ct.SOLUONG * ct.DONGIANHAP;

                if (ct.THANHTIEN < 0)
                    return $"Thành tiền của sản phẩm '{ct.MASP}' không hợp lệ.";
            }

            return null;
        }


        public string ThemMoi(PhieuNhapKho pnk)
        {
            string error = ValidatePhieuNhap(pnk, false);

            if (error != null)
                return error;

            try
            {
                return pnk_dal.Insert(pnk) ? null : "Không thể thêm phiếu nhập kho.";
            }
            catch (Exception ex)
            {
                return "Lỗi thêm phiếu nhập: " + ex.Message;
            }
        }


        public string CapNhat(PhieuNhapKho pnk)
        {
            string error = ValidatePhieuNhap(pnk, true);

            if (error != null)
                return error;

            if (!pnk_dal.KiemTraTonTai(pnk.MAPHIEUNHAP))
                return $"Không tìm thấy phiếu nhập có mã '{pnk.MAPHIEUNHAP}'.";

            try
            {
                return pnk_dal.Update(pnk)? null : "Không thể cập nhật phiếu nhập kho.";
            }
            catch (Exception ex)
            {
                return "Lỗi cập nhật phiếu nhập: " + ex.Message;
            }
        }


        public string Xoa(string maphieunhap)
        {
            if (string.IsNullOrWhiteSpace(maphieunhap))
                return "Mã phiếu nhập không được để trống.";

            maphieunhap = maphieunhap.Trim();

            if (maphieunhap.Length > 15)
                return "Mã phiếu nhập không được vượt quá 15 ký tự.";

            if (!pnk_dal.KiemTraTonTai(maphieunhap))
                return $"Không tìm thấy phiếu nhập có mã '{maphieunhap}'.";

            try
            {
                return pnk_dal.Delete(maphieunhap)  ? null   : "Không thể xoá phiếu nhập kho.";
            }
            catch (Exception ex)
            {
                return "Lỗi xoá phiếu nhập: " + ex.Message;
            }
        }
    }
}