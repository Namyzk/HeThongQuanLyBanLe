/**
 * dieu-huong.js — Điều hướng trang độc lập và kiểm tra phân quyền
 */
import { daDangNhap, layVaiTro } from './xac-thuc.js';
import { hienToast } from './thanh-phan/thong-bao.js';

/** Danh sách thông tin trang */
export const DANH_SACH_TRANG = {
  'dang-nhap':           { ten: 'Đăng nhập',          file: 'dang-nhap.html',           vaiTro: [] },
  'tong-quan':           { ten: 'Tổng quan',           file: 'tong-quan.html',           vaiTro: ['Admin', 'ThuNgan', 'ThuKho', 'KeToan'] },
  'quan-ly-tai-khoan':   { ten: 'Quản lý Tài khoản',  file: 'quan-ly-tai-khoan.html',   vaiTro: ['Admin'] },
  'quan-ly-nhan-vien':   { ten: 'Quản lý Nhân viên',  file: 'quan-ly-nhan-vien.html',   vaiTro: ['Admin'] },
  'quan-ly-khuyen-mai':  { ten: 'Quản lý Khuyến mãi', file: 'quan-ly-khuyen-mai.html',  vaiTro: ['Admin'] },
  'quan-ly-danh-muc':    { ten: 'Quản lý Danh mục',   file: 'quan-ly-danh-muc.html',    vaiTro: ['Admin', 'ThuKho'] },
  'quan-ly-san-pham':    { ten: 'Quản lý Sản phẩm',   file: 'quan-ly-san-pham.html',    vaiTro: ['Admin', 'ThuKho'] },
  'quan-ly-ton-kho':     { ten: 'Tồn kho',             file: 'quan-ly-ton-kho.html',     vaiTro: ['Admin', 'ThuKho', 'ThuNgan'] },
  'quan-ly-phieu-nhap':  { ten: 'Phiếu nhập kho',     file: 'quan-ly-phieu-nhap.html',  vaiTro: ['Admin', 'ThuKho', 'KeToan'] },
  'quan-ly-nha-cung-cap':{ ten: 'Nhà cung cấp',       file: 'quan-ly-nha-cung-cap.html',vaiTro: ['Admin', 'ThuKho'] },
  'quan-ly-khach-hang':  { ten: 'Khách hàng',          file: 'quan-ly-khach-hang.html',  vaiTro: ['Admin', 'ThuNgan'] },
  'ban-hang':            { ten: 'Bán hàng',             file: 'ban-hang.html',            vaiTro: ['Admin', 'ThuNgan'] },
  'quan-ly-hoa-don':     { ten: 'Quản lý Hóa đơn',    file: 'quan-ly-hoa-don.html',     vaiTro: ['Admin', 'ThuNgan', 'KeToan'] },
  'doi-tra':             { ten: 'Đổi trả',              file: 'doi-tra.html',             vaiTro: ['Admin', 'ThuNgan'] },
  'quan-ly-thanh-toan':  { ten: 'Thanh toán',          file: 'quan-ly-thanh-toan.html',  vaiTro: ['Admin', 'ThuNgan', 'KeToan'] },
  'quan-ly-cong-no':     { ten: 'Quản lý Công nợ',     file: 'quan-ly-cong-no.html',     vaiTro: ['Admin', 'KeToan'] },
  'bao-cao-thong-ke':    { ten: 'Báo cáo & Thống kê',  file: 'bao-cao-thong-ke.html',    vaiTro: ['Admin', 'KeToan'] },
};

/** Lấy ID trang hiện tại từ tên file HTML */
export function layTrangHienTai() {
  const path = window.location.pathname;
  const fileName = path.split('/').pop().replace('.html', '') || 'index';
  if (fileName === 'index' || !fileName) return 'tong-quan';
  return fileName;
}

/** Chuyển trang trực tiếp bằng URL file HTML */
export function dinhHuong(trangId) {
  const config = DANH_SACH_TRANG[trangId];
  const targetFile = config ? config.file : `${trangId}.html`;
  
  // Xác định đường dẫn tương đối tùy theo trang hiện tại đang ở root hay trong trang/
  const dangOThuMucTrang = window.location.pathname.includes('/trang/');
  if (dangOThuMucTrang) {
    window.location.href = targetFile;
  } else {
    window.location.href = `trang/${targetFile}`;
  }
}

/**
 * Kiểm tra phân quyền truy cập trang hiện tại
 * @param {Array<string>} [vaiTroChoPhep]
 * @returns {boolean}
 */
export function kiemTraQuyenTrang(vaiTroChoPhep = null) {
  const trangId = layTrangHienTai();
  if (trangId === 'dang-nhap') {
    if (daDangNhap()) {
      dinhHuong('tong-quan');
      return false;
    }
    return true;
  }

  // Trang nghiệp vụ cần đăng nhập
  if (!daDangNhap()) {
    dinhHuong('dang-nhap');
    return false;
  }

  const config = DANH_SACH_TRANG[trangId];
  const choPhep = vaiTroChoPhep || config?.vaiTro || [];
  if (choPhep.length > 0) {
    const vaiTro = layVaiTro();
    if (!choPhep.includes(vaiTro)) {
      hienToast('Không có quyền', `Tài khoản vai trò ${vaiTro || 'hiện tại'} không có quyền truy cập trang này.`, 'canh-bao', 3000);
      setTimeout(() => dinhHuong('tong-quan'), 1200);
      return false;
    }
  }

  return true;
}
