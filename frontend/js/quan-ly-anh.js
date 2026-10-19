import { BASE_URL, TIEN_TO_API } from './cau-hinh.js';
import { apiUpload, apiDelete } from './goi-api.js';
import { escHtml } from './tien-ich.js';

const TIEN_TO_ANH_SP = 'sp_hinhanh_';
const TIEN_TO_AVATAR_NV = 'nv_avatar_';
const TIEN_TO_SERVER_SP = 'sp_server_img_';
const TIEN_TO_SERVER_NV = 'nv_server_img_';

/**
 * Danh mục ảnh mẫu chất lượng cao cho các sản phẩm mặc định
 */
export const ANH_SAN_PHAM_MAC_DINH = {
  'SP001': 'https://images.unsplash.com/photo-1544233726-9f1d2b27be8b?auto=format&fit=crop&w=500&q=80',
  'SP002': 'https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?auto=format&fit=crop&w=500&q=80',
  'SP003': 'https://images.unsplash.com/photo-1594213114663-ddf4f24ec2dd?auto=format&fit=crop&w=500&q=80',
  'SP004': 'https://images.unsplash.com/photo-1570222094114-d054a817e56b?auto=format&fit=crop&w=500&q=80',
  'SP005': 'https://images.unsplash.com/photo-1584990347449-39906666ec48?auto=format&fit=crop&w=500&q=80',
  'SP006': 'https://images.unsplash.com/photo-1584990347460-63167191bf9b?auto=format&fit=crop&w=500&q=80',
  'SP007': 'https://images.unsplash.com/photo-1593618998160-e34014e67546?auto=format&fit=crop&w=500&q=80',
  'SP008': 'https://images.unsplash.com/photo-1590794056226-79ef3a8147e1?auto=format&fit=crop&w=500&q=80',
  'SP009': 'https://images.unsplash.com/photo-1610701596007-11502861dcfa?auto=format&fit=crop&w=500&q=80',
  'SP010': 'https://images.unsplash.com/photo-1544816155-12df9643f363?auto=format&fit=crop&w=500&q=80',
  'SP011': 'https://images.unsplash.com/photo-1602143407151-7111542de6e8?auto=format&fit=crop&w=500&q=80',
  'SP012': 'https://images.unsplash.com/photo-1567401893414-76b7b1e5a7a5?auto=format&fit=crop&w=500&q=80',
  'SP013': 'https://images.unsplash.com/photo-1585837575652-267c041d77d4?auto=format&fit=crop&w=500&q=80',
  'SP014': 'https://images.unsplash.com/photo-1558317374-067fb5f30001?auto=format&fit=crop&w=500&q=80',
  'SP015': 'https://images.unsplash.com/photo-1581578731548-c64695cc6952?auto=format&fit=crop&w=500&q=80',
  'SP016': 'https://images.unsplash.com/photo-1607613009820-a29f7bb81c04?auto=format&fit=crop&w=500&q=80',
  'SP017': 'https://images.unsplash.com/photo-1610557892470-55d9e80c0bce?auto=format&fit=crop&w=500&q=80',
  'SP018': 'https://images.unsplash.com/photo-1585421514284-efb74c2b69ba?auto=format&fit=crop&w=500&q=80',
  'SP019': 'https://images.unsplash.com/photo-1584622650111-993a426fbf0a?auto=format&fit=crop&w=500&q=80',
  'SP020': 'https://images.unsplash.com/photo-1528323273322-d81458248d40?auto=format&fit=crop&w=500&q=80',
};

/**
 * Danh mục ảnh mẫu cho một số nhân viên demo
 */
export const ANH_NHAN_VIEN_MAC_DINH = {
  'NV001': 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=350&q=80',
  'NV002': 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=350&q=80',
  'NV003': 'https://images.unsplash.com/photo-1580489944761-15a19d654956?auto=format&fit=crop&w=350&q=80',
  'NV004': 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=350&q=80',
  'NV005': 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&w=350&q=80',
  'NV006': 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?auto=format&fit=crop&w=350&q=80',
  'NV007': 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=350&q=80',
  'NV008': 'https://images.unsplash.com/photo-1522075469751-3a6694fb2f61?auto=format&fit=crop&w=350&q=80',
};

