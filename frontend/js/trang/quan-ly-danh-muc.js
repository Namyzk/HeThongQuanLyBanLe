/**
 * quan-ly-danh-muc.js — AngularJS Controller cho Quản lý Danh mục hàng hóa
 * Role: Admin, ThuKho
 * Base API: /api/thukho/QuanLyDanhMuc
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('DanhMucController', [
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    function (ApiService, AuthService, ToastService, ModalService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.thukho + '/QuanLyDanhMuc';

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
          MADANHMUC: '',
          TENDANHMUC: '',
          MOTA: ''
        }
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuKho'])) return;
        SidebarHeaderService.khoiTao('quan-ly-danh-muc', 'Quản lý Danh mục', 'Trang chủ / Quản lý Danh mục');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        ApiService.get(BASE + '/get-all-danhmuc')
          .then(function (res) {
            var rawList = ApiService.layMang(res);
            vm.danhSach = rawList.map(function (it) {
              var ma = String(ApiService.layThuocTinh(it, 'MADANHMUC') || '').trim();
              var ten = String(ApiService.layThuocTinh(it, 'TENDANHMUC') || '').trim();
              var mota = String(ApiService.layThuocTinh(it, 'MOTA') || '').trim();
              return {
                MADANHMUC: ma,
                madanhmuc: ma,
                TENDANHMUC: ten,
                tendanhmuc: ten,
                MOTA: mota,
                mota: mota
              };
            });
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải danh sách danh mục';
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
          var ma = (item.MADANHMUC || '').toLowerCase();
          var ten = (item.TENDANHMUC || '').toLowerCase();
          var mota = (item.MOTA || '').toLowerCase();
          return ma.indexOf(tk) !== -1 || ten.indexOf(tk) !== -1 || mota.indexOf(tk) !== -1;
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
          MADANHMUC: 'DM' + Date.now().toString().slice(-4),
          TENDANHMUC: '',
          MOTA: ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.moModalSua = function (dm) {
        vm.modalForm.laSua = true;
        vm.modalForm.data = {
          MADANHMUC: dm.MADANHMUC || '',
          TENDANHMUC: dm.TENDANHMUC || '',
          MOTA: dm.MOTA || ''
        };
        vm.modalForm.hienThi = true;
      };

      vm.dongModal = function () {
        vm.modalForm.hienThi = false;
      };

      vm.luuDanhMuc = function () {
        var ma = (vm.modalForm.data.MADANHMUC || '').trim();
        var ten = (vm.modalForm.data.TENDANHMUC || '').trim();
        if (!ma || !ten) {
          ToastService.warning('Cảnh báo', 'Vui lòng điền mã và tên danh mục.');
          return;
        }

        var payload = {
          MADANHMUC: ma,
          TENDANHMUC: ten,
          MOTA: (vm.modalForm.data.MOTA || '').trim() || null
        };

        vm.modalForm.dangLuu = true;
        var req = vm.modalForm.laSua
          ? ApiService.post(BASE + '/update-danhmuc', payload)
          : ApiService.post(BASE + '/insert-danhmuc', payload);

        req
          .then(function () {
            ToastService.success('Thành công', (vm.modalForm.laSua ? 'Cập nhật' : 'Thêm') + ' danh mục thành công.');
            vm.dongModal();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể lưu danh mục');
          })
          .finally(function () {
            vm.modalForm.dangLuu = false;
          });
      };

      vm.xoaDanhMuc = function (dm) {
        if (!dm || !dm.MADANHMUC) return;
        ModalService.xacNhan('Xóa danh mục', 'Bạn có chắc chắn muốn xóa danh mục <strong>' + (dm.TENDANHMUC || dm.MADANHMUC) + '</strong>?', 'Xóa')
          .then(function () {
            return ApiService.delete(BASE + '/del-byID-danhmuc', { madanhmuc: dm.MADANHMUC });
          })
          .then(function () {
            ToastService.success('Thành công', 'Xóa danh mục thành công.');
            vm.taiDuLieu();
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể xóa danh mục');
            }
          });
      };

      vm.khoiTao();
    }
  ]);
})();
