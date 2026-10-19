/**
 * thanh-ben.js — Sidebar navigation cho các trang độc lập
 */
import { layVaiTro, layNguoiDung } from '../xac-thuc.js';
import { layTrangHienTai } from '../dieu-huong.js';
import { escHtml } from '../tien-ich.js';
import { TEN_VAI_TRO } from '../cau-hinh.js';

/** Cấu hình menu theo vai trò */
export const CAU_HINH_MENU = {
  Admin: [
    { nhom: 'Tổng quan', muc: [
      { id: 'tong-quan', nhan: 'Tổng quan', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
    ]},
    { nhom: 'Quản lý người dùng', muc: [
      { id: 'quan-ly-tai-khoan', nhan: 'Tài khoản', icon: 'bi-person-lock', tooltip: 'Tài khoản' },
      { id: 'quan-ly-nhan-vien', nhan: 'Nhân viên', icon: 'bi-people', tooltip: 'Nhân viên' },
    ]},
    { nhom: 'Kho & Hàng hóa', muc: [
      { id: 'quan-ly-danh-muc', nhan: 'Danh mục', icon: 'bi-folder2-open', tooltip: 'Danh mục' },
      { id: 'quan-ly-san-pham', nhan: 'Sản phẩm', icon: 'bi-box-seam', tooltip: 'Sản phẩm' },
      { id: 'quan-ly-ton-kho', nhan: 'Tồn kho', icon: 'bi-archive', tooltip: 'Tồn kho' },
      { id: 'quan-ly-phieu-nhap', nhan: 'Phiếu nhập kho', icon: 'bi-clipboard-plus', tooltip: 'Phiếu nhập' },
      { id: 'quan-ly-nha-cung-cap', nhan: 'Nhà cung cấp', icon: 'bi-truck', tooltip: 'NCC' },
      { id: 'quan-ly-khuyen-mai', nhan: 'Khuyến mãi', icon: 'bi-tag', tooltip: 'Khuyến mãi' },
    ]},
    { nhom: 'Bán hàng', muc: [
      { id: 'quan-ly-khach-hang', nhan: 'Khách hàng', icon: 'bi-person-heart', tooltip: 'Khách hàng' },
      { id: 'ban-hang', nhan: 'Bán hàng', icon: 'bi-cart-plus', tooltip: 'Bán hàng' },
      { id: 'quan-ly-hoa-don', nhan: 'Hóa đơn', icon: 'bi-receipt', tooltip: 'Hóa đơn' },
      { id: 'doi-tra', nhan: 'Đổi trả', icon: 'bi-arrow-left-right', tooltip: 'Đổi trả' },
      { id: 'quan-ly-thanh-toan', nhan: 'Thanh toán', icon: 'bi-credit-card', tooltip: 'Thanh toán' },
    ]},
    { nhom: 'Báo cáo', muc: [
      { id: 'bao-cao-thong-ke', nhan: 'Báo cáo & Thống kê', icon: 'bi-bar-chart-line', tooltip: 'Báo cáo' },
      { id: 'quan-ly-cong-no', nhan: 'Công nợ', icon: 'bi-journal-minus', tooltip: 'Công nợ' },
    ]},
  ],
  ThuNgan: [
    { nhom: 'Tổng quan', muc: [
      { id: 'tong-quan', nhan: 'Tổng quan', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
    ]},
    { nhom: 'Nghiệp vụ', muc: [
      { id: 'ban-hang', nhan: 'Bán hàng', icon: 'bi-cart-plus', tooltip: 'Bán hàng' },
      { id: 'quan-ly-hoa-don', nhan: 'Hóa đơn', icon: 'bi-receipt', tooltip: 'Hóa đơn' },
      { id: 'doi-tra', nhan: 'Đổi trả', icon: 'bi-arrow-left-right', tooltip: 'Đổi trả' },
      { id: 'quan-ly-khach-hang', nhan: 'Khách hàng', icon: 'bi-person-heart', tooltip: 'Khách hàng' },
      { id: 'quan-ly-thanh-toan', nhan: 'Thanh toán', icon: 'bi-credit-card', tooltip: 'Thanh toán' },
    ]},
    { nhom: 'Xem thêm', muc: [
      { id: 'quan-ly-ton-kho', nhan: 'Xem tồn kho', icon: 'bi-archive', tooltip: 'Tồn kho' },
    ]},
  ],
  ThuKho: [
    { nhom: 'Tổng quan', muc: [
      { id: 'tong-quan', nhan: 'Tổng quan', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
    ]},
    { nhom: 'Quản lý kho', muc: [
      { id: 'quan-ly-danh-muc', nhan: 'Danh mục', icon: 'bi-folder2-open', tooltip: 'Danh mục' },
      { id: 'quan-ly-san-pham', nhan: 'Sản phẩm', icon: 'bi-box-seam', tooltip: 'Sản phẩm' },
      { id: 'quan-ly-ton-kho', nhan: 'Tồn kho', icon: 'bi-archive', tooltip: 'Tồn kho' },
      { id: 'quan-ly-phieu-nhap', nhan: 'Phiếu nhập kho', icon: 'bi-clipboard-plus', tooltip: 'Phiếu nhập' },
      { id: 'quan-ly-nha-cung-cap', nhan: 'Nhà cung cấp', icon: 'bi-truck', tooltip: 'NCC' },
    ]},
  ],
  KeToan: [
    { nhom: 'Tổng quan', muc: [
      { id: 'tong-quan', nhan: 'Tổng quan', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
    ]},
    { nhom: 'Kế toán', muc: [
      { id: 'bao-cao-thong-ke', nhan: 'Báo cáo & Thống kê', icon: 'bi-bar-chart-line', tooltip: 'Báo cáo' },
      { id: 'quan-ly-cong-no', nhan: 'Công nợ', icon: 'bi-journal-minus', tooltip: 'Công nợ' },
      { id: 'quan-ly-thanh-toan', nhan: 'Thanh toán', icon: 'bi-credit-card', tooltip: 'Thanh toán' },
      { id: 'quan-ly-hoa-don', nhan: 'Hóa đơn', icon: 'bi-receipt', tooltip: 'Hóa đơn' },
      { id: 'quan-ly-phieu-nhap', nhan: 'Phiếu nhập kho', icon: 'bi-clipboard-plus', tooltip: 'Phiếu nhập' },
    ]},
  ],
};

/** Khởi tạo sidebar */
export function khoiSidebar(trangId = null) {
  const sidebar = document.getElementById('sidebar');
  if (!sidebar) return;

  const vaiTro = layVaiTro();
  const nguoiDung = layNguoiDung();
  const currentTrang = trangId || layTrangHienTai();

  if (!vaiTro || !nguoiDung) return;

  // Render menu theo vai trò
  const noiDungEl = sidebar.querySelector('.sidebar-noi-dung');
  if (noiDungEl) {
    const cauHinh = CAU_HINH_MENU[vaiTro] || [];
    noiDungEl.innerHTML = cauHinh.map(nhom => `
      <div class="sidebar-nhom">
        <div class="sidebar-nhom-ten">${escHtml(nhom.nhom)}</div>
        ${nhom.muc.map(muc => {
          const isActive = muc.id === currentTrang ? ' active' : '';
          return `
            <a class="menu-muc${isActive}" data-trang="${muc.id}" data-tooltip="${muc.tooltip}" href="${muc.id}.html" aria-label="${escHtml(muc.nhan)}">
              <span class="menu-muc-icon"><i class="bi ${muc.icon}"></i></span>
              <span class="menu-muc-nhan">${escHtml(muc.nhan)}</span>
            </a>
          `;
        }).join('')}
      </div>
    `).join('');

    // Khôi phục vị trí cuộn của sidebar để không bị nhảy về đầu khi click chuyển trang
    const viTriCuonDaLuu = sessionStorage.getItem('sidebar_scroll_top');
    if (viTriCuonDaLuu !== null) {
      noiDungEl.scrollTop = parseInt(viTriCuonDaLuu, 10);
    } else {
      const activeItem = noiDungEl.querySelector('.menu-muc.active');
      if (activeItem) {
        activeItem.scrollIntoView({ block: 'nearest' });
      }
    }

    // Lắng nghe sự kiện cuộn để lưu vị trí thời gian thực
    noiDungEl.addEventListener('scroll', () => {
      sessionStorage.setItem('sidebar_scroll_top', noiDungEl.scrollTop);
    }, { passive: true });

    // Khi người dùng click vào bất kỳ mục menu nào, ghi nhận vị trí cuộn ngay lập tức
    noiDungEl.querySelectorAll('.menu-muc').forEach(link => {
      link.addEventListener('click', () => {
        sessionStorage.setItem('sidebar_scroll_top', noiDungEl.scrollTop);
      });
    });
  }

  // Render user footer
  const footerEl = sidebar.querySelector('.sidebar-footer');
  if (footerEl) {
    const tenHienThi = nguoiDung.userName || nguoiDung.username || 'Người dùng';
    const tenVaiTro = TEN_VAI_TRO[vaiTro] || vaiTro;
    const kyTuDau = tenHienThi.charAt(0).toUpperCase();

    footerEl.innerHTML = `
      <div class="sidebar-user-info">
        <div class="sidebar-user-avatar">${kyTuDau}</div>
        <div class="sidebar-user-ten">
          <div class="ten">${escHtml(tenHienThi)}</div>
          <div class="vai-tro">${escHtml(tenVaiTro)}</div>
        </div>
      </div>
    `;
  }

  // Nút thu gọn sidebar
  const nutThuGon = document.getElementById('nut-thu-gon-sidebar');
  if (nutThuGon && !nutThuGon._hasListener) {
    nutThuGon._hasListener = true;
    nutThuGon.addEventListener('click', () => {
      sidebar.classList.toggle('thu-gon');
      const vungChinh = document.getElementById('vung-chinh');
      if (vungChinh) vungChinh.classList.toggle('mo-rong');
    });
  }

  // Mobile toggle
  const nutMenu = document.getElementById('nut-mo-menu');
  if (nutMenu && !nutMenu._hasListener) {
    nutMenu._hasListener = true;
    nutMenu.addEventListener('click', () => {
      if (window.innerWidth <= 768) {
        sidebar.classList.toggle('hien');
        document.getElementById('overlay-sidebar')?.classList.toggle('hien');
      } else {
        sidebar.classList.toggle('thu-gon');
        document.getElementById('vung-chinh')?.classList.toggle('mo-rong');
      }
    });
  }

  const overlay = document.getElementById('overlay-sidebar');
  if (overlay && !overlay._hasListener) {
    overlay._hasListener = true;
    overlay.addEventListener('click', dongSidebarMobile);
  }
}

export function dongSidebarMobile() {
  document.getElementById('sidebar')?.classList.remove('hien');
  document.getElementById('overlay-sidebar')?.classList.remove('hien');
}

/** Đánh dấu mục menu active */
export function capNhatMenuActive(trangId) {
  document.querySelectorAll('.menu-muc').forEach(el => {
    el.classList.toggle('active', el.dataset.trang === trangId);
  });
}