/** Fallback SVG chuẩn đã được mã hóa an toàn, không chứa dấu ngoặc kép hay ký tự HTML nguy hiểm */
export const SVG_FALLBACK_SP = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(
  `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><rect width="100" height="100" rx="12" fill="#f1f5f9"/><rect x="25" y="25" width="50" height="50" rx="8" fill="#e2e8f0"/><path d="M35 60 L45 46 L55 56 L65 42 L75 60 Z" fill="#94a3b8"/><circle cx="42" cy="40" r="5" fill="#94a3b8"/></svg>`
)}`;

export const SVG_FALLBACK_NV = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(
  `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><rect width="100" height="100" rx="50" fill="#4f46e5"/><circle cx="50" cy="40" r="18" fill="#ffffff" opacity="0.9"/><path d="M22 84 C22 66, 35 60, 50 60 C65 60, 78 84 Z" fill="#ffffff" opacity="0.9"/></svg>`
)}`;

/**
 * Tạo ảnh SVG placeholder cho sản phẩm với chữ cái đầu theo tên sản phẩm
 */
export function taoPlaceholderSanPham(tenSP = '', maSP = '') {
  const ten = String(tenSP || 'SP').trim();
  const chuCai = (ten.charAt(0) || 'S').toUpperCase();
  const tenNgan = ten.slice(0, 24) || 'Sản phẩm';
  const ma = String(maSP || '').trim().toUpperCase();

  const mauGradients = [
    ['#0ea5e9', '#38bdf8'],
    ['#10b981', '#34d399'],
    ['#8b5cf6', '#c084fc'],
    ['#f59e0b', '#fbbf24'],
    ['#f43f5e', '#fb7185'],
    ['#06b6d4', '#67e8f9'],
    ['#4f46e5', '#818cf8'],
    ['#14b8a6', '#2dd4bf'],
  ];
  const charCode = (ten.charCodeAt(0) || 65) + (ma ? ma.charCodeAt(ma.length - 1) : 0);
  const [c1, c2] = mauGradients[charCode % mauGradients.length];

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 320 240" width="100%" height="100%">
    <defs>
      <linearGradient id="sp_bg_${encodeURIComponent(ma || 'sp')}" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" stop-color="#f8fafc"/>
        <stop offset="100%" stop-color="#f1f5f9"/>
      </linearGradient>
      <linearGradient id="sp_c_${encodeURIComponent(ma || 'sp')}" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" stop-color="${c1}"/>
        <stop offset="100%" stop-color="${c2}"/>
      </linearGradient>
    </defs>
    <rect width="320" height="240" fill="url(#sp_bg_${encodeURIComponent(ma || 'sp')})"/>
    <circle cx="160" cy="85" r="46" fill="url(#sp_c_${encodeURIComponent(ma || 'sp')})"/>
    <text x="160" y="87" font-family="-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,sans-serif" font-size="44" font-weight="800" fill="#ffffff" text-anchor="middle" dominant-baseline="central">${chuCai}</text>
    <text x="160" y="165" font-family="-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,sans-serif" font-size="14" font-weight="700" fill="#334155" text-anchor="middle">${escHtml(tenNgan)}</text>
    ${ma ? `<text x="160" y="190" font-family="-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,sans-serif" font-size="12" font-weight="600" fill="#94a3b8" text-anchor="middle">${escHtml(ma)}</text>` : ''}
  </svg>`;
  return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
}

/**
 * Tạo ảnh SVG avatar với chữ cái đầu cho nhân viên (mã hóa URI an toàn 100%)
 */
export function taoPlaceholderAvatar(tenNV = '', maNV = '') {
  const ten = String(tenNV || '').trim();
  const tuCuoi = ten.split(/\s+/).pop() || '';
  const chuCai = (tuCuoi.charAt(0) || String(maNV || 'NV').replace(/\D/g, '').slice(-1) || 'N').toUpperCase();

  const mauGradient = [
    ['#4f46e5', '#818cf8'],
    ['#0ea5e9', '#38bdf8'],
    ['#10b981', '#34d399'],
    ['#8b5cf6', '#c084fc'],
    ['#f59e0b', '#fbbf24'],
    ['#f43f5e', '#fb7185'],
    ['#06b6d4', '#67e8f9'],
    ['#d946ef', '#f0abfc']
  ];
  const charCode = (ten.charCodeAt(0) || 65) + (maNV ? String(maNV).charCodeAt(String(maNV).length - 1) : 0);
  const [c1, c2] = mauGradient[charCode % mauGradient.length];

  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100" width="100%" height="100%">
    <defs>
      <linearGradient id="av_${encodeURIComponent(maNV || 'nv')}" x1="0%" y1="0%" x2="100%" y2="100%">
        <stop offset="0%" stop-color="${c1}"/>
        <stop offset="100%" stop-color="${c2}"/>
      </linearGradient>
    </defs>
    <circle cx="50" cy="50" r="50" fill="url(#av_${encodeURIComponent(maNV || 'nv')})"/>
    <text x="50" y="52" font-family="-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,sans-serif" font-size="44" font-weight="700" fill="#ffffff" text-anchor="middle" dominant-baseline="central">${chuCai}</text>
  </svg>`;
  return `data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`;
}

