/**
 * goi-api.js — Hàm gọi API qua Gateway với JWT
 */
import { BASE_URL, TIEN_TO_API } from './cau-hinh.js';
import { layNguoiDung, luuPhien, xoaPhien } from './xac-thuc.js';
import { hienToast } from './thanh-phan/thong-bao.js';
import { dinhHuong } from './dieu-huong.js';

let _activeBaseUrl = BASE_URL;
let _refreshPromise = null;

/**
 * ASP.NET có thể serialize MASP thành masp, trong khi một số màn hình cũ
 * vẫn đọc row.MASP. Proxy này cho phép đọc thuộc tính không phân biệt hoa
 * thường mà không thêm các key trùng vào Object.keys/JSON.stringify.
 */
function boSungTruyCapKhongPhanBietHoaThuong(value, cache = new WeakMap()) {
  if (value === null || typeof value !== 'object') return value;
  if (cache.has(value)) return cache.get(value);

  const proxy = new Proxy(value, {
    get(target, property, receiver) {
      if (typeof property !== 'string' || Reflect.has(target, property)) {
        return boSungTruyCapKhongPhanBietHoaThuong(
          Reflect.get(target, property, receiver), cache
        );
      }

      const actualProperty = Object.keys(target).find(
        key => key.toLowerCase() === property.toLowerCase()
      );
      return actualProperty === undefined
        ? undefined
        : boSungTruyCapKhongPhanBietHoaThuong(target[actualProperty], cache);
    },
    set(target, property, newValue, receiver) {
      if (typeof property === 'string' && !Reflect.has(target, property)) {
        const actualProperty = Object.keys(target).find(
          key => key.toLowerCase() === property.toLowerCase()
        );
        if (actualProperty !== undefined) {
          return Reflect.set(target, actualProperty, newValue, receiver);
        }
      }
      return Reflect.set(target, property, newValue, receiver);
    },
  });

  cache.set(value, proxy);
  return proxy;
}

/** Lấy Base URL đang hoạt động */
export function layBaseUrl() {
  return _activeBaseUrl;
}

/** Đổi Base URL thủ công nếu cần */
export function datBaseUrl(url) {
  _activeBaseUrl = url;
}

/** Refresh một lần dùng chung khi nhiều request cùng nhận 401 hoặc gọi chủ động định kỳ. */
export function lamMoiPhien() {
  if (!_refreshPromise) {
    _refreshPromise = (async () => {
      try {
        const json = await goiApi(`${TIEN_TO_API.login}/refresh`, {
          method: 'POST',
          anonymous: true,
          skipRefresh: true,
        });

        if (!json?.success) return false;
        luuPhien(
          json.data || layNguoiDung(),
          json.refreshTokenExpiresAtUtc || json.refreshTokenExpiresAt
        );
        return true;
      } catch {
        return false;
      }
    })().finally(() => {
      _refreshPromise = null;
    });
  }
  return _refreshPromise;
}

/**
 * Hàm gọi API chính
 * @param {string} path - Đường dẫn (bao gồm tiền tố, ví dụ /api/admin/QuanLyTaiKhoan/get-all-taikhoan)
 * @param {RequestInit & { params?: Record<string,any>, anonymous?: boolean }} options
 */
export async function goiApi(path, options = {}) {
  const { params, anonymous = false, skipRefresh = false, ...fetchOptions } = options;

  function buildUrl(base) {
    let url = base + path;
    if (params && Object.keys(params).length > 0) {
      const qs = new URLSearchParams();
      for (const [k, v] of Object.entries(params)) {
        if (v !== null && v !== undefined && v !== '') {
          qs.append(k, String(v));
        }
      }
      const qsStr = qs.toString();
      if (qsStr) url += (path.includes('?') ? '&' : '?') + qsStr;
    }
    return url;
  }

  // JWT không được đọc bằng JavaScript; trình duyệt tự gửi cookie HttpOnly.
  const headers = { ...(fetchOptions.headers || {}) };

  if (fetchOptions.body && typeof fetchOptions.body === 'string') {
    headers['Content-Type'] = 'application/json';
  }

  let response;
  try {
    response = await fetch(buildUrl(_activeBaseUrl), {
      ...fetchOptions,
      credentials: 'include',
      headers,
    });
  } catch {
    throw new LỗiAPI(0, 'Không thể kết nối API Gateway qua HTTPS. Hãy kiểm tra Gateway cổng 7144 và chứng chỉ HTTPS.');
  }

  // Access token hết hạn: đổi refresh token rồi thử lại request đúng một lần.
  if (response.status === 401 && !anonymous) {
    if (!skipRefresh && await lamMoiPhien()) {
      return goiApi(path, { ...options, skipRefresh: true });
    }

    try {
      await goiApi(`${TIEN_TO_API.login}/logout`, {
        method: 'POST', anonymous: true, skipRefresh: true,
      });
    } catch {}
    xoaPhien();
    hienToast('Phiên đăng nhập hết hạn', 'Vui lòng đăng nhập lại.', 'canh-bao');
    setTimeout(() => dinhHuong('dang-nhap'), 1500);
    throw new LỗiAPI(401, 'Phiên đăng nhập hết hạn.');
  }

  // Xử lý 403
  if (response.status === 403) {
    throw new LỗiAPI(403, 'Bạn không có quyền thực hiện thao tác này.');
  }

  // Parse JSON an toàn
  let json = null;
  const contentType = response.headers.get('content-type') || '';
  if (contentType.includes('application/json') || contentType.includes('+json')) {
    try {
      json = await response.json();
    } catch {
      json = null;
    }
  } else {
    try {
      await response.text(); // đọc body để tránh leak
    } catch {}
  }

  // Kiểm tra HTTP status
  if (!response.ok) {
    const msg = json?.message || `Lỗi HTTP ${response.status}`;
    throw new LỗiAPI(response.status, msg, json);
  }

  // Kiểm tra trường success trong JSON
  if (json !== null && json.success === false) {
    throw new LỗiAPI(response.status, json.message || 'Thao tác thất bại.', json);
  }

  // Chuẩn hóa truy cập dữ liệu dùng chung cho các trang danh sách và chi tiết.
  if (Array.isArray(json)) {
    json = boSungTruyCapKhongPhanBietHoaThuong(json);
  } else if (json && typeof json === 'object' && 'data' in json) {
    json.data = boSungTruyCapKhongPhanBietHoaThuong(json.data);
  }

  return json;
}

/** Lớp lỗi API có thêm thông tin chi tiết */
export class LỗiAPI extends Error {
  constructor(status, message, data = null) {
    super(message);
    this.status = status;
    this.data = data;
    this.name = 'LỗiAPI';
  }
}

/** GET */
export function apiGet(path, params) {
  return goiApi(path, { method: 'GET', params });
}

/** POST với JSON body */
export function apiPost(path, body, params) {
  return goiApi(path, {
    method: 'POST',
    body: body !== undefined ? JSON.stringify(body) : undefined,
    params,
  });
}

/** PUT với JSON body */
export function apiPut(path, body, params) {
  return goiApi(path, {
    method: 'PUT',
    body: body !== undefined ? JSON.stringify(body) : undefined,
    params,
  });
}

/** PATCH với params (không body) */
export function apiPatch(path, params) {
  return goiApi(path, { method: 'PATCH', params });
}

/** DELETE */
export function apiDelete(path, params) {
  return goiApi(path, { method: 'DELETE', params });
}

/** Upload FormData (multipart/form-data) */
export function apiUpload(path, formData, params) {
  return goiApi(path, {
    method: 'POST',
    body: formData,
    params,
  });
}

