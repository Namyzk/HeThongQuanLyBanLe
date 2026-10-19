/**
 * cau-hinh-thanh-toan.js — Cấu hình thông tin tài khoản ngân hàng nhận tiền qua QR
 * Phục vụ tạo mã VietQR chuẩn NAPAS 247 cho màn hình bán hàng POS.
 * 
 * HƯỚNG DẪN CẤU HÌNH TÀI KHOẢN NGÂN HÀNG THẬT:
 * 1. Chuyển CHUA_CAU_HINH thành false.
 * 2. Điền mã BIN ngân hàng vào MA_BIN (tra cứu danh sách DANH_SACH_NGAN_HANG bên dưới).
 * 3. Điền tên ngân hàng vào TEN_NGAN_HANG.
 * 4. Điền số tài khoản ngân hàng thụ hưởng vào SO_TAI_KHOAN.
 * 5. Điền tên chủ tài khoản (in hoa không dấu hoặc có dấu) vào CHU_TAI_KHOAN.
 * 6. Lưu file lại, màn hình bán hàng sẽ tự động tạo mã VietQR thật có thể quét bằng app ngân hàng.
 */

export const CAU_HINH_THANH_TOAN = {
  /**
   * Đặt là true nếu cửa hàng chưa cấu hình tài khoản thực tế (chế độ DEMO an toàn).
   * Đặt là false khi đã điền thông tin tài khoản ngân hàng thực tế bên dưới.
   */
  CHUA_CAU_HINH: false,

  /** Mã định danh ngân hàng (BIN) theo chuẩn NAPAS - 6 chữ số */
  MA_BIN: '970422',

  /** Tên đầy đủ hoặc tên viết tắt ngân hàng thụ hưởng */
  TEN_NGAN_HANG: 'MBBank (Ngân hàng TMCP Quân Đội)',

  /** Số tài khoản ngân hàng nhận tiền */
  SO_TAI_KHOAN: '008922972',

  /** Tên chủ tài khoản ngân hàng */
  CHU_TAI_KHOAN: 'PHAM TRONG NAM',

  /** Tiền tố nội dung chuyển khoản (sẽ ghép với mã hóa đơn, vd: THANHTOAN HD1234567) */
  TIEN_TO_NOI_DUNG: 'THANHTOAN ',

  /** Hướng dẫn quét mã mặc định */
  GHI_CHU: 'Mở ứng dụng ngân hàng hoặc ví điện tử bất kỳ để quét mã VietQR chuyển khoản nhanh 24/7.'
};

/**
 * Danh sách mã BIN một số ngân hàng phổ biến tại Việt Nam (chuẩn NAPAS 247)
 */
export const DANH_SACH_NGAN_HANG = [
  { bin: '970422', ten: 'MBBank - Ngân hàng TMCP Quân Đội', ma: 'MB' },
  { bin: '970436', ten: 'Vietcombank - Ngân hàng Ngoại Thương VN', ma: 'VCB' },
  { bin: '970415', ten: 'VietinBank - Ngân hàng Công Thương VN', ma: 'CTG' },
  { bin: '970418', ten: 'BIDV - Đầu tư và Phát triển VN', ma: 'BIDV' },
  { bin: '970405', ten: 'Agribank - Nông nghiệp và PTNT VN', ma: 'VBA' },
  { bin: '970407', ten: 'Techcombank - Ngân hàng Kỹ Thương VN', ma: 'TCB' },
  { bin: '970432', ten: 'VPBank - Ngân hàng Việt Nam Thịnh Vượng', ma: 'VPB' },
  { bin: '970423', ten: 'TPBank - Ngân hàng Tiên Phong', ma: 'TPB' },
  { bin: '970416', ten: 'ACB - Ngân hàng Á Châu', ma: 'ACB' },
  { bin: '970403', ten: 'Sacombank - Sài Gòn Thương Tín', ma: 'STB' },
  { bin: '970437', ten: 'HDBank - Ngân hàng Phát triển TP.HCM', ma: 'HDB' },
  { bin: '970448', ten: 'OCB - Ngân hàng Phương Đông', ma: 'OCB' },
  { bin: '970441', ten: 'VIB - Ngân hàng Quốc tế', ma: 'VIB' },
  { bin: '970443', ten: 'SHB - Ngân hàng Sài Gòn - Hà Nội', ma: 'SHB' },
  { bin: '970426', ten: 'MSB - Ngân hàng Hàng Hải', ma: 'MSB' },
  { bin: '970440', ten: 'SeABank - Ngân hàng Đông Nam Á', ma: 'SEAB' }
];

