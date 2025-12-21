using System;

namespace BTL_Nhom6.Models
{
    public class Location
    {
        public int LocationID { get; set; }
        public string LocationName { get; set; }
        public string Description { get; set; }

        // Dùng int? (nullable) vì ParentLocationID trong database có thể null (nếu là thư mục gốc)
        public int? ParentLocationID { get; set; }

        // Thuộc tính phụ: Dùng để hiển thị tên cha trên Grid (không lưu trong bảng này mà lấy qua JOIN)
        public string ParentLocationName { get; set; }

        // Constructor mặc định
        public Location() { }
    }
}