using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
// Import namespace của Helper
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    public partial class TDMK_va_TTCN : Window
    {
        public TDMK_va_TTCN()
        {
            InitializeComponent();
        }
        // Chuyển tới trang Quản Lý Người Dùng và Phân Quyền
        private void Button_QLND_va_PQ_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLND_va_PQ());
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
    }
}