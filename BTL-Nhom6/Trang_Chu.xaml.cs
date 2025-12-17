using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Tri_He_Thong;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6
{
    public partial class Trang_Chu : Window
    {
        public Trang_Chu()
        {
            InitializeComponent();
        }

        // Cho phép kéo thả cửa sổ khi click chuột trái vào vùng trống
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        // Xử lý sự kiện Button
        // Truy cập form QLND_va_PQ
        private void Button_QTHT_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLND_va_PQ());
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