const KHOA_LUU_LOCAL = 'cau_hinh_thanh_toan_qr_tuy_chinh';

/**
 * Lấy cấu hình thanh toán hiện hành (ưu tiên cấu hình lưu trong localStorage nếu có, fallback về CAU_HINH_THANH_TOAN)
 * @returns {typeof CAU_HINH_THANH_TOAN}
 */
export function layCauHinhThanhToan() {
  try {
    const raw = localStorage.getItem(KHOA_LUU_LOCAL);
    if (raw) {
      const parsed = JSON.parse(raw);
      if (typeof parsed === 'object' && parsed !== null) {
        return { ...CAU_HINH_THANH_TOAN, ...parsed };
      }
    }
  } catch (err) {
    console.warn('Không thể đọc cấu hình thanh toán từ localStorage:', err);
  }
  return { ...CAU_HINH_THANH_TOAN };
}

/**
 * Lưu cấu hình tùy chỉnh vào localStorage để thuận tiện kiểm thử hoặc thay đổi nhanh
 * @param {Partial<typeof CAU_HINH_THANH_TOAN>} cauHinhMoi 
 */
export function luuCauHinhThanhToan(cauHinhMoi) {
  try {
    const hienTai = layCauHinhThanhToan();
    const capNhat = { ...hienTai, ...cauHinhMoi };
    localStorage.setItem(KHOA_LUU_LOCAL, JSON.stringify(capNhat));
    return capNhat;
  } catch (err) {
    console.error('Không thể lưu cấu hình thanh toán:', err);
    return null;
  }
}

/**
 * Đặt lại cấu hình về mặc định trong file
 */
export function datLaiCauHinhMacDinh() {
  try {
    localStorage.removeItem(KHOA_LUU_LOCAL);
  } catch (err) {
    console.warn('Lỗi khi xóa cấu hình localStorage:', err);
  }
  return { ...CAU_HINH_THANH_TOAN };
}

/**
 * Kiểm tra xem cấu hình tài khoản nhận tiền đã đầy đủ và hợp lệ để tạo VietQR thật hay chưa
 * @param {typeof CAU_HINH_THANH_TOAN} [cauHinh]
 * @returns {{ hopLe: boolean, thongDiep: string, cacLoi: string[] }}
 */
export function kiemTraCauHinhNganHang(cauHinh) {
  const cfg = cauHinh || layCauHinhThanhToan();
  const cacLoi = [];

  if (cfg.CHUA_CAU_HINH === true) {
    cacLoi.push('Hệ thống đang ở chế độ DEMO (cờ CHUA_CAU_HINH = true).');
  }

  const maBin = (cfg.MA_BIN || '').toString().trim();
  if (!maBin) {
    cacLoi.push('Thiếu mã BIN ngân hàng thụ hưởng.');
  } else if (!/^\d{6}$/.test(maBin)) {
    cacLoi.push(`Mã BIN ngân hàng "${maBin}" không hợp lệ (phải gồm đúng 6 chữ số).`);
  }

  const soTaiKhoan = (cfg.SO_TAI_KHOAN || '').toString().trim();
  if (!soTaiKhoan) {
    cacLoi.push('Thiếu số tài khoản ngân hàng nhận tiền.');
  } else if (soTaiKhoan.length < 5 || !/^[A-Za-z0-9]+$/.test(soTaiKhoan)) {
    cacLoi.push('Số tài khoản nhận tiền không hợp lệ.');
  }

  const chuTaiKhoan = (cfg.CHU_TAI_KHOAN || '').toString().trim();
  if (!chuTaiKhoan) {
    cacLoi.push('Thiếu tên chủ tài khoản nhận tiền.');
  } else if (chuTaiKhoan.length < 2) {
    cacLoi.push('Tên chủ tài khoản quá ngắn.');
  }

  const tenNganHang = (cfg.TEN_NGAN_HANG || '').toString().trim();
  if (!tenNganHang) {
    cacLoi.push('Thiếu tên ngân hàng nhận tiền.');
  }

  const hopLe = cacLoi.length === 0;
  return {
    hopLe,
    thongDiep: hopLe
      ? 'Tài khoản ngân hàng đã được cấu hình hợp lệ.'
      : 'Chưa cấu hình tài khoản nhận tiền thực tế.',
    cacLoi
  };
}
