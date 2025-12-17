using System;
using System.Windows;
// Nhớ using namespace chứa Dang_Nhap (thường là BTL_Nhom6)
using BTL_Nhom6;

namespace BTL_Nhom6.Helper
{
    public static class NavigationHelper
    {
        public static void Navigate(Window currentWindow, Window nextWindow)
        {
            try
            {
                if (currentWindow == null || nextWindow == null) return;

                // --- LOGIC MỚI: KIỂM TRA LOẠI CỬA SỔ ---

                // Kiểm tra xem cửa sổ tiếp theo có phải là Form Đăng Nhập không?
                // (Dùng GetType().Name để so sánh tên Class)
                bool isLoginPage = nextWindow.GetType().Name == "Dang_Nhap";

                if (isLoginPage)
                {
                    // Nếu là Đăng nhập: Reset về kích thước bình thường
                    nextWindow.WindowState = WindowState.Normal;
                    nextWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
                else
                {
                    // Nếu là các trang Quản trị khác: Giữ nguyên trạng thái (Toàn màn hình/Thu nhỏ)
                    nextWindow.WindowState = currentWindow.WindowState;
                }

                // ---------------------------------------

                // Hiển thị form mới trước
                nextWindow.Show();

                // Đóng form cũ sau
                currentWindow.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi điều hướng: " + ex.Message);
            }
        }
    }
}