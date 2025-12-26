namespace BTL_Nhom6.Models
{
    public class WorkOrderViewModel
    {
        public int WorkOrderID { get; set; }

        public string MaPhieu => $"WO-{WorkOrderID:D4}"; // Format WO-0001

        public string TenThietBi { get; set; }
        public string MoTaLoi { get; set; }

        public string MucUuTien { get; set; } // Low, High...

        // Logic màu sắc cho Mức ưu tiên
        public string PriorityBg => MucUuTien == "High" || MucUuTien == "Critical" ? "#FEE2E2" : "#E0F2F1";
        public string PriorityFg => MucUuTien == "High" || MucUuTien == "Critical" ? "#D32F2F" : "#00695C";

        public string TrangThai { get; set; } // Mới tạo, Đang làm...

        // Logic Icon và màu cho Trạng thái
        public string StatusIcon => TrangThai == "Hoàn thành" ? "CheckCircle" : "ClockOutline";
        public string StatusColor => TrangThai == "Hoàn thành" ? "Green" : "Orange";
    }
}