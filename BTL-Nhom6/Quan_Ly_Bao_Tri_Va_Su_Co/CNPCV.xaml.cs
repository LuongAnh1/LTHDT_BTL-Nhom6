using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class CNPCV : Window
    {
        private readonly TechnicianService _techService = new TechnicianService();

        public CNPCV()
        {
            InitializeComponent();
            LoadData();
        }

        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Logic sidebar (nếu cần)
        }

        // ============================================================
        // 1. HÀM TẢI DỮ LIỆU (ĐÃ CẬP NHẬT LẤY FILTER)
        // ============================================================
        private void LoadData()
        {
            // === FIX LỖI: KIỂM TRA NULL ===
            // Nếu DataGrid chưa được tạo (lúc form mới khởi động), thì dừng hàm ngay
            if (dgKyThuatVien == null) return;

            // Lấy từ khóa tìm kiếm
            // Cần kiểm tra cả txtSearchKTV đề phòng nó cũng chưa tạo xong
            string keyword = (txtSearchKTV != null) ? txtSearchKTV.Text.Trim() : "";

            // Lấy giá trị lọc từ ComboBox
            string filter = "Tất cả";

            if (cboTrangThai != null && cboTrangThai.SelectedItem is ComboBoxItem item)
            {
                if (item.Content != null)
                {
                    filter = item.Content.ToString();
                }
            }

            // Gọi Service
            dgKyThuatVien.ItemsSource = _techService.GetTechnicianStats(keyword, filter);
        }

        // ============================================================
        // 2. SỰ KIỆN KHI CHỌN COMBOBOX -> RELOAD LẠI BẢNG
        // ============================================================
        private void CboTrangThai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        // Sự kiện tìm kiếm text
        private void TxtSearchKTV_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        private void Button_XemCongViec_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            // Lấy dữ liệu dòng hiện tại
            var tech = btn.DataContext as TechnicianViewModel;

            if (tech != null)
            {
                // 1. Khởi tạo form chi tiết, truyền vào ID của thợ
                ChiTietCongViecKTV detailWindow = new ChiTietCongViecKTV(tech.UserID);

                // 2. Chuyển sang màn hình chi tiết (ẩn form hiện tại, hiện form mới)
                NavigationHelper.Navigate(this, detailWindow);
            }
        }

        #region NAVIGATION
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new QLYCBT());
        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new LKH_va_DP());
        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new KKVT_va_NT());
        #endregion
    }
}