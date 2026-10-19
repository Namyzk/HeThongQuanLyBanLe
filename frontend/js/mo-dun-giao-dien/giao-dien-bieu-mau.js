/**
 * giao-dien-bieu-mau.js — Module HTML thiết kế các Biểu mẫu (Forms) cho các thực thể
 */
import { escHtml } from '../tien-ich.js';

/** Form Sản Phẩm */
export function taoHtmlFormSanPham({ row = null, laSua = false, danhMuc = [], anhSP = '' }) {
  return `
    <form id="form-sp" class="bieu-mau" novalidate>
      <!-- Khu vực hình ảnh sản phẩm -->
      <div class="nhom-truong" style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
        <label class="nhan-truong" style="margin-bottom:6px;">
          <i class="bi bi-image" style="color:var(--mau-chinh);"></i> Hình ảnh sản phẩm
        </label>
        <div style="display:flex;gap:var(--kc-m);align-items:center;flex-wrap:wrap;">
          <div style="width:110px;height:100px;border-radius:var(--bo-goc-vua);overflow:hidden;border:2px dashed var(--xam-300);background:#fff;display:flex;align-items:center;justify-content:center;position:relative;flex-shrink:0;">
            <img id="sp-preview-img" src="${anhSP || ''}" style="width:100%;height:100%;object-fit:cover;${anhSP ? '' : 'display:none;'}" alt="Ảnh sản phẩm" />
            <div id="sp-preview-placeholder" style="${anhSP ? 'display:none;' : ''}text-align:center;color:var(--van-ban-mo);font-size:11px;">
              <i class="bi bi-card-image" style="font-size:26px;display:block;margin-bottom:2px;"></i>
              Chưa có ảnh
            </div>
          </div>
          <div style="flex:1;min-width:220px;display:flex;flex-direction:column;gap:6px;">
            <div style="display:flex;gap:6px;flex-wrap:wrap;align-items:center;">
              <input type="file" id="sp-file-input" accept="image/*" style="display:none;" />
              <button type="button" id="sp-nut-chon-file" class="nut nut-vien nut-nho">
                <i class="bi bi-upload"></i> Chọn ảnh từ máy tính
              </button>
              <button type="button" id="sp-nut-xoa-anh" class="nut nut-vien nut-nguy-hiem nut-nho" style="${anhSP ? '' : 'display:none;'}">
                <i class="bi bi-trash"></i> Xóa ảnh
              </button>
            </div>
            <div style="display:flex;align-items:center;gap:6px;">
              <input type="url" id="sp-anh-url" class="o-nhap" style="font-size:var(--font-nho);padding:6px 10px;" placeholder="Hoặc dán liên kết URL ảnh sản phẩm..." value="${anhSP && !anhSP.startsWith('data:') ? escHtml(anhSP) : ''}" />
            </div>
            <input type="hidden" id="sp-anh-data" name="HINHANH" value="${escHtml(anhSP || '')}" />
          </div>
        </div>
      </div>

      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-ma">Mã sản phẩm <span class="bat-buoc">*</span></label>
          <input type="text" id="sp-ma" name="MASP" class="o-nhap" maxlength="15"
            value="${escHtml(row?.MASP || '')}"
            ${laSua ? 'readonly style="background:var(--xam-100)"' : ''}
            placeholder="VD: SP001" required>
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-ten">Tên sản phẩm <span class="bat-buoc">*</span></label>
          <input type="text" id="sp-ten" name="TENSP" class="o-nhap" maxlength="200"
            value="${escHtml(row?.TENSP || '')}" placeholder="Tên sản phẩm..." required>
        </div>
      </div>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-mavach">Mã vạch</label>
          <input type="text" id="sp-mavach" name="MAVACH" class="o-nhap" maxlength="50"
            value="${escHtml(row?.MAVACH || '')}" placeholder="Mã vạch EAN-13, barcode...">
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-danhmuc">Danh mục hàng hóa <span class="bat-buoc">*</span></label>
          <select id="sp-danhmuc" name="MADANHMUC" class="o-chon" required>
            <option value="">-- Chọn danh mục --</option>
            ${danhMuc.map(dm => `
              <option value="${escHtml(dm.MADANHMUC)}" ${dm.MADANHMUC === row?.MADANHMUC ? 'selected' : ''}>
                ${escHtml(dm.TENDANHMUC || dm.MADANHMUC)}
              </option>
            `).join('')}
          </select>
        </div>
      </div>
      <div class="luoi-bieu-mau ba-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-dongia">Đơn giá bán (VND) <span class="bat-buoc">*</span></label>
          <input type="number" id="sp-dongia" name="DONGIA" class="o-nhap" min="0" step="1000"
            value="${row?.DONGIA ?? ''}" placeholder="VD: 150000" required>
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-thue">Thuế VAT (%)</label>
          <input type="number" id="sp-thue" name="THUE" class="o-nhap" min="0" max="100"
            value="${row?.THUE ?? 0}">
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="sp-soluong">Số lượng tồn kho <span class="bat-buoc">*</span></label>
          <input type="number" id="sp-soluong" name="SOLUONGTON" class="o-nhap" min="0"
            value="${row?.SOLUONGTON ?? 0}" required>
        </div>
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="sp-thuoctinh">Thuộc tính (Quy cách, màu sắc, size)</label>
        <input type="text" id="sp-thuoctinh" name="THUOCTINH" class="o-nhap"
          value="${escHtml(row?.THUOCTINH || '')}" placeholder="VD: Hộp 500g, Màu đỏ...">
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="sp-mota">Mô tả sản phẩm</label>
        <textarea id="sp-mota" name="MOTA" class="o-nhap-nhieu-dong" rows="3"
          placeholder="Mô tả công dụng, hạn dùng...">${escHtml(row?.MOTA || '')}</textarea>
      </div>
      <div id="loi-form-sp" style="display:none;color:var(--mau-nguy-hiem)"></div>
    </form>
  `;
}

