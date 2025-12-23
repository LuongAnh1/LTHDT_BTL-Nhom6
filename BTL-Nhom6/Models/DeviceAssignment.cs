using System;

namespace BTL_Nhom6.Models
{
    public class DeviceAssignment
    {
        // --- Thuộc tính gốc (Mapping với Database) ---
        public int AssignmentID { get; set; }        // Khóa chính
        public string DeviceCode { get; set; }       // Khóa ngoại -> Devices
        public int UserID { get; set; }              // Khóa ngoại -> Users
        public DateTime AssignedDate { get; set; }   // Ngày cấp
        public DateTime? ReturnDate { get; set; }    // Ngày trả (Nullable vì đang giữ thì chưa có ngày trả)
        public string Note { get; set; }             // Ghi chú

        // --- Thuộc tính hiển thị (Dùng cho DataGridView hoặc Combobox) ---
        // Lấy từ bảng Devices
        public string DeviceName { get; set; }

        // Lấy từ bảng Users
        public string Username { get; set; }
        public string UserFullName { get; set; }     // Tên đầy đủ nhân viên (Quan trọng để hiển thị)

        // Constructor
        public DeviceAssignment()
        {
            // Mặc định ngày cấp là ngày hiện tại khi khởi tạo mới
            AssignedDate = DateTime.Now;
        }
    }
}