/**
 * bang-du-lieu.js — Render bảng dữ liệu dùng chung
 */
import { escHtml, formatNgay, formatTien, debounce } from '../tien-ich.js';

/** Danh sách trường cần ẩn */
const TRUONG_AN = ['PASS', 'password', 'mat_khau', 'matkhau'];

/** Kiểm tra field có nên ẩn không */
function canAn(key) {
  return TRUONG_AN.some(t => key.toLowerCase() === t.toLowerCase());
}

/** Tự động format giá trị ô với Badge trạng thái thông minh */
function formatGiaTri(key, value) {
  if (value === null || value === undefined) return '<span style="color:var(--van-ban-mo)">—</span>';
  const keyLower = key.toLowerCase();

  // 1. Format Ngày tháng
  if (keyLower.includes('ngay') || keyLower.includes('date')) {
    return `<span style="white-space:nowrap;font-size:var(--font-nho);">${formatNgay(value)}</span>`;
  }

  // 2. Format Tiền tệ
  if (keyLower.includes('tien') || keyLower.includes('gia') || keyLower.includes('thanhtien') || keyLower === 'thuevat' || keyLower.includes('con_no') || keyLower.includes('da_thanh_toan') || keyLower.includes('tong_phai_tra')) {
    const n = Number(value);
    if (!isNaN(n)) {
      if (keyLower.includes('con_no') && n > 0) {
        return `<strong style="color:var(--mau-nguy-hiem);">${formatTien(n)}</strong>`;
      }
      return `<span style="font-weight:600;color:var(--van-ban-chinh);">${formatTien(n)}</span>`;
    }
  }

  // 3. Format Tồn kho
  if (keyLower.includes('soluongton') || keyLower === 'tonkho') {
    const n = Number(value);
    if (!isNaN(n)) {
      if (n <= 0) return `<span class="badge badge-do"><i class="bi bi-x-circle"></i> Hết hàng (0)</span>`;
      if (n <= 5) return `<span class="badge badge-vang"><i class="bi bi-exclamation-triangle"></i> Sắp hết (${n})</span>`;
      return `<span class="badge badge-xanh"><i class="bi bi-check-circle"></i> Sẵn sàng (${n})</span>`;
    }
  }

  // 4. Format Trạng thái
  if (keyLower.includes('trangthai') || keyLower.includes('trang_thai')) {
    const str = String(value).trim();
    if (str.includes('Đã thu đủ') || str.includes('Đã thanh toán') || str.includes('Hoàn tất') || str.includes('Thành công')) {
      return `<span class="badge badge-xanh"><i class="bi bi-check2"></i> ${escHtml(str)}</span>`;
    }
    if (str.includes('Chưa') || str.includes('Hủy') || str.includes('Thất bại')) {
      return `<span class="badge badge-do"><i class="bi bi-clock"></i> ${escHtml(str)}</span>`;
    }
    if (str.includes('1 phần') || str.includes('Đang')) {
      return `<span class="badge badge-vang"><i class="bi bi-hourglass-split"></i> ${escHtml(str)}</span>`;
    }
    return `<span class="badge badge-lam">${escHtml(str)}</span>`;
  }

  // 5. Format Quyền tài khoản
  if (keyLower === 'quyen') {
    const q = Number(value);
    if (q === 1) return `<span class="badge badge-do"><i class="bi bi-shield-lock"></i> Admin</span>`;
    if (q === 2) return `<span class="badge badge-xanh"><i class="bi bi-cart"></i> ThuNgan</span>`;
    if (q === 3) return `<span class="badge badge-lam"><i class="bi bi-box-seam"></i> ThuKho</span>`;
    if (q === 4) return `<span class="badge badge-tim"><i class="bi bi-cash"></i> KeToan</span>`;
  }

  // 6. Format Hình ảnh / Avatar
  if (keyLower === 'avatar' || keyLower === 'hinhanh' || keyLower === 'anh') {
    if (!value) return '<span style="color:var(--van-ban-mo)">—</span>';
    return `<img src="${escHtml(value)}" alt="Hình ảnh" style="width:38px;height:38px;border-radius:var(--bo-goc-vua);object-fit:cover;vertical-align:middle;border:1px solid var(--duong-vien);background:#fff;" onerror="this.style.opacity='0.4'" />`;
  }

  return escHtml(String(value));
}

/**
 * Tạo bảng dữ liệu có tìm kiếm
 */