/**
 * Đọc file ảnh từ người dùng và nén nhẹ về kích thước chuẩn (max 700x700, quality 0.82)
 * giúp lưu vào localStorage nhẹ nhàng, mượt mà và không sợ tràn bộ nhớ.
 */
export function docVaNenAnh(file, kichThuocToiDa = 700, chatLuong = 0.82) {
  return new Promise((resolve, reject) => {
    if (!file || !file.type.startsWith('image/')) {
      reject(new Error('Vui lòng chọn đúng tệp hình ảnh (.jpg, .png, .webp, .jpeg).'));
      return;
    }

    const reader = new FileReader();
    reader.onerror = () => reject(new Error('Không thể đọc tệp ảnh đã chọn.'));
    reader.onload = (e) => {
      const img = new Image();
      img.onerror = () => reject(new Error('Hình ảnh bị lỗi hoặc định dạng không hỗ trợ.'));
      img.onload = () => {
        let w = img.width;
        let h = img.height;

        if (w > kichThuocToiDa || h > kichThuocToiDa) {
          if (w > h) {
            h = Math.round((h * kichThuocToiDa) / w);
            w = kichThuocToiDa;
          } else {
            w = Math.round((w * kichThuocToiDa) / h);
            h = kichThuocToiDa;
          }
        }

        const canvas = document.createElement('canvas');
        canvas.width = w;
        canvas.height = h;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(img, 0, 0, w, h);

        try {
          const dataUrl = canvas.toDataURL('image/jpeg', chatLuong);
          resolve(dataUrl);
        } catch {
          resolve(e.target.result);
        }
      };
      img.src = e.target.result;
    };
    reader.readAsDataURL(file);
  });
}

/**
 * Lấy URL xem ảnh sản phẩm trực tiếp từ API Gateway (stream từ D:\WebApi\Pic\sanpham)
 */
export function layUrlXemAnhSanPham(maSP) {
  if (!maSP) return '';
  const ma = String(maSP).trim().toUpperCase();
  const ver = localStorage.getItem(`${TIEN_TO_SERVER_SP}${ma}`) || '';
  const qs = ver ? `?v=${ver}` : '';
  return `${BASE_URL}${TIEN_TO_API.thukho}/QuanLySanPham/xem-anh-sp/${encodeURIComponent(ma)}${qs}`;
}

/**
 * Lấy URL xem ảnh đại diện nhân viên trực tiếp từ API Gateway (stream từ D:\WebApi\Pic\nhanvien)
 */
export function layUrlXemAnhNhanVien(maNV) {
  if (!maNV) return '';
  const ma = String(maNV).trim().toUpperCase();
  const ver = localStorage.getItem(`${TIEN_TO_SERVER_NV}${ma}`) || '';
  const qs = ver ? `?v=${ver}` : '';
  return `${BASE_URL}${TIEN_TO_API.admin}/QuanLyNhanVien/xem-anh-nv/${encodeURIComponent(ma)}${qs}`;
}

/**
 * Tải ảnh sản phẩm lên máy chủ (lưu vào D:\WebApi\Pic\sanpham, xoá ảnh cũ ngay lập tức)
 * @param {File|Blob} file - Tệp ảnh
 * @param {string} maSP - Mã sản phẩm
 * @param {string|null} oldFilePath - Đường dẫn file cũ (tuỳ chọn)
 * @returns {Promise<object>}
 */
export async function uploadAnhSanPhamServer(file, maSP, oldFilePath = null) {
  if (!file) throw new Error('Vui lòng chọn tệp hình ảnh sản phẩm.');
  const fd = new FormData();
  fd.append('file', file);
  if (maSP) fd.append('maSP', maSP);
  if (oldFilePath) fd.append('oldFilePath', oldFilePath);

  const res = await apiUpload(`${TIEN_TO_API.thukho}/QuanLySanPham/upload`, fd);

  if (maSP) {
    const ma = String(maSP).trim().toUpperCase();
    const ts = Date.now().toString();
    localStorage.setItem(`${TIEN_TO_SERVER_SP}${ma}`, ts);
    localStorage.removeItem(`${TIEN_TO_ANH_SP}${ma}`);
    localStorage.removeItem(`sp_da_xoa_${ma}`);
  }
  return res;
}

/**
 * Xóa ảnh sản phẩm trên máy chủ (xóa tệp trong D:\WebApi\Pic\sanpham)
 * @param {string} maSP
 * @param {string|null} filePath
 */