/** Form Cập nhật nhanh số lượng tồn kho */
export function taoHtmlFormSoLuong({ maSP, slHienTai }) {
  return `
    <form id="form-sl" class="bieu-mau" novalidate>
      <div style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);margin-bottom:var(--kc-s);">
        Mã sản phẩm: <strong style="color:var(--mau-chinh);">${escHtml(maSP)}</strong>
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="sl-moi">Số lượng tồn kho mới <span class="bat-buoc">*</span></label>
        <input type="number" id="sl-moi" name="soLuongMoi" class="o-nhap" min="0" value="${escHtml(slHienTai)}" required>
        <div class="mo-ta-truong">Số lượng tồn hiện tại trong kho: <strong>${escHtml(slHienTai)}</strong></div>
      </div>
      <div id="loi-form-sl" style="display:none;color:var(--mau-nguy-hiem)"></div>
    </form>
  `;
}

/** Form Khách Hàng */
export function taoHtmlFormKhachHang({ row = null, laSua = false }) {
  const maKHMoi = row?.MaKH || `KH${Date.now().toString().slice(-6)}`;
  return `
    <form id="form-khach-hang" class="bieu-mau" novalidate>
      <div class="nhom-truong">
        <label class="nhan-truong" for="kh-makh">Mã khách hàng <span class="bat-buoc">*</span></label>
        <input type="text" id="kh-makh" name="MaKH" class="o-nhap" maxlength="15" value="${escHtml(maKHMoi)}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} required />
        <span class="mo-ta-truong">Tối đa 15 ký tự, không trùng lặp</span>
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="kh-tenkh">Tên khách hàng <span class="bat-buoc">*</span></label>
        <input type="text" id="kh-tenkh" name="TenKH" class="o-nhap" maxlength="100" value="${escHtml(row?.TenKH || '')}" placeholder="VD: Nguyễn Văn A..." required />
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="kh-sdt">Số điện thoại</label>
        <input type="tel" id="kh-sdt" name="SDT" class="o-nhap" maxlength="10" value="${escHtml(row?.SDT || '')}" placeholder="0912345678 (10 số)" pattern="\\d{10}" />
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="kh-diachi">Địa chỉ</label>
        <textarea id="kh-diachi" name="DiaChi" class="o-nhap-nhieu-dong" rows="2" maxlength="300" placeholder="Địa chỉ giao hàng / liên hệ...">${escHtml(row?.DiaChi || '')}</textarea>
      </div>
    </form>
  `;
}

