using BTL_Nhom6.Helper;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    /// <summary>
    /// Interaction logic for QLND_va_PQ.xaml
    /// </summary>
    public partial class QLND_va_PQ : Window
    {
        public QLND_va_PQ()
        {
            InitializeComponent();
        }
        // --- Xử lý sự kiện Button ---
        // Chuyển tới trang Thay đổi Mật Khẩu và Thông Tin Cá Nhân
        private void Button_TDMK_va_TTCN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDMK_va_TTCN());
        }
        // Chuyển tới trang Quản lý hồ sơ kỹ năng
        private void Button_QLHSKN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLHSKN());
        }
        // Chuyển tới trang Nhật kỹ và sao lưu dữ liệu 
        private void Button_NK_va_SLDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NK_va_SLDL());
        }
        // Quay lại trang chủ
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
