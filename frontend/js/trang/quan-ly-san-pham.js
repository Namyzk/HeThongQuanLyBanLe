/**
 * quan-ly-san-pham.controller.js — AngularJS Controller cho Quản lý Sản phẩm
 * Tương thích 100% với API Gateway, giao diện chuẩn ảnh 2, 3, 4.
 * Bao gồm: Bảng danh sách, avatar, VAT (%), tồn kho nhanh, thêm/sửa modal 2 cột, xem chi tiết, xóa.
 */
(function () {
  'use strict';

  angular.module('qlblApp')
    .controller('SanPhamController', [
      'ApiService',
      'AuthService',
      'SidebarHeaderService',
      'ToastService',
      'ModalService',
      '$q',
      '$scope',
      '$window',
      SanPhamController
    ]);

  function SanPhamController(ApiService, AuthService, SidebarHeaderService, ToastService, ModalService, $q, $scope, $window) {
    var vm = this;

    // Phân quyền: Admin và Thủ kho
    if (!AuthService.kiemTraQuyen(['Admin', 'ThuKho'])) return;

    // Trạng thái dữ liệu
    vm.danhSachSP = [];
    vm.danhSachDM = [];
    vm.dangTai = false;
    vm.tuKhoa = '';
    vm.danhMucLoc = '';

    // Phân trang
    vm.trangHienTai = 1;
    vm.soDongMoiTrang = 15;

    // Bảng màu avatar
    var mauAvatarList = [
      '#4f46e5', '#0284c7', '#0d9488', '#16a34a',
      '#d97706', '#dc2626', '#9333ea', '#e11d48'
    ];

    // Modal Form (Thêm / Sửa)
    vm.modalForm = {
      hienThi: false,
      laSua: false,
      tieuDe: '',
      duLieu: {},
      fileAnh: null,
      anhPreview: null,
      dangLuu: false
    };

    // Modal Điều chỉnh tồn kho nhanh
    vm.modalTonKho = {
      hienThi: false,
      sp: null,
      soLuongMoi: 0,
      dangLuu: false
    };

    // Modal Chi tiết
    vm.modalChiTiet = {
      hienThi: false,
      sanPham: null
    };

    vm.$onInit = function () {
      SidebarHeaderService.khoiTao('quan-ly-san-pham', 'Quản lý Sản phẩm', 'Trang chủ / Quản lý Sản phẩm');
      vm.taiDuLieu();
    };

    // Chuẩn hóa bản ghi sản phẩm từ API
    function chuanHoaSP(raw) {
      var ma = String(ApiService.layThuocTinh(raw, 'MASP') || '').trim();
      var ten = String(ApiService.layThuocTinh(raw, 'TENSP') || '').trim();
      var thueVal = ApiService.layThuocTinh(raw, 'THUE');
      return {
        MASP: ma,
        TENSP: ten,
        MADANHMUC: String(ApiService.layThuocTinh(raw, 'MADANHMUC') || '').trim(),
        DONGIA: Number(ApiService.layThuocTinh(raw, 'DONGIA')) || 0,
        SOLUONGTON: Number(ApiService.layThuocTinh(raw, 'SOLUONGTON')) || 0,
        MAVACH: String(ApiService.layThuocTinh(raw, 'MAVACH') || '').trim(),
        THUOCTINH: String(ApiService.layThuocTinh(raw, 'THUOCTINH') || '').trim(),
        MOTA: String(ApiService.layThuocTinh(raw, 'MOTA') || '').trim(),
        THUE: thueVal !== undefined && thueVal !== null ? Number(thueVal) : 8,
        HINHANH: ApiService.layThuocTinh(raw, 'HINHANH') || null
      };
    }

    // Tải dữ liệu sản phẩm và danh mục
    vm.taiDuLieu = function () {
      vm.dangTai = true;

      var pSP = ApiService.get(ApiService.URL.thukho + '/QuanLySanPham/get-all-sanpham');
      var pDM = ApiService.get(ApiService.URL.thukho + '/QuanLyDanhMuc/get-all-danhmuc').catch(function () {
        return { data: [] };
      });

      $q.all([pSP, pDM]).then(function (results) {
        var rawSP = ApiService.layMang(results[0]);
        var rawDM = ApiService.layMang(results[1]);

        vm.danhSachSP = rawSP.map(chuanHoaSP);
        vm.danhSachDM = rawDM.map(function (dm) {
          return {
            MADANHMUC: String(ApiService.layThuocTinh(dm, 'MADANHMUC') || '').trim(),
            TENDANHMUC: String(ApiService.layThuocTinh(dm, 'TENDANHMUC') || '').trim()
          };
        });
      }).catch(function (err) {
        ToastService.error('Lỗi nạp sản phẩm', err);
      }).finally(function () {
        vm.dangTai = false;
      });
    };

    // Lấy ký tự đầu đại diện cho avatar
    vm.layChuCaiDau = function (ten) {
      if (!ten) return 'SP';
      var tu = ten.trim().split(/\s+/);
      if (tu.length === 1) return tu[0].substring(0, 2).toUpperCase();
      return (tu[0][0] + tu[tu.length - 1][0]).toUpperCase();
    };

    // Chọn màu cho avatar
    vm.layMauAvatar = function (ten) {
      if (!ten) return mauAvatarList[0];
      var hash = 0;
      for (var i = 0; i < ten.length; i++) {
        hash = ten.charCodeAt(i) + ((hash << 5) - hash);
      }
      var idx = Math.abs(hash) % mauAvatarList.length;
      return mauAvatarList[idx];
    };

    // Lọc danh sách theo từ khóa và danh mục
    vm.layDanhSachLoc = function () {
      var list = vm.danhSachSP || [];
      if (vm.danhMucLoc) {
        list = list.filter(function (sp) {
          return sp.MADANHMUC === vm.danhMucLoc;
        });
      }
      if (vm.tuKhoa) {
        var kw = vm.tuKhoa.toLowerCase().trim();
        list = list.filter(function (sp) {
          return (sp.TENSP && sp.TENSP.toLowerCase().includes(kw)) ||
                 (sp.MASP && sp.MASP.toLowerCase().includes(kw)) ||
                 (sp.MAVACH && sp.MAVACH.toLowerCase().includes(kw)) ||
                 (sp.MOTA && sp.MOTA.toLowerCase().includes(kw));
        });
      }
      return list;
    };

    // Lấy dữ liệu theo phân trang
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

    // Tra cứu tên danh mục
    vm.layTenDanhMuc = function (maDM) {
      if (!maDM) return '—';
      for (var i = 0; i < vm.danhSachDM.length; i++) {
        if (vm.danhSachDM[i].MADANHMUC === maDM) {
          return vm.danhSachDM[i].TENDANHMUC;
        }
      }
      return maDM;
    };

    // Lấy link ảnh sản phẩm qua API Gateway
    vm.layAnh = function (maSP) {
      if (!maSP) return '';
      return ApiService.URL.thukho + '/QuanLySanPham/xem-anh-sp/' + encodeURIComponent(maSP);
    };

    // Mở form Thêm mới (Ảnh 3)
    vm.moFormThem = function () {
      vm.modalForm = {
        hienThi: true,
        laSua: false,
        tieuDe: 'Thêm sản phẩm mới',
        duLieu: {
          MASP: '',
          TENSP: '',
          MADANHMUC: vm.danhSachDM.length > 0 ? vm.danhSachDM[0].MADANHMUC : '',
          DONGIA: 0,
          SOLUONGTON: 0,
          MAVACH: '',
          THUE: 8,
          THUOCTINH: '',
          MOTA: '',
          urlAnh: ''
        },
        fileAnh: null,
        anhPreview: null,
        dangLuu: false
      };
    };

    // Mở form Sửa (Ảnh 4)
    vm.moFormSua = function (sp) {
      vm.modalForm = {
        hienThi: true,
        laSua: true,
        tieuDe: 'Cập nhật sản phẩm — ' + sp.MASP,
        duLieu: angular.copy(sp),
        fileAnh: null,
        anhPreview: null,
        dangLuu: false
      };
    };

    // Đóng form
    vm.dongModalForm = function () {
      vm.modalForm.hienThi = false;
      var fileInp = document.getElementById('sp-file-input');
      if (fileInp) fileInp.value = '';
    };

    // Chọn file ảnh
    vm.chonFileAnh = function (input) {
      if (input.files && input.files[0]) {
        var file = input.files[0];
        vm.modalForm.fileAnh = file;
        var reader = new FileReader();
        reader.onload = function (e) {
          $scope.$apply(function () {
            vm.modalForm.anhPreview = e.target.result;
          });
        };
        reader.readAsDataURL(file);
      }
    };

    // Lưu sản phẩm (Thêm / Sửa)
    vm.luuSanPham = function () {
      var d = vm.modalForm.duLieu;
      if (!d.MASP || !d.TENSP) {
        ToastService.warning('Thiếu thông tin', 'Vui lòng nhập mã và tên sản phẩm.');
        return;
      }

      vm.modalForm.dangLuu = true;

      var endpoint = vm.modalForm.laSua
        ? ApiService.URL.thukho + '/QuanLySanPham/update-sanpham'
        : ApiService.URL.thukho + '/QuanLySanPham/create-sanpham';

      var payload = {
        MASP: d.MASP.trim(),
        TENSP: d.TENSP.trim(),
        MADANHMUC: d.MADANHMUC,
        DONGIA: Number(d.DONGIA) || 0,
        SOLUONGTON: Number(d.SOLUONGTON) || 0,
        MAVACH: d.MAVACH ? d.MAVACH.trim() : '',
        THUE: Number(d.THUE) || 8,
        THUOCTINH: d.THUOCTINH ? d.THUOCTINH.trim() : '',
        MOTA: d.MOTA ? d.MOTA.trim() : ''
      };

      ApiService.post(endpoint, payload).then(function () {
        // Tải ảnh lên nếu có file được chọn
        if (vm.modalForm.fileAnh) {
          var formData = new FormData();
          formData.append('file', vm.modalForm.fileAnh);
          formData.append('maSP', d.MASP.trim());

          fetch(ApiService.URL.thukho + '/QuanLySanPham/upload-anh-sp', {
            method: 'POST',
            body: formData,
            credentials: 'include'
          }).finally(function () {
            ToastService.success('Thành công', vm.modalForm.laSua ? 'Cập nhật sản phẩm thành công.' : 'Thêm sản phẩm thành công.');
            vm.dongModalForm();
            vm.taiDuLieu();
          });
        } else {
          ToastService.success('Thành công', vm.modalForm.laSua ? 'Cập nhật sản phẩm thành công.' : 'Thêm sản phẩm thành công.');
          vm.dongModalForm();
          vm.taiDuLieu();
        }
      }).catch(function (err) {
        ToastService.error('Lỗi lưu sản phẩm', err);
      }).finally(function () {
        vm.modalForm.dangLuu = false;
      });
    };

    // Mở modal điều chỉnh tồn kho nhanh (Nút xanh lá hộp kho trong ảnh 2)
    vm.moDieuChinhTonKho = function (sp) {
      vm.modalTonKho = {
        hienThi: true,
        sp: sp,
        soLuongMoi: sp.SOLUONGTON,
        dangLuu: false
      };
    };

    vm.dongModalTonKho = function () {
      vm.modalTonKho.hienThi = false;
    };

    vm.luuDieuChinhTonKho = function () {
      if (!vm.modalTonKho.sp) return;
      var sl = Number(vm.modalTonKho.soLuongMoi);
      if (isNaN(sl) || sl < 0) {
        ToastService.warning('Không hợp lệ', 'Số lượng tồn phải là số không âm.');
        return;
      }

      vm.modalTonKho.dangLuu = true;
      var url = ApiService.URL.thukho + '/QuanLySanPham/update-soluong-sanpham?maSP=' + encodeURIComponent(vm.modalTonKho.sp.MASP) + '&soLuongMoi=' + sl;

      ApiService.post(url, {}).then(function () {
        ToastService.success('Thành công', 'Đã cập nhật tồn kho cho sản phẩm ' + vm.modalTonKho.sp.MASP + ' thành ' + sl + '.');
        vm.dongModalTonKho();
        vm.taiDuLieu();
      }).catch(function (err) {
        ToastService.error('Lỗi cập nhật tồn kho', err);
      }).finally(function () {
        vm.modalTonKho.dangLuu = false;
      });
    };

    // Xóa sản phẩm
    vm.xoaSanPham = function (sp) {
      ModalService.xacNhan('Xóa sản phẩm', 'Bạn có chắc chắn muốn xóa sản phẩm <strong>' + sp.TENSP + '</strong> (' + sp.MASP + ')?', 'Xóa ngay').then(function () {
        ApiService.delete(ApiService.URL.thukho + '/QuanLySanPham/delete-sanpham', { maSP: sp.MASP }).then(function () {
          ToastService.success('Đã xóa', 'Xóa sản phẩm ' + sp.MASP + ' thành công.');
          vm.taiDuLieu();
        }).catch(function (err) {
          ToastService.error('Lỗi xóa sản phẩm', err);
        });
      });
    };

    // Xem chi tiết
    vm.xemChiTiet = function (sp) {
      vm.modalChiTiet.sanPham = sp;
      vm.modalChiTiet.hienThi = true;
    };

    // Chuyển từ chi tiết sang sửa
    vm.chuyenSangSua = function (sp) {
      vm.modalChiTiet.hienThi = false;
      vm.moFormSua(sp);
    };
  }

})();
