using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class TDDLC2 : Window
    {
        public TDDLC2()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            icDonViTinh.ItemsSource = new List<object> {
                new { Ma = "CAI", Ten = "Cái", MoTa = "Đơn vị tính đếm được." },
                new { Ma = "BO", Ten = "Bộ", MoTa = "Tập hợp các vật thể." }
            };
        }

        #region Chuyển đổi Danh mục lớn
        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new QLVTPB());
        private void Button_QLLTB_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new QLLTB_va_Model());
        private void Button_NCC_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new NCC_va_BGLK());
        #endregion

        #region Chuyển đổi Tab con
        private void Button_TrangThai_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC()); // Về trang 1

        private void Button_Loi_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC3()); // Sang trang 3
        #endregion
    }
}