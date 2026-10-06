using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class ChiTietBan_BLL
    {
        private readonly ChiTietBan_DAL ctb_dal;

        public ChiTietBan_BLL(ChiTietBan_DAL ctb_dal)
        {
            this.ctb_dal = ctb_dal;
        }

        public List<ChiTietBan> LayTatCa()
        {
            return ctb_dal.GetAll();
        }

        public List<ChiTietBan> LayTheoHoaDon(string maHDB)
        {
            if (string.IsNullOrEmpty(maHDB))
                return null;

            return ctb_dal.GetByHoaDon(maHDB);
        }

        public string ThemMoi(ChiTietBan ct)
        {
            if (ct == null)
                return "Dữ liệu chi tiết bán không được để trống.";

            if (string.IsNullOrWhiteSpace(ct.MAHDBAN))
                return "Mã hóa đơn không được để trống.";

            ct.MAHDBAN = ct.MAHDBAN.Trim();

            if (ct.MAHDBAN.Length > 15)
                return "Mã hóa đơn không được vượt quá 15 ký tự.";

            if (string.IsNullOrWhiteSpace(ct.MASP))
                return "Mã sản phẩm không được để trống.";

            ct.MASP = ct.MASP.Trim();

            if (ct.MASP.Length > 15)
                return "Mã sản phẩm không được vượt quá 15 ký tự.";

            if (ct.SOLUONG <= 0)
                return "Số lượng phải lớn hơn 0.";

            if (ct.DONGIA <= 0)
                return "Đơn giá phải lớn hơn 0.";

          
            if (!ctb_dal.KiemTraHoaDonTonTai(ct.MAHDBAN))
                return "Mã hóa đơn " + ct.MAHDBAN + " không tồn tại.";

           
            if (!ctb_dal.KiemTraSanPhamTonTai(ct.MASP))
                return "Mã sản phẩm " + ct.MASP + " không tồn tại.";

           
            if (ctb_dal.KiemTraTonTai(ct.MAHDBAN, ct.MASP))
                return "Chi tiết sản phẩm " + ct.MASP +
                       " đã tồn tại trong hóa đơn " + ct.MAHDBAN + ".";

            if (!ctb_dal.Insert(ct))
                return "Thêm chi tiết bán thất bại.";

            return null;
        }
        public bool Sua(ChiTietBan ct)
        {
            if (ct == null)
                return false;

            if (string.IsNullOrEmpty(ct.MAHDBAN) || string.IsNullOrEmpty(ct.MASP))
                return false;

            if (!ctb_dal.KiemTraTonTai(ct.MAHDBAN, ct.MASP))
                return false;

            return ctb_dal.Update(ct);
        }

        public bool Xoa(string maHDB, string maSP)
        {
            if (string.IsNullOrEmpty(maHDB) || string.IsNullOrEmpty(maSP))
                return false;

            if (!ctb_dal.KiemTraTonTai(maHDB, maSP))
                return false;

            return ctb_dal.Delete(maHDB, maSP);
        }
    }
}
