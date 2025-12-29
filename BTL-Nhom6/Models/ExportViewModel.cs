using System;

namespace BTL_Nhom6.Models
{
    public class ExportViewModel
    {
        public int ExportID { get; set; }
        public string MaPhieu { get; set; }

        public DateTime NgayXuatRaw { get; set; }
        public string NgayXuat => NgayXuatRaw.ToString("dd/MM/yyyy HH:mm");

        public string NguoiNhan { get; set; } // Tên nhân viên nhận
        public string BoPhan { get; set; }    // Phòng ban/Vị trí của nhân viên

        public int TongSL { get; set; }

        public string StatusRaw { get; set; }
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