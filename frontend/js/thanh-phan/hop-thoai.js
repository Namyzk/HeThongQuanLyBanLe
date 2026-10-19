/**
 * hop-thoai.js — Modal dùng chung
 * Hỗ trợ các options:
 * - tieuDe, noiDung, chieuRong, loai, chan, nutDongY, nutHuy, anNutDongDauTrang, chiMotNutDong
 */

let _modalEl = null;
let _currentOnDong = null;

function layModal() {
  if (!_modalEl) {
    _modalEl = document.getElementById('modal-chung');
  }
  return _modalEl;
}

/**
 * Hiện modal
 */
export function hienModal(arg1, arg2, arg3) {
  let tieuDe = '';
  let noiDung = '';
  let options = {};

  if (typeof arg1 === 'object' && arg1 !== null && !(arg1 instanceof HTMLElement)) {
    tieuDe = arg1.tieuDe || '';
    noiDung = arg1.noiDung || '';
    options = arg1;
  } else {
    tieuDe = arg1 || '';
    noiDung = arg2 || '';
    options = arg3 || {};
  }

  const modal = layModal();
  if (!modal) {
    console.error('Không tìm thấy #modal-chung trong DOM');
    return { dong: () => {}, hopEl: null };
  }

  const hopEl = modal.querySelector('.modal-hop');
  const tieuDeEl = modal.querySelector('.modal-tieu-de');
  const noiDungEl = modal.querySelector('.modal-noi-dung');
  const chanEl = modal.querySelector('.modal-chan');
  const nutDongDau = modal.querySelector('.modal-nut-dong');

  // Reset classes & styles
  hopEl.className = 'modal-hop';
  if (options.loai) hopEl.classList.add(options.loai);
  if (options.chieuRong) {
    hopEl.style.maxWidth = options.chieuRong;
  } else {
    hopEl.style.maxWidth = '';
  }

  // Tùy chọn ẩn nút đóng góc trên để chỉ giữ 1 nút đóng duy nhất
  if (nutDongDau) {
    if (options.anNutDongDauTrang || options.chiMotNutDong) {
      nutDongDau.style.display = 'none';
    } else {
      nutDongDau.style.display = '';
    }
  }

  tieuDeEl.textContent = tieuDe;

  if (typeof noiDung === 'string') {
    noiDungEl.innerHTML = noiDung;
  } else {
    noiDungEl.innerHTML = '';
    noiDungEl.appendChild(noiDung);
  }

  _currentOnDong = typeof options.onDong === 'function' ? options.onDong : null;

  const dong = () => {
    modal.classList.remove('hien');
    modal.setAttribute('aria-hidden', 'true');
    noiDungEl.innerHTML = '';
    chanEl.innerHTML = '';
    if (nutDongDau) nutDongDau.style.display = '';
    const cb = _currentOnDong;
    _currentOnDong = null;
    if (cb) {
      try { cb(); } catch (err) { console.error('Lỗi onDong modal:', err); }
    }
  };

  // Tạo footer nếu có cấu hình nút hoặc options.chan
  if (options.chan) {
    chanEl.innerHTML = options.chan;
  } else if (options.nutDongY) {
    const nhanDongY = options.nutDongY.nhan || 'Đồng ý';
    const loaiDongY = options.nutDongY.loai || 'chinh';
    const coNutHuy = options.nutHuy && !options.chiMotNutDong;
    const nhanHuy = options.nutHuy?.nhan || 'Hủy';

    chanEl.innerHTML = `
      ${coNutHuy ? `<button type="button" id="modal-nut-huy" class="nut nut-vien">${nhanHuy}</button>` : ''}
      <button type="button" id="modal-nut-dong-y" class="nut nut-${loaiDongY}">${nhanDongY}</button>
    `;

    const btnHuy = document.getElementById('modal-nut-huy');
    const btnDongY = document.getElementById('modal-nut-dong-y');

    if (btnHuy) btnHuy.onclick = dong;
    if (btnDongY) {
      btnDongY.onclick = async () => {
        if (typeof options.nutDongY.onClick === 'function') {
          btnDongY.disabled = true;
          const textGoc = btnDongY.innerHTML;
          btnDongY.innerHTML = '<span class="quay-vong quay-vong-nho"></span> Đang xử lý...';
          try {
            const kq = await options.nutDongY.onClick();
            if (kq !== false) {
              dong();
            }
          } catch (err) {
            console.error('Lỗi nút modal đồng ý:', err);
          } finally {
            btnDongY.disabled = false;
            btnDongY.innerHTML = textGoc;
          }
        } else {
          dong();
        }
      };
    }
  } else {
    chanEl.innerHTML = '';
  }

  modal.classList.add('hien');
  modal.setAttribute('aria-hidden', 'false');

  // Focus vào trường đầu tiên
  setTimeout(() => {
    const nutDauTien = modal.querySelector('input:not([disabled]), select:not([disabled]), textarea:not([disabled]), button:not([disabled])');
    if (nutDauTien) nutDauTien.focus();
  }, 60);

  // Đóng khi click icon đóng
  if (nutDongDau) {
    nutDongDau.onclick = dong;
  }

  // Đóng khi click nền ngoài modal
  modal.onclick = (e) => {
    if (e.target === modal) dong();
  };

  // Đóng khi nhấn phím Escape
  const handleEsc = (e) => {
    if (e.key === 'Escape') {
      dong();
      document.removeEventListener('keydown', handleEsc);
    }
  };
  document.addEventListener('keydown', handleEsc);

  return { dong, hopEl };
}

