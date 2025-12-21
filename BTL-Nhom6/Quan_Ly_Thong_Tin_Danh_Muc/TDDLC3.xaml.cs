using System.Collections.Generic;
using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo folder Helper chứa NavigationHelper.cs

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class TDDLC3 : Window
    {
        public TDDLC3()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Dữ liệu mẫu hiển thị lên bảng "Các lỗi thường gặp"
            var listLoi = new List<object>
            {
                new { Ma = "E001", Ten = "Lỗi nguồn", MoTa = "Thiết bị không lên nguồn hoặc tự động tắt.", GiaiPhap = "Kiểm tra dây nguồn, ổ cắm." },
                new { Ma = "E002", Ten = "Lỗi kết nối", MoTa = "Không thể kết nối LAN/Wifi.", GiaiPhap = "Kiểm tra cáp mạng và Router." },
                new { Ma = "E003", Ten = "Lỗi màn hình", MoTa = "Màn hình sọc hoặc không hiển thị.", GiaiPhap = "Kiểm tra cáp tín hiệu." }
            };
            icLoiThuongGap.ItemsSource = listLoi;
        }

        #region Chuyển đổi Tab chính (Main Tabs)

        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLVTPB());

        private void Button_QLLTB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLLTB_va_Model());

        private void Button_NCC_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new NCC_va_BGLK());

        #endregion

        #region Chuyển đổi Tab con (Sub Tabs)
        private void Button_TrangThai_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        // Thêm hàm xử lý sự kiện Click cho nút Đơn vị tính
        private void Button_DonViTinh_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC2());
        }

        #endregion
    }
}