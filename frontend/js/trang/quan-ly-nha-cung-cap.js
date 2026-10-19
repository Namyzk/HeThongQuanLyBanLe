/**
 * quan-ly-nha-cung-cap.js — AngularJS Controller cho Quản lý Nhà cung cấp
 * Role: Admin, ThuKho
 * Base API: /api/thukho/QuanLyNhapKho
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('NhaCungCapController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, ModalService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.thukho + '/QuanLyNhapKho';

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
          MaNCC: '',
          TenNCC: '',
          SDT: '',
          EMAIL: '',
          DiaChi: ''
        }
      };

      function hopLeSdt(sdt) {
        if (!sdt) return true;
        var clean = sdt.replace(/[\s.-]/g, '');
        return /^(0[2-9]\d{8}|01\d{9})$/.test(clean);
      }

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuKho'])) return;
        SidebarHeaderService.khoiTao('quan-ly-nha-cung-cap', 'Quản lý Nhà cung cấp', 'Trang chủ / Quản lý Nhà cung cấp');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE + '/get-all-nhacungcap')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MANCC') || ApiService.layThuocTinh(it, 'MaNCC') || '').trim();
              var ten = String(ApiService.layThuocTinh(it, 'TENNCC') || ApiService.layThuocTinh(it, 'TenNCC') || '').trim();
              var sdt = String(ApiService.layThuocTinh(it, 'SDT') || '').trim();
              var dc = String(ApiService.layThuocTinh(it, 'DIACHI') || ApiService.layThuocTinh(it, 'DiaChi') || '').trim();
              var em = String(ApiService.layThuocTinh(it, 'EMAIL') || ApiService.layThuocTinh(it, 'Email') || '').trim();
              return {
                MANCC: ma,
                MaNCC: ma,
                mancc: ma,
                TENNCC: ten,
                TenNCC: ten,
                tenncc: ten,
                SDT: sdt,
                sdt: sdt,
                DIACHI: dc,
                DiaChi: dc,
                diachi: dc,
                EMAIL: em,
                Email: em,
                email: em
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách nhà cung cấp';
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
          var ma = (item.MaNCC || item.MANCC || '').toLowerCase();
          var ten = (item.TenNCC || item.TENNCC || '').toLowerCase();
          var sdt = (item.SDT || '').toLowerCase();
          var email = (item.EMAIL || '').toLowerCase();
          var dc = (item.DiaChi || item.DIACHI || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || ten.indexOf(tk) !== -1 || sdt.indexOf(tk) !== -1 || email.indexOf(tk) !== -1 || dc.indexOf(tk) !== -1;
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
          MaNCC: 'NCC' + Date.now().toString().slice(-5),
          TenNCC: '',
          SDT: '',
          EMAIL: '',
          DiaChi: ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.moModalSua = function (row) {
        vm.modalForm.laSua = true;
        vm.modalForm.data = {
          MaNCC: row.MaNCC || row.MANCC || '',
          TenNCC: row.TenNCC || row.TENNCC || '',
          SDT: row.SDT || '',
          EMAIL: row.EMAIL || '',
          DiaChi: row.DiaChi || row.DIACHI || ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.dongModal = function () {
        vm.modalForm.hienThi = false;
      };

      vm.luuNhaCungCap = function () {
        var ma = (vm.modalForm.data.MaNCC || '').trim();
        var ten = (vm.modalForm.data.TenNCC || '').trim();
        var sdt = (vm.modalForm.data.SDT || '').trim();
        var email = (vm.modalForm.data.EMAIL || '').trim();

        if (!ma || !ten) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền mã và tên nhà cung cấp.');
          return;
        }

        if (sdt && !hopLeSdt(sdt)) {
          ToastService.warning('Cảnh báo', 'Số điện thoại không hợp lệ (cần 10 chữ số).');
          return;
        }

        var payload = {
          MaNCC: ma,
          TenNCC: ten,
          SDT: sdt || null,
          EMAIL: email || null,
          DiaChi: (vm.modalForm.data.DiaChi || '').trim() || null
        };

        vm.modalForm.dangLuu = true;
        var req = vm.modalForm.laSua
          ? ApiService.post(BASE + '/update-nhacungcap', payload)
          : ApiService.post(BASE + '/create-nhacungcap', payload);

        req
          .then(function () {
            ToastService.success('Thành công', (vm.modalForm.laSua ? 'Cập nhật' : 'Thêm') + ' nhà cung cấp thành công.');
            vm.dongModal();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể lưu nhà cung cấp');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.xoaNhaCungCap = function (row) {
        var ma = row.MaNCC || row.MANCC;
        var ten = row.TenNCC || row.TENNCC || ma;
        if (!ma) return;

        ModalService.xacNhan('Xóa nhà cung cấp', 'Bạn có chắc chắn muốn xóa nhà cung cấp <strong>' + ten + '</strong>?', 'Xóa')
          .then(function () {
            return ApiService.delete(BASE + '/del-nhacungcap', { ma: ma });
          })
          .then(function () {
            ToastService.success('Thành công', 'Xóa nhà cung cấp thành công.');
            vm.taiDuLieu();
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể xóa nhà cung cấp');
            }
          });
      };

      vm.khoiTao();
    }
  ]);
})();
