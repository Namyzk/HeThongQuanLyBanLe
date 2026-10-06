using DAL;
using Models;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL
{
    public class NhaCungCap_BLL
    {
        private readonly NhaCungCap_DAL NCC_DAL;

        public NhaCungCap_BLL(NhaCungCap_DAL NCC_DAL)
        {
            this.NCC_DAL = NCC_DAL;
        }

        public DataTable GetAll()
        {
            return NCC_DAL.GetAll();
        }


        public string ValidateMa(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                return "Mã nhà cung cấp không được để trống.";

            ma = ma.Trim();

            if (ma.Length > 15)
                return "Mã nhà cung cấp không được vượt quá 15 ký tự.";

            return null;
        }



        private string ValidateModel(Models.NhaCungCap model)
        {
            if (model == null)
                return "Thông tin nhà cung cấp không được để trống.";

            // MÃ
            string error = ValidateMa(model.MaNCC);

            if (error != null)
                return error;

            model.MaNCC = model.MaNCC.Trim();


            // TÊN
            if (string.IsNullOrWhiteSpace(model.TenNCC))
                return "Tên nhà cung cấp không được để trống.";

            model.TenNCC = model.TenNCC.Trim();

            if (model.TenNCC.Length > 300)
                return "Tên nhà cung cấp không được vượt quá 300 ký tự.";


            // ĐỊA CHỈ
            if (!string.IsNullOrWhiteSpace(model.DiaChi))
            {
                model.DiaChi = model.DiaChi.Trim();

                if (model.DiaChi.Length > 500)
                    return "Địa chỉ không được vượt quá 500 ký tự.";
            }


            // SỐ ĐIỆN THOẠI
            if (!string.IsNullOrWhiteSpace(model.SDT))
            {
                model.SDT = model.SDT.Trim();

                if (model.SDT.Length > 12)
                    return "Số điện thoại không được vượt quá 12 ký tự.";

                if (!model.SDT.All(char.IsDigit))
                    return "Số điện thoại chỉ được chứa chữ số.";

                if (model.SDT.Length != 10)
                    return "Số điện thoại phải có đúng 10 chữ số.";
            }


            // EMAIL
            if (!string.IsNullOrWhiteSpace(model.EMAIL))
            {
                model.EMAIL = model.EMAIL.Trim();

                if (model.EMAIL.Length > 320)
                    return "Email không được vượt quá 320 ký tự.";

                if (!Regex.IsMatch(  model.EMAIL,   @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    return "Email không đúng định dạng.";
                }
            }

            return null;
        }



        public DataTable GetById(string ma)
        {
            string error = ValidateMa(ma);

            if (error != null)
                throw new Exception(error);

            return NCC_DAL.GetById(ma.Trim());
        }



        public string Create(Models.NhaCungCap model)
        {
            string error = ValidateModel(model);

            if (error != null)
                return error;

            try
            {
                DataTable dt = NCC_DAL.GetById(model.MaNCC);

                if (dt != null && dt.Rows.Count > 0)
                {
                    return $"Mã nhà cung cấp '{model.MaNCC}' đã tồn tại.";
                }

                NCC_DAL.Create(model);

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }


        public string Update(Models.NhaCungCap model)
        {
            string error = ValidateModel(model);

            if (error != null)
                return error;

            try
            {
                DataTable dt = NCC_DAL.GetById(model.MaNCC);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return $"Không tìm thấy nhà cung cấp có mã '{model.MaNCC}'.";
                }

                NCC_DAL.Update(model);

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }


        public string Delete(string ma)
        {
            string error = ValidateMa(ma);

            if (error != null)
                return error;

            ma = ma.Trim();

            try
            {
                DataTable dt = NCC_DAL.GetById(ma);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return $"Không tìm thấy nhà cung cấp có mã '{ma}'.";
                }

                NCC_DAL.Delete(ma);

                return null;
            }
            catch (Exception ex)
            {
                return "Lỗi hệ thống khi xử lý yêu cầu.";
            }
        }
    }
}
