/**
 * quan-ly-hoa-don.js — AngularJS Controller cho Quản lý & In Hóa đơn bán hàng
 * Role: Admin, ThuNgan, KeToan
 * Base API: /api/thungan/QuanLyBanHang và /api/thungan/QuanLyDoiTra
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('HoaDonController', [
    '$q',
    'ApiService',
    'AuthService',
    'ToastService',
    'ModalService',
    'SidebarHeaderService',
    '$window',
    function ($q, ApiService, AuthService, ToastService, ModalService, SidebarHeaderService, $window) {
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

      var spThueMap = {};

      function layGiaTriSo(val) {
        var n = Number(val);
        if (isNaN(n) || Math.abs(n) < 0.001 || Object.is(n, -0)) return 0;
        return n;
      }

      function chuanHoaChiTietBan(ct) {
        var obj = ApiService.chuanHoaDoiTuong(ct || {});
        var maSP = String(obj.MASP || '').trim();
        var sl = layGiaTriSo(obj.SOLUONG) || 1;
        var donGia = layGiaTriSo(obj.DONGIA);
        var tienHangSP = layGiaTriSo(obj.TONGTIEN) || (sl * donGia);
        var thuePhanTram = spThueMap[maSP] !== undefined ? spThueMap[maSP] : 10;
        var tienThueSP = Math.round(tienHangSP * thuePhanTram / 100);
        return {
          ...obj,
          MASP: maSP,
          SOLUONG: sl,
          DONGIA: donGia,
          TONGTIEN: tienHangSP,
          THUE: thuePhanTram,
          TIENTHUE: tienThueSP,
          THANHTIEN_SAU_THUE: tienHangSP + tienThueSP
        };
      }

      function chuanHoaHoaDon(hd) {
        if (!hd) return {};
        var rawCT = ApiService.layMang(ApiService.layThuocTinh(hd, 'listjson_chitietban'));
        var dsChiTiet = rawCT.map(chuanHoaChiTietBan);

        var tienHang = layGiaTriSo(ApiService.layThuocTinh(hd, 'TONGTIENHANG'));
        var thue = layGiaTriSo(ApiService.layThuocTinh(hd, 'THUEVAT'));
        var giam = layGiaTriSo(ApiService.layThuocTinh(hd, 'GIAMGIA'));

        if (dsChiTiet.length > 0) {
          var tongTienTheoSP = dsChiTiet.reduce(function (s, it) { return s + it.TONGTIEN; }, 0);
          var tongThueTheoSP = dsChiTiet.reduce(function (s, it) { return s + it.TIENTHUE; }, 0);
          if (tongTienTheoSP > 0) tienHang = tongTienTheoSP;
          if (thue <= 0 && tongThueTheoSP > 0) thue = tongThueTheoSP;
        }

        return {
          ...hd,
          MAHDBAN: ApiService.layThuocTinh(hd, 'MAHDBAN'),
          MANV: ApiService.layThuocTinh(hd, 'MANV'),
          MAKH: ApiService.layThuocTinh(hd, 'MAKH'),
          NGAYLAP: ApiService.layThuocTinh(hd, 'NGAYLAP'),
          TONGTIENHANG: tienHang,
          THUEVAT: thue,
          TONG_TIEN_SAU_THUE: tienHang + thue,
          GIAMGIA: giam,
          TONG_THANH_TOAN: Math.max(0, tienHang + thue - giam),
          listjson_chitietban: dsChiTiet
        };
      }

      function layCacDangNgay(ngayGoc) {
        if (!ngayGoc) return [];

        var ngay = ngayGoc instanceof Date ? ngayGoc : null;
        var chuoi = String(ngayGoc).trim();
        if (!ngay && !chuoi) return [];

        // Đọc trực tiếp phần ngày của chuỗi SQL dạng yyyy-MM-dd[THH:mm:ss]
        // để không bị lệch ngày do múi giờ khi parse chuỗi ngày thuần.
        var iso = chuoi.match(/^(\d{4})-(\d{1,2})-(\d{1,2})/);
        var viet = chuoi.match(/^(\d{1,2})[\/.-](\d{1,2})[\/.-](\d{4})$/);
        if (!ngay && iso) {
          ngay = new Date(Number(iso[1]), Number(iso[2]) - 1, Number(iso[3]));
        } else if (!ngay && viet) {
          ngay = new Date(Number(viet[3]), Number(viet[2]) - 1, Number(viet[1]));
        } else if (!ngay) {
          ngay = new Date(chuoi);
        }

        if (!ngay || isNaN(ngay.getTime())) return [chuoi.toLowerCase()];

        var d = ngay.getDate();
        var m = ngay.getMonth() + 1;
        var y = ngay.getFullYear();
        var dd = ('0' + d).slice(-2);
        var mm = ('0' + m).slice(-2);

        return [
          dd + '/' + mm + '/' + y,
          d + '/' + m + '/' + y,
          dd + '-' + mm + '-' + y,
          d + '-' + m + '-' + y,
          y + '-' + mm + '-' + dd,
          y + '-' + m + '-' + d,
          mm + '/' + y,
          m + '/' + y,
          y + '-' + mm,
          y + '-' + m,
          String(y),
          chuoi.toLowerCase()
        ];
      }

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan', 'KeToan'])) return;
        SidebarHeaderService.khoiTao('quan-ly-hoa-don', 'Quản lý Hóa đơn', 'Trang chủ / Quản lý Hóa đơn');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';
        var pSP = ApiService.get(ApiService.URL.thukho + '/QuanLySanPham/get-all-sanpham')
          .catch(function () {
            return ApiService.get(ApiService.URL.ketoan + '/BaoCaoThongKe/get-all-sanpham')
              .catch(function () { return { data: [] }; });
          });
        var pHD = ApiService.get(BASE_BH + '/get-all-hoadonban');

        $q.all([pSP, pHD])
          .then(function (results) {
            var rawSP = ApiService.layMang(results[0]);
            spThueMap = {};
            rawSP.forEach(function (sp) {
              var ma = String(ApiService.layThuocTinh(sp, 'MASP') || '').trim();
              var thueVal = ApiService.layThuocTinh(sp, 'THUE');
              if (thueVal === undefined || thueVal === null) thueVal = ApiService.layThuocTinh(sp, 'THUEVAT');
              var thueNum = (thueVal !== undefined && thueVal !== null && thueVal !== '') ? Number(thueVal) : 10;
              if (ma) spThueMap[ma] = isNaN(thueNum) ? 10 : thueNum;
            });

            var rawList = ApiService.layMang(results[1]);
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
        // Chuỗi chỉ gồm số/ngày được hiểu là tìm theo ngày lập. Nhờ vậy,
        // nhập "06" không trả thêm hóa đơn ngày khác chỉ vì mã nhân viên
        // hoặc mã khách hàng có chứa số 06.
        var timTheoNgay = /^\d{1,4}(?:[\/.-]\d{1,2}(?:[\/.-]\d{2,4})?)?$/.test(tk);
        return vm.danhSach.filter(function (item) {
          var ngayKhacNhau = layCacDangNgay(item.NGAYLAP);
          var khopNgay = ngayKhacNhau.some(function (dangNgay) {
            return dangNgay.indexOf(tk) !== -1;
          });
          if (timTheoNgay) return khopNgay;

          var maHD = (item.MAHDBAN || '').toLowerCase();
          var maNV = (item.MANV || '').toLowerCase();
          var maKH = (item.MAKH || '').toLowerCase();
          return maHD.indexOf(tk) !== -1 || maNV.indexOf(tk) !== -1 || maKH.indexOf(tk) !== -1 || khopNgay;
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

        $q.all([pHD, pCT])
          .then(function (results) {
            var hdRes = results[0];
            var ctRes = results[1];
            var hoaDonData = ApiService.layThuocTinh(hdRes, 'data') || row;
            if (Array.isArray(hoaDonData)) hoaDonData = hoaDonData[0] || row;

            var chiTiet = ApiService.layMang(ctRes);
            if (!chiTiet.length) {
              // Một số API trả chi tiết bán lồng trong hóa đơn; dùng dữ liệu này
              // khi endpoint chi tiết riêng không trả danh sách.
              chiTiet = ApiService.layMang(
                ApiService.layThuocTinh(hoaDonData, 'listjson_chitietban') ||
                ApiService.layThuocTinh(row, 'listjson_chitietban')
              );
            }

            var dsChiTiet = chiTiet.map(chuanHoaChiTietBan);
            var hdChuan = chuanHoaHoaDon(hoaDonData);
            if (dsChiTiet.length > 0) {
              var tongTienSP = dsChiTiet.reduce(function (s, it) { return s + it.TONGTIEN; }, 0);
              var tongThueSP = dsChiTiet.reduce(function (s, it) { return s + it.TIENTHUE; }, 0);
              if (tongTienSP > 0) hdChuan.TONGTIENHANG = tongTienSP;
              if (hdChuan.THUEVAT <= 0 && tongThueSP > 0) hdChuan.THUEVAT = tongThueSP;
              hdChuan.TONG_TIEN_SAU_THUE = hdChuan.TONGTIENHANG + hdChuan.THUEVAT;
              hdChuan.TONG_THANH_TOAN = Math.max(0, hdChuan.TONGTIENHANG + hdChuan.THUEVAT - hdChuan.GIAMGIA);
            }
            vm.modalChiTiet.hoaDon = hdChuan;
            vm.modalChiTiet.chiTiet = dsChiTiet;
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
        var vung = document.getElementById('vung-xem-in-hd');
        if (!vung) return;
        var w = $window.open('', '_blank');
        if (!w) return;
        w.document.write('<!doctype html><html lang="vi"><head><meta charset="utf-8"><title>Hóa đơn</title><style>@page{size:A4 portrait;margin:8mm}*{box-sizing:border-box}body{font:11px/1.3 Arial,sans-serif;color:#111;margin:0 auto;width:180mm}#print{transform-origin:top left;width:100%}table{width:100%;border-collapse:collapse}th,td{padding:3px 5px;border-bottom:1px solid #bbb;text-align:left}header{text-align:center}.hoa-don-chi-tiet-hop{display:flex;flex-direction:column;gap:8px}.hoa-don-header-in{border-bottom:1px dashed #888;padding-bottom:7px}.hoa-don-ten-cua-hang{font-size:18px;margin:0}.hoa-don-phu-de{margin:3px}.hoa-don-tieu-de-phieu{font-size:15px;margin:8px 0}.hoa-don-thong-tin-chung{display:grid;grid-template-columns:1fr 1fr;gap:5px;padding:8px;background:#f7f7f7}.hoa-don-tong-ket{display:flex;flex-direction:column;gap:5px;padding:8px;background:#f7f7f7}.hoa-don-tong-dong{display:flex;justify-content:space-between}.hoa-don-tong-dong.tong-cong{font-weight:bold;border-top:1px dashed #555;padding-top:6px}.hoa-don-bang-sp-hop,.bang-khung{max-height:none!important;overflow:visible!important}tr,header,.hoa-don-thong-tin-chung,.hoa-don-tong-ket{break-inside:avoid;page-break-inside:avoid}</style></head><body><main id="print">'+vung.innerHTML+'</main></body></html>');
        w.document.close();
        setTimeout(function () {
          var node=w.document.getElementById('print'), available=1040, h=node.scrollHeight;
          if(h>available){var scale=available/h;node.style.zoom=scale;}
          w.focus();w.print();w.close();
        },300);
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
            return ApiService.delete(BASE_DT + '/delete-hoadonban', { maHoaDon: maHD });
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
