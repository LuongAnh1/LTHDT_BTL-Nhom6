using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co; // Để điều hướng nếu cần

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class DMVT_va_DM : Window
    {
        private readonly MaterialService _service = new MaterialService();

        public DMVT_va_DM()
        {
            InitializeComponent();
            LoadData();
        }

        // Hàm tải dữ liệu
        private void LoadData()
        {
            string ma = txtSearchMa.Text.Trim();
            string ten = txtSearchTen.Text.Trim();

            dgVatTu.ItemsSource = _service.GetCatalog(ma, ten);
        }

        // Sự kiện tìm kiếm (Gõ phím Enter hoặc TextChanged tùy bạn)
        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadData();
            }
        }

        // Nút Thêm mới
        private void Button_ThemMoi_Click(object sender, RoutedEventArgs e)
        {
            // Tạo hiệu ứng mờ
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            this.Effect = blur;

            ThemVatTuDialog dlg = new ThemVatTuDialog();
            if (dlg.ShowDialog() == true)
            {
                LoadData(); // Load lại bảng sau khi thêm thành công
            }

            this.Effect = null;
        }

        // Sự kiện xóa (Cần gán trong XAML vào nút Delete: Click="Button_Delete_Click")
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dòng hiện tại
            Button btn = sender as Button;
            var item = btn.DataContext as MaterialCatalogViewModel;

            if (item != null)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa vật tư: {item.TenDM}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    if (_service.DeleteMaterial(item.MaterialID))
                    {
                        MessageBox.Show("Xóa thành công!");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa vật tư này vì đã phát sinh dữ liệu nhập/xuất kho.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // Sự kiện sửa 
        private void Button_Edit_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dòng dữ liệu hiện tại
            Button btn = sender as Button;
            var item = btn.DataContext as MaterialCatalogViewModel;

            if (item != null)
            {
                // 2. Tạo hiệu ứng mờ
                System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
                this.Effect = blur;

                // 3. Mở Dialog với Constructor sửa (Truyền item vào)
                ThemVatTuDialog dlg = new ThemVatTuDialog(item);

                if (dlg.ShowDialog() == true)
                {
                    LoadData(); // Load lại bảng sau khi sửa xong
                }

                this.Effect = null;
            }
        }

        #region Navigation
        // Copy logic điều hướng Sidebar/Tab của bạn vào đây
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e) { /* Trang hiện tại */ }
        private void Button_NhapKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NKVT()); 
        }
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new XKVT()); 
        }
        private void Button_TheKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LSGD()); 
        }
        #endregion
    }
}