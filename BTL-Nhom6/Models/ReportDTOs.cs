using System;
using System.Collections.Generic;

namespace BTL_Nhom6.Models // Hoặc namespace DTOs tùy bạn
{
    // Class dùng cho DataGrid "Thiết bị sắp hết hạn"
    public class DeviceWarrantyDTO
    {
        public string MaTB { get; set; }        // Map từ DeviceCode
        public string TenTB { get; set; }       // Map từ DeviceName
        public DateTime? NgayMua { get; set; }  // Map từ PurchaseDate
        public DateTime? NgayHetHan { get; set; } // Map từ WarrantyExpiry
    }

    // Class dùng cho ItemsControl "Đánh giá Nhà cung cấp"
    public class SupplierEvaluationDTO
    {
        public int SupplierID { get; set; }
        public string TenCongTy { get; set; }   // Map từ SupplierName
        public string LienHe { get; set; }      // Map từ ContactPerson + Phone
        public string MoTa { get; set; }        // Map từ Address
        public string DanhGia { get; set; }     // Logic tự tính (Tốt/Trung bình/Kém)
    }

    public class DeviceStatusDTO
    {
        public string MaTB { get; set; }      // Map từ DeviceCode
        public string TenTB { get; set; }     // Map từ DeviceName
        public string TrangThai { get; set; } // Map từ StatusName (Tốt, Hỏng, Thanh lý...)
    }

    // 2. DTO dùng cho Biểu đồ tròn: Thống kê số lượng theo trạng thái
    public class StatusChartDTO
    {
        public string StatusName { get; set; }
        public int Quantity { get; set; }
    }

}