using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Services; // Để dùng TechnicianService
using BTL_Nhom6.Models;   // Để dùng TechnicianViewModel (Quan trọng)
using BTL_Nhom6.Helper;   // Để dùng NavigationHelper

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class CNPCV : Window
    {
        // Khởi tạo Service
        private readonly TechnicianService _techService = new TechnicianService();

        public CNPCV()
        {
            InitializeComponent();
            LoadData();
        }

        // Sự kiện Loaded của Sidebar (nếu có dùng trong XAML)
        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Xử lý logic sidebar nếu cần
        }

        // Hàm tải dữ liệu
        private void LoadData()
        {
            string keyword = txtSearchKTV.Text.Trim();

            // Lấy giá trị lọc từ ComboBox (xử lý an toàn null)
            string filter = "Tất cả";
            // Giả sử ComboBox tên là cboFilter (bạn cần đặt x:Name="cboFilter" trong XAML nếu muốn dùng code này)
            // hoặc hardcode tạm thời để test:
            // if (cboFilter != null && cboFilter.SelectedItem is ComboBoxItem item)
            //    filter = item.Content.ToString();

            // Gọi Service lấy danh sách
            // LƯU Ý: Hàm GetTechnicianStats trả về List<TechnicianViewModel>
            dgKyThuatVien.ItemsSource = _techService.GetTechnicianStats(keyword, filter);
        }

        // Sự kiện khi gõ phím tìm kiếm
        private void TxtSearchKTV_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        // Sự kiện nút "Xem công việc" trong DataGrid
        private void Button_XemCongViec_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            // --- SỬA LỖI Ở ĐÂY: Dùng TechnicianViewModel thay vì KyThuatVienModel ---
            var tech = btn.DataContext as TechnicianViewModel;

            if (tech != null)
            {
                // Mở form chi tiết (Form này mình sẽ làm ở bước tiếp theo nếu bạn yêu cầu)
                MessageBox.Show($"Đang mở danh sách việc của: {tech.TenKTV}\nMã NV: {tech.MaNV}");

                // Ví dụ mở form chi tiết:
                // ChiTietCongViecKTV form = new ChiTietCongViecKTV(tech.UserID);
                // form.ShowDialog();
            }
        }

        #region CHUYỂN TRANG (NAVIGATION)
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLYCBT());
        }

        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LKH_va_DP());
        }

        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new KKVT_va_NT());
        }
        #endregion
    }
}