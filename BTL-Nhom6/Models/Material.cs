using System;

namespace BTL_Nhom6.Models
{
    public class Material
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }

        // Thay vì string Unit, giờ ta dùng UnitID
        public int UnitID { get; set; }

        // Bổ sung thêm UnitName để hiển thị lên Grid (khi JOIN bảng)
        // Thuộc tính này không lưu trực tiếp trong bảng Materials mà lấy từ bảng Units
        public string UnitName { get; set; }

        public int CurrentStock { get; set; }
        public decimal UnitPrice { get; set; }
    }
}