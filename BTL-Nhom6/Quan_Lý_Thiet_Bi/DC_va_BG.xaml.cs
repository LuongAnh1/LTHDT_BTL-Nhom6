using System;
using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo namespace này khớp với file NavigationHelper của bạn

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    /// <summary>
    /// Interaction logic for DC_va_BG.xaml
    /// </summary>
    public partial class DC_va_BG : Window
    {
        public DC_va_BG()
        {
            InitializeComponent();
        }

        #region ĐIỀU HƯỚNG TABS (THANH BAR NGANG)

        // 1. Hồ sơ thiết bị & QR
        private void Button_HSTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new HSTB_va_QR());
        }

        // 2. Tra cứu tài sản (TCTS)
        private void Button_TraCuu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TCTS());
        }

        // 3. Theo dõi bảo hành (TDBH)
        private void Button_BaoHanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDBH());
        }
      
        #endregion

        #region XỬ LÝ LOGIC TẠO PHIẾU ĐIỀU CHUYỂN

        // Sự kiện khi bấm nút "Xác nhận điều chuyển"
        private void Button_XacNhan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Logic kiểm tra dữ liệu đầu vào (Validation)
            // Ví dụ: Kiểm tra xem đã chọn thiết bị chưa, đã chọn vị trí mới chưa...

            // 2. Hiển thị thông báo xác nhận
            MessageBoxResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn thực hiện phiếu điều chuyển này không?",
                "Xác nhận điều chuyển",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Thực hiện lưu vào Database tại đây...

                MessageBox.Show("Đã tạo phiếu điều chuyển và cập nhật vị trí thiết bị thành công!",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Logic làm mới form hoặc cập nhật lại bảng lịch sử ở dưới
            }
        }

        #endregion

        #region XỬ LÝ TRÊN BẢNG LỊCH SỬ

        // In biên bản điều chuyển
        private void Button_InBienBan_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang chuẩn bị xuất biên bản bàn giao (PDF/Excel)...");
        }

        // Xem chi tiết lịch sử
        private void Button_XemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hiển thị thông tin chi tiết của phiếu điều chuyển này.");
        }

        #endregion
    }
}