export function taoKhungBang(container, duLieu, caiDat = {}) {
  const {
    cotHien: _cotHien,
    tenCot = {},
    nutHanhDong,
    cotTimKiem: _cotTimKiem,
    khiChonHang,
    soDongMoiTrang = 50,
    dinhDangCot = {},
  } = caiDat;

  // Xác định các cột
  const cotHien = _cotHien || Object.keys(duLieu[0] || {}).filter(k => !canAn(k));
  const kichThuocTrang = Math.max(1, Number(soDongMoiTrang) || 50);
  let trangHienTai = 1;

  // Hàm render bảng
  function renderBang(mangData, giuTrang = false) {
    if (mangData.length === 0) {
      trangHienTai = 1;
      container.innerHTML = `
        <div class="trang-thai-bang">
          <div class="bieu-tuong"><i class="bi bi-inbox" style="color:var(--xam-300);"></i></div>
          <div class="thong-bao">Không tìm thấy dữ liệu</div>
          <div class="mo-ta">Chưa có bản ghi nào hoặc không khớp từ khóa tìm kiếm.</div>
        </div>`;
      return;
    }

    if (!giuTrang) trangHienTai = 1;
    const tongTrang = Math.ceil(mangData.length / kichThuocTrang);
    trangHienTai = Math.min(Math.max(1, trangHienTai), tongTrang);
    const viTriBatDau = (trangHienTai - 1) * kichThuocTrang;
    const duLieuTrang = mangData.slice(viTriBatDau, viTriBatDau + kichThuocTrang);

    const headers = cotHien.map(k =>
      `<th>${escHtml(tenCot[k] || k)}</th>`
    ).join('');

    const rows = duLieuTrang.map((row, idx) => {
      const cells = cotHien.map(k => {
        if (dinhDangCot && typeof dinhDangCot[k] === 'function') {
          return `<td>${dinhDangCot[k](row[k], row)}</td>`;
        }
        return `<td>${formatGiaTri(k, row[k])}</td>`;
      }).join('');

      const hanhDongHtml = nutHanhDong ? `<td class="hanh-dong"><div class="cum-nut-hanh-dong">${nutHanhDong(row, viTriBatDau + idx)}</div></td>` : '';

      return `<tr data-idx="${viTriBatDau + idx}" style="cursor:${khiChonHang ? 'pointer' : 'default'}">${cells}${hanhDongHtml}</tr>`;
    }).join('');

    const colHanhDong = nutHanhDong ? '<th class="cot-thao-tac" style="min-width:160px;text-align:center;">Thao tác</th>' : '';
    const phanTrang = tongTrang > 1 ? `
      <div style="display:flex;align-items:center;justify-content:space-between;gap:12px;padding:12px 16px;">
        <span style="color:var(--van-ban-phu);font-size:var(--font-nho);">
          Hiển thị ${formatSoDong(viTriBatDau + 1)}–${formatSoDong(Math.min(viTriBatDau + kichThuocTrang, mangData.length))} / ${formatSoDong(mangData.length)} dòng
        </span>
        <div style="display:flex;gap:8px;align-items:center;">
          <button type="button" class="nut nut-vien nut-nho" data-trang="truoc" ${trangHienTai === 1 ? 'disabled' : ''}>Trước</button>
          <span style="font-size:var(--font-nho);">${trangHienTai} / ${tongTrang}</span>
          <button type="button" class="nut nut-vien nut-nho" data-trang="sau" ${trangHienTai === tongTrang ? 'disabled' : ''}>Sau</button>
        </div>
      </div>` : '';

    container.innerHTML = `
      <div class="vung-bang">
        <table class="bang-du-lieu" id="bang-chinh-${Date.now()}">
          <thead><tr>${headers}${colHanhDong}</tr></thead>
          <tbody>${rows}</tbody>
        </table>${phanTrang}
      </div>`;

    container.querySelector('[data-trang="truoc"]')?.addEventListener('click', () => {
      trangHienTai -= 1;
      renderBang(mangData, true);
    });
    container.querySelector('[data-trang="sau"]')?.addEventListener('click', () => {
      trangHienTai += 1;
      renderBang(mangData, true);
    });

    // Gắn sự kiện hàng
    if (khiChonHang) {
      container.querySelectorAll('tbody tr').forEach(tr => {
        tr.addEventListener('click', (e) => {
          if (e.target.closest('button, a, .nut')) return;
          const i = parseInt(tr.dataset.idx, 10);
          khiChonHang(duLieuTrang[i - viTriBatDau], i);
        });
      });
    }

    return container;
  }

  renderBang(duLieu);

  return {
    capNhat: (data) => {
      const duLieuMoi = data || [];
      const input = document.querySelector('[data-tim-kiem-bang]');
      const tuKhoa = input ? input.value.toLowerCase().trim() : '';
      if (tuKhoa) {
        const cotTK = _cotTimKiem || cotHien;
        const loc = duLieuMoi.filter(row =>
          cotTK.some(k => String(row[k] ?? '').toLowerCase().includes(tuKhoa))
        );
        renderBang(loc);
      } else {
        renderBang(duLieuMoi);
      }
    },
    renderBang,
  };
}

