using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLL
{
    public class ThanhToan_BLL
    {
        private readonly ThanhToan_DAL TT_DAL;

        public ThanhToan_BLL()
        {
            TT_DAL = new ThanhToan_DAL();
        }

        public DataTable getAll()
        {
            try
            {
                return TT_DAL.getAll();
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy danh sách thanh toán: " + ex.Message);
            }
        }


        public DataTable GetById(string? ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                    throw new Exception("Mã thanh toán không được để trống.");

                ma = ma.Trim();

                if (ma.Length > 15)
                    throw new Exception("Mã thanh toán không được vượt quá 15 ký tự.");

                return TT_DAL.GetById(ma);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy thông tin thanh toán: " + ex.Message);
            }
        }


        public DataTable Update(ThanhToan model)
        {
            try
            {
                if (model == null)
                    throw new Exception("Dữ liệu thanh toán không được để trống.");

                if (string.IsNullOrWhiteSpace(model.MaThanhToan))
                    throw new Exception("Mã thanh toán không được để trống.");

                model.MaThanhToan = model.MaThanhToan.Trim();

                if (model.MaThanhToan.Length > 15)
                    throw new Exception("Mã thanh toán không được vượt quá 15 ký tự.");

                return TT_DAL.Update(model);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Lỗi khi cập nhật thanh toán: " + ex.Message);
            }
        }

        public DataTable Delete(string? ma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ma))
                    throw new Exception("Mã thanh toán không được để trống.");

                ma = ma.Trim();

                if (ma.Length > 15)
                    throw new Exception("Mã thanh toán không được vượt quá 15 ký tự.");

                return TT_DAL.Delete(ma);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa thanh toán: " + ex.Message);
            }
        }

        public DataTable GetHoaDonChuaThanhToan()
        {
            try
            {
                return TT_DAL.GetHoaDonChuaThanhToan();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Lỗi khi lấy danh sách hóa đơn chưa thanh toán: "
                    + ex.Message);
            }
        }

        public DataTable GetHoaDonChuaThanhToanTheoTen(string? tenKh)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenKh))
                    throw new Exception("Tên khách hàng không được để trống.");

                tenKh = tenKh.Trim();

                if (tenKh.Length > 100)
                    throw new Exception("Tên khách hàng không được vượt quá 100 ký tự.");

                return TT_DAL.GetHoaDonChuaThanhToanTheoTen(tenKh);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tìm hóa đơn chưa thanh toán theo tên khách hàng: " + ex.Message);
            }
        }


        public int UpdateTrangThaiThanhToan(string? maHDBan, string? phuongThuc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDBan))
                    throw new Exception("Mã hóa đơn không được để trống.");

                if (string.IsNullOrWhiteSpace(phuongThuc))
                    throw new Exception("Phương thức thanh toán không được để trống.");

                maHDBan = maHDBan.Trim();
                phuongThuc = phuongThuc.Trim();

                if (maHDBan.Length > 15)
                    throw new Exception("Mã hóa đơn không được vượt quá 15 ký tự.");

                if (phuongThuc.Length > 50)
                    throw new Exception("Phương thức thanh toán không được vượt quá 50 ký tự.");

                return TT_DAL.UpdateTrangThaiThanhToan(maHDBan, phuongThuc);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật trạng thái thanh toán: " + ex.Message);
            }
        }
        public DataTable Create(Models.ThanhToan model)
        {
            if (model == null)
                throw new Exception("Thông tin thanh toán không được để trống.");


            if (string.IsNullOrWhiteSpace(model.MaThanhToan))
                throw new Exception("Mã thanh toán không được để trống.");

            model.MaThanhToan = model.MaThanhToan.Trim();

            if (model.MaThanhToan.Length > 15)
                throw new Exception("Mã thanh toán không được vượt quá 15 ký tự.");

            // Kiểm tra trùng mã thanh toán
            DataTable dt = TT_DAL.GetById(model.MaThanhToan);

            if (dt != null && dt.Rows.Count > 0)
                throw new Exception("Mã thanh toán đã tồn tại.");



            if (string.IsNullOrWhiteSpace(model.MaHDBan))
                throw new Exception("Mã hóa đơn bán không được để trống.");

            model.MaHDBan = model.MaHDBan.Trim();

            if (model.MaHDBan.Length > 15)
                throw new Exception("Mã hóa đơn bán không được vượt quá 15 ký tự.");

            // Kiểm tra hóa đơn tồn tại
            if (!TT_DAL.KiemTraHoaDonTonTai(model.MaHDBan))
                throw new Exception("Hóa đơn bán không tồn tại.");



            if (string.IsNullOrWhiteSpace(model.PhuongThuc))
                throw new Exception("Phương thức thanh toán không được để trống.");

            model.PhuongThuc = model.PhuongThuc.Trim();

            if (model.PhuongThuc.Length > 50)
                throw new Exception("Phương thức thanh toán không được vượt quá 50 ký tự."
                );


            if (model.SoTienThanhToan <= 0)
                throw new Exception("Số tiền thanh toán phải lớn hơn 0."
                );

            if (string.IsNullOrWhiteSpace(model.TrangThai))
                throw new Exception("Trạng thái thanh toán không được để trống.");

            model.TrangThai = model.TrangThai.Trim();

            if (model.TrangThai.Length > 50)
                throw new Exception("Trạng thái thanh toán không được vượt quá 50 ký tự.");

            try
            {
                return TT_DAL.Create(model);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm thanh toán: " + ex.Message
                );
            }
        }

        public DataTable ResetSoTienByHoaDon(string? maHDBan, decimal soTienMoi)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maHDBan))
                    throw new Exception("Mã hóa đơn không được để trống.");

                maHDBan = maHDBan.Trim();

                if (maHDBan.Length > 15)
                    throw new Exception("Mã hóa đơn không được vượt quá 15 ký tự.");

                if (soTienMoi < 0)
                    throw new Exception("Số tiền thanh toán không được nhỏ hơn 0.");

                return TT_DAL.ResetSoTienByHoaDon(maHDBan, soTienMoi);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi reset số tiền thanh toán: " + ex.Message);
            }
        }
    }
}