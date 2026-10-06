using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace BLL
{
    public class TaiKhoan_BLL
    {
        private readonly TaiKhoan_DAL tk_dal;

        public TaiKhoan_BLL(TaiKhoan_DAL tk_dal)
        {
            this.tk_dal = tk_dal;
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
            if (username.Length > 20)
                throw new ArgumentException("Tên đăng nhập không được vượt quá 20 ký tự.");

            if (password.Length > 128)
                throw new ArgumentException("Mật khẩu không được vượt quá 128 ký tự.");

            var account = tk_dal.Login(username);
            if (account == null || !VerifyPassword(password, account.PASS))
                return new List<TaiKhoan>();

            // Chuyển tài khoản cũ đang lưu PBKDF2 về dạng văn bản sau khi xác thực thành công.
            if (account.PASS.StartsWith("pbkdf2$", StringComparison.Ordinal))
            {
                account.PASS = password;
                tk_dal.CapNhatMatKhau(account.MATAIKHOAN, account.PASS);
            }

            return new List<TaiKhoan> { account };
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

            if (tk.PASS.Length < 6)
                throw new ArgumentException("Mật khẩu phải có ít nhất 6 ký tự.");

            if (tk.PASS.Length > 128)
                throw new ArgumentException("Mật khẩu không được vượt quá 128 ký tự.");

            // 4. Quyền: 1 - Admin, 2 - ThuNgan, 3 - ThuKho, 4 - KeToan
            if (tk.QUYEN < 1 || tk.QUYEN > 4)
                throw new ArgumentException("Quyền tài khoản không hợp lệ (1: Admin, 2: ThuNgan, 3: ThuKho, 4: KeToan).");
        }

        private static bool VerifyPassword(string password, string stored)
        {
            if (!stored.StartsWith("pbkdf2$", StringComparison.Ordinal))
                return string.Equals(password, stored, StringComparison.Ordinal);

            string[] parts = stored.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations) || iterations < 100_000 || iterations > 1_000_000)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
