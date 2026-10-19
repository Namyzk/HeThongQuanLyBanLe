/**
 * bieu-mau.js — Helpers xây dựng và xử lý form
 */
import { escHtml, homNay } from '../tien-ich.js';

/**
 * Lấy giá trị form thành object
 * @param {HTMLFormElement} form
 */
export function layGiaTriForm(form) {
  const data = {};
  const formData = new FormData(form);
  for (const [key, value] of formData.entries()) {
    data[key] = value;
  }
  return data;
}

/**
 * Điền giá trị vào form từ object
 * @param {HTMLFormElement} form
 * @param {object} data
 */
export function dienGiaTriForm(form, data) {
  if (!data) return;
  for (const [key, value] of Object.entries(data)) {
    const el = form.elements[key];
    if (!el) continue;
    if (el.type === 'checkbox') {
      el.checked = Boolean(value);
    } else if (el.type === 'date' && value) {
      // Đảm bảo ngày đúng định dạng YYYY-MM-DD
      el.value = String(value).substring(0, 10);
    } else {
      el.value = value ?? '';
    }
  }
}

/**
 * Reset tất cả trường form
 */
export function xoaForm(form) {
  form.reset();
  // Xóa class lỗi
  form.querySelectorAll('.loi').forEach(el => el.classList.remove('loi'));
  form.querySelectorAll('.loi-truong').forEach(el => el.remove());
}

/**
 * Hiện lỗi trên trường
 */
export function hienLoiTruong(form, tenTruong, moTa) {
  const el = form.elements[tenTruong];
  if (!el) return;
  el.classList.add('loi');
  let loiEl = el.parentElement.querySelector('.loi-truong');
  if (!loiEl) {
    loiEl = document.createElement('div');
    loiEl.className = 'loi-truong';
    el.parentElement.insertBefore(loiEl, el.nextSibling);
  }
  loiEl.innerHTML = `<i class="bi bi-exclamation-circle"></i> ${escHtml(moTa)}`;
}

/**
 * Xóa lỗi trên trường
 */
export function xoaLoiTruong(form, tenTruong) {
  const el = form.elements[tenTruong];
  if (!el) return;
  el.classList.remove('loi');
  el.parentElement.querySelector('.loi-truong')?.remove();
}

/**
 * Khóa/mở form khi đang submit
 */
export function khoaForm(form, laMo = false) {
  form.querySelectorAll('input, select, textarea, button').forEach(el => {
    el.disabled = !laMo;
  });
}

/**
 * Build HTML select từ mảng options
 * @param {Array<{value, label}>} opts
 * @param {string} placeholder
 */
export function buildSelect(opts, placeholder = '-- Chọn --') {
  const optionHtml = opts.map(o =>
    `<option value="${escHtml(String(o.value))}">${escHtml(o.label)}</option>`
  ).join('');
  return `<option value="">${escHtml(placeholder)}</option>${optionHtml}`;
}

/** Input ngày hôm nay là mặc định */
export function setDefaultDate(form, tenTruong) {
  const el = form.elements[tenTruong];
  if (el && !el.value) el.value = homNay();
}
