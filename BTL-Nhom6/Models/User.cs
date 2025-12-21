namespace BTL_Nhom6.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; } // Ví dụ: "Kỹ thuật viên"

        // --- CÁC THUỘC TÍNH BỔ SUNG CHO XAML ---

        // 1. RoleGroup: Để phân nhóm (Ví dụ: Nếu RoleName chứa "Admin" thì là Quản trị, còn lại là Nhân viên)
        public string RoleGroup =>
                    (string.IsNullOrEmpty(Username)) ? "Nhân viên" : // Kiểm tra Username
                    (Username.Trim().ToLower().StartsWith("admin")) ? "Quản trị viên" :
                    (Username.Trim().ToLower().StartsWith("customer")) ? "Khách hàng" :
                    "Nhân viên";

        // 2. PasswordMask: Để hiển thị dấu sao thay vì hash
        public string PasswordMask => "••••••••";

        // 3. IsActive: Binding với nút gạt (Toggle Switch)
        public bool IsActive { get; set; }
    }
}