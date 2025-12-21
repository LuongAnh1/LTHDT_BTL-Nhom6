using System;
using System.Windows;
// Import namespace của Helper để sử dụng lớp điều hướng
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    /// <summary>
    /// Interaction logic for QLVTPB.xaml
    /// </summary>
    public partial class QLVTPB : Window
    {
        public QLVTPB()
        {
            InitializeComponent();
        }

        // ==========================================================
        // SỰ KIỆN CHUYỂN TAB TRONG QUẢN LÝ DANH MỤC
        // ==========================================================

        // Chuyển tới Tab: Quản lý loại thiết bị & Model
        private void Button_QLLTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLLTB_va_Model());
        }

        // Chuyển tới Tab: Nhà cung cấp & Báo giá linh kiện
        private void Button_NCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NCC_va_BGLK());
        }

        // Chuyển tới Tab: Từ điển dữ liệu chung
        private void Button_TDDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        // ==========================================================
        // CÁC SỰ KIỆN ĐIỀU HƯỚNG CHUNG (SIDEBAR/SYSTEM)
        // ==========================================================

        // Quay lại trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Trang_Chu());
        }

        // Đăng xuất khỏi hệ thống
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            // Có thể thêm xác nhận đăng xuất nếu muốn
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                NavigationHelper.Navigate(this, new Dang_Nhap());
            }
        }
    }
}