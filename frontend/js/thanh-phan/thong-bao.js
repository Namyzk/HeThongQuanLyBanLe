/**
 * thong-bao.js — Toast notifications
 */

let _vungToast = null;

function layVungToast() {
  if (!_vungToast) {
    _vungToast = document.getElementById('vung-toast');
    if (!_vungToast) {
      _vungToast = document.createElement('div');
      _vungToast.id = 'vung-toast';
      document.body.appendChild(_vungToast);
    }
  }
  return _vungToast;
}

/**
 * Hiện toast
 * @param {string} tieuDe
 * @param {string} moTa
 * @param {'thanh-cong'|'loi'|'canh-bao'|'thong-tin'} loai
 * @param {number} thoiGian - ms
 */
export function hienToast(tieuDe, moTa = '', loai = 'thong-tin', thoiGian = 4000) {
  const vung = layVungToast();

  const bieututong = {
    'thanh-cong': '<i class="bi bi-check-circle-fill"></i>',
    'loi':        '<i class="bi bi-x-circle-fill"></i>',
    'canh-bao':   '<i class="bi bi-exclamation-triangle-fill"></i>',
    'thong-tin':  '<i class="bi bi-info-circle-fill"></i>',
  }[loai] || '<i class="bi bi-info-circle-fill"></i>';

  const el = document.createElement('div');
  el.className = `toast toast-${loai}`;
  el.innerHTML = `
    <div class="toast-bieu-tuong">${bieututong}</div>
    <div class="toast-noi-dung">
      <div class="toast-tieu-de">${escapeHtml(tieuDe)}</div>
      ${moTa ? `<div class="toast-mo-ta">${escapeHtml(moTa)}</div>` : ''}
    </div>
    <button class="nut-dong-toast" aria-label="Đóng" style="margin-left:auto;flex-shrink:0;padding:4px;color:var(--van-ban-mo)">
      <i class="bi bi-x"></i>
    </button>
  `;

  vung.appendChild(el);

  const dong = () => {
    el.classList.add('thoat');
    el.addEventListener('animationend', () => el.remove(), { once: true });
  };

  el.querySelector('.nut-dong-toast').addEventListener('click', dong);

  if (thoiGian > 0) {
    setTimeout(dong, thoiGian);
  }
}

function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

/** Shorthand */
export const toast = {
  ok:     (tieu, mo)  => hienToast(tieu, mo, 'thanh-cong'),
  loi:    (tieu, mo)  => hienToast(tieu, mo, 'loi'),
  canh:   (tieu, mo)  => hienToast(tieu, mo, 'canh-bao'),
  info:   (tieu, mo)  => hienToast(tieu, mo, 'thong-tin'),
};