/**
 * Hiện hộp thoại xác nhận
 */
export function hienXacNhan(arg1, arg2, arg3, arg4) {
  let tieuDe = '';
  let noiDung = '';
  let khiXacNhan = null;
  let loaiNut = 'nguy-hiem';

  if (typeof arg1 === 'object' && arg1 !== null) {
    tieuDe = arg1.tieuDe || 'Xác nhận';
    noiDung = arg1.noiDung || '';
    khiXacNhan = arg1.nutDongY?.onClick || null;
    loaiNut = arg1.loai || 'nguy-hiem';
  } else {
    tieuDe = arg1 || 'Xác nhận';
    noiDung = arg2 || '';
    khiXacNhan = arg3;
    loaiNut = arg4 || 'nguy-hiem';
  }

  const noiDungHtml = `
    <div class="xac-nhan-hop" style="display:flex;flex-direction:column;align-items:center;text-align:center;padding:var(--kc-l);">
      <div style="font-size:2.5rem;color:${loaiNut === 'nguy-hiem' ? 'var(--mau-nguy-hiem)' : 'var(--mau-chinh)'};margin-bottom:var(--kc-m);">
        <i class="bi ${loaiNut === 'nguy-hiem' ? 'bi-exclamation-triangle-fill' : 'bi-question-circle-fill'}"></i>
      </div>
      <div style="font-size:var(--font-lon);font-weight:700;margin-bottom:var(--kc-s);">${tieuDe}</div>
      <div style="color:var(--van-ban-phu);font-size:var(--font-binh);">${noiDung}</div>
    </div>
  `;

  const chanHtml = `
    <button type="button" id="nut-huy-xacnhan" class="nut nut-vien">Hủy</button>
    <button type="button" id="nut-ok-xacnhan" class="nut nut-${loaiNut}">Xác nhận</button>
  `;

  const { dong } = hienModal(tieuDe, noiDungHtml, { loai: 'nho', chan: chanHtml });

  const btnHuy = document.getElementById('nut-huy-xacnhan');
  const btnOk = document.getElementById('nut-ok-xacnhan');

  if (btnHuy) btnHuy.onclick = dong;
  if (btnOk) {
    btnOk.onclick = async () => {
      btnOk.disabled = true;
      btnOk.innerHTML = '<span class="quay-vong quay-vong-nho"></span> Đang xử lý...';
      try {
        if (typeof khiXacNhan === 'function') {
          const res = await khiXacNhan();
          if (res !== false) dong();
        } else {
          dong();
        }
      } catch (err) {
        console.error(err);
      } finally {
        btnOk.disabled = false;
        btnOk.textContent = 'Xác nhận';
      }
    };
  }
}

/** Đóng modal */
export function dongModal() {
  const modal = layModal();
  if (modal) {
    modal.classList.remove('hien');
    modal.setAttribute('aria-hidden', 'true');
    const nd = modal.querySelector('.modal-noi-dung');
    const chan = modal.querySelector('.modal-chan');
    const nutDongDau = modal.querySelector('.modal-nut-dong');
    if (nd) nd.innerHTML = '';
    if (chan) chan.innerHTML = '';
    if (nutDongDau) nutDongDau.style.display = '';
    const cb = _currentOnDong;
    _currentOnDong = null;
    if (cb) {
      try { cb(); } catch (err) { console.error('Lỗi onDong modal:', err); }
    }
  }
}
