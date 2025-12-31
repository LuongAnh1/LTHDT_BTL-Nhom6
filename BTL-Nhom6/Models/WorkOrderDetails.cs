using System;

namespace BTL_Nhom6.Models
{
    public class WorkOrderDetails
    {
        // Khóa chính phức hợp (Composite Key) trong Database, nhưng ở Model ta cứ liệt kê ra
        public int WorkOrderID { get; set; }
        public int MaterialID { get; set; }

        public int QuantityUsed { get; set; }  // Số lượng dùng
        public string Note { get; set; }       // Ghi chú

        // Cột này bạn đã thêm bằng lệnh ALTER TABLE, rất quan trọng để tính tiền
        public decimal UnitPrice { get; set; }
    }
}