using System;

namespace BTL_Nhom6.Models
{
    public class Device
    {
        // --- Thuộc tính gốc (Mapping với Database) ---
        public string DeviceCode { get; set; }
        public string DeviceName { get; set; }
        public int ModelID { get; set; }
        public string SerialNumber { get; set; }
        public int LocationID { get; set; }
        public int StatusID { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
        public int? SupplierID { get; set; }

        // --- Thuộc tính hiển thị (Lấy từ bảng khác qua JOIN) ---
        public string ModelName { get; set; }
        public string LocationName { get; set; }
        public string StatusName { get; set; }
        public string SupplierName { get; set; }

        // --- BỔ SUNG THUỘC TÍNH HIỂN THỊ THÔNG TIN LIÊN HỆ ---
        public string SupplierPhone { get; set; }        // SĐT nhà cung cấp
        public string SupplierContactPerson { get; set; } // Người liên hệ bảo hành

        // [QUAN TRỌNG] Bổ sung thuộc tính để hiển thị người đang giữ thiết bị
        // Dữ liệu này sẽ lấy từ bảng DeviceAssignments (những dòng có ReturnDate = NULL)
        public string CurrentUserFullName { get; set; }

        // Constructor
        public Device() { }
    }
}