using System;

namespace BTL_Nhom6.Models
{
    public class WorkOrder
    {
        public int WorkOrderID { get; set; }
        public string DeviceCode { get; set; }

        // Có thể null nếu tạo thủ công, không từ yêu cầu nào
        public int? RequestID { get; set; }

        // Có thể null nếu là sửa chữa sự cố bất ngờ
        public int? ScheduleID { get; set; }

        public int? TechnicianID { get; set; }
        public int StatusID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Solution { get; set; }

        
    }
}