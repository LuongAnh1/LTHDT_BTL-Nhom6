using System;

namespace BTL_Nhom6.Models
{
    public class ImportViewModel
    {
        public int ReceiptID { get; set; }

        // Binding: MaPhieu
        public string MaPhieu { get; set; }

        // Binding: NgayNhap (Hiển thị dạng dd/MM/yyyy)
        public DateTime NgayNhapRaw { get; set; }
        public string NgayNhap => NgayNhapRaw.ToString("dd/MM/yyyy HH:mm");

        // Binding: NhaCungCap
        public string NhaCungCap { get; set; }

        // Binding: DiaChi
        public string DiaChi { get; set; }

        // Binding: TongSL (Tổng số lượng vật tư trong phiếu)
        public int TongSL { get; set; }

        // Binding: TrangThai (Hiển thị tiếng Việt: Hoàn thành, Chờ duyệt...)
        public string StatusRaw { get; set; } // Pending, Completed
        public string TrangThai
        {
            get
            {
                if (StatusRaw == "Completed") return "Hoàn thành";
                if (StatusRaw == "Pending") return "Chờ duyệt";
                if (StatusRaw == "Cancelled") return "Đã hủy";
                return "Không rõ";
            }
        }
    }
}