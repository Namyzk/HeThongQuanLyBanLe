/**
 * giao-dien-chi-tiet.js — Module HTML thiết kế Modal xem chi tiết các đối tượng
 */
import { escHtml, formatTien, formatNgay } from '../tien-ich.js';
import { SVG_FALLBACK_SP, SVG_FALLBACK_NV, taoPlaceholderSanPham, taoPlaceholderAvatar } from '../quan-ly-anh.js';

/**
 * Trả về chuỗi HTML xem chi tiết Hóa đơn bán hàng
 * @param {object} params
 * @returns {string}
 */
export function taoHtmlChiTietHoaDon({ hd, chiTiet = [], maHD = '' }) {
  const tienHang = Number(hd.TONGTIENHANG) || 0;
  const thue = Number(hd.THUEVAT) || 0;
  const giam = Number(hd.GIAMGIA) || 0;
  const tongCong = Math.max(0, tienHang + thue - giam);

  return `
    <div id="vung-xem-in-hd" style="display:flex;flex-direction:column;gap:var(--kc-m);">
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:var(--kc-m);background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);font-size:var(--font-binh);">
        <div><strong>Mã HĐ:</strong> <span class="badge badge-lam" style="margin-left:4px;">${escHtml(hd.MAHDBAN || maHD)}</span></div>
        <div><strong>Ngày lập:</strong> ${formatNgay(hd.NGAYLAP)}</div>
        <div><strong>Nhân viên bán:</strong> ${escHtml(hd.MANV || '—')}</div>
        <div><strong>Khách hàng:</strong> ${escHtml(hd.MAKH || 'Khách vãng lai')}</div>
      </div>

      <h4 style="margin:var(--kc-xs) 0 0 0;font-size:var(--font-binh);font-weight:700;display:flex;align-items:center;gap:6px;">
        <i class="bi bi-basket" style="color:var(--mau-chinh);"></i> Danh sách sản phẩm trong đơn
      </h4>

      <div class="bang-khung" style="max-height:250px;overflow-y:auto;border:1px solid var(--duong-vien);border-radius:var(--bo-goc-vua);">
        <table class="bang">
          <thead>
            <tr>
              <th>Mã SP</th>
              <th>Tên SP</th>
              <th class="van-ban-phai">Số lượng</th>
              <th class="van-ban-phai">Đơn giá</th>
              <th class="van-ban-phai">Thành tiền</th>
            </tr>
          </thead>
          <tbody>
            ${chiTiet.length === 0 ? '<tr><td colspan="5" class="van-ban-trung" style="padding:var(--kc-l);color:var(--van-ban-mo);">Chưa có chi tiết sản phẩm</td></tr>' : chiTiet.map(ct => `
              <tr>
                <td><strong>${escHtml(ct.MASP)}</strong></td>
                <td>${escHtml(ct.TenSP || ct.TENSP || '—')}</td>
                <td class="van-ban-phai" style="font-weight:600;">${ct.SOLUONG}</td>
                <td class="van-ban-phai">${formatTien(ct.DONGIA)}</td>
                <td class="van-ban-phai" style="font-weight:700;color:var(--mau-chinh);">${formatTien(ct.TONGTIEN || (ct.SOLUONG * ct.DONGIA))}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      </div>

      <div style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);display:flex;flex-direction:column;gap:6px;font-size:var(--font-binh);">
        <div style="display:flex;justify-content:space-between;">
          <span style="color:var(--van-ban-phu);">Tiền hàng:</span>
          <span>${formatTien(tienHang)}</span>
        </div>
        <div style="display:flex;justify-content:space-between;">
          <span style="color:var(--van-ban-phu);">Thuế VAT:</span>
          <span>${formatTien(thue)}</span>
        </div>
        <div style="display:flex;justify-content:space-between;">
          <span style="color:var(--van-ban-phu);">Giảm giá:</span>
          <span>${Number(giam) > 0 ? `-${formatTien(giam)}` : '0 đ'}</span>
        </div>
        <div style="display:flex;justify-content:space-between;font-weight:800;font-size:var(--font-lon);color:var(--mau-chinh);border-top:1px dashed var(--xam-300);padding-top:8px;margin-top:2px;">
          <span>TỔNG THANH TOÁN:</span>
          <span>${formatTien(tongCong)}</span>
        </div>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML xem chi tiết Khách hàng
 * @param {object} kh
 * @returns {string}
 */
