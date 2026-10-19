/**
 * giao-dien-cong-no.js — Module HTML thiết kế trang Quản lý Công nợ
 */
import { escHtml, formatTien } from '../tien-ich.js';

/**
 * Trả về chuỗi HTML khung giao diện công nợ
 * @param {object} params
 * @returns {string}
 */
export function taoHtmlLayoutCongNo({
  tongNoPhaiThu = 0,
  soDonConNo = 0,
  soDonDaThu = 0,
  tongSoGiaoDich = 0,
  tabLoc = 'tat-ca'
}) {
  return `
    <div class="fade-in" style="display:flex;flex-direction:column;gap:var(--kc-l);">
      <!-- Thẻ tóm tắt chỉ số công nợ -->
      <div style="display:grid;grid-template-columns:repeat(auto-fit, minmax(240px, 1fr));gap:var(--kc-l);">
        <div class="the" style="padding:var(--kc-l);border-left:4px solid var(--mau-nguy-hiem);">
          <div style="display:flex;justify-content:space-between;align-items:center;">
            <span style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">TỔNG NỢ CẦN THU</span>
            <span class="badge badge-do">${soDonConNo} hóa đơn</span>
          </div>
          <div style="font-size:1.85rem;font-weight:800;color:var(--mau-nguy-hiem);margin-top:6px;">${formatTien(tongNoPhaiThu)}</div>
          <div style="font-size:var(--font-xs);color:var(--van-ban-phu);margin-top:4px;">Các khoản nợ khách hàng chưa đối soát</div>
        </div>

        <div class="the" style="padding:var(--kc-l);border-left:4px solid var(--mau-thanh-cong);">
          <div style="display:flex;justify-content:space-between;align-items:center;">
            <span style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">HÓA ĐƠN ĐÃ THU ĐỦ</span>
            <span class="badge badge-xanh">${soDonDaThu} hóa đơn</span>
          </div>
          <div style="font-size:1.85rem;font-weight:800;color:var(--mau-thanh-cong);margin-top:6px;">${soDonDaThu} đơn</div>
          <div style="font-size:var(--font-xs);color:var(--van-ban-phu);margin-top:4px;">Dòng tiền đã tất toán an toàn</div>
        </div>

        <div class="the" style="padding:var(--kc-l);border-left:4px solid var(--mau-chinh);">
          <div style="display:flex;justify-content:space-between;align-items:center;">
            <span style="font-size:var(--font-xs);color:var(--van-ban-mo);font-weight:700;text-transform:uppercase;">GIAO DỊCH THANH TOÁN</span>
            <span class="badge badge-lam">Hệ thống</span>
          </div>
          <div style="font-size:1.85rem;font-weight:800;color:var(--mau-chinh);margin-top:6px;">${tongSoGiaoDich} lượt</div>
          <div style="font-size:var(--font-xs);color:var(--van-ban-phu);margin-top:4px;">Lịch sử nộp tiền mặt & chuyển khoản</div>
        </div>
      </div>

      <!-- Thẻ thanh điều khiển bộ lọc & Bảng dữ liệu công nợ -->
      <div class="the" id="khung-chinh-cong-no">
        <div class="the-dau">
          <div style="display:flex;align-items:center;gap:var(--kc-s);">
            <div class="the-tieu-de">
              <i class="bi bi-journal-check" style="color:var(--mau-chinh);"></i> Sổ cái đối soát công nợ
            </div>
            <!-- Tabs lọc nhanh -->
            <div style="display:flex;gap:4px;margin-left:var(--kc-m);">
              <button type="button" class="nut nut-nho ${tabLoc === 'tat-ca' ? 'nut-chinh' : 'nut-vien'} tab-loc-cn" data-tab="tat-ca">
                Tất cả
              </button>
              <button type="button" class="nut nut-nho ${tabLoc === 'con-no' ? 'nut-chinh' : 'nut-vien'} tab-loc-cn" data-tab="con-no">
                Chỉ đơn còn nợ (${soDonConNo})
              </button>
              <button type="button" class="nut nut-nho ${tabLoc === 'da-tra' ? 'nut-chinh' : 'nut-vien'} tab-loc-cn" data-tab="da-tra">
                Đã thu đủ (${soDonDaThu})
              </button>
            </div>
          </div>
          <div class="thanh-cong-cu">
            <div class="o-tim-kiem">
              <i class="bi bi-search"></i>
              <input type="search" id="cn-tim-kiem" placeholder="Tìm theo mã HĐ, khách hàng..." />
            </div>
            <button type="button" id="cn-nut-tai-lai" class="nut nut-xam" title="Tải lại dữ liệu">
              <i class="bi bi-arrow-clockwise"></i>
            </button>
          </div>
        </div>
        <div class="the-noi-dung khong-padding" id="cn-vung-bang"></div>
      </div>
    </div>
  `;
}

/**
 * Trả về chuỗi HTML modal xác nhận thu nợ / thanh toán
 * @param {object} params
 * @returns {string}
 */
export function taoHtmlModalThuNo({ hoaDon, phaiTra = 0, daTra = 0, conNo = 0 }) {
  return `
    <div style="display:flex;flex-direction:column;gap:var(--kc-m);">
      <div style="background:var(--xam-50);padding:var(--kc-m);border-radius:var(--bo-goc-vua);border:1px solid var(--duong-vien);display:flex;flex-direction:column;gap:4px;font-size:var(--font-binh);">
        <div>Mã hóa đơn: <strong>${escHtml(hoaDon.MAHDBAN)}</strong></div>
        <div>Khách hàng: <strong>${escHtml(hoaDon.MAKH)}</strong></div>
        <div style="display:flex;justify-content:space-between;margin-top:4px;">
          <span>Tổng tiền đơn hàng:</span>
          <span>${formatTien(phaiTra)}</span>
        </div>
        <div style="display:flex;justify-content:space-between;">
          <span>Đã thanh toán:</span>
          <span style="color:var(--mau-thanh-cong);">${formatTien(daTra)}</span>
        </div>
        <div style="display:flex;justify-content:space-between;font-weight:700;border-top:1px dashed var(--xam-300);padding-top:4px;color:var(--mau-nguy-hiem);">
          <span>Còn nợ cần thu:</span>
          <span>${formatTien(conNo)}</span>
        </div>
      </div>

      <div class="nhom-truong">
        <label class="nhan-truong" for="modal-pt-thuno">Phương thức thu tiền <span class="bat-buoc">*</span></label>
        <select id="modal-pt-thuno" class="o-chon">
          <option value="Tiền mặt">💵 Tiền mặt</option>
          <option value="Chuyển khoản">💳 Chuyển khoản ngân hàng</option>
        </select>
      </div>

      <div class="nhom-truong">
        <label class="nhan-truong" for="modal-tien-thuno">Số tiền thu đợt này (VND) <span class="bat-buoc">*</span></label>
        <input type="number" id="modal-tien-thuno" class="o-nhap" value="${conNo}" max="${conNo}" min="1" step="1000" />
        <span class="mo-ta-truong">Mặc định thu toàn bộ số nợ còn lại</span>
      </div>
    </div>
  `;
}
