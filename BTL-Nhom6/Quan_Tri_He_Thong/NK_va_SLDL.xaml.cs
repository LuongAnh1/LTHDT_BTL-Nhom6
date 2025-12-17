using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
// Import namespace của Helper
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    public partial class NK_va_SLDL : Window
    {
        public NK_va_SLDL()
        {
            InitializeComponent();
        }

        // --- Xử lý sự kiện Button ---
        // Sử dụng hàm Helper để chuyển đến form Nhập kho và xuất số liệu dữ liệu
        // Chuyển đến form Quản lý người dùng và phân quyền
        private void Button_QLND_va_PQ_Click(object sender, RoutedEventArgs e)
        {
            // Gọi hàm Helper: truyền vào (cửa sổ hiện tại, cửa sổ mới)
            NavigationHelper.Navigate(this, new QLND_va_PQ());
        }

        // Chuyển đến form Quản lý hồ sơ kỹ năng
        private void Button_QLHSKN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLHSKN());
        }
        // Chuyển đến form Thay đổi mật khẩu và thông tin cá nhân
        private void Button_TDMK_va_TTCN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDMK_va_TTCN());
        }
        // Chuyển đến form Trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Trang_Chu());
        }
        // Nút đăng xuất
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Dang_Nhap());
        }
        // --- Xử lý sự kiện Window ---
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Maximize_Click(object sender, RoutedEventArgs e)
        {
            bool isNormal = this.WindowState == WindowState.Normal;
            this.WindowState = isNormal ? WindowState.Maximized : WindowState.Normal;
            iconMaximize.Kind = isNormal ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
        }
    }
}