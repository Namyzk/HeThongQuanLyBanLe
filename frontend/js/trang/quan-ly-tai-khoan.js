/**
 * quan-ly-tai-khoan.js — AngularJS Controller cho Quản lý Tài khoản người dùng
 * Role: Admin
 * Base API: /api/admin/QuanLyTaiKhoan
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('TaiKhoanController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, ModalService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.admin + '/QuanLyTaiKhoan';

      vm.danhSach = [];
      vm.tuKhoa = '';
      vm.dangTai = false;
      vm.loi = '';
      vm.trangHienTai = 1;
      vm.soDongMoiTrang = 20;

      vm.modalForm = {
        hienThi: false,
        laSua: false,
        dangLuu: false,
        data: {
          MATAIKHOAN: '',
          USERNAME: '',
          PASS: '',
          QUYEN: 1
        }
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin'])) return;
        SidebarHeaderService.khoiTao('quan-ly-tai-khoan', 'Quản lý Tài khoản', 'Trang chủ / Quản lý Tài khoản');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE + '/get-all-taikhoan')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MaTaiKhoan') || ApiService.layThuocTinh(it, 'MATAIKHOAN') || '').trim();
              var user = String(ApiService.layThuocTinh(it, 'UserName') || ApiService.layThuocTinh(it, 'USERNAME') || '').trim();
              var q = Number(ApiService.layThuocTinh(it, 'Quyen') || ApiService.layThuocTinh(it, 'QUYEN')) || 1;
              var role = ApiService.layThuocTinh(it, 'RoleName') || vm.layTenVaiTro(q);
              return {
                MaTaiKhoan: ma,
                MATAIKHOAN: ma,
                maTaiKhoan: ma,
                UserName: user,
                USERNAME: user,
                userName: user,
                Quyen: q,
                QUYEN: q,
                quyen: q,
                RoleName: role,
                roleName: role
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách tài khoản';
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
          var ma = (item.MaTaiKhoan || item.MATAIKHOAN || '').toLowerCase();
          var user = (item.UserName || item.USERNAME || '').toLowerCase();
          var role = (item.RoleName || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || user.indexOf(tk) !== -1 || role.indexOf(tk) !== -1;
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

      vm.layTenVaiTro = function (quyen) {
        var q = Number(quyen);
        if (q === 1) return 'Quản trị viên (Admin)';
        if (q === 2) return 'Thu ngân (ThuNgan)';
        if (q === 3) return 'Thủ kho (ThuKho)';
        if (q === 4) return 'Kế toán (KeToan)';
        return 'Người dùng (' + quyen + ')';
      };

      vm.moModalThem = function () {
        vm.modalForm.laSua = false;
        vm.modalForm.data = {
          MATAIKHOAN: 'TK' + Date.now().toString().slice(-4),
          USERNAME: '',
          PASS: '',
          QUYEN: 1
        };
        vm.modalForm.hienThi = true;
      };

      vm.moModalSua = function (row) {
        vm.modalForm.laSua = true;
        vm.modalForm.data = {
          MATAIKHOAN: row.MaTaiKhoan || row.MATAIKHOAN || '',
          USERNAME: row.UserName || row.USERNAME || '',
          PASS: '',
          QUYEN: Number(row.Quyen || row.QUYEN) || 1
        };
        vm.modalForm.hienThi = true;
      };

      vm.dongModal = function () {
        vm.modalForm.hienThi = false;
      };

      vm.luuTaiKhoan = function () {
        var ma = (vm.modalForm.data.MATAIKHOAN || '').trim();
        var user = (vm.modalForm.data.USERNAME || '').trim();
        var pass = (vm.modalForm.data.PASS || '').trim();
        var quyen = parseInt(vm.modalForm.data.QUYEN, 10);

        if (!ma || !user || (!vm.modalForm.laSua && !pass) || isNaN(quyen)) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền đầy đủ các trường bắt buộc (*).');
          return;
        }

        var payload = {
          MATAIKHOAN: ma,
          USERNAME: user,
          PASS: pass || '',
          QUYEN: quyen
        };

        vm.modalForm.dangLuu = true;
        var req = vm.modalForm.laSua
          ? ApiService.post(BASE + '/update-byID-taikhoan', payload)
          : ApiService.post(BASE + '/create-taikhoan', payload);

        req
          .then(function () {
            ToastService.success('Thành công', (vm.modalForm.laSua ? 'Cập nhật' : 'Tạo') + ' tài khoản thành công.');
            vm.dongModal();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể lưu tài khoản');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.xoaTaiKhoan = function (row) {
        var ma = row.MaTaiKhoan || row.MATAIKHOAN;
        var user = row.UserName || row.USERNAME || ma;
        if (!ma) return;

        ModalService.xacNhan(
          'Xóa tài khoản',
          'Bạn có chắc chắn muốn xóa tài khoản <strong>' + user + '</strong>? Thao tác không thể hoàn tác.',
          'Xóa tài khoản'
        )
          .then(function () {
            return ApiService.delete(BASE + '/del-byID-taikhoan', { mataikhoan: ma });
          })
          .then(function () {
            ToastService.success('Thành công', 'Xóa tài khoản thành công.');
            vm.taiDuLieu();
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể xóa tài khoản');
            }
          });
      };

      vm.khoiTao();
    }
  ]);
})();
