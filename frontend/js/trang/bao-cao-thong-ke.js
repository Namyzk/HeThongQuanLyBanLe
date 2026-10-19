/**
 * bao-cao-thong-ke.js — AngularJS Controller cho Báo cáo & Thống kê kinh doanh
 * Role: Admin, KeToan
 * Base API: /api/ketoan/BaoCaoThongKe
 */
(function () {
  'use strict';

  angular.module('qlblApp').controller('BaoCaoController', [
    '$q',
    'ApiService',
    'AuthService',
    'ToastService',
    'SidebarHeaderService',
    function ($q, ApiService, AuthService, ToastService, SidebarHeaderService) {
      var vm = this;

      var BASE = ApiService.URL.ketoan + '/BaoCaoThongKe';

      vm.hoaDon = [];
      vm.chiTietBan = [];
      vm.phieuNhap = [];
      vm.chiTietNhap = [];
      vm.sanPham = [];
      vm.danhMuc = [];

      vm.tuNgay = '';
      vm.denNgay = '';
      vm.dangTai = false;
      vm.loi = '';

      vm.kpi = {
        tongDoanhThu: 0,
        soDonHang: 0,
        tongChiPhiNhap: 0,
        soPhieuNhap: 0,
        loiNhuanGop: 0,
        giaTriTrungBinhDon: 0
      };

      vm.topSPBanChay = [];
      vm.topSPBanCham = [];
      vm.tatCaSPThongKe = [];

      vm.khoiTao = function () {
        if (!AuthService.kiemTraQuyen(['Admin', 'KeToan'])) return;
        SidebarHeaderService.khoiTao('bao-cao-thong-ke', 'Báo cáo & Thống kê', 'Trang chủ / Báo cáo & Thống kê');
        vm.taiDuLieu();
      };

      vm.taiDuLieu = function () {
        vm.dangTai = true;
        vm.loi = '';

        $q.all([
          ApiService.get(BASE + '/get-all-hoadonban').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-chitietban').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-phieunhapkho').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-chitietnhap').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-sanpham').catch(function () { return { data: [] }; }),
          ApiService.get(BASE + '/get-all-danhmuc').catch(function () { return { data: [] }; })
        ])
          .then(function (results) {
            var rawHD = ApiService.layMang(results[0]);
            var rawCTB = ApiService.layMang(results[1]);
            var rawPN = ApiService.layMang(results[2]);
            var rawCTN = ApiService.layMang(results[3]);
            var rawSP = ApiService.layMang(results[4]);
            var rawDM = ApiService.layMang(results[5]);

            vm.danhMuc = rawDM.map(function (it) {
              return {
                MADANHMUC: String(ApiService.layThuocTinh(it, 'MADANHMUC') || '').trim(),
                TENDANHMUC: String(ApiService.layThuocTinh(it, 'TENDANHMUC') || '').trim()
              };
            });

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

            vm.chiTietBan = rawCTB.map(function (it) {
              var sl = Number(ApiService.layThuocTinh(it, 'SOLUONG')) || 0;
              var gia = Number(ApiService.layThuocTinh(it, 'DONGIA')) || 0;
              var tong = Number(ApiService.layThuocTinh(it, 'TONGTIEN')) || (sl * gia);
              return {
                MAHDBAN: String(ApiService.layThuocTinh(it, 'MAHDBAN') || '').trim(),
                MASP: String(ApiService.layThuocTinh(it, 'MASP') || '').trim(),
                TenSP: String(ApiService.layThuocTinh(it, 'TenSP') || ApiService.layThuocTinh(it, 'TENSP') || '').trim(),
                SOLUONG: sl,
                DONGIA: gia,
                TONGTIEN: tong
              };
            });

            vm.phieuNhap = rawPN.map(function (it) {
              var ngay = ApiService.layThuocTinh(it, 'NGAYLAP') || ApiService.layThuocTinh(it, 'NGAYNHAP');
              return {
                MAPHIEUNHAP: String(ApiService.layThuocTinh(it, 'MAPHIEUNHAP') || '').trim(),
                MANCC: String(ApiService.layThuocTinh(it, 'MANCC') || '').trim(),
                MANV: String(ApiService.layThuocTinh(it, 'MANV') || '').trim(),
                NGAYLAP: ngay,
                NGAYNHAP: ngay,
                listjson_chitietnhap: ApiService.layThuocTinh(it, 'listjson_chitietnhap') || []
              };
            });

            vm.chiTietNhap = rawCTN.map(function (it) {
              var sl = Number(ApiService.layThuocTinh(it, 'SOLUONG')) || 0;
              var gia = Number(ApiService.layThuocTinh(it, 'DONGIANHAP')) || 0;
              var tong = Number(ApiService.layThuocTinh(it, 'THANHTIEN')) || (sl * gia);
              return {
                MAPHIEUNHAP: String(ApiService.layThuocTinh(it, 'MAPHIEUNHAP') || '').trim(),
                MASP: String(ApiService.layThuocTinh(it, 'MASP') || '').trim(),
                SOLUONG: sl,
                DONGIANHAP: gia,
                THANHTIEN: tong
              };
            });

            vm.sanPham = rawSP.map(function (it) {
              return {
                MASP: String(ApiService.layThuocTinh(it, 'MASP') || '').trim(),
                TENSP: String(ApiService.layThuocTinh(it, 'TENSP') || '').trim(),
                MADANHMUC: String(ApiService.layThuocTinh(it, 'MADANHMUC') || '').trim(),
                DONGIA: Number(ApiService.layThuocTinh(it, 'DONGIA')) || 0,
                SOLUONGTON: Number(ApiService.layThuocTinh(it, 'SOLUONGTON')) || 0
              };
            });

            vm.tinhToanBaoCao();
          })
          .catch(function (err) {
            vm.loi = typeof err === 'string' ? err : 'Không thể tải dữ liệu báo cáo thống kê';
            ToastService.error('Lỗi', vm.loi);
          })
          .finally(function () {
            vm.dangTai = false;
          });
      };

      vm.tinhToanBaoCao = function () {
        // Lọc hóa đơn bán
        var hdLoc = vm.hoaDon.filter(function (h) {
          var ngayStr = h.NGAYLAP;
          if (!ngayStr) return true;
          var d = String(ngayStr).substring(0, 10);
          if (vm.tuNgay && d < vm.tuNgay) return false;
          if (vm.denNgay && d > vm.denNgay) return false;
          return true;
        });

        // Lọc phiếu nhập kho
        var pnLoc = vm.phieuNhap.filter(function (p) {
          var ngayStr = p.NGAYLAP || p.NGAYNHAP;
          if (!ngayStr) return true;
          var d = String(ngayStr).substring(0, 10);
          if (vm.tuNgay && d < vm.tuNgay) return false;
          if (vm.denNgay && d > vm.denNgay) return false;
          return true;
        });

        // Tính tổng doanh thu
        var tongDoanhThu = hdLoc.reduce(function (acc, hd) {
          var tienHang = Number(hd.TONGTIENHANG) || 0;
          var vat = Number(hd.THUEVAT) || 0;
          var giam = Number(hd.GIAMGIA) || 0;
          return acc + Math.max(0, tienHang + vat - giam);
        }, 0);

        // Tính tổng chi phí nhập
        var tongChiPhiNhap = pnLoc.reduce(function (acc, pn) {
          var ct = pn.listjson_chitietnhap && pn.listjson_chitietnhap.length
            ? pn.listjson_chitietnhap
            : vm.chiTietNhap.filter(function (c) { return c.MAPHIEUNHAP === pn.MAPHIEUNHAP; });
          var sum = ct.reduce(function (s, it) {
            return s + (Number(it.THANHTIEN) || (Number(it.SOLUONG) * Number(it.DONGIANHAP)) || 0);
          }, 0);
          return acc + sum;
        }, 0);

        var loiNhuanGop = tongDoanhThu - tongChiPhiNhap;
        var giaTriTrungBinhDon = hdLoc.length > 0 ? Math.round(tongDoanhThu / hdLoc.length) : 0;

        vm.kpi = {
          tongDoanhThu: tongDoanhThu,
          soDonHang: hdLoc.length,
          tongChiPhiNhap: tongChiPhiNhap,
          soPhieuNhap: pnLoc.length,
          loiNhuanGop: loiNhuanGop,
          giaTriTrungBinhDon: giaTriTrungBinhDon
        };

        // Phân tích sản phẩm
        var dmMap = {};
        vm.danhMuc.forEach(function (dm) {
          if (dm.MADANHMUC) dmMap[dm.MADANHMUC] = dm.TENDANHMUC;
        });

        var setMaHD = new Set(hdLoc.map(function (h) { return h.MAHDBAN; }));
        var spBanMap = {};

        vm.sanPham.forEach(function (sp) {
          spBanMap[sp.MASP] = {
            maSP: sp.MASP,
            tenSP: sp.TENSP || sp.MASP,
            danhMuc: dmMap[sp.MADANHMUC] || sp.MADANHMUC || '—',
            giaBan: Number(sp.DONGIA) || 0,
            soLuongTon: Number(sp.SOLUONGTON) || 0,
            soLuongBan: 0,
            doanhThu: 0
          };
        });

        vm.chiTietBan.forEach(function (ct) {
          if (setMaHD.has(ct.MAHDBAN)) {
            var ma = ct.MASP;
            var sl = Number(ct.SOLUONG) || 0;
            var tien = Number(ct.TONGTIEN) || (sl * (Number(ct.DONGIA) || 0));

            if (!spBanMap[ma]) {
              spBanMap[ma] = {
                maSP: ma,
                tenSP: ct.TENSP || ct.TenSP || ma,
                danhMuc: '—',
                giaBan: Number(ct.DONGIA) || 0,
                soLuongTon: 0,
                soLuongBan: 0,
                doanhThu: 0
              };
            }
            spBanMap[ma].soLuongBan += sl;
            spBanMap[ma].doanhThu += tien;
          }
        });

        vm.tatCaSPThongKe = Object.values(spBanMap);

        vm.topSPBanChay = vm.tatCaSPThongKe
          .filter(function (s) { return s.soLuongBan > 0; })
          .sort(function (a, b) { return (b.soLuongBan - a.soLuongBan) || (b.doanhThu - a.doanhThu); })
          .slice(0, 5);

        vm.topSPBanCham = vm.tatCaSPThongKe
          .slice()
          .sort(function (a, b) { return (a.soLuongBan - b.soLuongBan) || (a.doanhThu - b.doanhThu); })
          .slice(0, 5);
      };

      vm.locDuLieu = function () {
        vm.tinhToanBaoCao();
      };

      vm.xoaLoc = function () {
        vm.tuNgay = '';
        vm.denNgay = '';
        vm.tinhToanBaoCao();
      };

      vm.xuatBaoCaoCSV = function () {
        if (!vm.tatCaSPThongKe.length && vm.kpi.soDonHang === 0) {
          ToastService.warning('Cảnh báo', 'Không có dữ liệu báo cáo trong khoảng thời gian đã chọn để xuất.');
          return;
        }

        var thoiGianStr = (vm.tuNgay || vm.denNgay)
          ? ('Tu_' + (vm.tuNgay || 'dau') + '_den_' + (vm.denNgay || 'nay'))
          : 'Tat_ca_thoi_gian';

        var csvContent = '\uFEFF';
        csvContent += '"BÁO CÁO THỐNG KÊ KINH DOANH VÀ SẢN PHẨM"\n';
        csvContent += '"Thời gian áp dụng:","' + (vm.tuNgay ? 'Từ ngày ' + vm.tuNgay : 'Từ trước tới nay') + (vm.denNgay ? ' đến ngày ' + vm.denNgay : '') + '"\n';
        csvContent += '"Thời điểm xuất:","' + new Date().toLocaleString('vi-VN') + '"\n\n';

        csvContent += '"CHỈ SỐ TỔNG QUAN"\n';
        csvContent += '"Chỉ số","Giá trị"\n';
        csvContent += '"Số lượng hóa đơn bán","' + vm.kpi.soDonHang + '"\n';
        csvContent += '"Tổng doanh thu bán hàng","' + vm.kpi.tongDoanhThu + '"\n';
        csvContent += '"Số lượng phiếu nhập kho","' + vm.kpi.soPhieuNhap + '"\n';
        csvContent += '"Tổng chi phí nhập hàng","' + vm.kpi.tongChiPhiNhap + '"\n';
        csvContent += '"Lợi nhuận gộp ước tính","' + vm.kpi.loiNhuanGop + '"\n\n';

        csvContent += '"CHI TIẾT HIỆU QUẢ KINH DOANH TỪNG SẢN PHẨM"\n';
        csvContent += '"Mã sản phẩm","Tên sản phẩm","Danh mục","Đơn giá bán","Số lượng đã bán","Doanh thu bán","Số lượng tồn kho","Đánh giá"\n';

        vm.tatCaSPThongKe.forEach(function (sp) {
          var danhGia = 'Bình thường';
          if (sp.soLuongBan >= 10) danhGia = 'Bán rất chạy';
          else if (sp.soLuongBan > 0) danhGia = 'Đang bán tốt';
          else danhGia = 'Chưa phát sinh bán';

          var tenAnToan = (sp.tenSP || '').replace(/"/g, '""');
          var maAnToan = (sp.maSP || '').replace(/"/g, '""');
          var dmAnToan = (sp.danhMuc || '').replace(/"/g, '""');

          csvContent += '"' + maAnToan + '","' + tenAnToan + '","' + dmAnToan + '",' + sp.giaBan + ',' + sp.soLuongBan + ',' + sp.doanhThu + ',' + sp.soLuongTon + ',"' + danhGia + '"\n';
        });

        var blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = 'Bao_cao_thong_ke_' + thoiGianStr + '.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);

        ToastService.success('Thành công', 'Đã xuất file Bao_cao_thong_ke_' + thoiGianStr + '.csv');
      };

      vm.khoiTao();
    }
  ]);
})();