/** Form Nhà Cung Cấp */
export function taoHtmlFormNhaCungCap({ row = null, laSua = false }) {
  const maNCCMoi = row?.MaNCC || row?.MANCC || `NCC${Date.now().toString().slice(-5)}`;
  return `
    <form id="form-ncc" class="bieu-mau" novalidate>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="ncc-ma">Mã NCC <span class="bat-buoc">*</span></label>
          <input type="text" id="ncc-ma" name="MaNCC" class="o-nhap" maxlength="15" value="${escHtml(maNCCMoi)}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} required />
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="ncc-ten">Tên nhà cung cấp <span class="bat-buoc">*</span></label>
          <input type="text" id="ncc-ten" name="TenNCC" class="o-nhap" maxlength="100" value="${escHtml(row?.TenNCC || row?.TENNCC || '')}" placeholder="Công ty TNHH / Đại lý..." required />
        </div>
      </div>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="ncc-sdt">Số điện thoại</label>
          <input type="tel" id="ncc-sdt" name="SDT" class="o-nhap" maxlength="15" value="${escHtml(row?.SDT || '')}" placeholder="0901234567" />
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="ncc-email">Email</label>
          <input type="email" id="ncc-email" name="EMAIL" class="o-nhap" maxlength="100" value="${escHtml(row?.EMAIL || '')}" placeholder="ncc@example.com" />
        </div>
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="ncc-dc">Địa chỉ trụ sở</label>
        <textarea id="ncc-dc" name="DiaChi" class="o-nhap-nhieu-dong" rows="2" maxlength="300" placeholder="Địa chỉ kho hàng hoặc công ty...">${escHtml(row?.DiaChi || row?.DIACHI || '')}</textarea>
      </div>
      <div id="loi-form-ncc" style="display:none;color:var(--mau-nguy-hiem);"></div>
    </form>
  `;
}

/** Form Danh Mục Hàng Hóa */
export function taoHtmlFormDanhMuc({ row = null, laSua = false }) {
  return `
    <form id="form-dm" class="bieu-mau" novalidate>
      <div class="nhom-truong">
        <label class="nhan-truong" for="dm-ma">Mã danh mục <span class="bat-buoc">*</span></label>
        <input type="text" id="dm-ma" name="MADANHMUC" class="o-nhap" maxlength="15" value="${escHtml(row?.MADANHMUC || '')}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} placeholder="VD: DM01" required />
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="dm-ten">Tên danh mục <span class="bat-buoc">*</span></label>
        <input type="text" id="dm-ten" name="TENDANHMUC" class="o-nhap" maxlength="100" value="${escHtml(row?.TENDANHMUC || '')}" placeholder="Tên nhóm sản phẩm..." required />
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="dm-mota">Mô tả</label>
        <textarea id="dm-mota" name="MOTA" class="o-nhap-nhieu-dong" rows="3" placeholder="Mô tả danh mục...">${escHtml(row?.MOTA || '')}</textarea>
      </div>
    </form>
  `;
}

/** Form Khuyến Mãi */
export function taoHtmlFormKhuyenMai({ row = null, laSua = false }) {
  const ngayBD = row?.NGAYBATDAU ? String(row.NGAYBATDAU).substring(0, 10) : '';
  const ngayKT = row?.NGAYKETTHUC ? String(row.NGAYKETTHUC).substring(0, 10) : '';
  return `
    <form id="form-km" class="bieu-mau" novalidate>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="km-ma">Mã khuyến mãi <span class="bat-buoc">*</span></label>
          <input type="text" id="km-ma" name="MaKM" class="o-nhap" maxlength="15" value="${escHtml(row?.MAKM || '')}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} placeholder="VD: KM001" required />
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="km-ten">Tên khuyến mãi <span class="bat-buoc">*</span></label>
          <input type="text" id="km-ten" name="TenKM" class="o-nhap" maxlength="100" value="${escHtml(row?.TENKM || '')}" placeholder="Tên chương trình..." required />
        </div>
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="km-masp">Mã sản phẩm áp dụng <span class="bat-buoc">*</span></label>
        <input type="text" id="km-masp" name="MaSP" class="o-nhap" maxlength="15" value="${escHtml(row?.MASP || '')}" placeholder="Mã sản phẩm (VD: SP001)..." required />
      </div>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="km-ngaybd">Ngày bắt đầu <span class="bat-buoc">*</span></label>
          <input type="date" id="km-ngaybd" name="NgayBD" class="o-nhap" value="${escHtml(ngayBD)}" required />
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="km-ngaykt">Ngày kết thúc <span class="bat-buoc">*</span></label>
          <input type="date" id="km-ngaykt" name="NgayKT" class="o-nhap" value="${escHtml(ngayKT)}" required />
        </div>
      </div>
      <div id="loi-form-km" style="display:none;color:var(--mau-nguy-hiem)"></div>
    </form>
  `;
}

