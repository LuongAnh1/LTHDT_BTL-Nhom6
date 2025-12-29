using System;
using System.Windows.Media; // Để dùng Brush

namespace BTL_Nhom6.Models
{
    public class TransactionViewModel
    {
        public int TransactionID { get; set; }

        public DateTime NgayRaw { get; set; }
        public string Ngay => NgayRaw.ToString("dd/MM/yyyy HH:mm");

        public string MaPhieu { get; set; } // Lấy từ ImportCode hoặc ExportCode

        public string LoaiGiaoDich { get; set; } // "Nhập kho", "Xuất kho"

        public int SoLuong { get; set; }

        // Màu sắc số lượng: Nhập -> Xanh, Xuất -> Cam
        public string MauSoLuong => LoaiGiaoDich == "Nhập kho" ? "#2E7D32" : "#EF6C00";

        public string DonVi { get; set; }

        public string NguoiLienQuan { get; set; } // Nhà cung cấp hoặc Nhân viên nhận
        public string BoPhan { get; set; }        // Địa chỉ NCC hoặc Bộ phận nhân viên

        public string GhiChu { get; set; }
    }
}