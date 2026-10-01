using DAL;
using System;
using System.Data;

namespace BLL
{
    public class KhuyenMai_BLL
    {
        private readonly KhuyenMai_DAL _DAL;

        public KhuyenMai_BLL()
        {
            _DAL = new KhuyenMai_DAL();
        }

        public DataTable getAll()
        {
            return _DAL.getAll();
        }

        public DataTable GetById(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                throw new Exception("Mã khuyến mại không được để trống.");

            ma = ma.Trim();

            if (ma.Length > 15)
                throw new Exception("Mã khuyến mại không được vượt quá 15 ký tự.");

            return _DAL.GetById(ma);
        }

        public DataTable Delete(string ma)
        {
            if (string.IsNullOrWhiteSpace(ma))
                throw new Exception("Mã khuyến mại không được để trống.");

            ma = ma.Trim();

            if (ma.Length > 15)
                throw new Exception("Mã khuyến mại không được vượt quá 15 ký tự.");

            return _DAL.Delete(ma);
        }

        public DataTable Update(Models.KhuyenMai model)
        {
            ValidateKhuyenMai(model);

            return _DAL.Update(model);
        }

        public DataTable Create(Models.KhuyenMai model)
        {
            ValidateKhuyenMai(model);

            DataTable checkKM = _DAL.GetById(model.MaKM!.Trim());

            if (checkKM.Rows.Count > 0)
                throw new Exception("Mã khuyến mại đã tồn tại.");

            // Kiểm tra sản phẩm tồn tại
            DataTable checkSP = _DAL.CheckSanPham(model.MaSP!.Trim());

            if (checkSP.Rows.Count == 0)
                throw new Exception("Sản phẩm không tồn tại.");

            return _DAL.Create(model);
        }

        private void ValidateKhuyenMai(Models.KhuyenMai model)
        {
            if (model == null)
                throw new Exception("Dữ liệu khuyến mại không được để trống.");

            // MAKM
            if (string.IsNullOrWhiteSpace(model.MaKM))
                throw new Exception("Mã khuyến mại không được để trống.");

            model.MaKM = model.MaKM.Trim();

            if (model.MaKM.Length > 15)
                throw new Exception("Mã khuyến mại không được vượt quá 15 ký tự.");

            // TENKM
            if (string.IsNullOrWhiteSpace(model.TenKM))
                throw new Exception("Tên khuyến mại không được để trống.");

            model.TenKM = model.TenKM.Trim();

            if (model.TenKM.Length > 300)
                throw new Exception("Tên khuyến mại không được vượt quá 300 ký tự.");

            // MASP
            if (string.IsNullOrWhiteSpace(model.MaSP))
                throw new Exception("Mã sản phẩm không được để trống.");

            model.MaSP = model.MaSP.Trim();

            if (model.MaSP.Length > 15)
                throw new Exception("Mã sản phẩm không được vượt quá 15 ký tự.");

            // Ngày bắt đầu
            if (!model.NgayBD.HasValue)
                throw new Exception("Ngày bắt đầu không được để trống.");

            // Ngày kết thúc
            if (!model.NgayKT.HasValue)
                throw new Exception("Ngày kết thúc không được để trống.");

            // CHECK: NGAYKETTHUC >= NGAYBATDAU
            if (model.NgayKT.Value < model.NgayBD.Value)
                throw new Exception(  "Ngày kết thúc không được trước ngày bắt đầu."  );
        }
    }
}