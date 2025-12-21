using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo bạn đã có thư mục Helper và file NavigationHelper

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class TDDLC : Window
    {
        public TDDLC()
        {
            InitializeComponent();
        }

        #region Chuyển đổi Danh mục lớn (Main Tabs)

        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLVTPB());

        private void Button_QLLTB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLLTB_va_Model());

        private void Button_NCC_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new NCC_va_BGLK());

        #endregion

        #region Chuyển đổi Tab con (Sub Tabs)

        private void Button_DonViTinh_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC2());

        private void Button_LoiThuongGap_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC3());

        #endregion
    }
}