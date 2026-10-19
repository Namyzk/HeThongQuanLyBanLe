/**
 * quan-ly-cong-no.js — AngularJS Controller cho Quản lý Công nợ khách hàng
 * Role: Admin, KeToan
 * Base API: /api/ketoan/QuanLyCongNo
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('CongNoController', [
    '$q',
    'ApiService',
    'AuthService',
    'ToastService',
    'SidebarHeaderService',
    function ($q, ApiService, AuthService, ToastService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.ketoan + '/QuanLyCongNo';

      vm.hoaDon = [];
      vm.thanhToan = [];
      vm.khachHang = [];
      vm.duLieuBang = [];

      vm.tuKhoa = '';
      vm.tabLoc = 'tat-ca'; // 'tat-ca' | 'con-no' | 'da-tra'
      vm.dangTai = false;
      vm.loi = '';
      vm.trangHienTai = 1;
      vm.soDongMoiTrang = 20;

      vm.modalThuNo = {
        hienThi: false,
        dangLuu: false,
        maHDBan: '',
        tenKH: '',
        soTienNo: 0,
        phuongThuc: 'Tiền mặt'
      };

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'KeToan'])) return;
        SidebarHeaderService.khoiTao('quan-ly-cong-no', 'Quản lý Công nợ', 'Trang chủ / Quản lý Công nợ');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';

        $q.all([
          ApiService.get(BASE + '/get-all-hoadonban').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-thanhtoan').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-khachhang').catch(function () { return { data: [] }; })
        ])
          .then(function (results) {
            var rawHD = ApiService.layMang(results[0]);
            var rawTT = ApiService.layMang(results[1]);
            var rawKH = ApiService.layMang(results[2]);

            vm.hoaDon = rawHD.map(function (it) {
              return {
                MAHDBAN: String(ApiService.layThuocTinh(it, 'MAHDBAN') || '').trim(),
                MANV: String(ApiService.layThuocTinh(it, 'MANV') || '').trim(),
                MAKH: String(ApiService.layThuocTinh(it, 'MAKH') || '').trim(),
                NGAYLAP: ApiService.layThuocTinh(it, 'NGAYLAP'),
                TONGTIENHANG: Number(ApiService.layThuocTinh(it, 'TONGTIENHANG')) || 0,
                THUEVAT: Number(ApiService.layThuocTinh(it, 'THUEVAT')) || 0,
                GIAMGIA: Number(ApiService.layThuocTinh(it, 'GIAMGIA')) || 0,
                listjson_chitietban: ApiService.layThuocTinh(it, 'listjson_chitietban') || []
              };
            });

            vm.thanhToan = rawTT.map(function (it) {
              return {
                MATHANHTOAN: String(ApiService.layThuocTinh(it, 'MATHANHTOAN') || ApiService.layThuocTinh(it, 'MaThanhToan') || '').trim(),
                MAHDBAN: String(ApiService.layThuocTinh(it, 'MAHDBAN') || ApiService.layThuocTinh(it, 'MaHDBan') || '').trim(),
                PHUONGTHUC: String(ApiService.layThuocTinh(it, 'PHUONGTHUC') || ApiService.layThuocTinh(it, 'PhuongThuc') || '').trim(),
                SOTIENTHANHTOAN: Number(ApiService.layThuocTinh(it, 'SOTIENTHANHTOAN') || ApiService.layThuocTinh(it, 'SoTienThanhToan')) || 0,
                NGAYTHANHTOAN: ApiService.layThuocTinh(it, 'NGAYTHANHTOAN') || ApiService.layThuocTinh(it, 'NgayThanhToan') || null,
                TRANGTHAI: String(ApiService.layThuocTinh(it, 'TRANGTHAI') || ApiService.layThuocTinh(it, 'TrangThai') || '').trim()
              };
            });

            vm.khachHang = rawKH.map(function (it) {
              return {
                MAKH: String(ApiService.layThuocTinh(it, 'MAKH') || ApiService.layThuocTinh(it, 'MaKH') || '').trim(),
                TENKH: String(ApiService.layThuocTinh(it, 'TENKH') || ApiService.layThuocTinh(it, 'TenKH') || '').trim(),
                SDT: String(ApiService.layThuocTinh(it, 'SDT') || '').trim(),
                DIACHI: String(ApiService.layThuocTinh(it, 'DIACHI') || ApiService.layThuocTinh(it, 'DiaChi') || '').trim()
              };
            });

            vm.tinhToanCongNo();
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải dữ liệu công nợ';
            ToastService.error('Lỗi', vm.loi);
          })
          .finally(function () {
            vm.dangTai = false;
          });
      };

      vm.tinhToanCongNo = function () {
        var khMap = {};
        vm.khachHang.forEach(function (k) {
          if (k.MAKH) khMap[k.MAKH.toUpperCase()] = k.TENKH;
        });

        vm.duLieuBang = vm.hoaDon.map(function (hd) {
          var tienHang = Number(hd.TONGTIENHANG) || 0;
          var vat = Number(hd.THUEVAT) || 0;
          var giam = Number(hd.GIAMGIA) || 0;
          var phaiTra = Math.max(0, tienHang + vat - giam);

          var maHDUpper = String(hd.MAHDBAN || '').trim().toUpperCase();
          var dsTT = vm.thanhToan.filter(function (t) {
            return String(t.MAHDBAN || '').trim().toUpperCase() === maHDUpper;
          });

          var daTra = dsTT
            .filter(function (t) {
              var st = String(t.TRANGTHAI || '').toLowerCase();
              return st.indexOf('đã thanh toán') !== -1 || st.indexOf('thành công') !== -1 || st === 'paid' || st === 'completed';
            })
            .reduce(function (sum, t) { return sum + (Number(t.SOTIENTHANHTOAN) || 0); }, 0);

          var conNo = Math.max(0, phaiTra - daTra);
          var trangThai = conNo <= 0 ? 'Đã thu đủ' : (daTra > 0 ? 'Thanh toán 1 phần' : 'Chưa thu');

          var maKHUpper = String(hd.MAKH || '').trim().toUpperCase();
          var tenKH = khMap[maKHUpper] ? (khMap[maKHUpper] + ' (' + hd.MAKH + ')') : (hd.MAKH || 'Khách vãng lai');

          return {
            MAHDBAN: hd.MAHDBAN,
            MAKH: tenKH,
            NGAYLAP: hd.NGAYLAP,
            TONG_PHAI_TRA: phaiTra,
            DA_THANH_TOAN: daTra,
            CON_NO: conNo,
            TRANG_THAI: trangThai,
            _raw: hd,
            _dsTT: dsTT
          };
        });
      };

      vm.tongNoPhaiThu = function () {
        return vm.duLieuBang.reduce(function (sum, it) { return sum + it.CON_NO; }, 0);
      };

      vm.soDonConNo = function () {
        return vm.duLieuBang.filter(function (it) { return it.CON_NO > 0; }).length;
      };

      vm.soDonDaThu = function () {
        return vm.duLieuBang.filter(function (it) { return it.CON_NO <= 0; }).length;
      };

      vm.layDanhSachLoc = function () {
        var ds = vm.duLieuBang.filter(function (item) {
          if (vm.tabLoc === 'con-no') return item.CON_NO > 0;
          if (vm.tabLoc === 'da-tra') return item.CON_NO <= 0;
          return true;
        });

        if (!vm.tuKhoa || !vm.tuKhoa.trim()) {
          return ds;
        }

        var tk = vm.tuKhoa.trim().toLowerCase();
        return ds.filter(function (item) {
          var mahd = (item.MAHDBAN || '').toLowerCase();
          var makh = (item.MAKH || '').toLowerCase();
          var tt = (item.TRANG_THAI || '').toLowerCase();
          return mahd.indexOf(tk) !== -1 || makh.indexOf(tk) !== -1 || tt.indexOf(tk) !== -1;
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

      vm.doiTab = function (tab) {
        vm.tabLoc = tab;
        vm.trangHienTai = 1;
      };

      vm.moModalThuNo = function (row) {
        vm.modalThuNo.maHDBan = row.MAHDBAN;
        vm.modalThuNo.tenKH = row.MAKH || '';
        vm.modalThuNo.soTienNo = row.CON_NO;
        vm.modalThuNo.phuongThuc = 'Tiền mặt';
        vm.modalThuNo.hienThi = true;
      };

      vm.dongModalThuNo = function () {
        vm.modalThuNo.hienThi = false;
      };

      vm.xacNhanThuNo = function () {
        vm.modalThuNo.dangLuu = true;

        ApiService.put(
          BASE + '/update-trangthai-thanhtoan?maHDBan=' + encodeURIComponent(vm.modalThuNo.maHDBan) + '&phuongThuc=' + encodeURIComponent(vm.modalThuNo.phuongThuc),
          null
        )
          .then(function () {
            ToastService.success('Thành công', 'Đã xác nhận thu nợ và cập nhật trạng thái hóa đơn ' + vm.modalThuNo.maHDBan);
            vm.dongModalThuNo();
            vm.taiDuLieu();
          })
          .catch(function (err) {
            ToastService.error('Lỗi', typeof err === 'string' ? err : 'Không thể cập nhật thu nợ');
          })
          .finally(function () {
            vm.modalThuNo.dangLuu = false;
          });
      };

      vm.khoiTao();
    }
  ]);
})();
