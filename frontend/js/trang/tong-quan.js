/**
 * tong-quan.controller.js — AngularJS Controller cho Dashboard Tổng quan
 * Hiển thị số liệu vận hành theo thời gian thực dựa trên phân quyền vai trò.
 */
(function () {
  'use strict';

  angular.module('qlblApp')
    .controller('TongQuanController', [
      'ApiService',
      'AuthService',
      'SidebarHeaderService',
      'APP_CONFIG',
      '$q',
      TongQuanController
    ]);

  function TongQuanController(ApiService, AuthService, SidebarHeaderService, APP_CONFIG, $q) {
    var vm = this;

    // Phân quyền
    if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan', 'ThuKho', 'KeToan'])) return;

    vm.vaiTro = AuthService.layVaiTro() || 'Admin';
    vm.tenVaiTro = APP_CONFIG.TEN_VAI_TRO[vm.vaiTro] || vm.vaiTro;
    vm.thoiGianHienTai = new Date().toLocaleDateString('vi-VN', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });

    vm.danhSachKPI = [];
    vm.dangTai = true;

    // Danh sách lối tắt theo vai trò
    vm.danhSachLoiTat = APP_CONFIG.CAU_HINH_MENU[vm.vaiTro] || APP_CONFIG.CAU_HINH_MENU.Admin;

    vm.$onInit = function () {
      SidebarHeaderService.khoiTao('tong-quan', 'Tổng quan', 'Trang chủ / Tổng quan');
      vm.taiKPI();
    };

    vm.taiKPI = function () {
      vm.dangTai = true;
      var promises = [];

      // Nhóm Kho / Hàng hóa
      if (vm.vaiTro === 'Admin' || vm.vaiTro === 'ThuKho') {
        promises.push(
          ApiService.get(ApiService.URL.thukho + '/QuanLySanPham/get-all-sanpham')
            .then(function (res) {
              var data = ApiService.layMang(res);
              var tongTon = data.reduce(function (s, sp) {
                var ton = ApiService.layThuocTinh(sp, 'SOLUONGTON') || 0;
                return s + (Number(ton) || 0);
              }, 0);
              return [
                { icon: 'bi-box-seam', mau: '#4f46e5', nen: '#eef2ff', giaTri: data.length.toLocaleString('vi-VN'), nhan: 'Tổng sản phẩm' },
                { icon: 'bi-archive', mau: '#8b5cf6', nen: '#f5f3ff', giaTri: tongTon.toLocaleString('vi-VN'), nhan: 'Tổng tồn kho' }
              ];
            }).catch(function () { return []; })
        );

        promises.push(
          ApiService.get(ApiService.URL.thukho + '/QuanLyNhapKho/get-all-phieunhapkho')
            .then(function (res) {
              var data = ApiService.layMang(res);
              return [
                { icon: 'bi-clipboard-plus', mau: '#10b981', nen: '#ecfdf5', giaTri: data.length.toLocaleString('vi-VN'), nhan: 'Phiếu nhập kho' }
              ];
            }).catch(function () { return []; })
        );
      }

      // Nhóm Bán hàng
      if (vm.vaiTro === 'Admin' || vm.vaiTro === 'ThuNgan') {
        promises.push(
          ApiService.get(ApiService.URL.thungan + '/QuanLyBanHang/get-all-hoadonban')
            .then(function (res) {
              var data = ApiService.layMang(res);
              var tongDT = data.reduce(function (s, hd) {
                var tien = ApiService.layThuocTinh(hd, 'TONGTIEN') || ApiService.layThuocTinh(hd, 'TONGTIENHANG') || 0;
                return s + (Number(tien) || 0);
              }, 0);
              return [
                { icon: 'bi-receipt', mau: '#f59e0b', nen: '#fffbeb', giaTri: data.length.toLocaleString('vi-VN'), nhan: 'Hóa đơn bán' },
                { icon: 'bi-cash-stack', mau: '#f43f5e', nen: '#fff1f2', giaTri: tongDT.toLocaleString('vi-VN') + ' đ', nhan: 'Tổng doanh thu' }
              ];
            }).catch(function () { return []; })
        );

        promises.push(
          ApiService.get(ApiService.URL.thungan + '/QuanLyBanHang/get-all-khachhang')
            .then(function (res) {
              var data = ApiService.layMang(res);
              return [
                { icon: 'bi-people', mau: '#0ea5e9', nen: '#f0f9ff', giaTri: data.length.toLocaleString('vi-VN'), nhan: 'Khách hàng' }
              ];
            }).catch(function () { return []; })
        );
      }

      $q.all(promises).then(function (results) {
        var merged = [];
        results.forEach(function (r) {
          if (Array.isArray(r)) merged = merged.concat(r);
        });
        vm.danhSachKPI = merged;
      }).finally(function () {
        vm.dangTai = false;
      });
    };
  }

})();
