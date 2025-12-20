using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo đúng namespace của NavigationHelper

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class QLLTB_va_Model : Window
    {
        public QLLTB_va_Model()
        {
            InitializeComponent();
        }

        #region Điều hướng Tab chính

        // Chuyển sang Quản lý vị trí phòng ban
        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLVTPB());
        }

        // Chuyển sang Nhà cung cấp & Báo giá
        private void Button_NCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NCC_va_BGLK());
        }

        // Chuyển sang Từ điển dữ liệu chung (TDDLC.xaml)
        private void Button_TDDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        #endregion

        #region Các hành động khác (Search, Thêm, Sửa, Xóa)
        // Bạn có thể viết logic lọc dữ liệu cho các ô txtSearch tại đây
        #endregion
    }
}