using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo đúng namespace của NavigationHelper

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    public partial class HSTB_va_QR : Window
    {
        public HSTB_va_QR()
        {
            InitializeComponent();
        }

        #region Điều hướng Tab (Thanh bar)

        // 1. Hồ sơ thiết bị & QR (Trang hiện tại)
        private void Button_HSTB_Click(object sender, RoutedEventArgs e) { }

        // 2. Tra cứu tài sản
        private void Button_TraCuu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TCTS());
        }

        // 3. Theo dõi bảo hành
        private void Button_BaoHanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDBH());
        }

        // 4. Điều chuyển & Bàn giao
        private void Button_BanGiao_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DC_va_BG());
        }

        #endregion
    }
}