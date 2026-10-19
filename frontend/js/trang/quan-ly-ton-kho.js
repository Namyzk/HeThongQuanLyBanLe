/**
 * quan-ly-ton-kho.js — AngularJS Controller cho Quản lý Tồn kho sản phẩm
 * Role: Admin, ThuKho, ThuNgan
 * Base API: /api/thukho/QuanLyTonKho
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('TonKhoController', [
    '$q',
    'ApiService',
    'AuthService',
    'ToastService',
    'SidebarHeaderService',
    function ($q, ApiService, AuthService, ToastService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.thukho + '/QuanLyTonKho';

      vm.danhSach = [];
      vm.danhSachDM = {};
      vm.tuKhoa = '';
      vm.dangTai = false;
      vm.loi = '';
      vm.trangHienTai = 1;
      vm.soDongMoiTrang = 20;
      vm.coTheSua = false;

      vm.modalCapNhat = {
        hienThi: false,
        dangLuu: false,
        maSP: '',
        tenSP: '',
        slHienTai: 0,
        soLuongMoi: 0,
        loi: ''
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuKho', 'ThuNgan'])) return;

        var vaiTro = AuthService.layVaiTro();
        vm.coTheSua = (vaiTro === 'Admin' || vaiTro === 'ThuKho');

        SidebarHeaderService.khoiTao('quan-ly-ton-kho', 'Quản lý Tồn kho', 'Trang chủ / Quản lý Tồn kho');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';

        var pTonKho = ApiService.get(BASE + '/get-all-sanpham-tonkho');
        var pDM = ApiService.get(ApiService.URL.thukho + '/QuanLyDanhMuc/get-all-danhmuc').catch(function () {
          return { data: [] };
        });

        $q.all([pTonKho, pDM])
          .then(function (results) {
            var rawTK = ApiService.layMang(results[0]);
            var rawDM = ApiService.layMang(results[1]);

            var dmMap = {};
            rawDM.forEach(function (dm) {
              var maDM = String(ApiService.layThuocTinh(dm, 'MADANHMUC') || '').trim();
              var tenDM = String(ApiService.layThuocTinh(dm, 'TENDANHMUC') || maDM).trim();
              if (maDM) dmMap[maDM] = tenDM;
            });
            vm.danhSachDM = dmMap;

            vm.danhSach = rawTK.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MASP') || '').trim();
              var ten = String(ApiService.layThuocTinh(it, 'TENSP') || '').trim();
              var dm = String(ApiService.layThuocTinh(it, 'MADANHMUC') || '').trim();
              var sl = Number(ApiService.layThuocTinh(it, 'SOLUONGTON')) || 0;
              var gia = Number(ApiService.layThuocTinh(it, 'DONGIA')) || 0;
              var tenDM = dmMap[dm] || dm || '—';
              return {
                MASP: ma,
                masp: ma,
                TENSP: ten,
                tensp: ten,
                MADANHMUC: dm,
                madanhmuc: dm,
                TENDANHMUC: tenDM,
                SOLUONGTON: sl,
                soluongton: sl,
                DONGIA: gia,
                dongia: gia
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách tồn kho';
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
          var ma = (item.MASP || '').toLowerCase();
          var ten = (item.TENSP || '').toLowerCase();
          var dm = (item.MADANHMUC || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || ten.indexOf(tk) !== -1 || dm.indexOf(tk) !== -1;
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

      vm.moModalCapNhat = function (row) {
        if (!vm.coTheSua) return;
        vm.modalCapNhat.maSP = row.MASP || '';
        vm.modalCapNhat.tenSP = row.TENSP || '';
        vm.modalCapNhat.slHienTai = Number(row.SOLUONGTON) || 0;
        vm.modalCapNhat.soLuongMoi = Number(row.SOLUONGTON) || 0;
        vm.modalCapNhat.loi = '';
        vm.modalCapNhat.hienThi = true;
      };

      vm.dongModal = function () {
        vm.modalCapNhat.hienThi = false;
      };

      vm.luuTonKho = function () {
        var sl = parseInt(vm.modalCapNhat.soLuongMoi, 10);
        if (isNaN(sl) || sl < 0) {
          vm.modalCapNhat.loi = 'Số lượng tồn kho phải là số nguyên >= 0.';
          return;
        }

        vm.modalCapNhat.dangLuu = true;
        vm.modalCapNhat.loi = '';

        ApiService.post(BASE + '/update-soluong-sanpham-tonkho', null, {
          maSP: vm.modalCapNhat.maSP,
          soLuongMoi: sl
        })
          .then(function () {
            ToastService.success('Thành công', 'Cập nhật tồn kho sản phẩm ' + vm.modalCapNhat.maSP + ' thành công.');
            vm.dongModal();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            vm.modalCapNhat.loi = typeof err === 'string' ? err : 'Lỗi cập nhật tồn kho';
            ToastService.error('Lỗi', vm.modalCapNhat.loi);
          })
          .finally(function () {
            vm.modalCapNhat.dangLuu = false;
          });
      };

      vm.khoiTao();
    }
  ]);
})();
