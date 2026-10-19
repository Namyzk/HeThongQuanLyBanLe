/**
 * quan-ly-khuyen-mai.js — AngularJS Controller cho Quản lý Khuyến mãi
 * Role: Admin
 * Base API: /api/admin/QuanLyKhuyenMai
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('KhuyenMaiController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, ModalService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.admin + '/QuanLyKhuyenMai';

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
          MaKM: '',
          TenKM: '',
          MaSP: '',
          NgayBD: '',
          NgayKT: ''
        }
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin'])) return;
        SidebarHeaderService.khoiTao('quan-ly-khuyen-mai', 'Quản lý Khuyến mãi', 'Trang chủ / Quản lý Khuyến mãi');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE + '/get-all-khuyenmai')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MAKM') || ApiService.layThuocTinh(it, 'MaKM') || '').trim();
              var ten = String(ApiService.layThuocTinh(it, 'TENKM') || ApiService.layThuocTinh(it, 'TenKM') || '').trim();
              var sp = String(ApiService.layThuocTinh(it, 'MASP') || ApiService.layThuocTinh(it, 'MaSP') || '').trim();
              var bd = ApiService.layThuocTinh(it, 'NGAYBATDAU') || ApiService.layThuocTinh(it, 'NgayBD') || null;
              var kt = ApiService.layThuocTinh(it, 'NGAYKETTHUC') || ApiService.layThuocTinh(it, 'NgayKT') || null;
              return {
                MAKM: ma,
                MaKM: ma,
                makm: ma,
                TENKM: ten,
                TenKM: ten,
                tenkm: ten,
                MASP: sp,
                MaSP: sp,
                masp: sp,
                NGAYBATDAU: bd,
                NgayBD: bd,
                ngaybatdau: bd,
                NGAYKETTHUC: kt,
                NgayKT: kt,
                ngayketthuc: kt
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách khuyến mãi';
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
          var ma = (item.MAKM || item.MaKM || '').toLowerCase();
          var ten = (item.TENKM || item.TenKM || '').toLowerCase();
          var sp = (item.MASP || item.MaSP || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || ten.indexOf(tk) !== -1 || sp.indexOf(tk) !== -1;
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
        var today = new Date().toISOString().substring(0, 10);
        vm.modalForm.data = {
          MaKM: 'KM' + Date.now().toString().slice(-4),
          TenKM: '',
          MaSP: '',
          NgayBD: today,
          NgayKT: today
        };
        vm.modalForm.hienThi = true;
      };

      vm.moModalSua = function (row) {
        vm.modalForm.laSua = true;
        var nbd = row.NGAYBATDAU || row.NgayBD;
        var nkt = row.NGAYKETTHUC || row.NgayKT;
        vm.modalForm.data = {
          MaKM: row.MAKM || row.MaKM || '',
          TenKM: row.TENKM || row.TenKM || '',
          MaSP: row.MASP || row.MaSP || '',
          NgayBD: nbd ? String(nbd).substring(0, 10) : '',
          NgayKT: nkt ? String(nkt).substring(0, 10) : ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.dongModal = function () {
        vm.modalForm.hienThi = false;
      };

      vm.luuKhuyenMai = function () {
        var makm = (vm.modalForm.data.MaKM || '').trim();
        var tenkm = (vm.modalForm.data.TenKM || '').trim();
        var masp = (vm.modalForm.data.MaSP || '').trim();
        var ngaybd = vm.modalForm.data.NgayBD;
        var ngaykt = vm.modalForm.data.NgayKT;

        if (!makm || !tenkm || !masp || !ngaybd || !ngaykt) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền đầy đủ thông tin bắt buộc.');
          return;
        }

        if (new Date(ngaybd) > new Date(ngaykt)) {
          ToastService.warning('Cảnh báo', 'Ngày bắt đầu không thể sau ngày kết thúc.');
          return;
        }

        var payload = {
          MaKM: makm,
          TenKM: tenkm,
          MaSP: masp,
          NgayBD: ngaybd,
          NgayKT: ngaykt
        };

        vm.modalForm.dangLuu = true;
        var req = vm.modalForm.laSua
          ? ApiService.post(BASE + '/update-khuyenmai', payload)
          : ApiService.post(BASE + '/create-khuyenmai', payload);

        req
          .then(function () {
            ToastService.success('Thành công', (vm.modalForm.laSua ? 'Cập nhật' : 'Thêm') + ' khuyến mãi thành công.');
            vm.dongModal();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể lưu khuyến mãi');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.xoaKhuyenMai = function (row) {
        var ma = row.MAKM || row.MaKM;
        var ten = row.TENKM || row.TenKM || ma;
        if (!ma) return;

        ModalService.xacNhan('Xóa khuyến mãi', 'Bạn có chắc chắn muốn xóa khuyến mãi <strong>' + ten + '</strong>?', 'Xóa')
          .then(function () {
            return ApiService.delete(BASE + '/del-khuyenmai', { ma: ma });
          })
          .then(function () {
            ToastService.success('Thành công', 'Xóa khuyến mãi thành công.');
            vm.taiDuLieu();
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể xóa khuyến mãi');
            }
          });
      };

      vm.khoiTao();
    }
  ]);
})();
