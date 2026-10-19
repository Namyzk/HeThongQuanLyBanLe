
import { escHtml, formatTien } from '../tien-ich.js';
import { layAnhSanPham, SVG_FALLBACK_SP, taoPlaceholderSanPham } from '../quan-ly-anh.js';

/**
 * Trả về chuỗi HTML khung giao diện POS 2 cột
 * @param {object} options
 * @param {Array} options.danhMuc
 * @param {Array} options.khachHang
 * @param {string} options.maHD
 * @param {string} options.maNV
 * @returns {string}
 */
export function taoHtmlLayoutBanHang({ danhMuc = [], khachHang = [], nhanVien = [], maHD = '', maNV = '' }) {
  return `
    <div class="khung-pos fade-in">
      <!-- CỘT TRÁI: DANH SÁCH MẶT HÀNG & BỘ LỌC -->
      <div class="the pos-hang-hoa" style="display:flex;flex-direction:column;gap:var(--kc-m);padding:var(--kc-l);">
        <!-- Thanh công cụ tìm kiếm & lọc danh mục -->
        <div style="display:flex;gap:var(--kc-m);flex-wrap:wrap;align-items:center;">
          <div style="flex:1;position:relative;min-width:240px;">
            <i class="bi bi-search" style="position:absolute;left:14px;top:50%;transform:translateY(-50%);color:var(--xam-400);"></i>
            <input
              type="text"
              id="pos-tim-kiem"
              class="o-nhap"
              placeholder="Tìm tên hàng, mã SP hoặc quét mã vạch..."
              style="padding-left:40px;background:var(--xam-50);"
            />
          </div>
          <!-- Tabs lọc theo danh mục -->
          <div style="display:flex;gap:var(--kc-s);overflow-x:auto;padding-bottom:4px;max-width:100%;" id="pos-danh-muc-tabs">
            <button type="button" class="nut nut-nho pos-tab active" data-dm="tat-ca">Tất cả</button>
            ${danhMuc.map(dm => `
              <button type="button" class="nut nut-nho nut-vien pos-tab" data-dm="${escHtml(dm.MADANHMUC)}">
                ${escHtml(dm.TENDANHMUC || dm.MADANHMUC)}
              </button>
            `).join('')}
          </div>
        </div>

        <!-- Lưới sản phẩm -->
        <div id="pos-luoi-sp" style="display:grid;grid-template-columns:repeat(auto-fill, minmax(185px, 1fr));gap:var(--kc-m);max-height:calc(100vh - 250px);overflow-y:auto;padding-right:4px;">
        </div>
      </div>

      <!-- CỘT PHẢI: GIỎ HÀNG & THỦ TỤC THANH TOÁN -->
      <div class="the pos-don-hang" style="padding:var(--kc-l);display:flex;flex-direction:column;gap:var(--kc-m);box-shadow:var(--bong-lon);">
        <div style="display:flex;justify-content:space-between;align-items:center;border-bottom:1px solid var(--duong-vien);padding-bottom:var(--kc-s);">
          <div style="display:flex;align-items:center;gap:var(--kc-s);">
            <div style="width:34px;height:34px;border-radius:var(--bo-goc-vua);background:var(--mau-chinh-sang);color:var(--mau-chinh);display:flex;align-items:center;justify-content:center;font-size:18px;">
              <i class="bi bi-cart3"></i>
            </div>
            <h3 style="margin:0;font-size:var(--font-lon);font-weight:700;">Hóa đơn bán hàng</h3>
          </div>
          <button type="button" class="nut nut-vien nut-nho nut-nguy-hiem" id="pos-nut-xoa-gio" title="Xóa toàn bộ sản phẩm trong giỏ">
            <i class="bi bi-trash"></i> Làm trống
          </button>
        </div>

        <!-- Thông tin khách hàng & tạo nhanh -->
        <div style="display:grid;grid-template-columns:1fr auto;gap:var(--kc-s);align-items:center;">
          <select id="pos-chon-kh" class="o-chon" style="font-size:var(--font-binh);padding:8px 12px;">
            <option value="">-- Khách vãng lai (Mặc định) --</option>
            ${khachHang.map(kh => `
              <option value="${escHtml(kh.MaKH)}">${escHtml(kh.TenKH)} — ${escHtml(kh.SDT || kh.MaKH)}</option>
            `).join('')}
          </select>
          <button type="button" class="nut nut-phu nut-nho" id="pos-them-nhanh-kh" title="Thêm nhanh khách hàng mới">
            <i class="bi bi-person-plus"></i>
          </button>
        </div>

        <!-- Thông tin mã đơn & nhân viên -->
        <div style="display:grid;grid-template-columns:1fr 1fr;gap:var(--kc-s);font-size:var(--font-nho);background:var(--xam-50);padding:8px 12px;border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
          <div>
            <span style="color:var(--van-ban-mo);display:block;font-size:11px;">MÃ HÓA ĐƠN</span>
            <input type="text" id="pos-ma-hd" class="o-nhap" style="padding:4px 8px;font-size:var(--font-nho);font-weight:700;margin-top:2px;" value="${escHtml(maHD)}" />
          </div>
          <div>
            <span style="color:var(--van-ban-mo);display:block;font-size:11px;">NHÂN VIÊN BÁN</span>
            <select id="pos-ma-nv" class="o-chon" style="padding:4px 8px;font-size:var(--font-nho);margin-top:2px;" required>
              <option value="">-- Chọn nhân viên --</option>
              ${nhanVien.map(nv => `
                <option value="${escHtml(nv.MANV)}" ${nv.MANV === maNV ? 'selected' : ''}>
                  ${escHtml(nv.TENNV)} (${escHtml(nv.MANV)})
                </option>
              `).join('')}
            </select>
          </div>
        </div>

        <!-- Danh sách mặt hàng trong giỏ -->
        <div style="border-bottom:1px solid var(--duong-vien);padding-bottom:var(--kc-xs);">
          <div style="display:flex;justify-content:space-between;font-size:11px;font-weight:700;color:var(--van-ban-mo);text-transform:uppercase;padding:0 4px 6px 4px;">
            <span>Sản phẩm</span>
            <span style="margin-left:auto;margin-right:20px;">Số lượng</span>
            <span>Thành tiền</span>
          </div>
          <div id="pos-danh-sach-gio" style="max-height:220px;overflow-y:auto;display:flex;flex-direction:column;gap:4px;padding-right:2px;">
          </div>
        </div>

        <!-- Tóm tắt thanh toán & thuế VAT / Giảm giá -->
        <div style="display:flex;flex-direction:column;gap:6px;font-size:var(--font-binh);background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);">
          <div style="display:flex;justify-content:space-between;">
            <span style="color:var(--van-ban-phu);">Tiền hàng:</span>
            <span id="pos-tien-hang" style="font-weight:600;">0 đ</span>
          </div>
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:var(--kc-s);align-items:center;">
            <div style="display:flex;align-items:center;gap:4px;">
              <span style="color:var(--van-ban-phu);font-size:var(--font-nho);">VAT (%):</span>
              <input type="number" id="pos-vat" class="o-nhap" min="0" max="100" value="0" style="padding:4px 8px;width:60px;font-size:var(--font-nho);text-align:center;" />
            </div>
            <div style="display:flex;align-items:center;gap:4px;justify-content:flex-end;">
              <span style="color:var(--van-ban-phu);font-size:var(--font-nho);">Giảm giá:</span>
              <input type="number" id="pos-giam-gia" class="o-nhap" min="0" step="1000" value="0" style="padding:4px 8px;width:95px;font-size:var(--font-nho);text-align:right;" />
            </div>
          </div>
          <div style="display:flex;justify-content:space-between;align-items:center;border-top:1px dashed var(--xam-300);padding-top:8px;margin-top:2px;">
            <span style="font-weight:700;font-size:var(--font-lon);">KHÁCH PHẢI TRẢ:</span>
            <span id="pos-tong-cong" style="font-weight:800;font-size:1.45rem;color:var(--mau-chinh);">0 đ</span>
          </div>
        </div>

        <!-- Phương thức thanh toán & Tiền khách đưa -->
        <div style="display:flex;flex-direction:column;gap:var(--kc-s);">
          <div style="display:grid;grid-template-columns:1fr 1fr;gap:var(--kc-s);">
            <select id="pos-pt-thanh-toan" class="o-chon" style="padding:8px 12px;font-size:var(--font-nho);">
              <option value="Tiền mặt">💵 Tiền mặt</option>
              <option value="Thanh toán QR">📱 Thanh toán QR</option>
              <option value="Ghi nợ">📝 Ghi nợ (Khách quen)</option>
            </select>
            <input type="number" id="pos-tien-khach-dua" class="o-nhap" placeholder="Tiền khách đưa..." style="padding:8px 12px;font-size:var(--font-nho);text-align:right;" />
          </div>

          <!-- Nút tạo hóa đơn và mở QR PayOS khi chọn thanh toán QR -->
          <div id="pos-vung-nut-mo-qr" style="display:none;margin-top:2px;">
            <button type="button" id="pos-nut-xem-qr-pos" class="pos-nut-mo-qr" style="width:100%;">
              <i class="bi bi-qr-code"></i> Tạo hóa đơn &amp; mở QR PayOS
            </button>
          </div>

          <!-- Nút tiền nhanh hỗ trợ thu ngân -->
          <div style="display:flex;gap:4px;flex-wrap:wrap;">
            <button type="button" class="pos-nut-tien-nhanh" data-tien="100000">100k</button>
            <button type="button" class="pos-nut-tien-nhanh" data-tien="200000">200k</button>
            <button type="button" class="pos-nut-tien-nhanh" data-tien="500000">500k</button>
            <button type="button" class="pos-nut-tien-nhanh" id="pos-nut-tien-du" style="background:var(--mau-chinh-sang);color:var(--mau-chinh-dam);border-color:var(--mau-chinh-nhat);">Đúng số tiền</button>
          </div>

          <div style="display:flex;justify-content:space-between;font-size:var(--font-binh);padding:4px 2px;">
            <span style="color:var(--van-ban-phu);">Số tiền trả lại:</span>
            <span id="pos-tien-thua" style="font-weight:700;color:var(--mau-thanh-cong);">0 đ</span>
          </div>
        </div>

        <!-- Nút Thanh toán lớn (F9) -->
        <button type="button" id="pos-nut-thanh-toan" class="nut nut-chinh nut-lon" style="width:100%;font-size:var(--font-vua);padding:14px;border-radius:var(--bo-goc-vua);font-weight:700;">
          <i class="bi bi-check2-circle" style="font-size:1.3rem;"></i> THANH TOÁN (F9)
        </button>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML danh sách sản phẩm trong lưới
 * @param {Array} dsLoc
 * @returns {string}
 */
export function taoHtmlLuoiSanPham(dsLoc = []) {
  if (dsLoc.length === 0) {
    return `
      <div style="grid-column:1/-1;text-align:center;padding:var(--kc-xxl);color:var(--van-ban-phu);">
        <i class="bi bi-box" style="font-size:2.5rem;color:var(--xam-300);"></i>
        <p style="margin-top:var(--kc-s);font-weight:500;">Không tìm thấy sản phẩm phù hợp</p>
      </div>
    `;
  }

  return dsLoc.map(sp => {
    const tonKho = Number(sp.SOLUONGTON) || 0;
    const hetHang = tonKho <= 0;
    const anhSP = layAnhSanPham(sp.MASP, sp);
    const badgeTonKhoCls = tonKho > 5 ? 'badge-xanh' : (tonKho > 0 ? 'badge-vang' : 'badge-do');

    return `
      <div class="pos-sp-the ${hetHang ? 'het-hang' : ''}" data-ma="${escHtml(sp.MASP)}" style="display:flex;flex-direction:column;justify-content:space-between;padding:0;overflow:hidden;border-radius:var(--bo-goc-lon);border:1px solid var(--duong-vien);background:var(--nen-the);box-shadow:var(--bong-nho);transition:all 0.2s ease;cursor:pointer;">
        <!-- Khung ảnh sản phẩm cao cấp -->
        <div style="position:relative;width:100%;height:118px;background:var(--xam-50);overflow:hidden;display:flex;align-items:center;justify-content:center;">
          <img
            src="${escHtml(anhSP)}"
            alt="${escHtml(sp.TENSP)}"
            loading="lazy"
            style="width:100%;height:100%;object-fit:cover;transition:transform 0.3s ease;"
            class="pos-sp-the-img"
            onerror="this.onerror=null;this.src='${taoPlaceholderSanPham(sp?.TENSP, sp?.MASP)}';"
          />
          <!-- Badge mã SP -->
          <span style="position:absolute;top:8px;left:8px;background:rgba(15,23,42,0.72);backdrop-filter:blur(4px);color:#fff;font-size:10px;font-weight:700;padding:2px 6px;border-radius:var(--bo-goc-xs);letter-spacing:0.04em;">
            ${escHtml(sp.MASP)}
          </span>
          <!-- Badge tồn kho -->
          <span class="badge ${badgeTonKhoCls}" style="position:absolute;top:8px;right:8px;font-size:10px;padding:2px 6px;box-shadow:0 2px 4px rgba(0,0,0,0.12);">
            ${hetHang ? 'Hết hàng' : `Tồn: ${tonKho}`}
          </span>
          <!-- Nút xem nhanh chi tiết -->
          <button
            type="button"
            class="pos-nut-xem-nhanh"
            data-ma="${escHtml(sp.MASP)}"
            title="Xem chi tiết sản phẩm"
            style="position:absolute;bottom:6px;right:6px;background:rgba(255,255,255,0.92);border:none;border-radius:50%;width:26px;height:26px;display:flex;align-items:center;justify-content:center;cursor:pointer;color:var(--van-ban-chinh);box-shadow:0 2px 6px rgba(0,0,0,0.15);transition:all 0.15s ease;"
          >
            <i class="bi bi-eye" style="font-size:12px;"></i>
          </button>
        </div>

        <!-- Thông tin sản phẩm -->
        <div style="padding:10px 12px 12px 12px;display:flex;flex-direction:column;flex:1;justify-content:space-between;">
          <div>
            <div style="font-weight:600;font-size:var(--font-binh);color:var(--van-ban-chinh);line-height:1.35;margin-bottom:4px;display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;min-height:36px;" title="${escHtml(sp.TENSP)}">
              ${escHtml(sp.TENSP)}
            </div>
            ${sp.MAVACH ? `<div style="font-size:10px;color:var(--van-ban-mo);margin-bottom:6px;display:flex;align-items:center;gap:4px;"><i class="bi bi-upc"></i> ${escHtml(sp.MAVACH)}</div>` : ''}
          </div>

          <div style="display:flex;justify-content:space-between;align-items:center;margin-top:6px;padding-top:8px;border-top:1px solid var(--duong-vien-nhe);">
            <div style="font-weight:800;color:var(--mau-chinh);font-size:var(--font-vua);">
              ${formatTien(sp.DONGIA)}
            </div>
            <button
              type="button"
              class="nut nut-chinh nut-nho nut-them-sp-pos"
              data-ma="${escHtml(sp.MASP)}"
              ${hetHang ? 'disabled' : ''}
              style="border-radius:50%;width:32px;height:32px;padding:0;display:flex;align-items:center;justify-content:center;box-shadow:var(--bong-nho);"
              title="Thêm vào giỏ"
            >
              <i class="bi bi-plus-lg"></i>
            </button>
          </div>
        </div>
      </div>
    `;
  }).join('');
}

/**
 * Trả về chuỗi HTML danh sách giỏ hàng
 * @param {Array} gioHang
 * @returns {string}
 */
export function taoHtmlGioHang(gioHang = []) {
  if (gioHang.length === 0) {
    return `
      <div style="text-align:center;padding:var(--kc-xl) var(--kc-m);color:var(--van-ban-phu);">
        <i class="bi bi-cart-x" style="font-size:2.5rem;color:var(--xam-300);"></i>
        <p style="margin-top:var(--kc-s);font-size:var(--font-nho);">Chưa có sản phẩm nào trong giỏ hàng</p>
        <span style="font-size:11px;color:var(--van-ban-mo);">Nhấn chọn sản phẩm từ danh mục bên trái</span>
      </div>
    `;
  }

  return gioHang.map((item, index) => {
    const thanhTien = item.SOLUONG * item.DONGIA;
    const anhThumb = layAnhSanPham(item.MASP, item);
    return `
      <div style="display:flex;justify-content:space-between;align-items:center;padding:8px 0;border-bottom:1px solid var(--duong-vien-nhe);font-size:var(--font-nho);">
        <img
          src="${escHtml(anhThumb)}"
          alt=""
          style="width:34px;height:34px;border-radius:var(--bo-goc-xs);object-fit:cover;border:1px solid var(--duong-vien);flex-shrink:0;margin-right:8px;background:#fff;"
          onerror="this.onerror=null;this.src='${taoPlaceholderSanPham(item?.TENSP, item?.MASP)}';"
        />
        <div style="flex:1;padding-right:8px;min-width:0;">
          <div style="font-weight:600;color:var(--van-ban-chinh);white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${escHtml(item.TENSP)}</div>
          <div style="color:var(--van-ban-mo);font-size:11px;">${escHtml(item.MASP)} • ${formatTien(item.DONGIA)}</div>
        </div>
        <div style="display:flex;align-items:center;gap:3px;">
          <button type="button" class="nut nut-vien nut-nho pos-giam-sl" data-index="${index}" style="padding:2px 7px;line-height:1;border-radius:var(--bo-goc-xs);">-</button>
          <input type="number" class="o-nhap pos-nhap-sl" data-index="${index}" value="${item.SOLUONG}" min="1" style="width:44px;text-align:center;padding:3px 2px;font-size:var(--font-nho);border-radius:var(--bo-goc-xs);" />
          <button type="button" class="nut nut-vien nut-nho pos-tang-sl" data-index="${index}" style="padding:2px 7px;line-height:1;border-radius:var(--bo-goc-xs);">+</button>
        </div>
        <div style="width:85px;text-align:right;font-weight:700;color:var(--mau-chinh);">
          ${formatTien(thanhTien)}
        </div>
        <button type="button" class="nut nut-vien nut-nguy-hiem nut-nho pos-xoa-item" data-index="${index}" style="padding:3px 6px;margin-left:6px;border-radius:var(--bo-goc-xs);" title="Xóa món">
          <i class="bi bi-x"></i>
        </button>
      </div>
    `;
  }).join('');
}

/**
 * Trả về chuỗi HTML hóa đơn in nhiệt chuyên nghiệp
 * @param {object} hoaDon
 * @returns {string}
 */
export function taoHtmlHoaDonIn(hoaDon) {
  return `
    <div id="vung-in-hoa-don">
      <div style="text-align:center;border-bottom:1px dashed #000;padding-bottom:12px;margin-bottom:12px;">
        <h3 style="margin:0;font-size:18px;font-weight:800;letter-spacing:1px;">CỬA HÀNG BÁN LẺ</h3>
        <p style="margin:4px 0;font-size:12px;">Hotline: 1900 1234 — Website: banle.vn</p>
        <h4 style="margin:8px 0 0 0;font-size:16px;font-weight:700;">HÓA ĐƠN BÁN HÀNG</h4>
      </div>

      <div style="font-size:12px;margin-bottom:12px;line-height:1.6;">
        <div>Số HĐ: <strong>${escHtml(hoaDon.maHD)}</strong></div>
        <div>Ngày lập: ${hoaDon.ngayLap}</div>
        <div>Thu ngân: ${escHtml(hoaDon.maNV)}</div>
        <div>Khách hàng: ${escHtml(hoaDon.maKH)}</div>
        <div>Hình thức: ${escHtml(hoaDon.pttt)}</div>
      </div>

      <table style="width:100%;font-size:12px;border-collapse:collapse;margin-bottom:12px;">
        <thead>
          <tr style="border-top:1px dashed #000;border-bottom:1px dashed #000;">
            <th style="text-align:left;padding:6px 0;">Mặt hàng</th>
            <th style="text-align:center;padding:6px 0;">SL</th>
            <th style="text-align:right;padding:6px 0;">Đơn giá</th>
            <th style="text-align:right;padding:6px 0;">T.Tiền</th>
          </tr>
        </thead>
        <tbody>
          ${hoaDon.chiTiet.map(it => `
            <tr>
              <td style="padding:4px 0;max-width:140px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;">${escHtml(it.TENSP)}</td>
              <td style="text-align:center;padding:4px 0;">${it.SOLUONG}</td>
              <td style="text-align:right;padding:4px 0;">${formatTien(it.DONGIA)}</td>
              <td style="text-align:right;padding:4px 0;font-weight:600;">${formatTien(it.SOLUONG * it.DONGIA)}</td>
            </tr>
          `).join('')}
        </tbody>
      </table>

      <div style="border-top:1px dashed #000;padding-top:10px;font-size:12px;display:flex;flex-direction:column;gap:4px;">
        <div style="display:flex;justify-content:space-between;">
          <span>Cộng tiền hàng:</span>
          <span>${formatTien(hoaDon.tienHang)}</span>
        </div>
        ${hoaDon.tienVat > 0 ? `
          <div style="display:flex;justify-content:space-between;">
            <span>Thuế VAT:</span>
            <span>${formatTien(hoaDon.tienVat)}</span>
          </div>
        ` : ''}
        ${Number(hoaDon.giamGia) > 0 ? `
          <div style="display:flex;justify-content:space-between;">
            <span>Giảm giá:</span>
            <span>-${formatTien(hoaDon.giamGia)}</span>
          </div>
        ` : `
          <div style="display:flex;justify-content:space-between;">
            <span>Giảm giá:</span>
            <span>0 đ</span>
          </div>
        `}
        <div style="display:flex;justify-content:space-between;font-weight:800;font-size:16px;margin-top:6px;border-top:1px solid #000;padding-top:6px;">
          <span>TỔNG CỘNG:</span>
          <span>${formatTien(hoaDon.tongCong)}</span>
        </div>
      </div>

      <div style="text-align:center;margin-top:18px;font-size:11px;border-top:1px dashed #000;padding-top:12px;">
        <p style="margin:0;font-weight:600;">Cảm ơn Quý khách & Hẹn gặp lại!</p>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML modal thanh toán QR PayOS với đầy đủ thông tin hóa đơn và mã QR
 * @param {object} options
 * @param {string} options.maHD
 * @param {string} options.ngayLap
 * @param {string} options.tenNV
 * @param {string} options.tenKH
 * @param {Array}  options.chiTiet
 * @param {number} options.tienHang
 * @param {number} options.tienVat
 * @param {number} options.giamGia
 * @param {number} options.tongCong
 * @param {string} options.checkoutUrl
 * @param {string} options.qrImageDataUrl
 * @returns {string}
 */
export function taoHtmlModalThanhToanQr({
  maHD = '',
  ngayLap = '',
  tenNV = '',
  tenKH = '',
  chiTiet = [],
  tienHang = 0,
  tienVat = 0,
  giamGia = 0,
  tongCong = 0,
  checkoutUrl = '',
  qrImageDataUrl = '',
  qrImageUrl = ''
}) {
  const laKhachLe = !tenKH || tenKH.trim() === '' || tenKH === 'Khách lẻ';
  const tenKhachHienThi = laKhachLe ? 'Khách lẻ' : tenKH;
  const dataUrlHopLe = typeof qrImageDataUrl === 'string' && qrImageDataUrl.startsWith('data:image/png;base64,');
  let urlQrHopLe = false;
  try {
    const url = new URL(qrImageUrl);
    urlQrHopLe = url.protocol === 'https:' &&
      url.hostname === 'api.qrserver.com' &&
      url.pathname === '/v1/create-qr-code/';
  } catch {
    urlQrHopLe = false;
  }
  const srcAnhQr = dataUrlHopLe ? qrImageDataUrl : (urlQrHopLe ? qrImageUrl : '');
  const coAnhQr = Boolean(srcAnhQr);

  return `
    <div id="pos-payos-modal-container" class="payos-modal-container">
      <div class="payos-modal-grid">
        <!-- CỘT 1: MÃ QR & TRẠNG THÁI THANH TOÁN -->
        <div class="payos-modal-qr-col">
          <div class="payos-qr-hop">
            <div id="pos-payos-qr-khung" class="payos-qr-khung" aria-label="Mã QR thanh toán">
              ${coAnhQr ? `
                <img id="pos-payos-qr-img" class="payos-qr-img" src="${escHtml(srcAnhQr)}" alt="Mã QR thanh toán hóa đơn ${escHtml(maHD)}" width="240" height="240" />
              ` : `
                <div class="payos-qr-khong-anh">
                  <i class="bi bi-qr-code" style="font-size:3rem;color:var(--xam-400);"></i>
                  <p style="margin-top:10px;font-size:var(--font-nho);color:var(--van-ban-phu);line-height:1.4;">
                    Chưa có ảnh QR trực tiếp từ ngân hàng.<br />Quý khách có thể mở trang thanh toán PayOS.
                  </p>
                </div>
              `}
            </div>

            <!-- Trạng thái thanh toán -->
            <div id="pos-payos-trang-thai-vung" class="payos-trang-thai-vung">
              <div id="pos-payos-badge" class="payos-badge payos-badge-cho">
                <span class="quay-vong quay-vong-nho"></span>
                <span id="pos-payos-badge-chu">Đang chờ thanh toán</span>
              </div>
              <p id="pos-payos-huong-dan" class="payos-qr-huong-dan">
                Mở ứng dụng ngân hàng hoặc ví điện tử để quét mã QR thanh toán.
              </p>
            </div>

            <!-- Nút mở trang PayOS trực tiếp nếu cần -->
            ${checkoutUrl ? `
              <a class="nut nut-vien nut-nho payos-btn-checkout" href="${escHtml(checkoutUrl)}" target="_blank" rel="noopener noreferrer">
                <i class="bi bi-box-arrow-up-right"></i> Mở trang thanh toán PayOS
              </a>
            ` : ''}

            <!-- Nút kiểm tra thủ công & ghi chú -->
            <div class="payos-kiem-tra-phu">
              <button type="button" id="pos-payos-nut-kiem-tra" class="nut nut-vien nut-nho" style="font-size:12px;padding:4px 10px;">
                <i class="bi bi-arrow-repeat"></i> Kiểm tra lại ngay
              </button>
              <span class="payos-ghi-chu-he-thong">Hệ thống tự động kiểm tra mỗi 3–4 giây</span>
            </div>
          </div>
        </div>

        <!-- CỘT 2: THÔNG TIN HÓA ĐƠN & DANH SÁCH MẶT HÀNG -->
        <div class="payos-modal-info-col">
          <!-- Thông tin hóa đơn -->
          <div class="payos-info-the">
            <div class="payos-info-dong">
              <span class="payos-info-nhan">Mã hóa đơn:</span>
              <strong class="payos-info-giatri">${escHtml(maHD)}</strong>
            </div>
            <div class="payos-info-dong">
              <span class="payos-info-nhan">Ngày lập:</span>
              <span class="payos-info-giatri">${escHtml(ngayLap)}</span>
            </div>
            <div class="payos-info-dong">
              <span class="payos-info-nhan">Nhân viên:</span>
              <span class="payos-info-giatri">${escHtml(tenNV)}</span>
            </div>
            <div class="payos-info-dong">
              <span class="payos-info-nhan">Khách hàng:</span>
              <span class="payos-info-giatri">${escHtml(tenKhachHienThi)}</span>
            </div>
          </div>

          <!-- Bảng danh sách sản phẩm -->
          <div class="payos-bang-sp-hop">
            <table class="payos-bang-sp">
              <thead>
                <tr>
                  <th style="text-align:left;">Sản phẩm</th>
                  <th style="text-align:center;width:45px;">SL</th>
                  <th style="text-align:right;width:85px;">Đơn giá</th>
                  <th style="text-align:right;width:95px;">Thành tiền</th>
                </tr>
              </thead>
              <tbody>
                ${chiTiet.map(it => {
                  const masp = it.MASP || it.MaSP || it.masp || '';
                  const tensp = it.TENSP || it.TenSP || it.tensp || '';
                  const sl = Number(it.SOLUONG || it.SoLuong || it.soluong || 1);
                  const gia = Number(it.DONGIA || it.DonGia || it.dongia || 0);
                  const tt = sl * gia;
                  return `
                    <tr>
                      <td>
                        <div class="payos-sp-ten">${escHtml(tensp)}</div>
                        <div class="payos-sp-ma">${escHtml(masp)}</div>
                      </td>
                      <td style="text-align:center;">${sl}</td>
                      <td style="text-align:right;">${formatTien(gia)}</td>
                      <td style="text-align:right;font-weight:600;">${formatTien(tt)}</td>
                    </tr>
                  `;
                }).join('')}
              </tbody>
            </table>
          </div>

          <!-- Tóm tắt số tiền -->
          <div class="payos-tong-hop-tien">
            <div class="payos-dong-tien">
              <span>Tiền hàng:</span>
              <span>${formatTien(tienHang)}</span>
            </div>
            ${tienVat > 0 ? `
              <div class="payos-dong-tien">
                <span>Thuế VAT:</span>
                <span>${formatTien(tienVat)}</span>
              </div>
            ` : ''}
            ${giamGia > 0 ? `
              <div class="payos-dong-tien">
                <span>Giảm giá:</span>
                <span style="color:var(--mau-thanh-cong);font-weight:600;">-${formatTien(giamGia)}</span>
              </div>
            ` : ''}
            <div class="payos-dong-tien payos-dong-tong">
              <span>Tổng tiền phải thanh toán:</span>
              <strong class="payos-so-tien-lon">${formatTien(tongCong)}</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  `;
}
