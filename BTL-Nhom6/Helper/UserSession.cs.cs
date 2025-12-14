using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTL_Nhom6.Helper
{
    // Class tĩnh đẻe lưu thông tin người đang đăng nhập trong suốt phiên làm việc
    public static class UserSession
    {
        public static int CurrentUserID { get; set; }
        public static string CurrentUserName { get; set; }
        public static int CurrentRoleID { get; set; }
        public static string CurrentFullName { get; set; }

        // Hàm xóa session khi đăng xuất
        public static void Clear()
        {
            CurrentUserID = 0;
            CurrentUserName = null;
            CurrentRoleID = 0;
            CurrentFullName = null;
        }
    }
}
