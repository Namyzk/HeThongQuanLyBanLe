/**
 * ban-hang.controller.js — AngularJS Controller cho màn hình Bán hàng (POS)
 * Xử lý: Lưới sản phẩm compact chuẩn ảnh 3, giỏ hàng, thanh toán tiền mặt & PayOS QR chuẩn ảnh 2
 * Không sửa API backend.
 */
(function () {
  'use strict';

  angular.module('qlblApp')
    .controller('BanHangController', [
      'ApiService',
      'AuthService',
      'SidebarHeaderService',
      'ToastService',
      'ModalService',
      '$q',
      '$scope',
      '$window',
      '$interval',
      BanHangController
    ]);

  function BanHangController(ApiService, AuthService, SidebarHeaderService, ToastService, ModalService, $q, $scope, $window, $interval) {
    var vm = this;

    // Phân quyền: Chỉ Quản trị viên và Thu ngân
    if (!AuthService.kiemTraQuyen(['Admin', 'ThuNgan'])) return;

    // Dữ liệu danh mục và hàng hóa
    vm.danhSachSP = [];
    vm.danhSachDM = [];
    vm.danhSachKH = [];
    vm.danhSachNV = [];
    vm.dangTai = false;

    // Tìm kiếm và lọc
    vm.tuKhoa = '';
    vm.danhMucLoc = '';

    // Giỏ hàng & Hóa đơn
    vm.gioHang = [];
    vm.khachHangChon = 'KH_LE';
    vm.nhanVienChon = '';
    vm.maHoaDonHienTai = '';
    vm.vatPhanTram = 0;
    vm.giamGia = 0;
    vm.ptThanhToan = 'Tiền mặt';
    vm.tienKhachDua = 0;
    vm.dangThanhToan = false;

    // Modal hóa đơn sau thanh toán
    vm.modalHoaDon = {
      hienThi: false,
      duLieu: null
    };

    // Modal xem nhanh sản phẩm
    vm.modalXemNhanh = {
      hienThi: false,
      sp: null
    };

    // Modal PayOS QR theo đúng ảnh 2
    vm.modalPayOs = {
      hienThi: false,
      maHD: '',
      ngayLap: null,
      tenNV: '',
      tenKH: '',
      chiTiet: [],
      tienHang: 0,
      tienVAT: 0,
      giamGia: 0,
      tongCong: 0,
      orderCode: null,
      checkoutUrl: '',
      qrCode: '',
      qrImageDataUrl: '',
      qrImageUrl: '',
      trangThai: 'Chưa thanh toán',
      dangKiemTra: false
    };

    var timerPayOs = null;

    // Bảng màu avatar
    var mauAvatarList = [
      '#4f46e5', '#0284c7', '#0d9488', '#16a34a',
      '#d97706', '#dc2626', '#9333ea', '#e11d48'
    ];

    vm.$onInit = function () {
      SidebarHeaderService.khoiTao('ban-hang', 'Bán hàng (POS)', 'Trang chủ / Bán hàng');
      vm.taoMaHoaDonMoi();
      vm.taiDuLieu();

      // Đăng ký phím tắt F9 để thanh toán
      var xuLyPhimTat = function (e) {
        if (e.key === 'F9') {
          e.preventDefault();
          $scope.$apply(function () {
            if (!vm.dangThanhToan && vm.gioHang.length > 0) {
              vm.thanhToan();
            }
          });
        }
      };

      $window.addEventListener('keydown', xuLyPhimTat);
      $scope.$on('$destroy', function () {
        $window.removeEventListener('keydown', xuLyPhimTat);
        vm.dungPollingPayOs();
      });
    };

    // Tạo mã hóa đơn mới (tối đa 15 ký tự: HD + 7 số)
    vm.taoMaHoaDonMoi = function () {
      var soNgauNhien = Math.floor(1000000 + Math.random() * 9000000);
      vm.maHoaDonHienTai = 'HD' + soNgauNhien;
    };

    // Tải dữ liệu từ API
    vm.taiDuLieu = function () {
      vm.dangTai = true;

      var pSP = ApiService.get(ApiService.URL.thukho + '/QuanLySanPham/get-all-sanpham');
      var pDM = ApiService.get(ApiService.URL.thukho + '/QuanLyDanhMuc/get-all-danhmuc').catch(function () { return []; });
      var pKH = ApiService.get(ApiService.URL.thungan + '/QuanLyBanHang/get-all-khachhang').catch(function () { return []; });
      var pNV = ApiService.get(ApiService.URL.thungan + '/QuanLyBanHang/get-all-nhanvien-ban-hang').catch(function () { return []; });

      $q.all([pSP, pDM, pKH, pNV]).then(function (results) {
        var rawSP = ApiService.layMang(results[0]);
        var rawDM = ApiService.layMang(results[1]);
        var rawKH = ApiService.layMang(results[2]);
        var rawNV = ApiService.layMang(results[3]);

        // Chuẩn hóa SP
        vm.danhSachSP = rawSP.map(function (s) {
          var ma = String(ApiService.layThuocTinh(s, 'MASP') || '').trim();
          var ten = String(ApiService.layThuocTinh(s, 'TENSP') || '').trim();
          var coAnh = Boolean(ApiService.layThuocTinh(s, 'HINHANH') || ApiService.layThuocTinh(s, 'ANH'));
          return {
            MASP: ma,
            TENSP: ten,
            MADANHMUC: String(ApiService.layThuocTinh(s, 'MADANHMUC') || '').trim(),
            DONGIA: Number(ApiService.layThuocTinh(s, 'DONGIA')) || 0,
            SOLUONGTON: Number(ApiService.layThuocTinh(s, 'SOLUONGTON')) || 0,
            MAVACH: String(ApiService.layThuocTinh(s, 'MAVACH') || '').trim(),
            THUOCTINH: String(ApiService.layThuocTinh(s, 'THUOCTINH') || '').trim(),
            CO_ANH: coAnh
          };
        });

        // Chuẩn hóa DM
        vm.danhSachDM = rawDM.map(function (d) {
          return {
            MADANHMUC: String(ApiService.layThuocTinh(d, 'MADANHMUC') || '').trim(),
            TENDANHMUC: String(ApiService.layThuocTinh(d, 'TENDANHMUC') || '').trim()
          };
        });

        // Chuẩn hóa KH
        vm.danhSachKH = rawKH.map(function (k) {
          return {
            MAKHACHHANG: String(ApiService.layThuocTinh(k, 'MAKHACHHANG') || ApiService.layThuocTinh(k, 'MAKH') || '').trim(),
            TENKHACHHANG: String(ApiService.layThuocTinh(k, 'TENKHACHHANG') || ApiService.layThuocTinh(k, 'TENKH') || '').trim(),
            SDT: String(ApiService.layThuocTinh(k, 'SDT') || '').trim()
          };
        });

        // Chuẩn hóa NV
        vm.danhSachNV = rawNV.map(function (n) {
          return {
            MANV: String(ApiService.layThuocTinh(n, 'MANV') || '').trim(),
            TENNV: String(ApiService.layThuocTinh(n, 'TENNV') || ApiService.layThuocTinh(n, 'HOTEN') || '').trim()
          };
        });

        // Thiết lập nhân viên bán mặc định
        var user = AuthService.layNguoiDung() || {};
        var matchNv = vm.danhSachNV.find(function (n) { return n.MANV === user.userName; });
        if (matchNv) {
          vm.nhanVienChon = matchNv.MANV;
        } else if (vm.danhSachNV.length > 0) {
          vm.nhanVienChon = vm.danhSachNV[0].MANV;
        } else {
          vm.nhanVienChon = user.userName || 'NV001';
        }

      }).catch(function (err) {
        ToastService.error('Lỗi nạp dữ liệu POS', err);
      }).finally(function () {
        vm.dangTai = false;
      });
    };

    // Lọc sản phẩm trên lưới
    vm.locSanPham = function (sp) {
      if (vm.danhMucLoc && sp.MADANHMUC !== vm.danhMucLoc) return false;
      if (!vm.tuKhoa) return true;
      var kw = vm.tuKhoa.toLowerCase().trim();
      return (sp.TENSP && sp.TENSP.toLowerCase().includes(kw)) ||
             (sp.MASP && sp.MASP.toLowerCase().includes(kw)) ||
             (sp.MAVACH && sp.MAVACH.toLowerCase().includes(kw));
    };

    // URL ảnh SP
    vm.layAnh = function (maSP) {
      if (!maSP) return '';
      return ApiService.URL.thukho + '/QuanLySanPham/xem-anh-sp/' + encodeURIComponent(maSP);
    };

    // Lấy ký tự đầu đại diện cho avatar
    vm.layChuCaiDau = function (ten) {
      if (!ten) return 'SP';
      var tu = ten.trim().split(/\s+/);
      if (tu.length === 1) return tu[0].substring(0, 2).toUpperCase();
      return (tu[0][0] + tu[tu.length - 1][0]).toUpperCase();
    };

    // Chọn màu cho avatar sản phẩm
    vm.layMauAvatar = function (ten) {
      if (!ten) return mauAvatarList[0];
      var hash = 0;
      for (var i = 0; i < ten.length; i++) {
        hash = ten.charCodeAt(i) + ((hash << 5) - hash);
      }
      var idx = Math.abs(hash) % mauAvatarList.length;
      return mauAvatarList[idx];
    };

    // Xem nhanh sản phẩm
    vm.xemNhanh = function (sp) {
      vm.modalXemNhanh.sp = sp;
      vm.modalXemNhanh.hienThi = true;
    };

    vm.dongXemNhanh = function () {
      vm.modalXemNhanh.hienThi = false;
    };

    // Thêm sản phẩm vào giỏ
    vm.themVaoGio = function (sp) {
      var ton = Number(sp.SOLUONGTON) || 0;
      var itemTonTai = vm.gioHang.find(function (item) { return item.MASP === sp.MASP; });

      if (itemTonTai) {
        if (ton > 0 && itemTonTai.SOLUONG >= ton) {
          ToastService.warning('Hết hàng', 'Số lượng trong đơn đã đạt mức tồn kho tối đa (' + ton + ').');
          return;
        }
        itemTonTai.SOLUONG += 1;
        itemTonTai.THANHTIEN = itemTonTai.SOLUONG * itemTonTai.DONGIA;
      } else {
        if (ton <= 0) {
          ToastService.warning('Hết hàng', 'Sản phẩm này hiện đã hết hàng trong kho.');
          return;
        }
        vm.gioHang.push({
          MASP: sp.MASP,
          TENSP: sp.TENSP,
          DONGIA: sp.DONGIA,
          SOLUONG: 1,
          THANHTIEN: sp.DONGIA,
          SOLUONGTON: ton
        });
      }
    };

    // Tăng số lượng
    vm.tangSoLuong = function (item) {
      if (item.SOLUONGTON > 0 && item.SOLUONG >= item.SOLUONGTON) {
        ToastService.warning('Hết hàng', 'Số lượng vượt quá tồn kho khả dụng (' + item.SOLUONGTON + ').');
        return;
      }
      item.SOLUONG += 1;
      item.THANHTIEN = item.SOLUONG * item.DONGIA;
    };

    // Giảm số lượng
    vm.giamSoLuong = function (item) {
      if (item.SOLUONG > 1) {
        item.SOLUONG -= 1;
        item.THANHTIEN = item.SOLUONG * item.DONGIA;
      } else {
        var idx = vm.gioHang.indexOf(item);
        if (idx > -1) vm.gioHang.splice(idx, 1);
      }
    };

    // Điều chỉnh số lượng trực tiếp
    vm.capNhatThanhTien = function (item) {
      var sl = Number(item.SOLUONG) || 1;
      if (sl < 1) sl = 1;
      if (item.SOLUONGTON > 0 && sl > item.SOLUONGTON) {
        sl = item.SOLUONGTON;
        ToastService.warning('Giới hạn tồn', 'Số lượng đã được tự động đưa về mức tối đa: ' + sl);
      }
      item.SOLUONG = sl;
      item.THANHTIEN = item.SOLUONG * item.DONGIA;
    };

    // Xóa món
    vm.xoaKhoiGio = function (idx) {
      vm.gioHang.splice(idx, 1);
    };

    // Xóa hết giỏ
    vm.xoaHetGio = function () {
      if (vm.gioHang.length === 0) return;
      vm.gioHang = [];
      vm.giamGia = 0;
      vm.tienKhachDua = 0;
    };

    // Tính toán tiền
    vm.tinhTienHang = function () {
      return vm.gioHang.reduce(function (s, item) { return s + item.THANHTIEN; }, 0);
    };

    vm.tinhTienVAT = function () {
      var vat = Number(vm.vatPhanTram) || 0;
      return Math.round(vm.tinhTienHang() * vat / 100);
    };

    vm.tinhTongCong = function () {
      var tong = vm.tinhTienHang() + vm.tinhTienVAT() - (Number(vm.giamGia) || 0);
      return Math.max(0, tong);
    };

    vm.tinhTienThua = function () {
      var tong = vm.tinhTongCong();
      var dua = Number(vm.tienKhachDua) || 0;
      return Math.max(0, dua - tong);
    };

    // Nút chọn tiền nhanh
    vm.datTienNhanh = function (soTien) {
      vm.tienKhachDua = soTien;
    };

    vm.datTienDung = function () {
      vm.tienKhachDua = vm.tinhTongCong();
    };

    // Thanh toán đơn hàng (Hỗ trợ cả Tiền mặt và QR PayOS)
    vm.thanhToan = function () {
      if (vm.gioHang.length === 0) {
        ToastService.warning('Giỏ hàng trống', 'Vui lòng chọn ít nhất một sản phẩm vào đơn.');
        return;
      }

      var tongCong = vm.tinhTongCong();
      if (vm.ptThanhToan === 'Tiền mặt' && (Number(vm.tienKhachDua) || 0) < tongCong) {
        ToastService.warning('Chưa đủ tiền', 'Số tiền khách đưa chưa đủ để thanh toán hóa đơn.');
        return;
      }

      vm.dangThanhToan = true;
      var maHD = vm.maHoaDonHienTai || ('HD' + Math.floor(1000000 + Math.random() * 9000000));
      var user = AuthService.layNguoiDung() || {};

      var tenKH = 'Khách lẻ';
      if (vm.khachHangChon && vm.khachHangChon !== 'KH_LE') {
        var kh = vm.danhSachKH.find(function (k) { return k.MAKHACHHANG === vm.khachHangChon; });
        if (kh) tenKH = kh.TENKHACHHANG;
      }

      var tenNV = vm.nhanVienChon || user.userName || 'NV001';
      var nvObj = vm.danhSachNV.find(function (n) { return n.MANV === vm.nhanVienChon; });
      if (nvObj && nvObj.TENNV) tenNV = nvObj.TENNV;

      // 1. Tạo hóa đơn trong DB trước (khớp chính xác Models.HoaDonBan)
      var payloadHD = {
        MAHDBAN: maHD,
        MANV: vm.nhanVienChon || user.userName || 'NV001',
        MAKH: (vm.khachHangChon === 'KH_LE' || !vm.khachHangChon) ? null : vm.khachHangChon,
        NGAYLAP: new Date().toISOString().substring(0, 10),
        TONGTIENHANG: vm.tinhTienHang(),
        THUEVAT: vm.tinhTienVAT(),
        GIAMGIA: Number(vm.giamGia) || 0,
        listjson_chitietban: vm.gioHang.map(function (item) {
          return {
            MAHDBAN: maHD,
            MASP: item.MASP,
            SOLUONG: item.SOLUONG,
            DONGIA: item.DONGIA,
            TONGTIEN: item.THANHTIEN
          };
        })
      };

      ApiService.post(ApiService.URL.thungan + '/QuanLyBanHang/create-hoadonban', payloadHD).then(function () {
        if (vm.ptThanhToan === 'Thanh toán QR') {
          // 2. Nếu chọn Thanh toán QR: Gọi API PayOS để lấy link và mã QR
          vm.khoiTaoThanhToanPayOs(maHD, tenKH, tenNV, tongCong);
        } else {
          // Thanh toán Tiền mặt: Hoàn tất ngay
          ToastService.success('Thành công', 'Thanh toán đơn hàng ' + maHD + ' thành công!');
          vm.hienModalHoaDonThanhCong(maHD, tenKH, tenNV, tongCong);
          vm.dangThanhToan = false;
        }
      }).catch(function (err) {
        ToastService.error('Lỗi thanh toán', err);
        vm.dangThanhToan = false;
      });
    };

    // Khởi tạo quy trình thanh toán QR PayOS (Ảnh 2)
    vm.khoiTaoThanhToanPayOs = function (maHD, tenKH, tenNV, tongCong) {
      vm.modalPayOs = {
        hienThi: true,
        maHD: maHD,
        ngayLap: new Date(),
        tenNV: tenNV,
        tenKH: tenKH,
        chiTiet: angular.copy(vm.gioHang),
        tienHang: vm.tinhTienHang(),
        tienVAT: vm.tinhTienVAT(),
        giamGia: Number(vm.giamGia) || 0,
        tongCong: tongCong,
        orderCode: null,
        checkoutUrl: '',
        qrCode: '',
        qrImageDataUrl: '',
        qrImageUrl: '',
        trangThai: 'Chưa thanh toán',
        dangKiemTra: false
      };
      vm.dangThanhToan = false;

      // Gọi API PayOS create
      ApiService.post(ApiService.URL.thungan + '/QuanLyBanHang/payos/create', { maHoaDon: maHD }).then(function (res) {
        var d = (res && res.data) ? res.data : (res || {});
        vm.modalPayOs.orderCode = d.orderCode;
        vm.modalPayOs.checkoutUrl = d.checkoutUrl || '';
        vm.modalPayOs.qrCode = d.qrCode || '';
        vm.modalPayOs.qrImageDataUrl = d.qrImageDataUrl || '';
        vm.modalPayOs.qrImageUrl = d.qrImageUrl || (d.qrCode ? ('https://api.qrserver.com/v1/create-qr-code/?data=' + encodeURIComponent(d.qrCode) + '&size=240x240') : '');
        vm.modalPayOs.trangThai = d.trangThai || 'Chưa thanh toán';

        // Bắt đầu tự động kiểm tra mỗi 3.5 giây
        vm.batDauPollingPayOs(maHD);
      }).catch(function (err) {
        var msg = (err && err.data && err.data.message) ? err.data.message : 'Không thể tạo link QR PayOS';
        ToastService.warning('Lưu ý PayOS', msg);
        // Fallback QR ảnh tĩnh nếu có mã
        vm.modalPayOs.trangThai = 'Chưa thanh toán';
        vm.batDauPollingPayOs(maHD);
      });
    };

    // Polling tự động kiểm tra trạng thái thanh toán PayOS
    vm.batDauPollingPayOs = function (maHD) {
      vm.dungPollingPayOs();
      timerPayOs = $interval(function () {
        vm.kiemTraTrangThaiPayOs(false);
      }, 3500);
    };

    vm.dungPollingPayOs = function () {
      if (timerPayOs) {
        $interval.cancel(timerPayOs);
        timerPayOs = null;
      }
    };

    // Kiểm tra trạng thái thanh toán PayOS (thủ công hoặc tự động)
    vm.kiemTraTrangThaiPayOs = function (thuCong) {
      if (!vm.modalPayOs.maHD) return;
      if (thuCong) vm.modalPayOs.dangKiemTra = true;

      ApiService.get(ApiService.URL.thungan + '/QuanLyBanHang/payos/status', { maHoaDon: vm.modalPayOs.maHD }).then(function (res) {
        var d = (res && res.data) ? res.data : (res || {});
        var status = d.trangThai || '';
        if (status === 'Đã thanh toán') {
          vm.modalPayOs.trangThai = 'Đã thanh toán';
          vm.dungPollingPayOs();
          ToastService.success('Thành công', 'Hóa đơn ' + vm.modalPayOs.maHD + ' đã thanh toán thành công qua PayOS!');

          // Chờ 1.5 giây để người dùng thấy badge xanh, sau đó chuyển sang hóa đơn in
          setTimeout(function () {
            $scope.$apply(function () {
              var maHD = vm.modalPayOs.maHD;
              var tenKH = vm.modalPayOs.tenKH;
              var tenNV = vm.modalPayOs.tenNV;
              var tongCong = vm.modalPayOs.tongCong;
              var chiTiet = angular.copy(vm.modalPayOs.chiTiet);
              var tienHang = vm.modalPayOs.tienHang;
              var tienVAT = vm.modalPayOs.tienVAT;
              var giamGia = vm.modalPayOs.giamGia;

              vm.dongModalPayOs(true);
              vm.hienModalHoaDonThanhCong(maHD, tenKH, tenNV, tongCong, chiTiet, tienHang, tienVAT, giamGia);
            });
          }, 1500);
        } else if (thuCong) {
          ToastService.info('Đang chờ', 'Giao dịch chưa hoàn tất. Vui lòng quét mã trên app ngân hàng.');
        }
      }).catch(function (err) {
        if (thuCong) {
          ToastService.error('Lỗi kiểm tra', err);
        }
      }).finally(function () {
        if (thuCong) vm.modalPayOs.dangKiemTra = false;
      });
    };

    // Đóng modal PayOS ("Để sau")
    vm.dongModalPayOs = function (daThanhToan) {
      vm.dungPollingPayOs();
      vm.modalPayOs.hienThi = false;

      // Làm trống giỏ hàng và chuẩn bị đơn hàng mới
      vm.gioHang = [];
      vm.giamGia = 0;
      vm.tienKhachDua = 0;
      vm.taoMaHoaDonMoi();
      vm.taiDuLieu();

      if (!daThanhToan) {
        ToastService.info('Đã lưu đơn', 'Đơn hàng đã được lưu trên hệ thống. Quý khách có thể đối soát thanh toán sau.');
      }
    };

    // Mở modal hóa đơn thành công
    vm.hienModalHoaDonThanhCong = function (maHD, tenKH, tenNV, tongCong, chiTiet, tienHang, tienVAT, giamGia) {
      vm.modalHoaDon.duLieu = {
        maHD: maHD,
        ngayBan: new Date(),
        tenKH: tenKH,
        thuNgan: tenNV,
        ptThanhToan: vm.ptThanhToan,
        chiTiet: chiTiet || angular.copy(vm.gioHang),
        tienHang: tienHang !== undefined ? tienHang : vm.tinhTienHang(),
        tienVAT: tienVAT !== undefined ? tienVAT : vm.tinhTienVAT(),
        giamGia: giamGia !== undefined ? giamGia : (Number(vm.giamGia) || 0),
        tongCong: tongCong,
        tienKhachDua: Number(vm.tienKhachDua) || tongCong,
        tienThua: vm.tinhTienThua()
      };
      vm.modalHoaDon.hienThi = true;

      // Reset dữ liệu giỏ và tạo mã HĐ mới
      vm.gioHang = [];
      vm.giamGia = 0;
      vm.tienKhachDua = 0;
      vm.taoMaHoaDonMoi();
      vm.taiDuLieu();
    };

    // Đóng modal hóa đơn
    vm.dongModalHoaDon = function () {
      vm.modalHoaDon.hienThi = false;
    };

    // In hóa đơn
    vm.inHoaDon = function () {
      var vungIn = document.getElementById('vung-in-hoa-don');
      if (!vungIn) return;
      var w = $window.open('', '_blank');
      w.document.write('<html><head><title>Hóa Đơn Bán Lẻ</title>');
      w.document.write('<style>body{font-family:monospace,sans-serif;padding:16px;color:#000;max-width:380px;margin:auto;}table{width:100%;border-collapse:collapse;margin:12px 0;}th,td{padding:4px 0;font-size:12px;}hr{border:none;border-top:1px dashed #000;margin:8px 0;}</style>');
      w.document.write('</head><body>');
      w.document.write(vungIn.innerHTML);
      w.document.write('</body></html>');
      w.document.close();
      w.focus();
      setTimeout(function () {
        w.print();
        w.close();
      }, 300);
    };
  }

})();
