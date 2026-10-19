/**
 * quan-ly-thanh-toan.js — AngularJS Controller cho Quản lý Giao dịch Thanh toán
 * Role: Admin, ThuNgan, KeToan
 * Base API: /api/thungan/QuanLyBanHang và /api/ketoan/QuanLyCongNo
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('ThanhToanController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, SidebarHeaderService) {
      var vm = this;

      var BASE_TN = ApiService.URL.thungan + '/QuanLyBanHang';
      var BASE_KT = ApiService.URL.ketoan + '/QuanLyCongNo';

      vm.danhSach = [];
      vm.tuKhoa = '';
      vm.dangTai = false;
      vm.loi = '';
      vm.trangHienTai = 1;
      vm.soDongMoiTrang = 20;
      vm.coQuyenSua = false;

      vm.modalForm = {
        hienThi: false,
        laSua: false,
        dangLuu: false,
        data: {
          MaThanhToan: '',
          MaHDBan: '',
          PhuongThuc: 'Tiền mặt',
          TrangThai: 'Đã thanh toán',
          SoTienThanhToan: 0
        }
      };

      vm.modalChiTiet = {
        hienThi: false,
        data: null
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan', 'KeToan'])) return;

        var vaiTro = AuthService.layVaiTro();
        vm.coQuyenSua = (vaiTro === 'KeToan' || vaiTro === 'Admin');

        SidebarHeaderService.khoiTao('quan-ly-thanh-toan', 'Quản lý Thanh toán', 'Trang chủ / Quản lý Thanh toán');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';

        var vaiTro = AuthService.layVaiTro();
        var primaryUrl = (vaiTro === 'KeToan') ? (BASE_KT + '/get-all-thanhtoan') : (BASE_TN + '/get-all-thanhtoan');
        var fallbackUrl = (vaiTro === 'KeToan') ? (BASE_TN + '/get-all-thanhtoan') : (BASE_KT + '/get-all-thanhtoan');

        ApiService.get(primaryUrl)
          .catch(function () {
            return ApiService.get(fallbackUrl);
          })
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var matt = String(ApiService.layThuocTinh(it, 'MATHANHTOAN') || ApiService.layThuocTinh(it, 'MaThanhToan') || '').trim();
              var mahd = String(ApiService.layThuocTinh(it, 'MAHDBAN') || ApiService.layThuocTinh(it, 'MaHDBan') || '').trim();
              var pt = String(ApiService.layThuocTinh(it, 'PHUONGTHUC') || ApiService.layThuocTinh(it, 'PhuongThuc') || '').trim();
              var tien = Number(ApiService.layThuocTinh(it, 'SOTIENTHANHTOAN') || ApiService.layThuocTinh(it, 'SoTienThanhToan')) || 0;
              var ngay = ApiService.layThuocTinh(it, 'NGAYTHANHTOAN') || ApiService.layThuocTinh(it, 'NgayThanhToan') || null;
              var tt = String(ApiService.layThuocTinh(it, 'TRANGTHAI') || ApiService.layThuocTinh(it, 'TrangThai') || '').trim();
              return {
                MATHANHTOAN: matt,
                MaThanhToan: matt,
                mathanhtoan: matt,
                MAHDBAN: mahd,
                MaHDBan: mahd,
                mahdban: mahd,
                PHUONGTHUC: pt,
                PhuongThuc: pt,
                phuongthuc: pt,
                SOTIENTHANHTOAN: tien,
                SoTienThanhToan: tien,
                sotienthanhtoan: tien,
                NGAYTHANHTOAN: ngay,
                NgayThanhToan: ngay,
                ngaythanhtoan: ngay,
                TRANGTHAI: tt,
                TrangThai: tt,
                trangthai: tt
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách giao dịch thanh toán';
            ToastService.error('Lỗi', vm.loi);
          })
          .finally(function () {
            vm.dangTai = false;
          });
      };

      vm.layDanhSachLoc = function () {
        if (!vm.tuKhoa || !vm.tuKhoa.trim()) {
          return vm.danhSach;
        }
        var tk = vm.tuKhoa.trim().toLowerCase();
        return vm.danhSach.filter(function (item) {
          var matt = (item.MaThanhToan || '').toLowerCase();
          var mahd = (item.MaHDBan || '').toLowerCase();
          var pt = (item.PhuongThuc || '').toLowerCase();
          var tt = (item.TrangThai || '').toLowerCase();
          return matt.indexOf(tk) !== -1 || mahd.indexOf(tk) !== -1 || pt.indexOf(tk) !== -1 || tt.indexOf(tk) !== -1;
        });
      };

      vm.tongSoTrang = function () {
        var tong = vm.layDanhSachLoc().length;
        return Math.max(1, Math.ceil(tong / vm.soDongMoiTrang));
      };

      vm.layDanhSachPhanTrang = function () {
        var loc = vm.layDanhSachLoc();
        var tongTrang = vm.tongSoTrang();
        if (vm.trangHienTai > tongTrang) vm.trangHienTai = tongTrang;
        if (vm.trangHienTai < 1) vm.trangHienTai = 1;
        var batDau = (vm.trangHienTai - 1) * vm.soDongMoiTrang;
        return loc.slice(batDau, batDau + vm.soDongMoiTrang);
      };

      vm.layChiSoCuoi = function () {
        var loc = vm.layDanhSachLoc();
        return Math.min(vm.trangHienTai * vm.soDongMoiTrang, loc.length);
      };

      vm.chuyenTrang = function (trang) {
        if (trang >= 1 && trang <= vm.tongSoTrang()) {
          vm.trangHienTai = trang;
        }
      };

      vm.moModalThem = function () {
        vm.modalForm.laSua = false;
        vm.modalForm.data = {
          MaThanhToan: 'TT' + Date.now().toString().slice(-7),
          MaHDBan: '',
          PhuongThuc: 'Tiền mặt',
          TrangThai: 'Đã thanh toán',
          SoTienThanhToan: 0
        };
        vm.modalForm.hienThi = true;
      };

      vm.moModalSua = function (row) {
        if (!vm.coQuyenSua) return;
        vm.modalForm.laSua = true;
        vm.modalForm.data = {
          MaThanhToan: row.MaThanhToan || '',
          MaHDBan: row.MaHDBan || '',
          PhuongThuc: row.PhuongThuc || 'Tiền mặt',
          TrangThai: row.TrangThai || 'Đã thanh toán',
          SoTienThanhToan: Number(row.SoTienThanhToan) || 0
        };
        vm.modalForm.hienThi = true;
      };

      vm.dongModalForm = function () {
        vm.modalForm.hienThi = false;
      };

      vm.xemChiTiet = function (row) {
        vm.modalChiTiet.data = row;
        vm.modalChiTiet.hienThi = true;
      };

      vm.dongModalChiTiet = function () {
        vm.modalChiTiet.hienThi = false;
      };

      vm.luuThanhToan = function () {
        var matt = (vm.modalForm.data.MaThanhToan || '').trim();
        var mahd = (vm.modalForm.data.MaHDBan || '').trim();
        var sotien = parseFloat(vm.modalForm.data.SoTienThanhToan);

        if (!matt || !mahd || isNaN(sotien) || sotien <= 0) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền đầy đủ và chính xác thông tin bắt buộc.');
          return;
        }

        var payload = {
          MaThanhToan: matt,
          MaHDBan: mahd,
          PhuongThuc: vm.modalForm.data.PhuongThuc,
          SoTienThanhToan: sotien,
          TrangThai: vm.modalForm.data.TrangThai
        };

        vm.modalForm.dangLuu = true;
        var req = vm.modalForm.laSua
          ? ApiService.post(BASE_KT + '/update-thanhtoan', payload)
          : ApiService.post(BASE_TN + '/insert-thanhtoan', payload);

        req
          .then(function () {
            ToastService.success('Thành công', (vm.modalForm.laSua ? 'Cập nhật' : 'Ghi nhận') + ' thanh toán thành công.');
            vm.dongModalForm();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể lưu thanh toán');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.khoiTao();
    }
  ]);
})();
