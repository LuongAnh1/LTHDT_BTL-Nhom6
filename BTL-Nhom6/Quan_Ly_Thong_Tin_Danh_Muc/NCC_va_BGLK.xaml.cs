using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo đúng namespace của NavigationHelper

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class NCC_va_BGLK : Window
    {
        public NCC_va_BGLK()
        {
            InitializeComponent();
        }

        #region Điều hướng Tab chính

        // Xử lý nút Quản lý vị trí phòng ban
        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLVTPB());
        }

        // Xử lý nút Quản lý loại thiết bị
        private void Button_QLLTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLLTB_va_Model());
        }

        // Xử lý nút Từ điển dữ liệu chung (Chuyển sang TDDLC.xaml)
        private void Button_TDDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        #endregion

        #region Các hành động khác (Thêm, Sửa, Xóa NCC)
        // Bạn có thể viết các hàm xử lý dữ liệu nhà cung cấp tại đây
        #endregion
    }
}