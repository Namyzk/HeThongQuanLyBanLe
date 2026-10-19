
(function () {
  'use strict';

  var app = angular.module('qlblApp', []);

  // Cấu hình $http gửi cookie HttpOnly qua API Gateway
  app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.defaults.withCredentials = true;
  }]);

  /* ==========================================================================
     1. HẰNG SỐ CẤU HÌNH (APP_CONFIG)
     ========================================================================== */
  app.constant('APP_CONFIG', {
    BASE_URL: 'https://localhost:7144',
    API_URL: {
      login:   'https://localhost:7144/api/Login',
      admin:   'https://localhost:7144/api/admin',
      thukho:  'https://localhost:7144/api/thukho',
      thungan: 'https://localhost:7144/api/thungan',
      ketoan:  'https://localhost:7144/api/ketoan',
    },
    // Khóa lưu phiên tương thích hoàn toàn với xac-thuc.js và cau-hinh.js
    KHOA_PHIEN: {
      nguoi_dung: 'current_user',
      thoi_gian: 'token_received_at',
      refresh_expires_at: 'refresh_token_expires_at',
      // Khóa dự phòng
      nguoi_dung_phu: 'nguoi_dung_hien_tai',
      thoi_gian_phu: 'thoi_gian_phien',
      refresh_expires_phu: 'thoi_gian_refresh_token_het_han'
    },
    THOI_GIAN_TOKEN: 900, // 15 phút (tính bằng giây)
    CANH_BAO_TRUOC: 120, // Cảnh báo trước 2 phút
    QUYEN_ROLE: {
      1: 'Admin',
      2: 'ThuNgan',
      3: 'ThuKho',
      4: 'KeToan',
    },
    TEN_VAI_TRO: {
      Admin:   'Quản trị viên',
      ThuNgan: 'Thu ngân',
      ThuKho:  'Thủ kho',
      KeToan:  'Kế toán',
    },
    CAU_HINH_MENU: {
      Admin: [
        { nhom: 'Tổng quan', muc: [
          { id: 'tong-quan', nhan: 'Tổng quan', file: 'tong-quan.html', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
        ]},
        { nhom: 'Quản lý người dùng', muc: [
          { id: 'quan-ly-tai-khoan', nhan: 'Tài khoản', file: 'quan-ly-tai-khoan.html', icon: 'bi-person-lock', tooltip: 'Tài khoản' },
          { id: 'quan-ly-nhan-vien', nhan: 'Nhân viên', file: 'quan-ly-nhan-vien.html', icon: 'bi-people', tooltip: 'Nhân viên' },
        ]},
        { nhom: 'Kho & Hàng hóa', muc: [
          { id: 'quan-ly-danh-muc', nhan: 'Danh mục', file: 'quan-ly-danh-muc.html', icon: 'bi-folder2-open', tooltip: 'Danh mục' },
          { id: 'quan-ly-san-pham', nhan: 'Sản phẩm', file: 'quan-ly-san-pham.html', icon: 'bi-box-seam', tooltip: 'Sản phẩm' },
          { id: 'quan-ly-ton-kho', nhan: 'Tồn kho', file: 'quan-ly-ton-kho.html', icon: 'bi-archive', tooltip: 'Tồn kho' },
          { id: 'quan-ly-phieu-nhap', nhan: 'Phiếu nhập kho', file: 'quan-ly-phieu-nhap.html', icon: 'bi-clipboard-plus', tooltip: 'Phiếu nhập' },
          { id: 'quan-ly-nha-cung-cap', nhan: 'Nhà cung cấp', file: 'quan-ly-nha-cung-cap.html', icon: 'bi-truck', tooltip: 'NCC' },
          { id: 'quan-ly-khuyen-mai', nhan: 'Khuyến mãi', file: 'quan-ly-khuyen-mai.html', icon: 'bi-tag', tooltip: 'Khuyến mãi' },
        ]},
        { nhom: 'Bán hàng', muc: [
          { id: 'quan-ly-khach-hang', nhan: 'Khách hàng', file: 'quan-ly-khach-hang.html', icon: 'bi-person-heart', tooltip: 'Khách hàng' },
          { id: 'ban-hang', nhan: 'Bán hàng (POS)', file: 'ban-hang.html', icon: 'bi-cart-plus', tooltip: 'Bán hàng' },
          { id: 'quan-ly-hoa-don', nhan: 'Hóa đơn', file: 'quan-ly-hoa-don.html', icon: 'bi-receipt', tooltip: 'Hóa đơn' },
          { id: 'doi-tra', nhan: 'Đổi trả', file: 'doi-tra.html', icon: 'bi-arrow-left-right', tooltip: 'Đổi trả' },
          { id: 'quan-ly-thanh-toan', nhan: 'Thanh toán', file: 'quan-ly-thanh-toan.html', icon: 'bi-credit-card', tooltip: 'Thanh toán' },
        ]},
        { nhom: 'Báo cáo', muc: [
          { id: 'bao-cao-thong-ke', nhan: 'Báo cáo & Thống kê', file: 'bao-cao-thong-ke.html', icon: 'bi-bar-chart-line', tooltip: 'Báo cáo' },
          { id: 'quan-ly-cong-no', nhan: 'Công nợ', file: 'quan-ly-cong-no.html', icon: 'bi-journal-minus', tooltip: 'Công nợ' },
        ]},
      ],
      ThuNgan: [
        { nhom: 'Tổng quan', muc: [
          { id: 'tong-quan', nhan: 'Tổng quan', file: 'tong-quan.html', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
        ]},
        { nhom: 'Nghiệp vụ thu ngân', muc: [
          { id: 'ban-hang', nhan: 'Bán hàng (POS)', file: 'ban-hang.html', icon: 'bi-cart-plus', tooltip: 'Bán hàng' },
          { id: 'quan-ly-hoa-don', nhan: 'Hóa đơn', file: 'quan-ly-hoa-don.html', icon: 'bi-receipt', tooltip: 'Hóa đơn' },
          { id: 'doi-tra', nhan: 'Đổi trả', file: 'doi-tra.html', icon: 'bi-arrow-left-right', tooltip: 'Đổi trả' },
          { id: 'quan-ly-khach-hang', nhan: 'Khách hàng', file: 'quan-ly-khach-hang.html', icon: 'bi-person-heart', tooltip: 'Khách hàng' },
          { id: 'quan-ly-thanh-toan', nhan: 'Thanh toán', file: 'quan-ly-thanh-toan.html', icon: 'bi-credit-card', tooltip: 'Thanh toán' },
        ]},
        { nhom: 'Tra cứu', muc: [
          { id: 'quan-ly-ton-kho', nhan: 'Xem tồn kho', file: 'quan-ly-ton-kho.html', icon: 'bi-archive', tooltip: 'Tồn kho' },
        ]},
      ],
      ThuKho: [
        { nhom: 'Tổng quan', muc: [
          { id: 'tong-quan', nhan: 'Tổng quan', file: 'tong-quan.html', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
        ]},
        { nhom: 'Quản lý kho hàng', muc: [
          { id: 'quan-ly-danh-muc', nhan: 'Danh mục', file: 'quan-ly-danh-muc.html', icon: 'bi-folder2-open', tooltip: 'Danh mục' },
          { id: 'quan-ly-san-pham', nhan: 'Sản phẩm', file: 'quan-ly-san-pham.html', icon: 'bi-box-seam', tooltip: 'Sản phẩm' },
          { id: 'quan-ly-ton-kho', nhan: 'Tồn kho', file: 'quan-ly-ton-kho.html', icon: 'bi-archive', tooltip: 'Tồn kho' },
          { id: 'quan-ly-phieu-nhap', nhan: 'Phiếu nhập kho', file: 'quan-ly-phieu-nhap.html', icon: 'bi-clipboard-plus', tooltip: 'Phiếu nhập' },
          { id: 'quan-ly-nha-cung-cap', nhan: 'Nhà cung cấp', file: 'quan-ly-nha-cung-cap.html', icon: 'bi-truck', tooltip: 'NCC' },
        ]},
      ],
      KeToan: [
        { nhom: 'Tổng quan', muc: [
          { id: 'tong-quan', nhan: 'Tổng quan', file: 'tong-quan.html', icon: 'bi-speedometer2', tooltip: 'Tổng quan' },
        ]},
        { nhom: 'Nghiệp vụ kế toán', muc: [
          { id: 'bao-cao-thong-ke', nhan: 'Báo cáo & Thống kê', file: 'bao-cao-thong-ke.html', icon: 'bi-bar-chart-line', tooltip: 'Báo cáo' },
          { id: 'quan-ly-cong-no', nhan: 'Công nợ', file: 'quan-ly-cong-no.html', icon: 'bi-journal-minus', tooltip: 'Công nợ' },
          { id: 'quan-ly-thanh-toan', nhan: 'Thanh toán', file: 'quan-ly-thanh-toan.html', icon: 'bi-credit-card', tooltip: 'Thanh toán' },
          { id: 'quan-ly-hoa-don', nhan: 'Hóa đơn', file: 'quan-ly-hoa-don.html', icon: 'bi-receipt', tooltip: 'Hóa đơn' },
          { id: 'quan-ly-phieu-nhap', nhan: 'Phiếu nhập kho', file: 'quan-ly-phieu-nhap.html', icon: 'bi-clipboard-plus', tooltip: 'Phiếu nhập' },
        ]},
      ],
    }
  });

  /* ==========================================================================
     2. DỊCH VỤ XÁC THỰC VÀ PHÂN QUYỀN (AuthService)
     ========================================================================== */
  app.factory('AuthService', ['APP_CONFIG', '$window', function (APP_CONFIG, $window) {
    var KHOA = APP_CONFIG.KHOA_PHIEN;

    function layNguoiDung() {
      try {
        var raw = $window.sessionStorage.getItem(KHOA.nguoi_dung) ||
                  $window.sessionStorage.getItem(KHOA.nguoi_dung_phu);
        return raw ? JSON.parse(raw) : null;
      } catch (e) {
        return null;
      }
    }

    function layVaiTro() {
      var nd = layNguoiDung();
      if (!nd) return null;
      if (nd.roleName) return nd.roleName;
      if (nd.quyen) return APP_CONFIG.QUYEN_ROLE[nd.quyen] || null;
      return null;
    }

    function conRefreshToken() {
      var raw = $window.sessionStorage.getItem(KHOA.refresh_expires_at) ||
                $window.sessionStorage.getItem(KHOA.refresh_expires_phu);
      if (!raw) return true; // Tin tưởng cookie HttpOnly
      var exp = Number(raw);
      return Number.isFinite(exp) && exp > Date.now();
    }

    function daDangNhap() {
      var nd = layNguoiDung();
      var tg = $window.sessionStorage.getItem(KHOA.thoi_gian) ||
               $window.sessionStorage.getItem(KHOA.thoi_gian_phu);
      if (!nd || !tg) return false;
      var giayDaQua = (Date.now() - parseInt(tg, 10)) / 1000;
      return giayDaQua < APP_CONFIG.THOI_GIAN_TOKEN || conRefreshToken();
    }

    function luuPhien(nguoiDung, refreshExpiresAt) {
      var now = Date.now();
      if (nguoiDung) {
        var json = JSON.stringify(nguoiDung);
        $window.sessionStorage.setItem(KHOA.nguoi_dung, json);
        $window.sessionStorage.setItem(KHOA.nguoi_dung_phu, json);
      }
      $window.sessionStorage.setItem(KHOA.thoi_gian, now.toString());
      $window.sessionStorage.setItem(KHOA.thoi_gian_phu, now.toString());

      var exp = refreshExpiresAt ? (typeof refreshExpiresAt === 'number' ? refreshExpiresAt : new Date(refreshExpiresAt).getTime()) : (now + 7 * 86400000);
      if (!isNaN(exp)) {
        $window.sessionStorage.setItem(KHOA.refresh_expires_at, exp.toString());
        $window.sessionStorage.setItem(KHOA.refresh_expires_phu, exp.toString());
      }
    }

    function dangXuat() {
      $window.sessionStorage.removeItem(KHOA.nguoi_dung);
      $window.sessionStorage.removeItem(KHOA.nguoi_dung_phu);
      $window.sessionStorage.removeItem(KHOA.thoi_gian);
      $window.sessionStorage.removeItem(KHOA.thoi_gian_phu);
      $window.sessionStorage.removeItem(KHOA.refresh_expires_at);
      $window.sessionStorage.removeItem(KHOA.refresh_expires_phu);
      $window.sessionStorage.removeItem('access_token');
      $window.sessionStorage.removeItem('refresh_token');

      var dangOTrang = $window.location.pathname.includes('/trang/');
      $window.location.href = dangOTrang ? 'dang-nhap.html' : 'trang/dang-nhap.html';
    }

    function kiemTraQuyen(vaiTroChoPhep) {
      if (!daDangNhap()) {
        dangXuat();
        return false;
      }
      if (vaiTroChoPhep && vaiTroChoPhep.length > 0) {
        var role = layVaiTro();
        if (vaiTroChoPhep.indexOf(role) === -1) {
          alert('Tài khoản của bạn (' + (role || 'Không xác định') + ') không có quyền truy cập trang này.');
          var dangOTrang = $window.location.pathname.includes('/trang/');
          $window.location.href = dangOTrang ? 'tong-quan.html' : 'trang/tong-quan.html';
          return false;
        }
      }
      return true;
    }

    return {
      layNguoiDung: layNguoiDung,
      layVaiTro: layVaiTro,
      daDangNhap: daDangNhap,
      luuPhien: luuPhien,
      dangXuat: dangXuat,
      kiemTraQuyen: kiemTraQuyen,
    };
  }]);

  /* ==========================================================================
     3. DỊCH VỤ THÔNG BÁO TOAST (ToastService)
     ========================================================================== */
  app.factory('ToastService', ['$timeout', function ($timeout) {
    function hienToast(tieuDe, noiDung, loai, thoiGianMs) {
      loai = loai || 'thanh-cong';
      thoiGianMs = thoiGianMs || 3000;

      var vung = document.getElementById('vung-toast');
      if (!vung) return;

      var toast = document.createElement('div');
      toast.className = 'toast ' + loai + ' fade-in';
      toast.setAttribute('role', 'alert');

      var iconCls = 'bi-check-circle-fill';
      if (loai === 'loi' || loai === 'nguy-hiem') iconCls = 'bi-x-circle-fill';
      else if (loai === 'canh-bao') iconCls = 'bi-exclamation-triangle-fill';
      else if (loai === 'thong-tin') iconCls = 'bi-info-circle-fill';

      toast.innerHTML =
        '<div class="toast-bieu-tuong"><i class="bi ' + iconCls + '"></i></div>' +
        '<div class="toast-noi-dung">' +
          '<div class="toast-tieu-de">' + (tieuDe || '') + '</div>' +
          '<div class="toast-thong-diep">' + (noiDung || '') + '</div>' +
        '</div>' +
        '<button class="toast-nut-dong" type="button" aria-label="Đóng"><i class="bi bi-x"></i></button>';

      var dongToast = function () {
        toast.classList.add('fade-out');
        $timeout(function () {
          if (toast.parentNode) toast.parentNode.removeChild(toast);
        }, 300);
      };

      toast.querySelector('.toast-nut-dong').addEventListener('click', dongToast);
      vung.appendChild(toast);

      $timeout(dongToast, thoiGianMs);
    }

    return {
      success: function (tieuDe, noiDung, ms) { hienToast(tieuDe, noiDung, 'thanh-cong', ms); },
      error: function (tieuDe, noiDung, ms) { hienToast(tieuDe, noiDung, 'loi', ms); },
      warning: function (tieuDe, noiDung, ms) { hienToast(tieuDe, noiDung, 'canh-bao', ms); },
      info: function (tieuDe, noiDung, ms) { hienToast(tieuDe, noiDung, 'thong-tin', ms); },
    };
  }]);

  /* ==========================================================================
     4. DỊCH VỤ GỌI API (ApiService)
     ========================================================================== */
  app.factory('ApiService', ['$http', '$q', 'APP_CONFIG', 'ToastService', 'AuthService', function ($http, $q, APP_CONFIG, ToastService, AuthService) {
    var _refreshing = null;

    function lamMoiToken() {
      if (!_refreshing) {
        _refreshing = $http({
          method: 'POST',
          url: APP_CONFIG.API_URL.login + '/refresh',
          withCredentials: true
        }).then(function (res) {
          if (res.data && res.data.success) {
            AuthService.luuPhien(res.data.data, res.data.refreshTokenExpiresAtUtc);
            return true;
          }
          return false;
        }).catch(function () {
          return false;
        }).finally(function () {
          _refreshing = null;
        });
      }
      return _refreshing;
    }

    function request(method, url, data, params, isRetry) {
      var reqConfig = {
        method: method,
        url: url,
        data: data,
        params: params,
        withCredentials: true,
        headers: {
          'Content-Type': 'application/json'
        }
      };

      return $http(reqConfig).then(function (res) {
        return res.data;
      }).catch(function (err) {
        if (err.status === 401 && !isRetry) {
          return lamMoiToken().then(function (ok) {
            if (ok) {
              return request(method, url, data, params, true);
            } else {
              ToastService.warning('Phiên đăng nhập hết hạn', 'Vui lòng đăng nhập lại.');
              AuthService.dangXuat();
              return $q.reject('Phiên đăng nhập hết hạn');
            }
          });
        }
        var msg = (err.data && (err.data.message || err.data.title)) || err.statusText || 'Lỗi kết nối máy chủ';
        return $q.reject(msg);
      });
    }

    /** Chuẩn hóa an toàn dữ liệu mảng từ API Gateway */
    function layMang(res) {
      if (!res) return [];
      if (Array.isArray(res)) return res;
      if (res.data && Array.isArray(res.data)) return res.data;
      if (res.Data && Array.isArray(res.Data)) return res.Data;
      if (res.items && Array.isArray(res.items)) return res.items;
      if (res.Items && Array.isArray(res.Items)) return res.Items;
      if (res.result && Array.isArray(res.result)) return res.result;
      if (res.Result && Array.isArray(res.Result)) return res.Result;
      if (res.data && res.data.items && Array.isArray(res.data.items)) return res.data.items;
      if (res.data && res.data.Items && Array.isArray(res.data.Items)) return res.data.Items;
      if (res.data && res.data.data && Array.isArray(res.data.data)) return res.data.data;
      return [];
    }

    /** Đọc giá trị thuộc tính không phân biệt chữ hoa/thường */
    function layThuocTinh(obj, prop) {
      if (!obj) return undefined;
      if (obj[prop] !== undefined) return obj[prop];
      var lower = String(prop).toLowerCase();
      for (var k in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, k) && k.toLowerCase() === lower) {
          return obj[k];
        }
      }
      return undefined;
    }

    /** Chuẩn hóa đối tượng: đồng bộ key gốc, UPPERCASE, lowercase, camelCase, PascalCase */
    function chuanHoaDoiTuong(obj) {
      if (!obj || typeof obj !== 'object' || Array.isArray(obj)) return obj;
      var out = {};
      for (var k in obj) {
        if (Object.prototype.hasOwnProperty.call(obj, k)) {
          var val = obj[k];
          out[k] = val;
          out[k.toUpperCase()] = val;
          out[k.toLowerCase()] = val;
          if (k.length > 0) {
            out[k.charAt(0).toLowerCase() + k.slice(1)] = val;
            out[k.charAt(0).toUpperCase() + k.slice(1)] = val;
          }
        }
      }
      return out;
    }

    return {
      get: function (url, params) { return request('GET', url, null, params); },
      post: function (url, data, params) { return request('POST', url, data, params); },
      put: function (url, data, params) { return request('PUT', url, data, params); },
      patch: function (url, data, params) { return request('PATCH', url, data, params); },
      delete: function (url, params) { return request('DELETE', url, null, params); },
      layMang: layMang,
      layThuocTinh: layThuocTinh,
      chuanHoaDoiTuong: chuanHoaDoiTuong,
      URL: APP_CONFIG.API_URL
    };
  }]);

  /* ==========================================================================
     5. DỊCH VỤ HỘP THOẠI XÁC NHẬN VÀ MODAL (ModalService)
     ========================================================================== */
  app.factory('ModalService', ['$q', function ($q) {
    function hienXacNhan(tieuDe, noiDung, nhanNutXacNhan) {
      var defer = $q.defer();
      var modal = document.getElementById('modal-chung');
      if (!modal) {
        defer.resolve(confirm(noiDung));
        return defer.promise;
      }

      var tieuDeEl = modal.querySelector('.modal-tieu-de');
      var noiDungEl = modal.querySelector('.modal-noi-dung');
      var chanEl = modal.querySelector('.modal-chan');
      var nutDongEl = modal.querySelector('.modal-nut-dong');

      tieuDeEl.textContent = tieuDe || 'Xác nhận';
      noiDungEl.innerHTML = '<p>' + noiDung + '</p>';
      chanEl.innerHTML =
        '<button type="button" class="nut nut-vien nut-modal-huy">Hủy</button>' +
        '<button type="button" class="nut nut-nguy-hiem nut-modal-dong-y">' + (nhanNutXacNhan || 'Xóa / Xác nhận') + '</button>';

      modal.classList.add('hien');
      modal.setAttribute('aria-hidden', 'false');

      function dong() {
        modal.classList.remove('hien');
        modal.setAttribute('aria-hidden', 'true');
      }

      modal.querySelector('.nut-modal-huy').onclick = function () {
        dong();
        defer.reject(false);
      };

      nutDongEl.onclick = function () {
        dong();
        defer.reject(false);
      };

      modal.querySelector('.nut-modal-dong-y').onclick = function () {
        dong();
        defer.resolve(true);
      };

      return defer.promise;
    }

    return {
      xacNhan: hienXacNhan
    };
  }]);

  /* ==========================================================================
     6. DỊCH VỤ BẢNG ĐIỀU HƯỚNG VÀ HEADER DÙNG CHUNG (SidebarHeaderService)
     Khởi tạo và quản lý sidebar + header theo đúng 100% markup, CSS và behavior gốc.
     ========================================================================== */
  app.factory('SidebarHeaderService', ['APP_CONFIG', 'AuthService', '$window', '$interval', function (APP_CONFIG, AuthService, $window, $interval) {
    function khoiTao(trangId, tieuDeTrang, duongDanTrang) {
      var vaiTro = AuthService.layVaiTro() || 'Admin';
      var nguoiDung = AuthService.layNguoiDung() || {};
      var tenVaiTro = APP_CONFIG.TEN_VAI_TRO[vaiTro] || vaiTro;
      var avatarChuCai = (nguoiDung.userName || 'U').charAt(0).toUpperCase();

      // 1. Khởi tạo Sidebar
      var sidebar = document.getElementById('sidebar');
      var vungChinh = document.getElementById('vung-chinh');
      var overlay = document.getElementById('overlay-sidebar');
      var nutThuGon = document.getElementById('nut-thu-gon-sidebar');

      if (sidebar) {
        // Render menu
        var noiDungEl = sidebar.querySelector('.sidebar-noi-dung');
        if (noiDungEl) {
          var menuData = APP_CONFIG.CAU_HINH_MENU[vaiTro] || APP_CONFIG.CAU_HINH_MENU.Admin;
          var htmlMenu = '';
          menuData.forEach(function (nhom) {
            htmlMenu += '<div class="sidebar-nhom"><div class="sidebar-nhom-ten">' + nhom.nhom + '</div>';
            nhom.muc.forEach(function (muc) {
              var isActive = muc.id === trangId ? ' active' : '';
              htmlMenu += '<a href="' + muc.file + '" class="menu-muc' + isActive + '" data-tooltip="' + muc.nhan + '">' +
                            '<span class="menu-muc-icon"><i class="bi ' + muc.icon + '"></i></span>' +
                            '<span class="menu-muc-nhan">' + muc.nhan + '</span>' +
                          '</a>';
            });
            htmlMenu += '</div>';
          });
          noiDungEl.innerHTML = htmlMenu;
        }

        // Render user footer
        var footerEl = sidebar.querySelector('.sidebar-footer');
        if (footerEl) {
          footerEl.innerHTML =
            '<div class="sidebar-user-info">' +
              '<div class="sidebar-user-avatar">' + avatarChuCai + '</div>' +
              '<div class="sidebar-user-ten">' +
                '<div class="ten">' + (nguoiDung.userName || 'Người dùng') + '</div>' +
                '<div class="vai-tro">' + tenVaiTro + '</div>' +
              '</div>' +
            '</div>';
        }

        // Khôi phục trạng thái thu gọn
        var daThuGon = $window.sessionStorage.getItem('sidebar_thu_gon') === '1';
        if (daThuGon) {
          sidebar.classList.add('thu-gon');
          if (vungChinh) vungChinh.classList.add('mo-rong');
        }

        if (nutThuGon) {
          nutThuGon.onclick = function () {
            sidebar.classList.toggle('thu-gon');
            if (vungChinh) vungChinh.classList.toggle('mo-rong');
            $window.sessionStorage.setItem('sidebar_thu_gon', sidebar.classList.contains('thu-gon') ? '1' : '0');
          };
        }
      }

      // 2. Khởi tạo Header
      var header = document.getElementById('header');
      if (header) {
        var tieuDeEl = document.getElementById('header-tieu-de-trang');
        if (tieuDeEl && tieuDeTrang) tieuDeEl.textContent = tieuDeTrang;

        var duongDanEl = document.getElementById('header-duong-dan');
        if (duongDanEl && duongDanTrang) duongDanEl.textContent = duongDanTrang;

        var userAvatarEl = document.getElementById('header-user-avatar');
        if (userAvatarEl) userAvatarEl.textContent = avatarChuCai;

        var userTenEl = document.getElementById('header-user-ten');
        if (userTenEl) userTenEl.textContent = nguoiDung.userName || 'Người dùng';

        var userVaiTroEl = document.getElementById('header-user-vai-tro');
        if (userVaiTroEl) userVaiTroEl.textContent = tenVaiTro;

        var nutDangXuat = document.getElementById('nut-dang-xuat');
        if (nutDangXuat) {
          nutDangXuat.onclick = function () {
            AuthService.dangXuat();
          };
        }

        var nutMoMenu = document.getElementById('nut-mo-menu');
        if (nutMoMenu && sidebar) {
          nutMoMenu.onclick = function () {
            sidebar.classList.toggle('mo-mobile');
            if (overlay) overlay.classList.toggle('hien');
          };
        }

        if (overlay && sidebar) {
          overlay.onclick = function () {
            sidebar.classList.remove('mo-mobile');
            overlay.classList.remove('hien');
          };
        }
      }

      // 3. Đếm ngược cảnh báo token phiên làm việc
      var canhBaoEl = document.getElementById('canh-bao-token');
      if (canhBaoEl) {
        var capNhatCanhBao = function () {
          var tg = $window.sessionStorage.getItem(APP_CONFIG.KHOA_PHIEN.thoi_gian) ||
                   $window.sessionStorage.getItem(APP_CONFIG.KHOA_PHIEN.thoi_gian_phu);
          if (!tg) return;
          var giayDaQua = (Date.now() - parseInt(tg, 10)) / 1000;
          var conLai = Math.max(0, Math.floor(APP_CONFIG.THOI_GIAN_TOKEN - giayDaQua));

          if (conLai <= APP_CONFIG.CANH_BAO_TRUOC && conLai > 0) {
            canhBaoEl.style.display = 'inline-flex';
            var phut = Math.floor(conLai / 60);
            var giay = conLai % 60;
            canhBaoEl.innerHTML = '<i class="bi bi-clock-history"></i> Phiên còn: ' + phut + ':' + (giay < 10 ? '0' : '') + giay;
          } else {
            canhBaoEl.style.display = 'none';
          }
        };

        capNhatCanhBao();
        var timer = $interval(capNhatCanhBao, 1000);
        $window.addEventListener('beforeunload', function () {
          $interval.cancel(timer);
        });
      }
    }

    return {
      khoiTao: khoiTao
    };
  }]);

  /* ==========================================================================
     7. CÁC BỘ LỌC DỮ LIỆU CHUẨN (FILTERS)
     ========================================================================== */
  app.filter('vnd', function () {
    return function (val) {
      if (val === null || val === undefined || val === '') return '0 đ';
      var num = Number(val);
      if (isNaN(num)) return '0 đ';
      return num.toLocaleString('vi-VN') + ' đ';
    };
  });

  app.filter('so', function () {
    return function (val) {
      if (val === null || val === undefined || val === '') return '0';
      var num = Number(val);
      if (isNaN(num)) return '0';
      return num.toLocaleString('vi-VN');
    };
  });

  app.filter('ngayVn', function () {
    return function (val) {
      if (!val) return '—';
      try {
        var d = new Date(val);
        if (isNaN(d.getTime())) return String(val).substring(0, 10);
        var day = ('0' + d.getDate()).slice(-2);
        var month = ('0' + (d.getMonth() + 1)).slice(-2);
        var year = d.getFullYear();
        return day + '/' + month + '/' + year;
      } catch (e) {
        return val;
      }
    };
  });

})();
