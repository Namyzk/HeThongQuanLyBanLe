/**
 * quan-ly-phieu-nhap.controller.js — AngularJS Controller cho Quản lý & In Phiếu nhập kho
 * Hiển thị CHUẨN TÊN NHÂN VIÊN và TÊN SẢN PHẨM thay vì mã ở tất cả các vị trí.
 */
(function () {
  'use strict';

  angular.module('qlblApp')
    .controller('PhieuNhapController', [
      'ApiService',
      'AuthService',
      'SidebarHeaderService',
      'ToastService',
      'ModalService',
      '$q',
      '$scope',
      '$window',
      PhieuNhapController
    ]);

  function PhieuNhapController(ApiService, AuthService, SidebarHeaderService, ToastService, ModalService, $q, $scope, $window) {
    var vm = this;

    // Phân quyền
    if (!AuthService.kiemTraQuyen(['Admin', 'ThuKho', 'KeToan'])) return;

    // Dữ liệu danh mục và danh sách
    vm.danhSachPN = [];
    vm.danhSachNCC = [];
    vm.danhSachSP = [];
    vm.danhSachNV = [];
    vm.dangTai = false;
    vm.tuKhoa = '';

    // Phân trang
    vm.trangHienTai = 1;
    vm.soDongMoiTrang = 15;

    // Modal In & Xem trước
    vm.modalIn = {
      hienThi: false,
      phieu: null
    };

    // Modal Form Tạo / Sửa
    vm.modalForm = {
      hienThi: false,
      laSua: false,
      tieuDe: '',
      duLieu: {},
      dangLuu: false
    };

    vm.$onInit = function () {
      SidebarHeaderService.khoiTao('quan-ly-phieu-nhap', 'Phiếu nhập kho', 'Trang chủ / Phiếu nhập kho');
      vm.taiDuLieu();
    };

    // Hàm tra cứu tên nhân viên từ mã
    vm.timTenNhanVien = function (maNV) {
      if (!maNV) return '—';
      var ma = String(maNV).trim().toUpperCase();
      for (var i = 0; i < vm.danhSachNV.length; i++) {
        var n = vm.danhSachNV[i];
        if (String(n.MANV).trim().toUpperCase() === ma) {
          return n.TENNV || n.MANV;
        }
      }
      return maNV;
    };

    // Hàm tra cứu tên nhà cung cấp từ mã
    vm.timTenNCC = function (maNCC) {
      if (!maNCC) return '—';
      var ma = String(maNCC).trim().toUpperCase();
      for (var i = 0; i < vm.danhSachNCC.length; i++) {
        var c = vm.danhSachNCC[i];
        if (String(c.MANCC).trim().toUpperCase() === ma) {
          return c.TENNCC || c.MANCC;
        }
      }
      return maNCC;
    };

    // Hàm tra cứu tên sản phẩm từ mã
    vm.timTenSanPham = function (maSP) {
      if (!maSP) return '—';
      var ma = String(maSP).trim().toUpperCase();
      for (var i = 0; i < vm.danhSachSP.length; i++) {
        var s = vm.danhSachSP[i];
        if (String(s.MASP).trim().toUpperCase() === ma) {
          return s.TENSP || s.MASP;
        }
      }
      return maSP;
    };

    // Nạp toàn bộ dữ liệu từ API Gateway với $q.all
    vm.taiDuLieu = function () {
      vm.dangTai = true;

      var pPN = ApiService.get(ApiService.URL.thukho + '/QuanLyNhapKho/get-all-phieunhapkho');
      var pNCC = ApiService.get(ApiService.URL.thukho + '/QuanLyNhapKho/get-all-nhacungcap').catch(function () { return []; });
      var pSP = ApiService.get(ApiService.URL.thukho + '/QuanLySanPham/get-all-sanpham').catch(function () { return []; });
      var pNV = ApiService.get(ApiService.URL.thukho + '/QuanLyNhapKho/get-all-nhanvien').catch(function () {
        return ApiService.get(ApiService.URL.admin + '/QuanLyNhanVien/get-all-nhanvien').catch(function () { return []; });
      });

      $q.all([pPN, pNCC, pSP, pNV]).then(function (results) {
        var rawPN = ApiService.layMang(results[0]);
        var rawNCC = ApiService.layMang(results[1]);
        var rawSP = ApiService.layMang(results[2]);
        var rawNV = ApiService.layMang(results[3]);

        // 1. Chuẩn hóa Nhà cung cấp
        vm.danhSachNCC = rawNCC.map(function (n) {
          var ma = ApiService.layThuocTinh(n, 'MANCC') || '';
          var ten = ApiService.layThuocTinh(n, 'TENNCC') || ma;
          var sdt = ApiService.layThuocTinh(n, 'SDT') || '';
          return { MANCC: String(ma).trim(), TENNCC: String(ten).trim(), SDT: String(sdt).trim() };
        });

        // 2. Chuẩn hóa Sản phẩm
        vm.danhSachSP = rawSP.map(function (s) {
          var ma = ApiService.layThuocTinh(s, 'MASP') || '';
          var ten = ApiService.layThuocTinh(s, 'TENSP') || ma;
          var gia = Number(ApiService.layThuocTinh(s, 'DONGIA')) || 0;
          return { MASP: String(ma).trim(), TENSP: String(ten).trim(), DONGIA: gia };
        });

        // 3. Chuẩn hóa Nhân viên
        vm.danhSachNV = rawNV.map(function (nv) {
          var ma = ApiService.layThuocTinh(nv, 'MANV') || '';
          var ten = ApiService.layThuocTinh(nv, 'TENNV') || ma;
          var sdt = ApiService.layThuocTinh(nv, 'SDT') || '';
          return { MANV: String(ma).trim(), TENNV: String(ten).trim(), SDT: String(sdt).trim() };
        });

        // 4. Chuẩn hóa Phiếu nhập kho và gán trực tiếp tên nhân viên, tên NCC
        vm.danhSachPN = rawPN.map(function (p) {
          var maPN = ApiService.layThuocTinh(p, 'MAPHIEUNHAP') || ApiService.layThuocTinh(p, 'MaPhieuNhap') || '';
          var maNCC = ApiService.layThuocTinh(p, 'MANCC') || ApiService.layThuocTinh(p, 'MaNCC') || '';
          var maNV = ApiService.layThuocTinh(p, 'MANV') || ApiService.layThuocTinh(p, 'MaNV') || '';
          var ngay = ApiService.layThuocTinh(p, 'NGAYLAP') || ApiService.layThuocTinh(p, 'NgayLap') || '';
          var tongTien = Number(ApiService.layThuocTinh(p, 'TONG_TIEN') || ApiService.layThuocTinh(p, 'TONGTIEN') || 0);

          var rawCT = ApiService.layThuocTinh(p, 'listjson_chitietnhap') || ApiService.layThuocTinh(p, 'CHI_TIET') || ApiService.layThuocTinh(p, 'ChiTiet') || [];
          var listCT = [];
          if (Array.isArray(rawCT)) {
            listCT = rawCT.map(function (c) {
              var msp = ApiService.layThuocTinh(c, 'MASP') || ApiService.layThuocTinh(c, 'MaSP') || '';
              var sl = Number(ApiService.layThuocTinh(c, 'SOLUONG') || ApiService.layThuocTinh(c, 'SoLuong') || 0);
              var dg = Number(ApiService.layThuocTinh(c, 'DONGIANHAP') || ApiService.layThuocTinh(c, 'DonGiaNhap') || ApiService.layThuocTinh(c, 'DONGIA') || 0);
              var tt = Number(ApiService.layThuocTinh(c, 'THANHTIEN') || ApiService.layThuocTinh(c, 'ThanhTien') || (sl * dg));
              return {
                MASP: String(msp).trim(),
                TENSP: vm.timTenSanPham(msp),
                SOLUONG: sl,
                DONGIANHAP: dg,
                THANHTIEN: tt > 0 ? tt : (sl * dg)
              };
            });
          }

          if (tongTien === 0 && listCT.length > 0) {
            tongTien = listCT.reduce(function (s, c) { return s + c.THANHTIEN; }, 0);
          }

          var soLuongSP = listCT.length || Number(ApiService.layThuocTinh(p, 'SO_LUONG_SP')) || 0;

          return {
            MAPHIEUNHAP: String(maPN).trim(),
            MANCC: String(maNCC).trim(),
            TENNCC: vm.timTenNCC(maNCC),
            MANV: String(maNV).trim(),
            TENNV: vm.timTenNhanVien(maNV), // Hiển thị chuẩn tên nhân viên
            NGAYLAP: ngay,
            TONG_TIEN: tongTien,
            SO_LUONG_SP: soLuongSP,
            CHI_TIET: listCT
          };
        });
      }).catch(function (err) {
        ToastService.error('Lỗi nạp phiếu nhập', err);
      }).finally(function () {
        vm.dangTai = false;
      });
    };

    // Lọc danh sách theo từ khóa
    vm.layDanhSachLoc = function () {
      var list = vm.danhSachPN || [];
      if (!vm.tuKhoa) return list;
      var kw = vm.tuKhoa.toLowerCase().trim();
      return list.filter(function (pn) {
        return (pn.MAPHIEUNHAP && pn.MAPHIEUNHAP.toLowerCase().includes(kw)) ||
               (pn.TENNCC && pn.TENNCC.toLowerCase().includes(kw)) ||
               (pn.MANCC && pn.MANCC.toLowerCase().includes(kw)) ||
               (pn.TENNV && pn.TENNV.toLowerCase().includes(kw)) ||
               (pn.MANV && pn.MANV.toLowerCase().includes(kw));
      });
    };

    // Lấy danh sách trang hiện tại
    vm.layDanhSachPhanTrang = function () {
      var loc = vm.layDanhSachLoc();
      var start = (vm.trangHienTai - 1) * vm.soDongMoiTrang;
      return loc.slice(start, start + vm.soDongMoiTrang);
    };

    // Tổng số trang
    vm.layTongSoTrang = function () {
      var loc = vm.layDanhSachLoc();
      return Math.ceil(loc.length / vm.soDongMoiTrang) || 1;
    };

    // Xem & In phiếu nhập
    vm.xemInPhieu = function (pn) {
      // Đảm bảo tên nhân viên và tên sản phẩm luôn đầy đủ
      var phieuIn = angular.copy(pn);
      phieuIn.TENNV = vm.timTenNhanVien(phieuIn.MANV);
      phieuIn.TENNCC = vm.timTenNCC(phieuIn.MANCC);

      if (phieuIn.CHI_TIET && phieuIn.CHI_TIET.length > 0) {
        phieuIn.CHI_TIET.forEach(function (ct) {
          ct.TENSP = vm.timTenSanPham(ct.MASP);
        });
      } else {
        // Tải chi tiết nếu chưa nạp sẵn
        ApiService.get(ApiService.URL.thukho + '/QuanLyNhapKho/get-chitiet-phieunhapkho-by-id', { id: phieuIn.MAPHIEUNHAP }).then(function (res) {
          var items = ApiService.layMang(res);
          phieuIn.CHI_TIET = items.map(function (c) {
            var msp = ApiService.layThuocTinh(c, 'MASP') || '';
            var sl = Number(ApiService.layThuocTinh(c, 'SOLUONG') || 0);
            var dg = Number(ApiService.layThuocTinh(c, 'DONGIANHAP') || 0);
            return {
              MASP: msp,
              TENSP: vm.timTenSanPham(msp),
              SOLUONG: sl,
              DONGIANHAP: dg,
              THANHTIEN: sl * dg
            };
          });
        });
      }

      vm.modalIn.phieu = phieuIn;
      vm.modalIn.hienThi = true;
    };

    // In phiếu
    vm.thucHienIn = function () {
      var vungIn = document.getElementById('vung-in-phieu-nhap');
      if (!vungIn) return;
      var w = $window.open('', '_blank');
      w.document.write('<html><head><title>Phiếu Nhập Kho</title>');
      w.document.write('<style>body{font-family:Arial,sans-serif;padding:24px;color:#1e293b;}table{width:100%;border-collapse:collapse;margin:16px 0;}th,td{border:1px solid #cbd5e1;padding:8px;font-size:13px;}th{background:#f1f5f9;}</style>');
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

    // Mở Form Thêm
    vm.moFormThem = function () {
      var user = AuthService.layNguoiDung() || {};
      var defaultNV = vm.danhSachNV.length > 0 ? vm.danhSachNV[0].MANV : (user.userName || '');

      vm.modalForm = {
        hienThi: true,
        laSua: false,
        tieuDe: 'Tạo phiếu nhập kho mới',
        duLieu: {
          MAPHIEUNHAP: '',
          NGAYLAP_OBJ: new Date(),
          MANCC: vm.danhSachNCC.length > 0 ? vm.danhSachNCC[0].MANCC : '',
          MANV: defaultNV,
          chiTiet: [
            {
              MASP: vm.danhSachSP.length > 0 ? vm.danhSachSP[0].MASP : '',
              SOLUONG: 1,
              DONGIANHAP: vm.danhSachSP.length > 0 ? vm.danhSachSP[0].DONGIA : 0,
              THANHTIEN: vm.danhSachSP.length > 0 ? vm.danhSachSP[0].DONGIA : 0
            }
          ]
        },
        dangLuu: false
      };
    };

    // Mở Form Sửa
    vm.moFormSua = function (pn) {
      vm.modalForm = {
        hienThi: true,
        laSua: true,
        tieuDe: 'Chỉnh sửa phiếu nhập kho — ' + pn.MAPHIEUNHAP,
        duLieu: {
          MAPHIEUNHAP: pn.MAPHIEUNHAP,
          NGAYLAP_OBJ: pn.NGAYLAP ? new Date(pn.NGAYLAP) : new Date(),
          MANCC: pn.MANCC,
          MANV: pn.MANV,
          chiTiet: angular.copy(pn.CHI_TIET && pn.CHI_TIET.length > 0 ? pn.CHI_TIET : [
            { MASP: vm.danhSachSP[0]?.MASP || '', SOLUONG: 1, DONGIANHAP: 0, THANHTIEN: 0 }
          ])
        },
        dangLuu: false
      };
    };

    // Thêm dòng hàng
    vm.themDongHang = function () {
      var spDau = vm.danhSachSP[0] || {};
      vm.modalForm.duLieu.chiTiet.push({
        MASP: spDau.MASP || '',
        SOLUONG: 1,
        DONGIANHAP: spDau.DONGIA || 0,
        THANHTIEN: spDau.DONGIA || 0
      });
    };

    // Xóa dòng hàng
    vm.xoaDongHang = function (idx) {
      if (vm.modalForm.duLieu.chiTiet.length > 1) {
        vm.modalForm.duLieu.chiTiet.splice(idx, 1);
      }
    };

    // Khi chọn sản phẩm trên dòng
    vm.khiChonSanPham = function (item) {
      var sp = vm.danhSachSP.find(function (s) { return s.MASP === item.MASP; });
      if (sp) {
        item.DONGIANHAP = sp.DONGIA || 0;
        item.THANHTIEN = item.SOLUONG * item.DONGIANHAP;
      }
    };

    // Tính thành tiền dòng
    vm.tinhThanhTienDong = function (item) {
      item.THANHTIEN = (Number(item.SOLUONG) || 0) * (Number(item.DONGIANHAP) || 0);
    };

    // Tính tổng tiền form
    vm.tinhTongTienForm = function () {
      if (!vm.modalForm.duLieu.chiTiet) return 0;
      return vm.modalForm.duLieu.chiTiet.reduce(function (s, c) {
        return s + ((Number(c.SOLUONG) || 0) * (Number(c.DONGIANHAP) || 0));
      }, 0);
    };

    // Lưu phiếu nhập kho
    vm.luuPhieuNhap = function () {
      var d = vm.modalForm.duLieu;
      if (!d.MAPHIEUNHAP || !d.MANCC || !d.MANV) {
        ToastService.warning('Thiếu thông tin', 'Vui lòng điền đủ mã phiếu, nhà cung cấp và nhân viên.');
        return;
      }

      vm.modalForm.dangLuu = true;

      var ngayStr = d.NGAYLAP_OBJ ? d.NGAYLAP_OBJ.toISOString().substring(0, 10) : new Date().toISOString().substring(0, 10);
      var payload = {
        MAPHIEUNHAP: d.MAPHIEUNHAP.trim(),
        MANCC: d.MANCC,
        MANV: d.MANV,
        NGAYLAP: ngayStr,
        TONG_TIEN: vm.tinhTongTienForm(),
        CHI_TIET: d.chiTiet.map(function (c) {
          return {
            MASP: c.MASP,
            SOLUONG: Number(c.SOLUONG) || 1,
            DONGIANHAP: Number(c.DONGIANHAP) || 0,
            THANHTIEN: (Number(c.SOLUONG) || 1) * (Number(c.DONGIANHAP) || 0)
          };
        })
      };

      var endpoint = vm.modalForm.laSua
        ? ApiService.URL.thukho + '/QuanLyNhapKho/update-phieunhapkho'
        : ApiService.URL.thukho + '/QuanLyNhapKho/create-phieunhapkho';

      ApiService.post(endpoint, payload).then(function () {
        ToastService.success('Thành công', vm.modalForm.laSua ? 'Cập nhật phiếu thành công.' : 'Tạo phiếu nhập kho thành công.');
        vm.modalForm.hienThi = false;
        vm.taiDuLieu();
      }).catch(function (err) {
        ToastService.error('Lỗi lưu phiếu', err);
      }).finally(function () {
        vm.modalForm.dangLuu = false;
      });
    };

    // Xóa phiếu nhập
    vm.xoaPhieuNhap = function (pn) {
      ModalService.xacNhan('Xóa phiếu nhập kho', 'Bạn có chắc chắn muốn xóa phiếu nhập <strong>' + pn.MAPHIEUNHAP + '</strong>?', 'Xóa ngay').then(function () {
        ApiService.delete(ApiService.URL.thukho + '/QuanLyNhapKho/delete-phieunhapkho', { mapn: pn.MAPHIEUNHAP }).then(function () {
          ToastService.success('Đã xóa', 'Xóa phiếu ' + pn.MAPHIEUNHAP + ' thành công.');
          vm.taiDuLieu();
        }).catch(function (err) {
          ToastService.error('Lỗi xóa phiếu', err);
        });
      });
    };
  }

})();
