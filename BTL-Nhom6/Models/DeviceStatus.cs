using System;

namespace BTL_Nhom6.Models
{
    public class DeviceStatus
    {
        // Khóa chính: StatusID
        public int StatusID { get; set; }

        // Tên trạng thái: StatusName
        public string StatusName { get; set; }

        // Mô tả: Description
        public string Description { get; set; }
    }
}