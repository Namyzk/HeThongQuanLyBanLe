/**
 * xac-thuc.js — Quản lý metadata phiên; token do trình duyệt gửi qua cookie HttpOnly.
 */
import { KHOA_PHIEN, THOI_GIAN_TOKEN, QUYEN_ROLE } from './cau-hinh.js';

// Dọn token phiên cũ đã từng được lưu bằng JavaScript.
sessionStorage.removeItem('access_token');
sessionStorage.removeItem('refresh_token');

/** Lưu thông tin phiên sau khi đăng nhập thành công */
export function luuPhien(nguoiDung, refreshTokenExpiresAt = null) {
  if (nguoiDung) {
    sessionStorage.setItem(KHOA_PHIEN.nguoi_dung, JSON.stringify(nguoiDung));
  }
  const bayGio = Date.now();
  sessionStorage.setItem(KHOA_PHIEN.thoi_gian, bayGio.toString());
  try {
    localStorage.setItem('qlbl_last_refresh', bayGio.toString());
  } catch {}

  if (refreshTokenExpiresAt) {
    let time = NaN;
    if (typeof refreshTokenExpiresAt === 'number') {
      time = refreshTokenExpiresAt;
    } else if (typeof refreshTokenExpiresAt === 'string') {
      time = new Date(refreshTokenExpiresAt).getTime();
      if (isNaN(time) && refreshTokenExpiresAt.includes('/')) {
        const parts = refreshTokenExpiresAt.split(/[\/\s:]/);
        if (parts.length >= 6) {
          const d = new Date(parts[2], parts[1] - 1, parts[0], parts[3], parts[4], parts[5]);
          time = d.getTime();
        }
      }
    }
    if (!isNaN(time) && time > bayGio) {
      sessionStorage.setItem(KHOA_PHIEN.refresh_expires_at, time.toString());
    } else if (!sessionStorage.getItem(KHOA_PHIEN.refresh_expires_at)) {
      sessionStorage.setItem(KHOA_PHIEN.refresh_expires_at, (bayGio + 7 * 24 * 3600 * 1000).toString());
    }
  } else if (!sessionStorage.getItem(KHOA_PHIEN.refresh_expires_at)) {
    sessionStorage.setItem(KHOA_PHIEN.refresh_expires_at, (bayGio + 7 * 24 * 3600 * 1000).toString());
  }
}

/** Xóa toàn bộ phiên */
export function xoaPhien() {
  sessionStorage.removeItem('access_token');
  sessionStorage.removeItem('refresh_token');
  sessionStorage.removeItem(KHOA_PHIEN.refresh_expires_at);
  sessionStorage.removeItem(KHOA_PHIEN.nguoi_dung);
  sessionStorage.removeItem(KHOA_PHIEN.thoi_gian);
  try {
    localStorage.removeItem('qlbl_last_refresh');
  } catch {}
}

/** Đồng bộ thời gian phiên giữa các tab */
if (typeof window !== 'undefined') {
  window.addEventListener('storage', (e) => {
    if (e.key === 'qlbl_last_refresh' && e.newValue) {
      sessionStorage.setItem(KHOA_PHIEN.thoi_gian, e.newValue);
    }
  });
}

/** Kiểm tra phiên còn trong thời hạn refresh cục bộ (server vẫn là nguồn xác thực). */
export function conRefreshToken() {
  const raw = sessionStorage.getItem(KHOA_PHIEN.refresh_expires_at);
  if (!raw) return true; // Mặc định tin tưởng cookie httpOnly nếu chưa lưu expiry
  const expiresAt = Number(raw);
  return Boolean(Number.isFinite(expiresAt) && expiresAt > Date.now());
}

/** Lấy thông tin người dùng hiện tại */
export function layNguoiDung() {
  const raw = sessionStorage.getItem(KHOA_PHIEN.nguoi_dung);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

/** Kiểm tra đã đăng nhập chưa */
export function daDangNhap() {
  const nguoiDung = layNguoiDung();
  const thoiGian = sessionStorage.getItem(KHOA_PHIEN.thoi_gian);
  if (!nguoiDung || !thoiGian) return false;

  const giayDaQua = (Date.now() - parseInt(thoiGian, 10)) / 1000;
  return giayDaQua < THOI_GIAN_TOKEN || conRefreshToken();
}

/** Kiểm tra token còn bao nhiêu giây */
export function conBaoNhieuGiay() {
  const thoiGian = sessionStorage.getItem(KHOA_PHIEN.thoi_gian);
  if (!thoiGian) return 0;
  const giayDaQua = (Date.now() - parseInt(thoiGian, 10)) / 1000;
  return Math.max(0, THOI_GIAN_TOKEN - giayDaQua);
}

/** Lấy RoleName của người dùng hiện tại */
export function layVaiTro() {
  const nd = layNguoiDung();
  if (!nd) return null;
  // Backend trả về roleName trong data
  if (nd.roleName) return nd.roleName;
  // Fallback dựa trên quyen số
  if (nd.quyen) return QUYEN_ROLE[nd.quyen] || null;
  return null;
}

/** Kiểm tra người dùng có vai trò nào đó không */
export function coVaiTro(...cacVaiTro) {
  const vaiTro = layVaiTro();
  return cacVaiTro.includes(vaiTro);
}