function formatSoDong(value) {
  return new Intl.NumberFormat('vi-VN').format(value);
}

/**
 * Tạo đầy đủ khung: toolbar + bảng + tìm kiếm
 */
export function khoiBangDayDu({
  container,
  title,
  duLieu,
  cotHien,
  tenCot = {},
  nutHanhDong,
  cotTimKiem,
  nutThem,
  khiChonHang,
  soHang = 0,
  dinhDangCot = {},
}) {
  const totalStr = soHang > 0 ? ` (${soHang})` : (duLieu.length ? ` (${duLieu.length})` : '');

  const timKiemId = `tim-kiem-${Date.now()}`;
  const bangId = `vung-bang-${Date.now()}`;

  let html = `
    <div class="the fade-in">
      <div class="the-dau">
        <div>
          <div class="the-tieu-de">${escHtml(title)}${totalStr}</div>
        </div>
        <div class="thanh-cong-cu">
          <div class="o-tim-kiem">
            <i class="bi bi-search"></i>
            <input type="search" id="${timKiemId}" placeholder="Tìm kiếm nhanh..." aria-label="Tìm kiếm">
          </div>`;

  if (nutThem && nutThem.nhan) {
    html += `<button id="nut-them-chinh" class="nut nut-chinh" aria-label="${escHtml(nutThem.nhan)}">
      <i class="bi bi-plus-lg"></i> ${escHtml(nutThem.nhan)}
    </button>`;
  }

  html += `
          <button id="nut-tai-lai-bang" class="nut nut-xam" title="Tải lại dữ liệu" aria-label="Tải lại">
            <i class="bi bi-arrow-clockwise"></i>
          </button>
        </div>
      </div>
      <div class="the-noi-dung khong-padding" id="${bangId}"></div>
    </div>`;

  container.innerHTML = html;

  const bangContainer = document.getElementById(bangId);
  let duLieuHienTai = [...duLieu];

  const { renderBang } = taoKhungBang(bangContainer, duLieuHienTai, {
    cotHien, tenCot, nutHanhDong, cotTimKiem, khiChonHang, dinhDangCot,
  });

  // Tìm kiếm debounce
  const inputTK = document.getElementById(timKiemId);
  if (inputTK) {
    inputTK.addEventListener('input', debounce(() => {
      const tk = inputTK.value.toLowerCase().trim();
      const cotTK = cotTimKiem || cotHien || Object.keys(duLieuHienTai[0] || {});
      if (tk) {
        const loc = duLieuHienTai.filter(row =>
          cotTK.some(k => String(row[k] ?? '').toLowerCase().includes(tk))
        );
        renderBang(loc);
      } else {
        renderBang(duLieuHienTai);
      }
    }, 250));
  }

  // Nút tải lại
  const nutTaiLai = document.getElementById('nut-tai-lai-bang');
  if (nutTaiLai) {
    nutTaiLai.addEventListener('click', () => {
      if (nutThem && nutThem.onReload) nutThem.onReload();
    });
  }

  // Nút thêm
  if (nutThem && nutThem.onClick) {
    const nutThemEl = document.getElementById('nut-them-chinh');
    if (nutThemEl) {
      nutThemEl.addEventListener('click', nutThem.onClick);
    }
  }

  return {
    capNhatDuLieu(data) {
      duLieuHienTai = [...data];
      renderBang(duLieuHienTai);
      if (inputTK) inputTK.value = '';
    },
  };
}

/** Render trạng thái đang tải */
export function hienDangTai(container, moTa = 'Đang tải dữ liệu...') {
  container.innerHTML = `
    <div class="vung-dang-tai">
      <div class="quay-vong"></div>
      <span style="font-weight:500;">${escHtml(moTa)}</span>
    </div>`;
}

/** Render trạng thái lỗi */
export function hienLoiBang(container, moTa = 'Không thể tải dữ liệu.') {
  container.innerHTML = `
    <div class="trang-thai-bang">
      <div class="bieu-tuong"><i class="bi bi-exclamation-triangle" style="color:var(--mau-nguy-hiem)"></i></div>
      <div class="thong-bao" style="color:var(--mau-nguy-hiem)">Lỗi tải dữ liệu</div>
      <div class="mo-ta">${escHtml(moTa)}</div>
    </div>`;
}
