using System;
// Đây là bảng ModelSuppliers
namespace BTL_Nhom6.Models
{
    public class SupplierQuoteDTO
    {
        // Các cột hiển thị trên lưới
        public int ModelID { get; set; }
        public int SupplierID { get; set; }

        public string ModelName { get; set; }     // Tên linh kiện/Model
        public string SupplierName { get; set; }  // Tên NCC
        public decimal Price { get; set; }        // Đơn giá
        public DateTime? LastSupplyDate { get; set; } // Ngày cung cấp gần nhất (dùng làm Ghi chú)

        // Vì bảng ModelSuppliers không có Tồn kho/Đơn vị, 
        // tạm thời mình để dữ liệu giả lập hoặc bạn cần JOIN thêm bảng Materials nếu muốn chính xác
        //public string Unit { get; set; } = "Cái";
        //public int Stock { get; set; } = 0;
        //public int MinStock { get; set; } = 5;
    }
}