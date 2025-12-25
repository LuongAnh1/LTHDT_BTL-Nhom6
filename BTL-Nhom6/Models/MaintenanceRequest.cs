using System;

namespace BTL_Nhom6.Models
{
    public class MaintenanceRequest
    {
        // --- 1. CÁC THUỘC TÍNH GỐC (Mapping với cột trong Database) ---
        public int RequestID { get; set; }
        public string DeviceCode { get; set; }
        public int RequestedBy { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ActualCompletion { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string ProblemDescription { get; set; }

        // --- 2. CÁC THUỘC TÍNH BỔ SUNG (BỊ THIẾU - CẦN THÊM VÀO ĐÂY) ---
        // Hai thuộc tính này dùng để chứa kết quả JOIN từ bảng Devices và Users
        public string DeviceName { get; set; }
        public string RequesterName { get; set; }

        // --- 3. CÁC THUỘC TÍNH BINDING CHO XAML (Tiếng Việt) ---

        public string MucUuTien => Priority;

        // Sửa lại: Lấy tên thiết bị từ DeviceName chứ không phải DeviceCode
        public string ThietBi => DeviceName;

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

        public string NguoiXuLy { get; set; } = "Admin";

        public string NgayYeuCau => RequestDate.ToString("dd/MM/yyyy HH:mm");

        public string NgayHoanTat => ActualCompletion?.ToString("dd/MM/yyyy") ?? "--";

        public string GhiChu { get; set; }
    }
}