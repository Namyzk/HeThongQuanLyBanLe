/**
 * cau-hinh.js — Cấu hình toàn cục của ứng dụng
 */

/** Base URL của API Gateway tập trung (HTTPS cổng 7144) */
export const BASE_URL = 'https://localhost:7144';

/** Tiền tố theo vai trò */
export const TIEN_TO_API = {
  login:   '/api/Login',
  admin:   '/api/admin',
  thukho:  '/api/thukho',
  thungan: '/api/thungan',
  ketoan:  '/api/ketoan',
};

/** Chỉ lưu metadata phiên; JWT và refresh token nằm trong cookie HttpOnly. */
export const KHOA_PHIEN = {
  refresh_expires_at: 'refresh_token_expires_at',
  nguoi_dung: 'current_user',
  thoi_gian:  'token_received_at',
};

/** JWT hết hạn sau 15 phút (900 giây) */
export const THOI_GIAN_TOKEN = 900;

/** Cảnh báo khi còn bao nhiêu giây */
export const CANH_BAO_TRUOC = 120;

/** Ánh xạ Quyen (số) -> RoleName */
export const QUYEN_ROLE = {
  1: 'Admin',
  2: 'ThuNgan',
  3: 'ThuKho',
  4: 'KeToan',
};

/** Tên hiển thị vai trò */
export const TEN_VAI_TRO = {
  Admin:   'Quản trị viên',
  ThuNgan: 'Thu ngân',
  ThuKho:  'Thủ kho',
  KeToan:  'Kế toán',
};

/** Màu sắc badge */
export const MAU_BADGE_VAI_TRO = {
  Admin:   'badge-do',
  ThuNgan: 'badge-lam',
  ThuKho:  'badge-vang',
  KeToan:  'badge-xanh',
};