/** Form Quản lý Tài Khoản Người Dùng (Admin) */
export function taoHtmlFormTaiKhoan({ row = null, laSua = false }) {
  const maTKMoi = row?.MaTaiKhoan || `TK${Date.now().toString().slice(-4)}`;
  return `
    <form id="form-tai-khoan" class="bieu-mau" novalidate>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="tk-ma">Mã tài khoản <span class="bat-buoc">*</span></label>
          <input type="text" id="tk-ma" name="MATAIKHOAN" class="o-nhap" maxlength="20"
            value="${escHtml(maTKMoi)}"
            ${laSua ? 'readonly style="background:var(--xam-100)"' : ''}
            placeholder="VD: TK001" required>
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="tk-username">Tên đăng nhập <span class="bat-buoc">*</span></label>
          <input type="text" id="tk-username" name="USERNAME" class="o-nhap" maxlength="50"
            value="${escHtml(row?.UserName || '')}"
            ${laSua ? 'readonly style="background:var(--xam-100)"' : ''}
            placeholder="Tên đăng nhập..." required>
        </div>
      </div>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="tk-pass">Mật khẩu <span class="bat-buoc">*</span></label>
          <div class="nhom-o-nhap">
            <i class="bi bi-lock bieu-tuong-nhap"></i>
            <input type="password" id="tk-pass" name="PASS" class="o-nhap"
              placeholder="${laSua ? 'Nhập mật khẩu mới' : 'Mật khẩu'}" required autocomplete="new-password">
          </div>
          ${laSua ? '<div class="mo-ta-truong">Bắt buộc nhập để cập nhật.</div>' : ''}
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="tk-quyen">Phân quyền vai trò <span class="bat-buoc">*</span></label>
          <select id="tk-quyen" name="QUYEN" class="o-chon" required>
            <option value="">-- Chọn vai trò --</option>
            <option value="1" ${row?.Quyen == 1 ? 'selected' : ''}>👑 Quản trị viên (Admin)</option>
            <option value="2" ${row?.Quyen == 2 ? 'selected' : ''}>🛒 Thu ngân (Bán hàng POS)</option>
            <option value="3" ${row?.Quyen == 3 ? 'selected' : ''}>📦 Thủ kho (Quản lý kho hàng)</option>
            <option value="4" ${row?.Quyen == 4 ? 'selected' : ''}>📊 Kế toán (Tài chính & Công nợ)</option>
          </select>
        </div>
      </div>
      <div id="loi-form-tk" style="display:none;color:var(--mau-nguy-hiem);font-size:var(--font-binh)"></div>
    </form>
  `;
}

/** Form Quản lý Nhân Viên (Admin) */
export function taoHtmlFormNhanVien({ row = null, laSua = false, anhAvatar = '' }) {
  const maNVMoi = row?.MANV || `NV${Date.now().toString().slice(-4)}`;
  return `
    <form id="form-nv" class="bieu-mau" novalidate>
      <!-- Ảnh đại diện nhân viên -->
      <div class="nhom-truong" style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
        <label class="nhan-truong" style="margin-bottom:6px;">
          <i class="bi bi-person-bounding-box" style="color:var(--mau-chinh);"></i> Ảnh đại diện nhân viên
        </label>
        <div style="display:flex;gap:var(--kc-m);align-items:center;flex-wrap:wrap;">
          <div style="width:76px;height:76px;border-radius:50%;overflow:hidden;border:2px dashed var(--xam-300);background:#fff;display:flex;align-items:center;justify-content:center;position:relative;flex-shrink:0;">
            <img id="nv-preview-img" src="${anhAvatar || ''}" style="width:100%;height:100%;object-fit:cover;${anhAvatar ? '' : 'display:none;'}" alt="Avatar" />
            <div id="nv-preview-placeholder" style="${anhAvatar ? 'display:none;' : ''}text-align:center;color:var(--van-ban-mo);font-size:11px;">
              <i class="bi bi-person" style="font-size:32px;display:block;"></i>
            </div>
          </div>
          <div style="flex:1;min-width:200px;display:flex;flex-direction:column;gap:6px;">
            <div style="display:flex;gap:6px;flex-wrap:wrap;align-items:center;">
              <input type="file" id="nv-file-input" accept="image/*" style="display:none;" />
              <button type="button" id="nv-nut-chon-file" class="nut nut-vien nut-nho">
                <i class="bi bi-upload"></i> Tải ảnh từ máy
              </button>
              <button type="button" id="nv-nut-xoa-anh" class="nut nut-vien nut-nguy-hiem nut-nho" style="${anhAvatar ? '' : 'display:none;'}">
                <i class="bi bi-trash"></i> Xóa ảnh
              </button>
            </div>
            <input type="url" id="nv-anh-url" class="o-nhap" style="font-size:var(--font-nho);padding:6px 10px;" placeholder="Hoặc dán URL ảnh đại diện..." value="${anhAvatar && !anhAvatar.startsWith('data:') ? escHtml(anhAvatar) : ''}" />
            <input type="hidden" id="nv-anh-data" name="HINHANH" value="${escHtml(anhAvatar || '')}" />
          </div>
        </div>
      </div>

      <div class="nhom-truong">
        <label class="nhan-truong" for="nv-ma">Mã nhân viên <span class="bat-buoc">*</span></label>
        <input type="text" id="nv-ma" name="MANV" class="o-nhap" maxlength="15" value="${escHtml(maNVMoi)}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} required />
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="nv-ten">Họ và tên nhân viên <span class="bat-buoc">*</span></label>
        <input type="text" id="nv-ten" name="TENNV" class="o-nhap" maxlength="100" value="${escHtml(row?.TENNV || '')}" placeholder="Nguyễn Văn B..." required />
      </div>
      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="nv-sdt">Số điện thoại</label>
          <input type="tel" id="nv-sdt" name="SDT" class="o-nhap" maxlength="15" value="${escHtml(row?.SDT || '')}" placeholder="09xxxxxxxx..." />
        </div>
        <div class="nhom-truong">
          <label class="nhan-truong" for="nv-email">Email</label>
          <input type="email" id="nv-email" name="EMAIL" class="o-nhap" maxlength="100" value="${escHtml(row?.EMAIL || '')}" placeholder="nhanvien@congty.com..." />
        </div>
      </div>
      <div class="nhom-truong">
        <label class="nhan-truong" for="nv-dc">Địa chỉ</label>
        <textarea id="nv-dc" name="DIACHI" class="o-nhap-nhieu-dong" rows="2" placeholder="Địa chỉ thường trú...">${escHtml(row?.DIACHI || '')}</textarea>
      </div>
      <div id="loi-form-nv" style="display:none;color:var(--mau-nguy-hiem);font-size:var(--font-binh)"></div>
    </form>
  `;
}

