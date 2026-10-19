/**
 * doi-tra.js — AngularJS Controller cho Đổi trả hàng hóa
 * Role: Admin, ThuNgan
 * Base API: /api/thungan/QuanLyDoiTra
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('DoiTraController', [
    '$q',
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    function ($q, ApiService, AuthService, ToastService, ModalService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.thungan + '/QuanLyDoiTra';
      var BASE_SP = ApiService.URL.thungan + '/QuanLyBanHang';

      vm.maHDTim = '';
      vm.dangTai = false;
      vm.daTim = false;
      vm.hoaDonHienTai = null;
      vm.chiTietHienTai = [];
      vm.danhSachSP = [];
      vm.loi = '';

      vm.modalDoiSP = {
        hienThi: false,
        dangLuu: false,
        data: {
          masp: '',
          soluong: 1,
          dongia: 0
        }
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan'])) return;
        SidebarHeaderService.khoiTao('doi-tra', 'Đổi trả hàng hóa', 'Trang chủ / Đổi trả hàng hóa');

        ApiService.get(BASE_SP + '/get-all-sanpham')
          .then(function (res) {
            vm.danhSachSP = ApiService.layMang(res).map(ApiService.chuanHoaDoiTuong);
          })
          .catch(function () {});
      };

      vm.timHoaDon = function () {
        var ma = (vm.maHDTim || '').trim();
        if (!ma) {
          ToastService.warning('Cảnh báo', 'Vui lòng nhập mã hóa đơn cần tra cứu.');
          return;
        }

        vm.dangTai = true;
        vm.daTim = true;
        vm.loi = '';
        vm.hoaDonHienTai = null;
        vm.chiTietHienTai = [];

        var pHD = ApiService.get(BASE + '/get-hoadonban-by-id?maHoaDon=' + encodeURIComponent(ma));
        var pCT = ApiService.get(BASE + '/get-chitietban-by-IDhoadon?maHDB=' + encodeURIComponent(ma));

        $q.all([pHD, pCT])
          .then(function (results) {
            var hdRes = results[0];
            var ctRes = results[1];

            var rawHD = hdRes.data || hdRes;
            vm.hoaDonHienTai = ApiService.chuanHoaDoiTuong(rawHD);
            vm.chiTietHienTai = ApiService.layMang(ctRes).map(ApiService.chuanHoaDoiTuong);

            if (!vm.hoaDonHienTai || !vm.hoaDonHienTai.MAHDBAN) {
              vm.loi = 'Không tìm thấy hóa đơn: ' + ma + '. Vui lòng kiểm tra lại mã.';
            }
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Lỗi tra cứu hóa đơn.';
            ToastService.error('Lỗi', vm.loi);
          })
          .finally(function () {
            vm.dangTai = false;
          });
      };

      vm.tinhTongTienHangMoi = function () {
        return vm.chiTietHienTai.reduce(function (sum, it) {
          var tt = Number(it.TONGTIEN) || (Number(it.SOLUONG) * Number(it.DONGIA));
          return sum + (Number(tt) || 0);
        }, 0);
      };

      vm.tinhTongPhaiTra = function () {
        if (!vm.hoaDonHienTai) return 0;
        var tongTien = vm.tinhTongTienHangMoi();
        var vat = Number(vm.hoaDonHienTai.THUEVAT) || 0;
        var giamGia = Number(vm.hoaDonHienTai.GIAMGIA) || 0;
        return Math.max(0, tongTien + vat - giamGia);
      };

      vm.giamSoLuong = function (index) {
        var item = vm.chiTietHienTai[index];
        if (!item) return;

        if (item.SOLUONG <= 1) {
          vm.xoaItem(index);
          return;
        }

        var soLuongMoi = item.SOLUONG - 1;
        var dongia = Number(item.DONGIA) || 0;
        var body = {
          MAHDBAN: vm.hoaDonHienTai.MAHDBAN,
          MASP: item.MASP,
          TenSP: item.TenSP || item.TENSP,
          SOLUONG: soLuongMoi,
          DONGIA: dongia,
          TONGTIEN: soLuongMoi * dongia
        };

        ApiService.put(BASE + '/update-chitietban', body)
          .then(function () {
            item.SOLUONG = soLuongMoi;
            item.TONGTIEN = soLuongMoi * dongia;
            ToastService.success('Thành công', 'Đã giảm số lượng sản phẩm ' + item.MASP + ', còn lại: ' + soLuongMoi);
          })
          .catch(function (err) {
            ToastService.error('Lỗi cập nhật', typeof err === 'string' ? err : 'Không thể giảm số lượng');
          });
      };

      vm.xoaItem = function (index) {
        var item = vm.chiTietHienTai[index];
        if (!item) return;

        ModalService.xacNhan(
          'Trả toàn bộ sản phẩm',
          'Xác nhận trả toàn bộ sản phẩm <strong>' + (item.TenSP || item.MASP) + '</strong> khỏi hóa đơn?',
          'Trả toàn bộ'
        )
          .then(function () {
            return ApiService.delete(BASE + '/delete-chitietban', {
              maHDB: vm.hoaDonHienTai.MAHDBAN,
              maSP: item.MASP
            });
          })
          .then(function () {
            vm.chiTietHienTai.splice(index, 1);
            ToastService.success('Thành công', 'Đã xóa sản phẩm khỏi hóa đơn');
          })
          .catch(function (err) {
            if (err !== false) {
              ToastService.error('Lỗi khi xóa', typeof err === 'string' ? err : 'Không thể xóa sản phẩm');
            }
          });
      };

      vm.moModalDoiSP = function () {
        vm.modalDoiSP.data = {
          masp: '',
          soluong: 1,
          dongia: 0
        };
        vm.modalDoiSP.hienThi = true;
      };

      vm.dongModalDoiSP = function () {
        vm.modalDoiSP.hienThi = false;
      };

      vm.thayDoiSanPhamDoi = function () {
        var masp = vm.modalDoiSP.data.masp;
        var sp = vm.danhSachSP.find(function (s) { return s.MASP === masp; });
        if (sp) {
          vm.modalDoiSP.data.dongia = Number(sp.DONGIA) || 0;
        }
      };

      vm.xacNhanDoiSP = function () {
        var masp = (vm.modalDoiSP.data.masp || '').trim();
        var sl = parseInt(vm.modalDoiSP.data.soluong, 10) || 1;
        var gia = parseFloat(vm.modalDoiSP.data.dongia) || 0;

        if (!masp) {
          ToastService.warning('Cảnh báo', 'Vui lòng chọn sản phẩm thay thế.');
          return;
        }
        if (sl < 1) {
          ToastService.warning('Cảnh báo', 'Số lượng phải >= 1.');
          return;
        }

        var sp = vm.danhSachSP.find(function (s) { return s.MASP === masp; });
        var body = {
          MAHDBAN: vm.hoaDonHienTai.MAHDBAN,
          MASP: masp,
          TenSP: sp ? (sp.TENSP || masp) : masp,
          SOLUONG: sl,
          DONGIA: gia,
          TONGTIEN: sl * gia
        };

        vm.modalDoiSP.dangLuu = true;
        ApiService.post(BASE + '/insert-chitietban', body)
          .then(function () {
            vm.chiTietHienTai.push(body);
            ToastService.success('Thành công', 'Đã thêm sản phẩm đổi vào hóa đơn.');
            vm.dongModalDoiSP();
          })
          .catch(function (err) {
            ToastService.error('Lỗi thêm sản phẩm', typeof err === 'string' ? err : 'Không thể thêm sản phẩm');
          })
          .finally(function () {
            vm.modalDoiSP.dangLuu = false;
          });
      };

      vm.capNhatTongTienHangHoaDon = function () {
        if (!vm.hoaDonHienTai) return;
        var tongTienMoi = vm.tinhTongTienHangMoi();

        ApiService.post(
          BASE + '/reset-tongtienhang-by-mahdban',
          null,
          {
            maHDBan: vm.hoaDonHienTai.MAHDBAN,
            tongTienMoi: tongTienMoi
          }
        )
          .then(function () {
            vm.hoaDonHienTai.TONGTIENHANG = tongTienMoi;
            ToastService.success('Thành công', 'Đã cập nhật lại tổng tiền hóa đơn thành công.');
          })
          .catch(function (err) {
            ToastService.error('Lỗi cập nhật tổng tiền', typeof err === 'string' ? err : 'Lỗi đồng bộ');
          });
      };

      vm.khoiTao();
    }
  ]);
})();
