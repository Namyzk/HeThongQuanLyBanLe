using DAL;
using Models;

namespace BLL
{
    public class DanhMuc_BLL
    {
        private readonly DanhMuc_DAL dm_dal;

        public DanhMuc_BLL()
        {
            dm_dal = new DanhMuc_DAL();
        }

    
        public List<DanhMuc> LayTatCa()
        {
            var list = dm_dal.GetAll();

            if (list == null || list.Count == 0)
                return new List<DanhMuc>();

            return list;
        }

       
        public List<DanhMuc> LayTheoID(string madanhmuc)
        {
            if (string.IsNullOrWhiteSpace(madanhmuc))
                return null;

            madanhmuc = madanhmuc.Trim();

            // Ràng buộc độ dài
            if (madanhmuc.Length > 15)
                return null;

            // Kiểm tra mã có tồn tại không
            if (!dm_dal.KiemTraTonTai(madanhmuc))
                return null;

            return dm_dal.GetbyID(madanhmuc);
        }

     
        public string ThemMoi(DanhMuc danhmuc)
        {
          
            if (danhmuc == null)
                return "Dữ liệu danh mục không được để trống.";

           
            if (string.IsNullOrWhiteSpace(danhmuc.MADANHMUC))
                return "Mã danh mục không được để trống.";

            danhmuc.MADANHMUC = danhmuc.MADANHMUC.Trim();

            if (danhmuc.MADANHMUC.Length > 15)
                return "Mã danh mục không được vượt quá 15 ký tự.";

            
            if (string.IsNullOrWhiteSpace(danhmuc.TENDANHMUC))
                return "Tên danh mục không được để trống.";

            danhmuc.TENDANHMUC = danhmuc.TENDANHMUC.Trim();

            if (danhmuc.TENDANHMUC.Length > 200)
                return "Tên danh mục không được vượt quá 200 ký tự.";

            
            if (!string.IsNullOrEmpty(danhmuc.MOTA))
            {
                danhmuc.MOTA = danhmuc.MOTA.Trim();

                if (danhmuc.MOTA.Length > 1000)
                    return "Mô tả không được vượt quá 1000 ký tự.";
            }

           
            if (dm_dal.KiemTraTonTai(danhmuc.MADANHMUC))
                return "Mã danh mục đã tồn tại.";

            
            bool result = dm_dal.Insert(danhmuc);

            if (!result)
                return "Thêm danh mục thất bại.";

            return null;
        }

      
        public string CapNhat(DanhMuc danhmuc)
        {
            if (danhmuc == null)
                return "Dữ liệu danh mục không được để trống.";

            
            if (string.IsNullOrWhiteSpace(danhmuc.MADANHMUC))
                return "Mã danh mục không được để trống.";

            danhmuc.MADANHMUC = danhmuc.MADANHMUC.Trim();

            if (danhmuc.MADANHMUC.Length > 15)
                return "Mã danh mục không được vượt quá 15 ký tự.";

           
            if (string.IsNullOrWhiteSpace(danhmuc.TENDANHMUC))
                return "Tên danh mục không được để trống.";

            danhmuc.TENDANHMUC = danhmuc.TENDANHMUC.Trim();

            if (danhmuc.TENDANHMUC.Length > 200)
                return "Tên danh mục không được vượt quá 200 ký tự.";

           
            if (!string.IsNullOrEmpty(danhmuc.MOTA))
            {
                danhmuc.MOTA = danhmuc.MOTA.Trim();

                if (danhmuc.MOTA.Length > 1000)
                    return "Mô tả không được vượt quá 1000 ký tự.";
            }

            
            if (!dm_dal.KiemTraTonTai(danhmuc.MADANHMUC))
                return "Danh mục không tồn tại.";

            // Cập nhật
            bool result = dm_dal.Update(danhmuc);

            if (!result)
                return "Cập nhật danh mục thất bại.";

            return null;
        }

        public string Xoa(string maDanhMuc)
        {
            
            if (string.IsNullOrWhiteSpace(maDanhMuc))
                return "Mã danh mục không được để trống.";

            maDanhMuc = maDanhMuc.Trim();

           
            if (maDanhMuc.Length > 15)
                return "Mã danh mục không được vượt quá 15 ký tự.";

            
            if (!dm_dal.KiemTraTonTai(maDanhMuc))
                return "Danh mục không tồn tại.";

          
            if (dm_dal.CoSanPhamThuocDanhMuc(maDanhMuc))
            {
                return "Không thể xóa danh mục vì đang có sản phẩm thuộc danh mục này.";
            }

            // Xóa
            bool result = dm_dal.Delete(maDanhMuc);

            if (!result)
                return "Xóa danh mục thất bại.";

            return null;
        }
    }
}