export function taoHtmlChiTietKhachHang(kh) {
  return `
    <div style="display:flex;flex-direction:column;gap:var(--kc-m);padding:var(--kc-xs);">
      <div style="display:flex;align-items:center;gap:var(--kc-m);margin-bottom:var(--kc-s);">
        <div style="width:54px;height:54px;border-radius:50%;background:linear-gradient(135deg,var(--mau-chinh),#818cf8);display:flex;align-items:center;justify-content:center;color:#fff;font-size:24px;font-weight:700;box-shadow:0 4px 12px rgba(79,70,229,0.3);">
          ${(kh.TenKH || 'K').charAt(0).toUpperCase()}
        </div>
        <div>
          <h3 style="margin:0;font-size:var(--font-lon);font-weight:700;">${escHtml(kh.TenKH)}</h3>
          <span class="badge badge-lam" style="margin-top:4px;">${escHtml(kh.MaKH)}</span>
        </div>
      </div>

      <div style="display:grid;grid-template-columns:1fr;gap:var(--kc-s);background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);font-size:var(--font-binh);">
        <div><i class="bi bi-telephone" style="color:var(--mau-chinh);margin-right:6px;"></i> <strong>Số điện thoại:</strong> ${escHtml(kh.SDT || '—')}</div>
        <div><i class="bi bi-geo-alt" style="color:var(--mau-chinh);margin-right:6px;"></i> <strong>Địa chỉ:</strong> ${escHtml(kh.DiaChi || '—')}</div>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML xem chi tiết Nhà cung cấp
 * @param {object} ncc
 * @returns {string}
 */
export function taoHtmlChiTietNhaCungCap(ncc) {
  return `
    <div style="display:flex;flex-direction:column;gap:var(--kc-m);">
      <div style="display:flex;align-items:center;gap:var(--kc-m);">
        <div style="width:52px;height:52px;border-radius:var(--bo-goc-vua);background:linear-gradient(135deg,#0ea5e9,#6366f1);display:flex;align-items:center;justify-content:center;color:#fff;font-size:24px;">
          <i class="bi bi-truck"></i>
        </div>
        <div>
          <h3 style="margin:0;font-size:var(--font-lon);font-weight:700;">${escHtml(ncc.TENNCC)}</h3>
          <span class="badge badge-lam" style="margin-top:4px;">${escHtml(ncc.MANCC)}</span>
        </div>
      </div>

      <div style="display:grid;grid-template-columns:1fr;gap:var(--kc-s);background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);font-size:var(--font-binh);">
        <div><strong>Hotline / SĐT:</strong> ${escHtml(ncc.SDT || '—')}</div>
        <div><strong>Địa chỉ trụ sở:</strong> ${escHtml(ncc.DIACHI || '—')}</div>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML xem chi tiết Giao dịch thanh toán
 * @param {object} tt
 * @returns {string}
 */
export function taoHtmlChiTietThanhToan(tt) {
  const maGiaoDich = tt.MaThanhToan || tt.MaTT || '—';
  const daTT = tt.TrangThai === 'Đã thanh toán';
  const badgeCls = daTT ? 'badge-xanh' : 'badge-vang';

  return `
    <div style="display:flex;flex-direction:column;gap:var(--kc-m);font-size:var(--font-binh);">
      <div style="background:var(--xam-50);padding:var(--kc-l);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);display:flex;flex-direction:column;gap:var(--kc-s);">
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>Mã giao dịch:</span>
          <strong>${escHtml(maGiaoDich)}</strong>
        </div>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>Mã hóa đơn gốc:</span>
          <span class="badge badge-lam">${escHtml(tt.MaHDBan || '—')}</span>
        </div>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>Ngày nộp tiền:</span>
          <span>${formatNgay(tt.NgayThanhToan)}</span>
        </div>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>Phương thức:</span>
          <strong>${escHtml(tt.PhuongThuc || 'Tiền mặt')}</strong>
        </div>
        <div style="display:flex;justify-content:space-between;align-items:center;">
          <span>Trạng thái:</span>
          <span class="badge ${badgeCls}">${escHtml(tt.TrangThai || 'Chưa thanh toán')}</span>
        </div>
        <div style="display:flex;justify-content:space-between;border-top:1px dashed var(--xam-300);padding-top:10px;margin-top:4px;font-size:var(--font-lon);font-weight:800;color:var(--mau-chinh);">
          <span>Số tiền giao dịch:</span>
          <span>${formatTien(tt.SoTienThanhToan)}</span>
        </div>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML xem chi tiết Nhân viên (kèm ảnh đại diện và nút upload ảnh nhanh)
 * @param {object} params
 * @param {object} params.nv - Dữ liệu nhân viên
 * @param {string} params.anhAvatar - URL hoặc dataURL ảnh đại diện
 * @returns {string}
 */
export function taoHtmlChiTietNhanVien({ nv, anhAvatar = '' }) {
  const ten = nv.TENNV || 'Nhân viên';
  const ma = nv.MANV || '—';
  const sdt = nv.SDT || '—';
  const email = nv.EMAIL || '—';
  const diaChi = nv.DIACHI || '—';

  return `
    <div class="chi-tiet-nv-khung" style="display:flex;flex-direction:column;gap:var(--kc-l);">
      <!-- Header thông tin nhân viên kèm avatar -->
      <div style="display:flex;align-items:center;gap:var(--kc-l);background:linear-gradient(135deg,var(--xam-50),var(--mau-chinh-sang));padding:var(--kc-l);border-radius:var(--bo-goc-lon);border:1px solid var(--mau-chinh-nhat);flex-wrap:wrap;">
        <div style="position:relative;flex-shrink:0;">
          <img
            id="ct-nv-avatar-img"
            src="${escHtml(anhAvatar)}"
            alt="${escHtml(ten)}"
            style="width:96px;height:96px;border-radius:50%;object-fit:cover;border:3px solid #ffffff;box-shadow:var(--bong-vua);background:#ffffff;"
            onerror="this.onerror=null;this.src='${taoPlaceholderAvatar(nv?.TENNV, nv?.MANV)}';"
          />
          <button
            type="button"
            id="ct-nv-nut-doi-anh-nhanh"
            class="nut nut-chinh nut-nho"
            style="position:absolute;bottom:0;right:0;border-radius:50%;width:32px;height:32px;padding:0;display:flex;align-items:center;justify-content:center;box-shadow:var(--bong-nho);"
            title="Tải lên ảnh đại diện mới"
            aria-label="Tải lên ảnh đại diện mới"
          >
            <i class="bi bi-camera"></i>
          </button>
        </div>

        <div style="flex:1;min-width:200px;">
          <div style="display:flex;align-items:center;gap:var(--kc-s);flex-wrap:wrap;margin-bottom:4px;">
            <h3 style="margin:0;font-size:1.35rem;font-weight:800;color:var(--van-ban-chinh);">${escHtml(ten)}</h3>
            <span class="badge badge-lam" style="font-weight:700;">${escHtml(ma)}</span>
            <span class="badge badge-xanh"><i class="bi bi-check-circle"></i> Đang hoạt động</span>
          </div>
          <div style="color:var(--van-ban-phu);font-size:var(--font-binh);display:flex;align-items:center;gap:var(--kc-m);flex-wrap:wrap;margin-top:6px;">
            <span><i class="bi bi-telephone" style="color:var(--mau-chinh);"></i> ${escHtml(sdt)}</span>
            ${email !== '—' ? `<span><i class="bi bi-envelope" style="color:var(--mau-chinh);"></i> ${escHtml(email)}</span>` : ''}
          </div>
          <div style="margin-top:10px;display:flex;gap:var(--kc-s);align-items:center;">
            <input type="file" id="ct-nv-input-file" accept="image/*" style="display:none;" />
            <button type="button" id="ct-nv-nut-chon-anh" class="nut nut-vien nut-nho">
              <i class="bi bi-upload"></i> Đổi ảnh đại diện
            </button>
            <button type="button" id="ct-nv-nut-xoa-anh" class="nut nut-vien nut-nguy-hiem nut-nho" title="Xóa ảnh về mặc định">
              <i class="bi bi-trash"></i> Xóa ảnh
            </button>
          </div>
        </div>
      </div>

      <!-- Lưới thông tin chi tiết -->
      <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(240px, 1fr));gap:var(--kc-m);">
        <div style="background:var(--xam-50);padding:var(--kc-m) var(--kc-l);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
          <div style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">Mã nhân viên</div>
          <div style="font-size:var(--font-binh);font-weight:700;color:var(--mau-chinh);margin-top:2px;">${escHtml(ma)}</div>
        </div>

        <div style="background:var(--xam-50);padding:var(--kc-m) var(--kc-l);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
          <div style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">Họ và tên</div>
          <div style="font-size:var(--font-binh);font-weight:700;color:var(--van-ban-chinh);margin-top:2px;">${escHtml(ten)}</div>
        </div>

        <div style="background:var(--xam-50);padding:var(--kc-m) var(--kc-l);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
          <div style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">Số điện thoại</div>
          <div style="font-size:var(--font-binh);font-weight:600;color:var(--van-ban-chinh);margin-top:2px;">
            ${sdt !== '—' ? `<a href="tel:${escHtml(sdt)}" style="color:var(--mau-chinh);text-decoration:none;">${escHtml(sdt)}</a>` : '—'}
          </div>
        </div>

        <div style="background:var(--xam-50);padding:var(--kc-m) var(--kc-l);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
          <div style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">Email công việc</div>
          <div style="font-size:var(--font-binh);font-weight:600;color:var(--van-ban-chinh);margin-top:2px;">
            ${email !== '—' ? `<a href="mailto:${escHtml(email)}" style="color:var(--mau-chinh);text-decoration:none;">${escHtml(email)}</a>` : '<span style="color:var(--van-ban-mo);">Chưa cập nhật</span>'}
          </div>
        </div>
      </div>

      <div style="background:var(--xam-50);padding:var(--kc-m) var(--kc-l);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
        <div style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;margin-bottom:2px;">Địa chỉ thường trú / liên hệ</div>
        <div style="font-size:var(--font-binh);color:var(--van-ban-chinh);display:flex;align-items:flex-start;gap:6px;">
          <i class="bi bi-geo-alt" style="color:var(--mau-chinh);margin-top:2px;"></i>
          <span>${escHtml(diaChi)}</span>
        </div>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML xem chi tiết Sản phẩm (kèm ảnh sản phẩm và thông tin chi tiết)
 * @param {object} params
 * @param {object} params.sp - Dữ liệu sản phẩm
 * @param {Array} params.danhMuc - Danh sách danh mục
 * @param {string} params.anhSP - URL hoặc dataURL ảnh sản phẩm
 * @returns {string}
 */
export function taoHtmlChiTietSanPham({ sp, danhMuc = [], anhSP = '' }) {
  const maSP = sp.MASP || '—';
  const tenSP = sp.TENSP || 'Sản phẩm';
  const donGia = Number(sp.DONGIA) || 0;
  const tonKho = Number(sp.SOLUONGTON) || 0;
  const maVach = sp.MAVACH || '—';
  const thue = sp.THUE ?? 0;
  const thuocTinh = sp.THUOCTINH || '—';
  const moTa = sp.MOTA || 'Chưa có thông tin mô tả chi tiết cho sản phẩm này.';

  // Tìm tên danh mục
  const dmObj = danhMuc.find(dm => dm.MADANHMUC === sp.MADANHMUC);
  const tenDM = dmObj ? (dmObj.TENDANHMUC || dmObj.MADANHMUC) : (sp.MADANHMUC || 'Chưa phân loại');

  // Trạng thái tồn kho badge
  let badgeTonKho = '';
  if (tonKho <= 0) {
    badgeTonKho = '<span class="badge badge-do"><i class="bi bi-x-circle"></i> Hết hàng (0)</span>';
  } else if (tonKho <= 5) {
    badgeTonKho = `<span class="badge badge-vang"><i class="bi bi-exclamation-triangle"></i> Sắp hết (${tonKho})</span>`;
  } else {
    badgeTonKho = `<span class="badge badge-xanh"><i class="bi bi-check-circle"></i> Sẵn sàng bán (${tonKho})</span>`;
  }

  return `
    <div class="chi-tiet-sp-khung" style="display:grid;grid-template-columns:300px 1fr;gap:var(--kc-xl);align-items:start;">
      <!-- Cột trái: Khung hiển thị ảnh sản phẩm lớn -->
      <div style="display:flex;flex-direction:column;gap:var(--kc-m);">
        <div style="width:100%;height:280px;border-radius:var(--bo-goc-lon);overflow:hidden;border:1px solid var(--duong-vien);background:var(--xam-50);box-shadow:var(--bong-nho);position:relative;display:flex;align-items:center;justify-content:center;">
          <img
            id="ct-sp-img"
            src="${escHtml(anhSP)}"
            alt="${escHtml(tenSP)}"
            style="width:100%;height:100%;object-fit:cover;transition:transform 0.3s ease;"
            onerror="this.onerror=null;this.src='${taoPlaceholderSanPham(sp?.TENSP, sp?.MASP)}';"
          />
          <div style="position:absolute;top:10px;left:10px;">
            <span class="badge badge-lam" style="font-weight:700;box-shadow:var(--bong-nho);">${escHtml(maSP)}</span>
          </div>
          <div style="position:absolute;top:10px;right:10px;">
            ${badgeTonKho}
          </div>
        </div>

        <div style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);display:flex;justify-content:space-between;align-items:center;">
          <span style="font-size:var(--font-nho);color:var(--van-ban-phu);">Danh mục:</span>
          <strong style="color:var(--van-ban-chinh);">${escHtml(tenDM)}</strong>
        </div>
      </div>

      <!-- Cột phải: Thông tin chi tiết toàn diện -->
      <div style="display:flex;flex-direction:column;gap:var(--kc-m);">
        <div>
          <div style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;letter-spacing:0.05em;">Thông tin sản phẩm</div>
          <h2 style="margin:4px 0 8px 0;font-size:1.4rem;font-weight:800;color:var(--van-ban-chinh);line-height:1.3;">
            ${escHtml(tenSP)}
          </h2>
          <div style="display:flex;align-items:center;gap:var(--kc-m);flex-wrap:wrap;">
            <div style="font-size:1.6rem;font-weight:800;color:var(--mau-chinh);">
              ${formatTien(donGia)}
            </div>
            <div style="font-size:var(--font-nho);color:var(--van-ban-phu);background:var(--mau-chinh-sang);padding:4px 10px;border-radius:var(--bo-goc-tron);font-weight:600;">
              VAT: ${thue}%
            </div>
          </div>
        </div>

        <div style="display:grid;grid-template-columns:1fr 1fr;gap:var(--kc-s);background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);font-size:var(--font-binh);">
          <div>
            <span style="color:var(--van-ban-mo);display:block;font-size:var(--font-xs);font-weight:700;text-transform:uppercase;">Mã vạch (Barcode)</span>
            <div style="font-weight:600;margin-top:2px;display:flex;align-items:center;gap:6px;">
              <i class="bi bi-upc-scan" style="color:var(--mau-chinh);"></i> ${escHtml(maVach)}
            </div>
          </div>
          <div>
            <span style="color:var(--van-ban-mo);display:block;font-size:var(--font-xs);font-weight:700;text-transform:uppercase;">Tồn kho thực tế</span>
            <div style="font-weight:700;margin-top:2px;color:var(--van-ban-chinh);">
              ${tonKho} đơn vị
            </div>
          </div>
        </div>

        <div style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);font-size:var(--font-binh);">
          <span style="color:var(--van-ban-mo);display:block;font-size:var(--font-xs);font-weight:700;text-transform:uppercase;margin-bottom:4px;">Thuộc tính / Quy cách</span>
          <div style="color:var(--van-ban-chinh);font-weight:500;">
            ${escHtml(thuocTinh)}
          </div>
        </div>

        <div style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);font-size:var(--font-binh);">
          <span style="color:var(--van-ban-mo);display:block;font-size:var(--font-xs);font-weight:700;text-transform:uppercase;margin-bottom:4px;">Mô tả sản phẩm</span>
          <div style="color:var(--van-ban-chinh);line-height:1.6;white-space:pre-line;">
            ${escHtml(moTa)}
          </div>
        </div>
      </div>
    </div>
  `;
}

