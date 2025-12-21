using System;
using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo namespace này đúng với file NavigationHelper của bạn

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    /// <summary>
    /// Interaction logic for TCTS.xaml
    /// </summary>
    public partial class TCTS : Window
    {
        public TCTS()
        {
            InitializeComponent();
        }

        #region ĐIỀU HƯỚNG TABS (THANH BAR NGANG)

        // 1. Hồ sơ thiết bị & QR
        private void Button_HSTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new HSTB_va_QR());
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

        #region XỬ LÝ LOGIC BỘ LỌC TÌM KIẾM

        // Hàm này dùng cho nút "ÁP DỤNG BỘ LỌC"
        private void Button_Loc_Click(object sender, RoutedEventArgs e)
        {
            // Logic xử lý tìm kiếm nâng cao tại đây
            // Ví dụ: Lấy giá trị từ TextBox và ComboBox để lọc DataGrid
            MessageBox.Show("Hệ thống đang tiến hành lọc dữ liệu tài sản theo yêu cầu của bạn!",
                            "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region XỬ LÝ TRÊN BẢNG KẾT QUẢ

        // Xem chi tiết một tài sản cụ thể (nút con mắt)
        private void Button_XemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hiển thị thông tin chi tiết tài sản nâng cao.");
        }

        #endregion
    }
}