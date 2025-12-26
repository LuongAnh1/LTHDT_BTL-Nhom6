using System.Windows.Media; // Cần tham chiếu PresentationCore

namespace BTL_Nhom6.Models
{
    public class TechnicianViewModel
    {
        public int UserID { get; set; }

        // Binding: MaNV
        public string MaNV => $"NV{UserID:D3}";

        // Binding: TenKTV
        public string TenKTV { get; set; }

        // Binding: Email
        public string Email { get; set; }

        // Binding: Initials (Lấy 2 chữ cái đầu của tên để hiện trong vòng tròn)
        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(TenKTV)) return "NV";
                var parts = TenKTV.Trim().Split(' ');
                if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
                return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
            }
        }

        // Binding: ChuyenMon (Ví dụ: "Điện, Cơ khí")
        public string ChuyenMon { get; set; }

        // Binding: CongViecCho (Số lượng việc đang làm)
        public int CongViecCho { get; set; }

        // Logic trạng thái
        public bool IsActive { get; set; }

        // Binding: TrangThai (Hiển thị chữ)
        public string TrangThai
        {
            get
            {
                if (!IsActive) return "Đã nghỉ việc";
                if (CongViecCho > 5) return "Quá tải";
                if (CongViecCho > 0) return "Đang bận";
                return "Đang rảnh";
            }
        }

        // Binding: StatusColor (Hiển thị màu chấm tròn)
        public string StatusColor
        {
            get
            {
                if (!IsActive) return "Gray";      // Nghỉ
                if (CongViecCho > 5) return "Red"; // Quá tải
                if (CongViecCho > 0) return "Orange"; // Bận
                return "#4CAF50";                  // Xanh lá (Rảnh)
            }
        }
    }
}