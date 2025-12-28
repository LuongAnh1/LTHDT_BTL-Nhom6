using System;

namespace BTL_Nhom6.Models
{
    public class Material
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public int UnitID { get; set; }
        public string UnitName { get; set; } // Hiển thị (Not mapped to Insert)
        public int CurrentStock { get; set; }
        public decimal UnitPrice { get; set; }

        // --- BỔ SUNG CÁC TRƯỜNG CÒN THIẾU ---
        public string Description { get; set; } // Mô tả
        public int MinStock { get; set; }       // Định mức tồn kho
    }
}