/** Form Ghi nhận / Cập nhật Giao dịch Thanh toán */
export function taoHtmlFormThanhToan({ item = null, laSua = false }) {
  const maTT = item?.MaThanhToan || `TT${Date.now().toString().slice(-7)}`;
  const trangThaiHienTai = item?.TrangThai || 'Đã thanh toán';

  return `
    <form id="form-thanh-toan" class="bieu-mau" style="display:flex;flex-direction:column;gap:var(--kc-m);" novalidate>
      <div class="nhom-truong">
        <label class="nhan-truong" for="tt-matt">Mã thanh toán <span class="bat-buoc">*</span></label>
        <input type="text" id="tt-matt" name="MaThanhToan" class="o-nhap" maxlength="15" value="${escHtml(maTT)}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} required />
      </div>

      <div class="nhom-truong">
        <label class="nhan-truong" for="tt-mahd">Mã hóa đơn <span class="bat-buoc">*</span></label>
        <input type="text" id="tt-mahd" name="MaHDBan" class="o-nhap" maxlength="15" value="${escHtml(item?.MaHDBan || '')}" ${laSua ? 'readonly style="background:var(--xam-100)"' : ''} placeholder="VD: HDB001..." required />
      </div>

      <div class="luoi-bieu-mau hai-cot">
        <div class="nhom-truong">
          <label class="nhan-truong" for="tt-phuongthuc">Phương thức <span class="bat-buoc">*</span></label>
          <select id="tt-phuongthuc" name="PhuongThuc" class="o-chon" required>
            <option value="Tiền mặt" ${item?.PhuongThuc === 'Tiền mặt' ? 'selected' : ''}>💵 Tiền mặt</option>
            <option value="Chuyển khoản" ${item?.PhuongThuc === 'Chuyển khoản' ? 'selected' : ''}>🏦 Chuyển khoản</option>
            <option value="Thẻ" ${item?.PhuongThuc === 'Thẻ' ? 'selected' : ''}>💳 Thẻ ngân hàng</option>
          </select>
        </div>

        <div class="nhom-truong">
          <label class="nhan-truong" for="tt-trangthai">Trạng thái <span class="bat-buoc">*</span></label>
          <select id="tt-trangthai" name="TrangThai" class="o-chon" required>
            <option value="Đã thanh toán" ${trangThaiHienTai === 'Đã thanh toán' ? 'selected' : ''}>Đã thanh toán</option>
            <option value="Chưa thanh toán" ${trangThaiHienTai === 'Chưa thanh toán' ? 'selected' : ''}>Chưa thanh toán</option>
          </select>
        </div>
      </div>

      <div class="nhom-truong">
        <label class="nhan-truong" for="tt-sotien">Số tiền thanh toán (VNĐ) <span class="bat-buoc">*</span></label>
        <input type="number" id="tt-sotien" name="SoTienThanhToan" class="o-nhap" min="1" step="1000" value="${item?.SoTienThanhToan || 0}" required />
      </div>
    </form>
  `;
}
