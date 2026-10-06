using DAL;
using Models;

namespace BLL
{
    public class SanPham_BLL
    {
        private readonly SanPham_DAL sp_dal;
        private readonly DanhMuc_DAL dm_dal; 

        public SanPham_BLL(SanPham_DAL sp_dal, DanhMuc_DAL dm_dal)
        {
            this.sp_dal = sp_dal;
            this.dm_dal = dm_dal;
        }

        public List<SanPham> LayTatCa()
        {
            var list = sp_dal.GetAll();
            return list ?? new List<SanPham>();
        }

        public List<SanPham> LayTheoID(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP))
                return null;

            maSP = maSP.Trim();

            if (maSP.Length > 15)
                return null;

            if (!sp_dal.KiemTraTonTai(maSP))
                return null;

            return sp_dal.GetByID(maSP);
        }

        public string ThemMoi(SanPham sp)
        {
            if (sp == null)
                return "Thông tin sản phẩm không được để trống.";

            // MÃ SẢN PHẨM
            if (string.IsNullOrWhiteSpace(sp.MASP))
                return "Mã sản phẩm không được để trống.";

            sp.MASP = sp.MASP.Trim();

            if (sp.MASP.Equals("string", StringComparison.OrdinalIgnoreCase))
                return "Vui lòng nhập mã sản phẩm hợp lệ, không được để trống.";

            if (sp.MASP.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            // TÊN SẢN PHẨM
            if (string.IsNullOrWhiteSpace(sp.TENSP))
                return "Tên sản phẩm không được để trống.";

            sp.TENSP = sp.TENSP.Trim();

            if (sp.TENSP.Length > 300)
                return "Tên sản phẩm không được vượt quá 300 ký tự.";

            // MÃ VẠCH
            if (!string.IsNullOrWhiteSpace(sp.MAVACH))
            {
                sp.MAVACH = sp.MAVACH.Trim();

                if (sp.MAVACH.Length > 128)
                    return "Mã vạch không được vượt quá 128 ký tự.";
            }

            // MÔ TẢ
            if (!string.IsNullOrWhiteSpace(sp.MOTA))
            {
                sp.MOTA = sp.MOTA.Trim();

                if (sp.MOTA.Length > 4000)
                    return "Mô tả không được vượt quá 4000 ký tự.";
            }

            // MÃ DANH MỤC (KIỂM TRA HỢP LỆ VÀ KHÓA NGOẠI)
            if (string.IsNullOrWhiteSpace(sp.MADANHMUC))
                return "Mã danh mục không được để trống.";

            sp.MADANHMUC = sp.MADANHMUC.Trim();

            if (sp.MADANHMUC.Equals("string", StringComparison.OrdinalIgnoreCase))
                return "Vui lòng chọn mã danh mục hợp lệ, không được để trống.";

            if (sp.MADANHMUC.Length > 15)
                return "Mã danh mục không được vượt quá 15 ký tự.";

            // Kiểm tra danh mục có tồn tại trong CSDL không trước khi INSERT
            if (!dm_dal.KiemTraTonTai(sp.MADANHMUC))
                return $"Mã danh mục '{sp.MADANHMUC}' không tồn tại trong hệ thống.";

            // ĐƠN GIÁ
            if (sp.DONGIA < 0)
                return "Đơn giá không được nhỏ hơn 0.";

            // THUẾ VAT
            if (sp.THUE < 0)
                return "Thuế VAT không được nhỏ hơn 0.";

            if (sp.THUE > 100)
                return "Thuế VAT không được lớn hơn 100%.";

            // SỐ LƯỢNG TỒN
            if (sp.SOLUONGTON < 0)
                return "Số lượng tồn không được nhỏ hơn 0.";

            // THUỘC TÍNH
            if (!string.IsNullOrWhiteSpace(sp.THUOCTINH))
            {
                sp.THUOCTINH = sp.THUOCTINH.Trim();

                if (sp.THUOCTINH.Length > 4000)
                    return "Thuộc tính sản phẩm không được vượt quá 4000 ký tự.";
            }

            // KIỂM TRA TRÙNG KHÓA CHÍNH
            if (sp_dal.KiemTraTonTai(sp.MASP))
                return $"Mã sản phẩm '{sp.MASP}' đã tồn tại.";

            try
            {
                bool result = sp_dal.Insert(sp);

                if (!result)
                    return "Không thể thêm sản phẩm.";

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }

        public string Sua(SanPham sp)
        {
            if (sp == null)
                return "Thông tin sản phẩm không được để trống.";

            // MÃ SẢN PHẨM
            if (string.IsNullOrWhiteSpace(sp.MASP))
                return "Mã sản phẩm không được để trống.";

            sp.MASP = sp.MASP.Trim();

            if (sp.MASP.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            // TÊN SẢN PHẨM
            if (string.IsNullOrWhiteSpace(sp.TENSP))
                return "Tên sản phẩm không được để trống.";

            sp.TENSP = sp.TENSP.Trim();

            if (sp.TENSP.Length > 300)
                return "Tên sản phẩm không được vượt quá 300 ký tự.";

            // MÃ VẠCH
            if (!string.IsNullOrWhiteSpace(sp.MAVACH))
            {
                sp.MAVACH = sp.MAVACH.Trim();

                if (sp.MAVACH.Length > 128)
                    return "Mã vạch không được vượt quá 128 ký tự.";
            }

            // MÔ TẢ
            if (!string.IsNullOrWhiteSpace(sp.MOTA))
            {
                sp.MOTA = sp.MOTA.Trim();

                if (sp.MOTA.Length > 4000)
                    return "Mô tả không được vượt quá 4000 ký tự.";
            }

            // MÃ DANH MỤC
            if (string.IsNullOrWhiteSpace(sp.MADANHMUC))
                return "Mã danh mục không được để trống.";

            sp.MADANHMUC = sp.MADANHMUC.Trim();

            if (sp.MADANHMUC.Equals("string", StringComparison.OrdinalIgnoreCase))
                return "Vui lòng chọn mã danh mục hợp lệ, không để mặc định là 'string'.";

            if (sp.MADANHMUC.Length > 15)
                return "Mã danh mục không được vượt quá 15 ký tự.";

            // Kiểm tra danh mục có tồn tại không
            if (!dm_dal.KiemTraTonTai(sp.MADANHMUC))
                return $"Mã danh mục '{sp.MADANHMUC}' không tồn tại trong hệ thống.";

            // ĐƠN GIÁ
            if (sp.DONGIA < 0)
                return "Đơn giá không được nhỏ hơn 0.";

            // THUẾ VAT
            if (sp.THUE < 0)
                return "Thuế VAT không được nhỏ hơn 0.";

            if (sp.THUE > 100)
                return "Thuế VAT không được lớn hơn 100%.";

            // SỐ LƯỢNG
            if (sp.SOLUONGTON < 0)
                return "Số lượng tồn không được nhỏ hơn 0.";

            // THUỘC TÍNH
            if (!string.IsNullOrWhiteSpace(sp.THUOCTINH))
            {
                sp.THUOCTINH = sp.THUOCTINH.Trim();

                if (sp.THUOCTINH.Length > 4000)
                    return "Thuộc tính sản phẩm không được vượt quá 4000 ký tự.";
            }

            // KIỂM TRA TỒN TẠI SẢN PHẨM
            if (!sp_dal.KiemTraTonTai(sp.MASP))
                return $"Không tìm thấy sản phẩm có mã '{sp.MASP}'.";

            try
            {
                bool result = sp_dal.Update(sp);

                if (!result)
                    return "Không thể cập nhật sản phẩm.";

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }

       
        public string SuaSoLuong(string maSP, int soLuongMoi)
        {
            if (string.IsNullOrWhiteSpace(maSP))
                return "Mã sản phẩm không được để trống.";

            maSP = maSP.Trim();

            if (maSP.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            if (soLuongMoi < 0)
                return "Số lượng tồn không được nhỏ hơn 0.";

            if (!sp_dal.KiemTraTonTai(maSP))
                return $"Không tìm thấy sản phẩm có mã '{maSP}'.";

            try
            {
                bool result = sp_dal.UpdateSoLuong(maSP, soLuongMoi);

                if (!result)
                    return "Không thể cập nhật số lượng sản phẩm.";

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }

        public string Xoa(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP))
                return "Mã sản phẩm không được để trống.";

            maSP = maSP.Trim();

            if (maSP.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            if (!sp_dal.KiemTraTonTai(maSP))
                return $"Không tìm thấy sản phẩm có mã '{maSP}'.";

            try
            {
                bool result = sp_dal.Delete(maSP);

                if (!result)
                    return "Không thể xóa sản phẩm.";

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }
    }
}
