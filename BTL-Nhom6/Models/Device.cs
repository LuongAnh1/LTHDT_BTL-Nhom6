using System;

namespace BTL_Nhom6.Models
{
    public class Device
    {
        // Thuộc tính gốc (Mapping với Database)
        public string DeviceCode { get; set; } // Khóa chính
        public string DeviceName { get; set; }
        public int ModelID { get; set; }
        public string SerialNumber { get; set; }
        public int LocationID { get; set; }
        public int StatusID { get; set; }
        public DateTime? PurchaseDate { get; set; }   // Nullable (có thể để trống)
        public DateTime? WarrantyExpiry { get; set; } // Nullable
        public int? SupplierID { get; set; }          // Nullable

        // --- Thuộc tính hiển thị (Lấy từ bảng khác qua JOIN) ---
        public string ModelName { get; set; }
        public string LocationName { get; set; }
        public string StatusName { get; set; }   // Cần bảng DeviceStatus
        public string SupplierName { get; set; } // Cần bảng Suppliers

        // Constructor
        public Device() { }
    }
}