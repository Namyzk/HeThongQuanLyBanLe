/**
 * dang-nhap.controller.js — AngularJS Controller cho trang Đăng nhập
 * Xử lý xác thực JWT qua HTTP-Only Cookie, ghi nhận phiên và điều hướng vai trò.
 */
(function () {
  'use strict';

  angular.module('qlblApp')
    .controller('DangNhapController', [
      'ApiService',
      'AuthService',
      'ToastService',
      '$window',
      DangNhapController
    ]);

  function DangNhapController(ApiService, AuthService, ToastService, $window) {
    var vm = this;

    // Dữ liệu mô hình biểu mẫu
    vm.username = '';
    vm.pass = '';
    vm.hienMatKhau = false;
    vm.xemPhanQuyen = false;
    vm.dangXuLy = false;
    vm.thongBaoLoi = '';

    // Khởi tạo
    vm.$onInit = function () {
      if (AuthService.daDangNhap()) {
        $window.location.href = 'tong-quan.html';
      }
    };

    // Đổi hiển thị mật khẩu
    vm.toggleMatKhau = function () {
      vm.hienMatKhau = !vm.hienMatKhau;
    };

    // Mở xem bảng phân quyền
    vm.moPhanQuyen = function () {
      vm.xemPhanQuyen = true;
    };

    // Quay lại màn hình đăng nhập
    vm.veDangNhap = function () {
      vm.xemPhanQuyen = false;
      vm.thongBaoLoi = '';
    };

    // Xử lý gửi biểu mẫu đăng nhập
    vm.xuLyDangNhap = function () {
      if (!vm.username || !vm.pass) {
        vm.thongBaoLoi = 'Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.';
        return;
      }

      vm.dangXuLy = true;
      vm.thongBaoLoi = '';

      var url = ApiService.URL.login + '/login';
      var payload = {
        username: vm.username.trim(),
        pass: vm.pass
      };

      ApiService.post(url, payload)
        .then(function (res) {
          if (!res || !res.success) {
            vm.thongBaoLoi = (res && res.message) || 'Đăng nhập không thành công.';
            return;
          }

          // Lưu thông tin phiên người dùng
          AuthService.luuPhien(res.data, res.refreshTokenExpiresAtUtc);

          // Điều hướng vào màn hình tổng quan
          $window.location.href = 'tong-quan.html';
        })
        .catch(function (errMsg) {
          vm.thongBaoLoi = errMsg || 'Sai tên đăng nhập hoặc mật khẩu.';
        })
        .finally(function () {
          vm.dangXuLy = false;
        });
    };
  }

})();
