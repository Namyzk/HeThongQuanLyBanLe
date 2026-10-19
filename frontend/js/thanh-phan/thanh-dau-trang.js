/**
 * thanh-dau-trang.js — Header dùng chung cho các trang
 */
import { layNguoiDung, layVaiTro, xoaPhien, conBaoNhieuGiay, conRefreshToken } from '../xac-thuc.js';
import { dinhHuong } from '../dieu-huong.js';
import { BASE_URL, TEN_VAI_TRO, CANH_BAO_TRUOC } from '../cau-hinh.js';
import { hienToast } from './thong-bao.js';
import { lamMoiPhien } from '../goi-api.js';

let _intervalCanhBao = null;
let _dangRefreshNgam = false;

/** Khởi tạo header */
export function khoiHeader(tieuDe = null, duongDan = null) {
  if (tieuDe) {
    capNhatTieuDe(tieuDe, duongDan || `Trang chủ / ${tieuDe}`);
  }
  capNhatThongTinUser();
  ganSuKienDangXuat();
  batDauDemNguoc();
  kiemTraVaRefreshNgam();
}

/** Cập nhật thông tin user trên header */
export function capNhatThongTinUser() {
  const nguoiDung = layNguoiDung();
  const vaiTro = layVaiTro();
  if (!nguoiDung) return;

  const tenHienThi = nguoiDung.userName || nguoiDung.username || 'Người dùng';
  const tenVT = TEN_VAI_TRO[vaiTro] || vaiTro || '';

  const elTen = document.getElementById('header-user-ten');
  const elVT  = document.getElementById('header-user-vai-tro');
  const elAV  = document.getElementById('header-user-avatar');

  if (elTen) elTen.textContent = tenHienThi;
  if (elVT)  elVT.textContent  = tenVT;
  if (elAV) {
    const maNV = nguoiDung.maNV || nguoiDung.manv || '';
    const anhCache = maNV ? localStorage.getItem('nv_avatar_' + maNV) : null;
    if (anhCache) {
      elAV.innerHTML = `<img src="${anhCache}" alt="" style="width:100%;height:100%;border-radius:50%;object-fit:cover;" />`;
    } else {
      elAV.textContent = tenHienThi.charAt(0).toUpperCase();
    }
  }
}

/** Gắn sự kiện nút đăng xuất */
function ganSuKienDangXuat() {
  const nutDangXuat = document.getElementById('nut-dang-xuat');
  if (nutDangXuat && !nutDangXuat._hasListener) {
    nutDangXuat._hasListener = true;
    nutDangXuat.addEventListener('click', thucHienDangXuat);
  }
}

/** Đăng xuất */
export function thucHienDangXuat() {
  fetch(`${BASE_URL}/api/Login/logout`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: '{}',
  }).catch(() => {});
  
  xoaPhien();
  if (_intervalCanhBao) clearInterval(_intervalCanhBao);
  hienToast('Đã đăng xuất', 'Hẹn gặp lại!', 'thong-tin', 2000);
  setTimeout(() => dinhHuong('dang-nhap'), 400);
}

/** Tự động gia hạn phiên ngầm nếu gần hết hạn */
async function kiemTraVaRefreshNgam() {
  if (_dangRefreshNgam) return;
  const conLai = conBaoNhieuGiay();
  if (conLai <= CANH_BAO_TRUOC && conRefreshToken()) {
    _dangRefreshNgam = true;
    try {
      const thanhCong = await lamMoiPhien();
      if (thanhCong) {
        const elCanhBao = document.getElementById('canh-bao-token');
        if (elCanhBao) elCanhBao.classList.remove('hien');
      }
    } catch {
      // Bỏ qua lỗi ngầm, sẽ retry ở lần kiểm tra kế tiếp
    } finally {
      _dangRefreshNgam = false;
    }
  }
}

/** Đếm ngược và tự động gia hạn token ngầm định kỳ */
function batDauDemNguoc() {
  if (_intervalCanhBao) clearInterval(_intervalCanhBao);

  // Tự động kiểm tra ngay khi người dùng quay lại tab sau khi ngủ/ẩn
  if (typeof window !== 'undefined' && !window._langNgheVisibility) {
    window._langNgheVisibility = true;
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'visible') {
        kiemTraVaRefreshNgam();
      }
    });
  }

  _intervalCanhBao = setInterval(async () => {
    const conLai = conBaoNhieuGiay();
    const elCanhBao = document.getElementById('canh-bao-token');

    // 1. Nếu còn dưới ngưỡng CANH_BAO_TRUOC (120s), tự động gia hạn ngầm
    if (conLai <= CANH_BAO_TRUOC && conRefreshToken()) {
      if (!_dangRefreshNgam) {
        _dangRefreshNgam = true;
        try {
          const thanhCong = await lamMoiPhien();
          if (thanhCong) {
            if (elCanhBao) elCanhBao.classList.remove('hien');
            return;
          }
        } catch {
          // Gặp lỗi mạng -> tiếp tục xuống dưới để hiển thị cảnh báo
        } finally {
          _dangRefreshNgam = false;
        }
      }
    }

    // 2. Nếu đã hết hạn (<= 0)
    if (conLai <= 0) {
      if (conRefreshToken()) {
        if (!_dangRefreshNgam) {
          _dangRefreshNgam = true;
          const ok = await lamMoiPhien().finally(() => { _dangRefreshNgam = false; });
          if (ok) {
            if (elCanhBao) elCanhBao.classList.remove('hien');
            return;
          }
        }
      }
      clearInterval(_intervalCanhBao);
      thucHienDangXuat();
      return;
    }

    // 3. Nếu token sắp hết hạn mà refresh ngầm chưa hoàn thành (ví dụ mạng chậm hoặc chập chờn)
    if (conLai <= CANH_BAO_TRUOC) {
      const giay = Math.ceil(conLai);
      if (elCanhBao) {
        elCanhBao.innerHTML = `
          <i class="bi bi-clock-history"></i>
          <span>Phiên hết sau ${giay}s</span>
          <button type="button" class="nut-gia-han" id="btn-gia-han-nhanh" title="Nhấn để gia hạn phiên ngay">Gia hạn</button>
        `;
        elCanhBao.classList.add('hien');

        const btnGiaHan = document.getElementById('btn-gia-han-nhanh');
        if (btnGiaHan && !btnGiaHan._hasListener) {
          btnGiaHan._hasListener = true;
          btnGiaHan.addEventListener('click', async (e) => {
            e.stopPropagation();
            btnGiaHan.disabled = true;
            btnGiaHan.textContent = '...';
            const ok = await lamMoiPhien();
            if (ok) {
              hienToast('Gia hạn thành công', 'Phiên đăng nhập đã được kéo dài.', 'thanh-cong');
              elCanhBao.classList.remove('hien');
            } else {
              hienToast('Gia hạn thất bại', 'Vui lòng kiểm tra lại kết nối mạng.', 'loi');
              btnGiaHan.disabled = false;
              btnGiaHan.textContent = 'Gia hạn';
            }
          });
        }
      }
    } else {
      if (elCanhBao) elCanhBao.classList.remove('hien');
    }
  }, 5000);
}

/** Cập nhật tiêu đề trang trên header */
export function capNhatTieuDe(ten, duongDan = '') {
  const elTen = document.getElementById('header-tieu-de-trang');
  const elDD  = document.getElementById('header-duong-dan');
  if (elTen) elTen.textContent = ten;
  if (elDD)  elDD.textContent  = duongDan;
}