export async function xoaAnhSanPhamServer(maSP, filePath = null) {
  if (!maSP) return;
  const ma = String(maSP).trim().toUpperCase();
  try {
    await apiDelete(`${TIEN_TO_API.thukho}/QuanLySanPham/xoa-anh`, {
      maSP: ma,
      filePath: filePath || undefined,
    });
  } catch (err) {
    console.warn('Lỗi khi gọi API xóa ảnh sản phẩm trên server:', err);
  }
  localStorage.setItem(`sp_da_xoa_${ma}`, 'true');
  localStorage.removeItem(`${TIEN_TO_SERVER_SP}${ma}`);
  localStorage.removeItem(`${TIEN_TO_ANH_SP}${ma}`);
}

/**
 * Tải ảnh đại diện nhân viên lên máy chủ (lưu vào D:\WebApi\Pic\nhanvien, xoá ảnh cũ ngay lập tức)
 * @param {File|Blob} file - Tệp ảnh đại diện
 * @param {string} maNV - Mã nhân viên
 * @param {string|null} oldFilePath - Đường dẫn file cũ (tuỳ chọn)
 * @returns {Promise<object>}
 */
export async function uploadAnhNhanVienServer(file, maNV, oldFilePath = null) {
  if (!file) throw new Error('Vui lòng chọn tệp ảnh đại diện.');
  const fd = new FormData();
  fd.append('file', file);
  if (maNV) fd.append('manv', maNV);
  if (oldFilePath) fd.append('oldFilePath', oldFilePath);

  const res = await apiUpload(`${TIEN_TO_API.admin}/QuanLyNhanVien/upload`, fd);

  if (maNV) {
    const ma = String(maNV).trim().toUpperCase();
    const ts = Date.now().toString();
    localStorage.setItem(`${TIEN_TO_SERVER_NV}${ma}`, ts);
    localStorage.removeItem(`${TIEN_TO_AVATAR_NV}${ma}`);
    localStorage.removeItem(`nv_da_xoa_${ma}`);
  }
  return res;
}

/**
 * Xóa ảnh đại diện nhân viên trên máy chủ (xóa tệp trong D:\WebApi\Pic\nhanvien)
 * @param {string} maNV
 * @param {string|null} filePath
 */
export async function xoaAnhNhanVienServer(maNV, filePath = null) {
  if (!maNV) return;
  const ma = String(maNV).trim().toUpperCase();
  try {
    await apiDelete(`${TIEN_TO_API.admin}/QuanLyNhanVien/xoa-anh`, {
      manv: ma,
      filePath: filePath || undefined,
    });
  } catch (err) {
    console.warn('Lỗi khi gọi API xóa ảnh nhân viên trên server:', err);
  }
  localStorage.setItem(`nv_da_xoa_${ma}`, 'true');
  localStorage.removeItem(`${TIEN_TO_SERVER_NV}${ma}`);
  localStorage.removeItem(`${TIEN_TO_AVATAR_NV}${ma}`);
}

/**
 * Lấy ảnh sản phẩm (Ưu tiên: Cache/Preview local -> Thuộc tính SP -> Stream từ máy chủ qua Gateway -> Placeholder SVG)
 */
export function layAnhSanPham(maSP, spObj = null) {
  if (!maSP) return taoPlaceholderSanPham(spObj?.TENSP, maSP);
  const ma = String(maSP).trim().toUpperCase();

  // 1. Kiểm tra preview local hoặc URL vừa dán trực tiếp
  const anhCache = localStorage.getItem(`${TIEN_TO_ANH_SP}${ma}`);
  if (anhCache && anhCache.trim()) return anhCache;

  // 2. Kiểm tra thuộc tính trong object sản phẩm (nếu backend gửi HINHANH/filePath)
  const anhObj = spObj?.HINHANH || spObj?.hinhanh || spObj?.ANH || spObj?.anh || spObj?.HinhAnh || spObj?.FILEPATH || spObj?.filePath;
  if (anhObj && String(anhObj).trim()) {
    const val = String(anhObj).trim();
    if (val.startsWith('http://') || val.startsWith('https://') || val.startsWith('data:')) {
      return val;
    }
    const fileName = val.split(/[/\\]/).pop();
    return `${BASE_URL}${TIEN_TO_API.thukho}/QuanLySanPham/xem-anh/${encodeURIComponent(fileName)}`;
  }

  // 3. Nếu người dùng vừa bấm xóa ảnh ở phiên hiện tại thì dùng ảnh chữ cái mặc định
  const daXoa = localStorage.getItem(`sp_da_xoa_${ma}`);
  if (daXoa === 'true') {
    return taoPlaceholderSanPham(spObj?.TENSP || spObj?.tensp || spObj?.TenSP || '', ma);
  }

  // 4. Mặc định lấy URL stream ảnh từ máy chủ. Nếu chưa có ảnh (404), thẻ <img> tự động kích hoạt onerror và hiện chữ cái theo tên!
  return layUrlXemAnhSanPham(ma);
}

