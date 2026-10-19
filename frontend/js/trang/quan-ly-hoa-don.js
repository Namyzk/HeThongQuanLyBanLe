/**
 * quan-ly-hoa-don.js — AngularJS Controller cho Quản lý & In Hóa đơn bán hàng
 * Role: Admin, ThuNgan, KeToan
 * Base API: /api/thungan/QuanLyBanHang và /api/thungan/QuanLyDoiTra
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('HoaDonController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    '$window',
    function (ApiService, AuthService, ToastService, ModalService, SidebarHeaderService, $window) {
      var vm = this;

      var BASE_BH = ApiService.URL.thungan + '/QuanLyBanHang';
      var BASE_DT = ApiService.URL.thungan + '/QuanLyDoiTra';

      vm.danhSach = [];
      vm.tuKhoa = '';
      vm.dangTai = false;
      vm.loi = '';
      vm.trangHienTai = 1;
      vm.soDongMoiTrang = 20;

      vm.modalChiTiet = {
        hienThi: false,
        dangTai: false,
        hoaDon: null,
        chiTiet: []
      };

      function layGiaTriSo(val) {
        var n = Number(val);
        if (isNaN(n) || Math.abs(n) < 0.001 || Object.is(n, -0)) return 0;
        return n;
      }

      function chuanHoaHoaDon(hd) {
        if (!hd) return {};
        var tienHang = layGiaTriSo(ApiService.layThuocTinh(hd, 'TONGTIENHANG'));
        var thue = layGiaTriSo(ApiService.layThuocTinh(hd, 'THUEVAT'));
        var giam = layGiaTriSo(ApiService.layThuocTinh(hd, 'GIAMGIA'));

        return {
          ...hd,
          MAHDBAN: ApiService.layThuocTinh(hd, 'MAHDBAN'),
          MANV: ApiService.layThuocTinh(hd, 'MANV'),
          MAKH: ApiService.layThuocTinh(hd, 'MAKH'),
          NGAYLAP: ApiService.layThuocTinh(hd, 'NGAYLAP'),
          TONGTIENHANG: tienHang,
          THUEVAT: thue,
          GIAMGIA: giam,
          TONG_THANH_TOAN: Math.max(0, tienHang + thue - giam)
        };
      }

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan', 'KeToan'])) return;
        SidebarHeaderService.khoiTao('quan-ly-hoa-don', 'Quản lý Hóa đơn', 'Trang chủ / Quản lý Hóa đơn');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE_BH + '/get-all-hoadonban')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(chuanHoaHoaDon);
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách hóa đơn';
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
          var maHD = (item.MAHDBAN || '').toLowerCase();
          var maNV = (item.MANV || '').toLowerCase();
          var maKH = (item.MAKH || '').toLowerCase();
          return maHD.indexOf(tk) !== -1 || maNV.indexOf(tk) !== -1 || maKH.indexOf(tk) !== -1;
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

      vm.xemChiTiet = function (row) {
        var maHD = row.MAHDBAN;
        if (!maHD) return;

        vm.modalChiTiet.hienThi = true;
        vm.modalChiTiet.dangTai = true;
        vm.modalChiTiet.hoaDon = row;
        vm.modalChiTiet.chiTiet = [];

        var pHD = ApiService.get(BASE_BH + '/get-hoadonban-by-id?maHoaDon=' + encodeURIComponent(maHD)).catch(function () {
          return { data: row };
        });
        var pCT = ApiService.get(BASE_BH + '/get-chitietban-by-IDhoadon?maHDB=' + encodeURIComponent(maHD)).catch(function () {
          return ApiService.get(BASE_DT + '/get-chitietban-by-IDhoadon?maHDB=' + encodeURIComponent(maHD)).catch(function () {
            return { data: [] };
          });
        });

        Promise.all([pHD, pCT])
          .then(function (results) {
            var hdRes = results[0];
            var ctRes = results[1];
            vm.modalChiTiet.hoaDon = chuanHoaHoaDon(hdRes.data || row);
            vm.modalChiTiet.chiTiet = ApiService.layMang(ctRes);
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể tải chi tiết hóa đơn');
          })
          .finally(function () {
            vm.modalChiTiet.dangTai = false;
          });
      };

      vm.dongModalChiTiet = function () {
        vm.modalChiTiet.hienThi = false;
      };

      vm.inHoaDon = function () {
        $window.print();
      };

      vm.xoaHoaDon = function (row) {
        var maHD = row.MAHDBAN;
        if (!maHD) return;

        ModalService.xacNhan(
          'Xóa hóa đơn',
          'Bạn có chắc chắn muốn xóa hóa đơn <strong>' + maHD + '</strong>? Thao tác này không thể hoàn tác.',
          'Xóa hóa đơn'
        )
          .then(function () {
            return ApiService.delete(BASE_BH + '/delete-hoadonban', { maHDBan: maHD });
          })
          .then(function () {
            ToastService.success('Thành công', 'Đã xóa hóa đơn ' + maHD);
            vm.taiDuLieu();
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể xóa hóa đơn');
            }
          });
      };

      vm.khoiTao();
    }
  ]);
})();
