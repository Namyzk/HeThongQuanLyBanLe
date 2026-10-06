using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interface
{
    public class @interface
    {
        public interface IChiTietBanBLL
        {
            List<ChiTietBan> LayTatCa();
            List<ChiTietBan> LayTheoHoaDon(string maHDB);
            string ThemMoi(ChiTietBan ct);
            bool Sua(ChiTietBan ct);
            bool Xoa(string maHDB, string maSP);
        }

        public interface IDanhMucBLL
        {
            List<DanhMuc> LayTatCa();
            List<DanhMuc> LayTheoID(string madanhmuc);
            string ThemMoi(DanhMuc danhmuc);
            string CapNhat(DanhMuc danhmuc);
            string Xoa(string maDanhMuc);
        }

        public interface IHoaDonBanBLL
        {
            List<HoaDonBan> LayTatCa();
            List<HoaDonBan> LayTheoID(string maHDB);
            bool ThemMoi(HoaDonBan hd);
            bool Sua(HoaDonBan hd);
            bool Xoa(string maHDB);
            bool ResetTongTienHangByHoaDon(string maHDBan, decimal tongTienMoi);
        }

        public interface IKhachHangBLL
        {
            DataTable getAllKH();
            DataTable GetByIdKH(string? makh);
            DataTable DeleteByIdKH(string? makh);
            DataTable UpdateByIdKH(KhachHang kh);
            DataTable CreateKH(KhachHang kh);
        }

        public interface IKhuyenMaiBLL
        {
            DataTable getAll();
            DataTable GetById(string ma);
            DataTable Delete(string ma);
            DataTable Update(KhuyenMai model);
            DataTable Create(KhuyenMai model);
        }

        public interface INhanVienBLL
        {
            List<NhanVien> LayTatCa();
            List<NhanVien> LayTheoID(string manv);
            bool ThemMoi(NhanVien nv);
            bool CapNhat(NhanVien nv);
            bool Xoa(string manv);
        }

        public interface ISanPhamBLL
        {
            List<SanPham> LayTatCa();
            List<SanPham> LayTheoID(string maSP);
            string ThemMoi(SanPham sp);
            string Sua(SanPham sp);
            string SuaSoLuong(string maSP, int soLuongMoi);
            string Xoa(string maSP);
        }

        public interface INhaCungCapBLL
        {
            DataTable GetAll();
            string ValidateMa(string ma);
            DataTable GetById(string ma);
            string Create(NhaCungCap model);
            string Update(NhaCungCap model);
            string Delete(string ma);
        }

        public interface IThanhToanBLL
        {
            DataTable getAll();
            DataTable GetById(string? ma);
            DataTable Update(ThanhToan model);
            DataTable Delete(string? ma);
            DataTable GetHoaDonChuaThanhToan();
            DataTable GetHoaDonChuaThanhToanTheoTen(string? tenKh);
            int UpdateTrangThaiThanhToan(string? maHDBan, string? phuongThuc);
            DataTable Create(ThanhToan model);
            DataTable ResetSoTienByHoaDon(string? maHDBan, decimal soTienMoi);
        }

        public interface ITaiKhoanBLL
        {
            List<TaiKhoan> LayTatCa();
            List<TaiKhoan> LayTheoID(string? mataikhoan);
            bool ThemMoi(TaiKhoan tk);
            bool CapNhat(TaiKhoan tk);
            bool Xoa(string? mataikhoan);
            List<TaiKhoan> DangNhap(string? username, string? password);
            int LayQuyen(string? username);
        }

        public interface IChiTietNhapBLL
        {
            List<ChiTietNhap> LayTatCa();
            List<ChiTietNhap> LayTheoPhieu(string maphieunhap);
            List<ChiTietNhap> LayTheoID(string maphieunhap, string masp);
            string ThemMoi(ChiTietNhap ctn);
            string CapNhat(ChiTietNhap ctn);
            string Xoa(string maphieunhap, string masp);
        }

        public interface IPhieuNhapKhoBLL
        {
            List<PhieuNhapKho> LayTatCa();
            List<PhieuNhapKho> LayTheoID(string maphieunhap);
            string ThemMoi(PhieuNhapKho pnk);
            string CapNhat(PhieuNhapKho pnk);
            string Xoa(string maphieunhap);
        }
    }
}
