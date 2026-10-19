/**
 * tien-ich.js — Hàm tiện ích dùng chung
 */

/**
 * Format số tiền VND
 * Chuẩn hóa nếu trị tuyệt đối nhỏ hơn sai số hoặc là -0 để luôn hiển thị 0 đ
 * @param {number|string} so
 */
export function formatTien(so) {
  if (so === null || so === undefined || isNaN(Number(so))) return '—';
  let num = Number(so);
  if (Math.abs(num) < 0.001 || Object.is(num, -0)) {
    num = 0;
  }
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(num);
}

/**
 * Format số tiền giảm giá
 * Nếu giảm giá bằng 0 thì hiển thị '0 đ', không bao giờ hiển thị '-0 đ'
 * @param {number|string} so
 */
export function formatGiamGia(so) {
  if (so === null || so === undefined || isNaN(Number(so))) return '0 đ';
  let num = Number(so);
  if (Math.abs(num) < 0.001 || Object.is(num, -0) || num <= 0) {
    return '0 đ';
  }
  return `-${formatTien(num)}`;
}

/**
 * Format số (không đơn vị)
 * @param {number} so
 */
export function formatSo(so) {
  if (so === null || so === undefined) return '—';
  let num = Number(so);
  if (Math.abs(num) < 0.001 || Object.is(num, -0)) num = 0;
  return new Intl.NumberFormat('vi-VN').format(num);
}

/**
 * Format ngày từ chuỗi ISO hoặc đối tượng Date
 * @param {string|Date} ngay
 */
export function formatNgay(ngay) {
  if (!ngay) return '—';
  try {
    const d = new Date(ngay);
    if (isNaN(d.getTime())) return ngay;
    return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  } catch {
    return ngay;
  }
}

/**
 * Format ngày giờ
 * @param {string|Date} ngay
 */
export function formatNgayGio(ngay) {
  if (!ngay) return '—';
  try {
    const d = new Date(ngay);
    if (isNaN(d.getTime())) return ngay;
    return d.toLocaleString('vi-VN');
  } catch {
    return ngay;
  }
}

/**
 * Chuyển Date thành chuỗi YYYY-MM-DD (để gửi API)
 * @param {Date} d
 */
export function toDateString(d) {
  if (!d) return '';
  const dt = d instanceof Date ? d : new Date(d);
  if (isNaN(dt.getTime())) return '';
  const y = dt.getFullYear();
  const m = String(dt.getMonth() + 1).padStart(2, '0');
  const day = String(dt.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

/**
 * Lấy ngày hôm nay dạng YYYY-MM-DD
 */
export function homNay() {
  return toDateString(new Date());
}

/**
 * Escape HTML để tránh XSS khi nhúng vào innerHTML
 * @param {string} str
 */
export function escHtml(str) {
  if (str === null || str === undefined) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

/**
 * Tạo ID ngẫu nhiên ngắn
 */
export function taoId(prefix = 'id') {
  return `${prefix}-${Math.random().toString(36).slice(2, 8)}`;
}

/**
 * Debounce hàm
 */
export function debounce(fn, delay = 300) {
  let timer;
  return function (...args) {
    clearTimeout(timer);
    timer = setTimeout(() => fn.apply(this, args), delay);
  };
}

/**
 * Trả về tên hiển thị cho quyền số
 */
export function tenQuyen(so) {
  const map = { 1: 'Admin', 2: 'Thu Ngân', 3: 'Thủ Kho', 4: 'Kế Toán' };
  return map[so] || `Quyền ${so}`;
}

/**
 * Tạo badge quyền
 */
export function badgeQuyen(roleName) {
  const map = {
    Admin:   ['Admin', 'badge-do'],
    ThuNgan: ['Thu Ngân', 'badge-lam'],
    ThuKho:  ['Thủ Kho', 'badge-vang'],
    KeToan:  ['Kế Toán', 'badge-xanh'],
  };
  const [ten, cls] = map[roleName] || [roleName, 'badge-xam'];
  return `<span class="badge ${cls}">${escHtml(ten)}</span>`;
}

/**
 * Kiểm tra số điện thoại Việt Nam
 */
export function hopLeSdt(sdt) {
  return /^(0|\+84)[3-9]\d{8}$/.test(String(sdt || '').trim());
}

/**
 * Lấy dữ liệu mảng từ response API (hỗ trợ data là mảng hoặc null)
 */
export function layDuLieuMang(json) {
  if (!json) return [];
  if (Array.isArray(json.data)) return json.data;
  if (Array.isArray(json)) return json;
  return [];
}

/**
 * Tính tổng một trường trong mảng
 */
export function tinhTong(mangDuLieu, truong) {
  return mangDuLieu.reduce((acc, item) => acc + (Number(item[truong]) || 0), 0);
}

/**
 * Highlight text tìm kiếm
 */
export function highlight(text, tuKhoa) {
  if (!tuKhoa) return escHtml(text);
  const regex = new RegExp(`(${tuKhoa.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi');
  return escHtml(text).replace(regex, '<mark style="background:var(--mau-canh-bao-nhat);padding:0 2px;border-radius:2px">$1</mark>');
}