/**
 * Lưu ảnh sản phẩm vào bộ nhớ tạm / localStorage (tương thích ngược)
 */
export function luuAnhSanPham(maSP, dataUrl) {
  if (!maSP) return;
  const ma = String(maSP).trim().toUpperCase();
  if (dataUrl) {
    localStorage.setItem(`${TIEN_TO_ANH_SP}${ma}`, dataUrl);
  } else {
    localStorage.removeItem(`${TIEN_TO_ANH_SP}${ma}`);
    localStorage.removeItem(`${TIEN_TO_SERVER_SP}${ma}`);
  }
}

/**
 * Xóa ảnh sản phẩm (cả local và bộ nhớ tạm)
 */
export function xoaAnhSanPham(maSP) {
  if (!maSP) return;
  const ma = String(maSP).trim().toUpperCase();
  localStorage.removeItem(`${TIEN_TO_ANH_SP}${ma}`);
  localStorage.removeItem(`${TIEN_TO_SERVER_SP}${ma}`);
}

/**
 * Lấy ảnh đại diện nhân viên (Ưu tiên: Cache/Preview local -> Thuộc tính NV -> Stream từ máy chủ qua Gateway -> Chữ cái theo tên nhân viên)
 */
export function layAnhNhanVien(maNV, nvObj = null) {
  if (!maNV) return taoPlaceholderAvatar(nvObj?.TENNV || nvObj?.tennv || nvObj?.TenNV || '', maNV);
  const ma = String(maNV).trim().toUpperCase();

  // 1. Kiểm tra preview local hoặc URL vừa dán
  const anhCache = localStorage.getItem(`${TIEN_TO_AVATAR_NV}${ma}`);
  if (anhCache && anhCache.trim()) return anhCache;

  // 2. Kiểm tra thuộc tính trong object nhân viên (nếu có)
  const anhObj = nvObj?.HINHANH || nvObj?.hinhanh || nvObj?.AVATAR || nvObj?.avatar || nvObj?.ANH || nvObj?.anh || nvObj?.FILEPATH || nvObj?.filePath;
  if (anhObj && String(anhObj).trim()) {
    const val = String(anhObj).trim();
    if (val.startsWith('http://') || val.startsWith('https://') || val.startsWith('data:')) {
      return val;
    }
    const fileName = val.split(/[/\\]/).pop();
    return `${BASE_URL}${TIEN_TO_API.admin}/QuanLyNhanVien/xem-anh/${encodeURIComponent(fileName)}`;
  }

  // 3. Nếu người dùng vừa bấm xóa ảnh ở phiên hiện tại thì dùng ảnh chữ cái mặc định
  const daXoa = localStorage.getItem(`nv_da_xoa_${ma}`);
  if (daXoa === 'true') {
    return taoPlaceholderAvatar(nvObj?.TENNV || nvObj?.tennv || nvObj?.TenNV || '', ma);
  }

  // 4. Mặc định lấy URL stream ảnh từ máy chủ. Nếu chưa có ảnh (404), thẻ <img> tự động kích hoạt onerror và hiện chữ cái theo tên!
  return layUrlXemAnhNhanVien(ma);
}

/**
 * Lưu ảnh đại diện nhân viên vào bộ nhớ tạm (tương thích ngược)
 */
export function luuAnhNhanVien(maNV, dataUrl) {
  if (!maNV) return;
  const ma = String(maNV).trim().toUpperCase();
  if (dataUrl) {
    localStorage.setItem(`${TIEN_TO_AVATAR_NV}${ma}`, dataUrl);
  } else {
    localStorage.removeItem(`${TIEN_TO_AVATAR_NV}${ma}`);
    localStorage.removeItem(`${TIEN_TO_SERVER_NV}${ma}`);
  }
}

/**
 * Xóa ảnh đại diện nhân viên (cả local và bộ nhớ tạm)
 */
export function xoaAnhNhanVien(maNV) {
  if (!maNV) return;
  const ma = String(maNV).trim().toUpperCase();
  localStorage.removeItem(`${TIEN_TO_AVATAR_NV}${ma}`);
  localStorage.removeItem(`${TIEN_TO_SERVER_NV}${ma}`);
}
