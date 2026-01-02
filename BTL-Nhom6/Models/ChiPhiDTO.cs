using System;

namespace BTL_Nhom6.Models
{
    // Class này chỉ dùng để hiển thị lên lưới Báo cáo
    public class ChiPhiDTO
    {
        public string MaPhieu { get; set; }        // Ví dụ: "WO-0001"
        public string LoaiChiPhi { get; set; }     // "Vật tư" hoặc "Nhân công"
        public string NoiDung { get; set; }        // Tên vật tư hoặc mô tả chi phí
        public DateTime Ngay { get; set; }         // Ngày hoàn thành
        public decimal SoTien { get; set; }        // Thành tiền

        public string TenPhanXuong { get; set; }

        // Các thuộc tính phụ để Binding lên XAML đẹp hơn
        public string NgayHienThi => Ngay.ToString("dd/MM/yyyy");
        public string SoTienHienThi => SoTien.ToString("N0");
    }
}