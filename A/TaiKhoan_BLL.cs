using DAL;
using Models;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class TaiKhoan_BLL
    {
        private readonly TaiKhoan_DAL tk_dal;

        public TaiKhoan_BLL()
        {
            tk_dal = new TaiKhoan_DAL();
        }

        public List<TaiKhoan> LayTatCa()
        {
            var list = tk_dal.GetAll();
            return list ?? new List<TaiKhoan>();
        }

        public List<TaiKhoan> LayTheoID(string? mataikhoan)
        {
            if (string.IsNullOrWhiteSpace(mataikhoan))
                throw new ArgumentException("Mã tài khoản không được để trống.");

            mataikhoan = mataikhoan.Trim();

            if (mataikhoan.Length > 15)
                throw new ArgumentException("Mã tài khoản không được vượt quá 15 ký tự.");

            if (!tk_dal.KiemTraTonTai(mataikhoan))
                return new List<TaiKhoan>();

            return tk_dal.GetByID(mataikhoan);
        }

        public bool ThemMoi(TaiKhoan tk)
        {
            ValidateTaiKhoan(tk);

            tk.MATAIKHOAN = tk.MATAIKHOAN.Trim();
            tk.USERNAME = tk.USERNAME.Trim();
            tk.PASS = tk.PASS.Trim();

            if (tk_dal.KiemTraTonTai(tk.MATAIKHOAN))
                throw new InvalidOperationException($"Mã tài khoản '{tk.MATAIKHOAN}' đã tồn tại.");

            if (tk_dal.KiemTraUsernameTonTai(tk.USERNAME))
                throw new InvalidOperationException($"Tên đăng nhập '{tk.USERNAME}' đã tồn tại.");

            return tk_dal.Insert(tk);
        }

        public bool CapNhat(TaiKhoan tk)
        {
            ValidateTaiKhoan(tk);

            tk.MATAIKHOAN = tk.MATAIKHOAN.Trim();
            tk.USERNAME = tk.USERNAME.Trim();
            tk.PASS = tk.PASS.Trim();

            if (!tk_dal.KiemTraTonTai(tk.MATAIKHOAN))
                throw new KeyNotFoundException($"Không tìm thấy tài khoản có mã '{tk.MATAIKHOAN}'.");

            if (tk_dal.KiemTraUsernameKhacTaiKhoan(tk.USERNAME, tk.MATAIKHOAN))
                throw new InvalidOperationException($"Tên đăng nhập '{tk.USERNAME}' đã được sử dụng bởi tài khoản khác.");

            return tk_dal.Update(tk);
        }

        public bool Xoa(string? mataikhoan)
        {
            if (string.IsNullOrWhiteSpace(mataikhoan))
                throw new ArgumentException("Mã tài khoản không được để trống.");

            mataikhoan = mataikhoan.Trim();

            if (mataikhoan.Length > 15)
                throw new ArgumentException("Mã tài khoản không được vượt quá 15 ký tự.");

            if (!tk_dal.KiemTraTonTai(mataikhoan))
                throw new KeyNotFoundException($"Không tìm thấy tài khoản có mã '{mataikhoan}'.");

            return tk_dal.Delete(mataikhoan);
        }

        public List<TaiKhoan> DangNhap(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Tên đăng nhập không được để trống.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Mật khẩu không được để trống.");

            username = username.Trim();
            password = password.Trim();

            if (username.Length > 20)
                throw new ArgumentException("Tên đăng nhập không được vượt quá 20 ký tự.");

            if (password.Length > 20)
                throw new ArgumentException("Mật khẩu không được vượt quá 20 ký tự.");

            return tk_dal.Login(username, password);
        }

        public int LayQuyen(string? username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return 0;

            return tk_dal.GetRoleByUsername(username.Trim());
        }

        private void ValidateTaiKhoan(TaiKhoan tk)
        {
            if (tk == null)
                throw new ArgumentNullException(nameof(tk), "Dữ liệu tài khoản không được để trống.");

            // 1. Mã tài khoản
            if (string.IsNullOrWhiteSpace(tk.MATAIKHOAN) || tk.MATAIKHOAN.Trim().Equals("string", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Mã tài khoản không hợp lệ.");

            tk.MATAIKHOAN = tk.MATAIKHOAN.Trim();
            if (tk.MATAIKHOAN.Length > 15)
                throw new ArgumentException("Mã tài khoản không được vượt quá 15 ký tự.");

            // 2. Username
            if (string.IsNullOrWhiteSpace(tk.USERNAME) || tk.USERNAME.Trim().Equals("string", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Tên đăng nhập không hợp lệ.");

            tk.USERNAME = tk.USERNAME.Trim();
            if (tk.USERNAME.Length > 20)
                throw new ArgumentException("Tên đăng nhập không được vượt quá 20 ký tự.");

            // 3. Password
            if (string.IsNullOrWhiteSpace(tk.PASS))
                throw new ArgumentException("Mật khẩu không được để trống.");

            tk.PASS = tk.PASS.Trim();
            if (tk.PASS.Length < 6)
                throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự.");

            if (tk.PASS.Length > 20)
                throw new ArgumentException("Mật khẩu không được vượt quá 20 ký tự.");

            // 4. Quyền: 1 - Admin, 2 - ThuNgan, 3 - ThuKho
            if (tk.QUYEN < 1 || tk.QUYEN > 3)
                throw new ArgumentException("Quyền tài khoản không hợp lệ (Chỉ chấp nhận 1: Admin, 2: ThuNgan, 3: ThuKho).");
        }
    }
}