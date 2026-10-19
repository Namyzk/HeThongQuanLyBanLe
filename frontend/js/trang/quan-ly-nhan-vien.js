/**
 * quan-ly-nhan-vien.js — AngularJS Controller cho Quản lý Nhân viên
 * Role: Admin
 * Base API: /api/admin/QuanLyNhanVien
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('NhanVienController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, ModalService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.admin + '/QuanLyNhanVien';

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
          MANV: '',
          TENNV: '',
          SDT: '',
          EMAIL: '',
          DIACHI: '',
          HINHANH: ''
        }
      };

      vm.modalChiTiet = {
        hienThi: false,
        data: null
      };

      function hopLeSdt(sdt) {
        if (!sdt) return true;
        var clean = sdt.replace(/[\s.-]/g, '');
        return /^(0[2-9]\d{8}|01\d{9})$/.test(clean);
      }

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin'])) return;
        SidebarHeaderService.khoiTao('quan-ly-nhan-vien', 'Quản lý Nhân viên', 'Trang chủ / Quản lý Nhân viên');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE + '/get-all-nhanvien')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MANV') || '').trim();
              var ten = String(ApiService.layThuocTinh(it, 'TENNV') || '').trim();
              var sdt = String(ApiService.layThuocTinh(it, 'SDT') || '').trim();
              var dc = String(ApiService.layThuocTinh(it, 'DIACHI') || '').trim();
              var em = String(ApiService.layThuocTinh(it, 'EMAIL') || '').trim();
              var anh = ApiService.layThuocTinh(it, 'HINHANH') || null;
              return {
                MANV: ma,
                manv: ma,
                TENNV: ten,
                tennv: ten,
                SDT: sdt,
                sdt: sdt,
                DIACHI: dc,
                diachi: dc,
                EMAIL: em,
                email: em,
                HINHANH: anh
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách nhân viên';
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
          var ma = (item.MANV || '').toLowerCase();
          var ten = (item.TENNV || '').toLowerCase();
          var sdt = (item.SDT || '').toLowerCase();
          var dc = (item.DIACHI || '').toLowerCase();
          var em = (item.EMAIL || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || ten.indexOf(tk) !== -1 || sdt.indexOf(tk) !== -1 || dc.indexOf(tk) !== -1 || em.indexOf(tk) !== -1;
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
          MANV: 'NV' + Date.now().toString().slice(-4),
          TENNV: '',
          SDT: '',
          EMAIL: '',
          DIACHI: '',
          HINHANH: ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.moModalSua = function (row) {
        vm.modalForm.laSua = true;
        vm.modalForm.data = {
          MANV: row.MANV || '',
          TENNV: row.TENNV || '',
          SDT: row.SDT || '',
          EMAIL: row.EMAIL || '',
          DIACHI: row.DIACHI || '',
          HINHANH: row.HINHANH || ''
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

      vm.chonTepAnh = function (files) {
        if (!files || !files[0]) return;
        var file = files[0];
        if (file.size > 2 * 1024 * 1024) {
          ToastService.warning('Cảnh báo', 'Ảnh vượt quá 2MB, vui lòng chọn ảnh nhẹ hơn.');
          return;
        }
        var reader = new FileReader();
        reader.onload = function (e) {
          vm.modalForm.data.HINHANH = e.target.result;
          var scope = angular.element(document.getElementById('modal-nhan-vien')).scope();
          if (scope) scope.$apply();
        };
        reader.readAsDataURL(file);
      };

      vm.xoaAnh = function () {
        vm.modalForm.data.HINHANH = '';
      };

      vm.luuNhanVien = function () {
        var ma = (vm.modalForm.data.MANV || '').trim();
        var ten = (vm.modalForm.data.TENNV || '').trim();
        var sdt = (vm.modalForm.data.SDT || '').trim();
        var email = (vm.modalForm.data.EMAIL || '').trim();

        if (!ma || !ten) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền mã và tên nhân viên.');
          return;
        }

        if (sdt && !hopLeSdt(sdt)) {
          ToastService.warning('Cảnh báo', 'Số điện thoại không hợp lệ (cần 10 chữ số).');
          return;
        }

        var payload = {
          MANV: ma,
          TENNV: ten,
          SDT: sdt || null,
          EMAIL: email || null,
          DIACHI: (vm.modalForm.data.DIACHI || '').trim() || null,
          HINHANH: vm.modalForm.data.HINHANH || null
        };

        vm.modalForm.dangLuu = true;
        var req = vm.modalForm.laSua
          ? ApiService.post(BASE + '/update-nhanvien', payload)
          : ApiService.post(BASE + '/create-nhanvien', payload);

        req
          .then(function () {
            ToastService.success('Thành công', (vm.modalForm.laSua ? 'Cập nhật' : 'Thêm') + ' nhân viên thành công.');
            vm.dongModalForm();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể lưu nhân viên');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.xoaNhanVien = function (row) {
        var ma = row.MANV;
        var ten = row.TENNV || ma;
        if (!ma) return;

        ModalService.xacNhan('Xóa nhân viên', 'Bạn có chắc chắn muốn xóa nhân viên <strong>' + ten + '</strong>?', 'Xóa nhân viên')
          .then(function () {
            return ApiService.delete(BASE + '/del-nhanvien', { manv: ma });
          })
          .then(function () {
            ToastService.success('Thành công', 'Xóa nhân viên thành công.');
            vm.taiDuLieu();
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể xóa nhân viên');
            }
          });
      };

      vm.khoiTao();
    }
  ]);
})();
