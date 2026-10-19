/**
 * quan-ly-khach-hang.js — AngularJS Controller cho Quản lý Khách hàng
 * Role: Admin, ThuNgan
 * Base API: /api/thungan/QuanLyBanHang
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('KhachHangController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.thungan + '/QuanLyBanHang';

      vm.danhSach = [];
      vm.tuKhoa = '';
      vm.dangTai = false;
      vm.loi = '';
      vm.trangHienTai = 1;
      vm.soDongMoiTrang = 20;

      vm.modalForm = {
        hienThi: false,
        dangLuu: false,
        data: {
          MaKH: '',
          TenKH: '',
          SDT: '',
          DiaChi: ''
        }
      };

      vm.modalChiTiet = {
        hienThi: false,
        data: null
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan'])) return;
        SidebarHeaderService.khoiTao('quan-ly-khach-hang', 'Khách hàng', 'Trang chủ / Khách hàng');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE + '/get-all-khachhang')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MAKH') || ApiService.layThuocTinh(it, 'MaKH') || '').trim();
              var ten = String(ApiService.layThuocTinh(it, 'TENKH') || ApiService.layThuocTinh(it, 'TenKH') || '').trim();
              var sdt = String(ApiService.layThuocTinh(it, 'SDT') || '').trim();
              var dc = String(ApiService.layThuocTinh(it, 'DIACHI') || ApiService.layThuocTinh(it, 'DiaChi') || '').trim();
              return {
                MAKH: ma,
                MaKH: ma,
                makh: ma,
                TENKH: ten,
                TenKH: ten,
                tenkh: ten,
                SDT: sdt,
                sdt: sdt,
                DIACHI: dc,
                DiaChi: dc,
                diachi: dc
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách khách hàng';
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
          var ma = (item.MaKH || '').toLowerCase();
          var ten = (item.TenKH || '').toLowerCase();
          var sdt = (item.SDT || '').toLowerCase();
          var dc = (item.DiaChi || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || ten.indexOf(tk) !== -1 || sdt.indexOf(tk) !== -1 || dc.indexOf(tk) !== -1;
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
        vm.modalForm.data = {
          MaKH: 'KH' + Date.now().toString().slice(-6),
          TenKH: '',
          SDT: '',
          DiaChi: ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.dongModalForm = function () {
        vm.modalForm.hienThi = false;
      };

      vm.luuKhachHang = function () {
        var makh = (vm.modalForm.data.MaKH || '').trim();
        var tenkh = (vm.modalForm.data.TenKH || '').trim();
        var sdt = (vm.modalForm.data.SDT || '').trim();

        if (!makh || !tenkh) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền mã và tên khách hàng.');
          return;
        }

        if (sdt && !/^\d{10}$/.test(sdt)) {
          ToastService.warning('Cảnh báo', 'Số điện thoại phải đúng 10 chữ số.');
          return;
        }

        var payload = {
          MaKH: makh,
          TenKH: tenkh,
          SDT: sdt || null,
          DiaChi: (vm.modalForm.data.DiaChi || '').trim() || null
        };

        vm.modalForm.dangLuu = true;
        ApiService.post(BASE + '/insert-khachhang', payload)
          .then(function () {
            ToastService.success('Thành công', 'Thêm khách hàng thành công.');
            vm.dongModalForm();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể thêm khách hàng');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.xemChiTiet = function (row) {
        var ma = row.MaKH || row.MAKH;
        if (!row || !ma) return;
        ApiService.get(BASE + '/get-khachhang-byid?maKH=' + encodeURIComponent(ma))
          .then(function (res) {
            var raw = res.data || row;
            var maKH = String(ApiService.layThuocTinh(raw, 'MAKH') || ApiService.layThuocTinh(raw, 'MaKH') || ma).trim();
            var tenKH = String(ApiService.layThuocTinh(raw, 'TENKH') || ApiService.layThuocTinh(raw, 'TenKH') || row.TenKH).trim();
            var sdt = String(ApiService.layThuocTinh(raw, 'SDT') || row.SDT || '').trim();
            var diaChi = String(ApiService.layThuocTinh(raw, 'DIACHI') || ApiService.layThuocTinh(raw, 'DiaChi') || row.DiaChi || '').trim();
            vm.modalChiTiet.data = { MaKH: maKH, TenKH: tenKH, SDT: sdt, DiaChi: diaChi };
            vm.modalChiTiet.hienThi = true;
          })
          .catch(function () {
            vm.modalChiTiet.data = row;
            vm.modalChiTiet.hienThi = true;
          });
      };

      vm.dongModalChiTiet = function () {
        vm.modalChiTiet.hienThi = false;
      };

      vm.khoiTao();
    }
  ]);
})();
