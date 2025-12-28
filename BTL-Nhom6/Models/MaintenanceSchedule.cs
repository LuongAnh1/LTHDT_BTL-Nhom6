using System;

namespace BTL_Nhom6.Models
{
    public class MaintenanceSchedule
    {
        // --- 1. Map với CSDL ---
        public int ScheduleID { get; set; }
        public string DeviceCode { get; set; }
        public string TaskName { get; set; }
        public int FrequencyDays { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime NextMaintenanceDate { get; set; }
        public string Status { get; set; } // 'Active' hoặc 'Inactive'

        // --- 2. Thuộc tính hiển thị (JOIN từ bảng Devices) ---
        public string DeviceName { get; set; }

        // --- 3. Thuộc tính Binding cho XAML (Tiếng Việt - để khớp với giao diện bạn gửi) ---
        public string MaThietBi => DeviceCode;
        public string TenThietBi => DeviceName;
        public string TenCongViec => TaskName;
        public int TanSuat => FrequencyDays;
        public DateTime NgayDenHan => NextMaintenanceDate;

        public bool IsProcessing { get; set; } // Đang có người làm
        public string TechnicianName { get; set; } // Tên người làm
    }
}