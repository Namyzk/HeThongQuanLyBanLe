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
    vm.daLaAdmin = false;
    vm.dangTiepTucTaoTaiKhoan = false;
    vm.dangTaoTaiKhoan = false;
    vm.loiTaoTaiKhoan = '';
    vm.dsVaiTro = [
      { quyen: 2, ten: 'Thu ngân' },
      { quyen: 3, ten: 'Thủ kho' },
      { quyen: 4, ten: 'Kế toán' },
      { quyen: 1, ten: 'Quản trị viên' }
    ];

    function taoMaTaiKhoanMoi() {
      return 'TK' + Date.now().toString().slice(-13);
    }

    function datLaiBieuMauTaiKhoan() {
      vm.taiKhoanMoi = {
        maTaiKhoan: taoMaTaiKhoanMoi(),
        username: '',
        password: '',
        email: '',
        xacNhanMatKhau: '',
        quyen: 2
      };
    }
    datLaiBieuMauTaiKhoan();

    // Khởi tạo
    vm.$onInit = function () {
      if (AuthService.daDangNhap()) {
        if (AuthService.layVaiTro() === 'Admin') {
          vm.daLaAdmin = true;
          vm.xemPhanQuyen = true;
        } else {
          $window.location.href = 'tong-quan.html';
        }
      }
    };


    // Đổi hiển thị mật khẩu
    vm.toggleMatKhau = function () {
      vm.hienMatKhau = !vm.hienMatKhau;
    };

    // Mở xem bảng phân quyền
    vm.moPhanQuyen = function () {
      vm.xemPhanQuyen = true;
      vm.loiTaoTaiKhoan = '';
      if (!vm.daLaAdmin) vm.dangTiepTucTaoTaiKhoan = true;
    };

    vm.dangNhapAdminDeTaoTaiKhoan = function () {
      vm.dangTiepTucTaoTaiKhoan = true;
      vm.xemPhanQuyen = false;
      vm.loiTaoTaiKhoan = '';
      vm.thongBaoLoi = 'Đăng nhập bằng tài khoản Admin để tiếp tục tạo tài khoản.';
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

          if (vm.dangTiepTucTaoTaiKhoan && res.data && res.data.roleName === 'Admin') {
            vm.daLaAdmin = true;
            vm.dangTiepTucTaoTaiKhoan = false;
            vm.xemPhanQuyen = true;
            vm.thongBaoLoi = '';
            vm.loiTaoTaiKhoan = '';
            return;
          }

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

    vm.taoTaiKhoan = function () {
      if (!vm.daLaAdmin || !AuthService.daDangNhap() || AuthService.layVaiTro() !== 'Admin') {
        vm.loiTaoTaiKhoan = 'Chỉ quản trị viên đã đăng nhập mới được tạo tài khoản.';
        return;
      }

      var username = (vm.taiKhoanMoi.username || '').trim();
      var password = vm.taiKhoanMoi.password || '';
      if (!username || username.length > 20) {
        vm.loiTaoTaiKhoan = 'Tên đăng nhập phải có từ 1 đến 20 ký tự.';
        return;
      }
      var email = (vm.taiKhoanMoi.email || '').trim();
      if (!/^\S+@\S+\.\S+$/.test(email) || email.length > 320) {
        vm.loiTaoTaiKhoan = 'Vui lòng nhập email hợp lệ (tối đa 320 ký tự).';
        return;
      }
      if (password.length < 6 || password.length > 20) {
        vm.loiTaoTaiKhoan = 'Mật khẩu phải có từ 6 đến 20 ký tự.';
        return;
      }
      if (password !== vm.taiKhoanMoi.xacNhanMatKhau) {
        vm.loiTaoTaiKhoan = 'Mật khẩu xác nhận không khớp.';
        return;
      }

      var payload = {
        MATAIKHOAN: vm.taiKhoanMoi.maTaiKhoan,
        USERNAME: username,
        EMAIL: (vm.taiKhoanMoi.email || '').trim(),
        PASS: password,
        QUYEN: Number(vm.taiKhoanMoi.quyen)
      };

      vm.dangTaoTaiKhoan = true;
      vm.loiTaoTaiKhoan = '';
      ApiService.post(ApiService.URL.admin + '/QuanLyTaiKhoan/create-taikhoan', payload)
        .then(function (res) {
          ToastService.success('Thành công', (res && res.message) || 'Tạo tài khoản thành công.');
          datLaiBieuMauTaiKhoan();
        })
        .catch(function (errMsg) {
          vm.loiTaoTaiKhoan = errMsg || 'Không thể tạo tài khoản.';
        })
        .finally(function () {
          vm.dangTaoTaiKhoan = false;
        });
    };
  }

})();
