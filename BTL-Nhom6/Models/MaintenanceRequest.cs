using System;

namespace BTL_Nhom6.Models
{
    public class MaintenanceRequest
    {
        // --- 1. CÁC THUỘC TÍNH GỐC (Mapping với cột trong Database) ---
        // ... Các thuộc tính gốc giữ nguyên ...
        public int RequestID { get; set; }
        public string DeviceCode { get; set; }
        public int RequestedBy { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ActualCompletion { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string ProblemDescription { get; set; }

        // --- THUỘC TÍNH BỔ SUNG TỪ JOIN ---
        public string DeviceName { get; set; }
        public string RequesterName { get; set; }

        // [MỚI] Thêm thuộc tính này để hứng dữ liệu từ Service
        public string TechnicianName { get; set; }

        // --- BINDING CHO XAML (Giữ nguyên Tiếng Việt) ---
        public string MucUuTien => Priority;
        public string ThietBi => DeviceName; // Binding đúng
        public string MaThietBi => DeviceCode;

        public string MoTaLoi => ProblemDescription;

        public string TrangThai
        {
            get
            {
                switch (Status)
                {
                    case "Pending": return "Đang chờ xử lý";
                    case "Approved": return "Đang thực hiện";
                    case "Rejected": return "Từ chối";
                    case "Completed": return "Hoàn thành";
                    default: return Status;
                }
            }
        }

        // Sửa lại: Lấy tên người dùng từ RequesterName
        public string NguoiYeuCau => RequesterName;

        // [SỬA] Logic hiển thị người xử lý
        public string NguoiXuLy => !string.IsNullOrEmpty(TechnicianName) ? TechnicianName : "Chưa phân công";

        public string NgayYeuCau => RequestDate.ToString("dd/MM/yyyy HH:mm");

        public string NgayHoanTat => ActualCompletion?.ToString("dd/MM/yyyy") ?? "--";
        public string GhiChu { get; set; } // Nếu bảng DB có cột Note thì map vào, ko thì để trống
    }
}