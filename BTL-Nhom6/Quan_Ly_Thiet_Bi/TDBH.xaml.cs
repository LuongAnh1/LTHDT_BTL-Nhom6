
using BTL_Nhom6.Helper; // Nhớ using namespace chứa file NavigationService
using System.Windows;
using System.Windows.Navigation;
// using BTL_Nhom6.Quan_Ly_Thiet_Bi; // Namespace chứa các Window khác (nếu cần)

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using BTL_Nhom6.Models;    // Namespace chứa Device, Supplier
using BTL_Nhom6.Services;  // Namespace chứa Services

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    public partial class TDBH : Window
    {
        private readonly DeviceService _deviceService;
        private readonly SupplierService _supplierService;
        private List<Device> _originalList; // Lưu danh sách gốc để lọc

        public TDBH()
        {
            InitializeComponent();
            _deviceService = new DeviceService();
            _supplierService = new SupplierService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSuppliers();
            LoadData(); // Mặc định load tất cả hoặc load những cái sắp hết hạn
        }

        // 1. Load danh sách Nhà cung cấp vào ComboBox
        private void LoadSuppliers()
        {
            try
            {
                var suppliers = _supplierService.GetAllSuppliers();

                // Thêm một option "Tất cả" vào đầu list
                suppliers.Insert(0, new Supplier { SupplierID = -1, SupplierName = "Tất cả nhà cung cấp" });

                cbbNhaCungCap.ItemsSource = suppliers;
                cbbNhaCungCap.SelectedIndex = 0; // Chọn mặc định là Tất cả
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load nhà cung cấp: " + ex.Message);
            }
        }

        // 2. Load dữ liệu thiết bị và hiển thị lên Grid
        private void LoadData()
        {
            try
            {
                // Lấy tất cả thiết bị từ CSDL
                _originalList = _deviceService.GetAllDevices();

                // Lọc sơ bộ: Chỉ lấy những thiết bị CÓ ngày bảo hành
                _originalList = _originalList.Where(x => x.WarrantyExpiry != null).ToList();

                // Gán vào Grid
                dgDanhSachBaoHanh.ItemsSource = _originalList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // 3. Xử lý nút LỌC
        private void Button_Loc_Click(object sender, RoutedEventArgs e)
        {
            if (_originalList == null) return;

            var filteredList = _originalList.AsEnumerable();

            // a. Lọc theo Nhà cung cấp
            if (cbbNhaCungCap.SelectedValue != null && (int)cbbNhaCungCap.SelectedValue != -1)
            {
                int selectedSupID = (int)cbbNhaCungCap.SelectedValue;
                // So sánh tên NCC hoặc ID nếu trong Model Device có ID
                // Do Model Device bạn cung cấp có SupplierID (nullable), ta dùng nó
                filteredList = filteredList.Where(x => x.SupplierID == selectedSupID);
            }

            // b. Lọc theo Ngày (Từ ngày - Đến ngày)
            if (dpTuNgay.SelectedDate.HasValue)
            {
                filteredList = filteredList.Where(x => x.WarrantyExpiry >= dpTuNgay.SelectedDate.Value);
            }

            if (dpDenNgay.SelectedDate.HasValue)
            {
                // Lấy đến cuối ngày của ngày được chọn
                DateTime endDay = dpDenNgay.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);
                filteredList = filteredList.Where(x => x.WarrantyExpiry <= endDay);
            }

            // Cập nhật UI
            dgDanhSachBaoHanh.ItemsSource = filteredList.ToList();
        }

        // 4. Xử lý nút XÓA BỘ LỌC
        private void Button_XoaLoc_Click(object sender, RoutedEventArgs e)
        {
            dpTuNgay.SelectedDate = null;
            dpDenNgay.SelectedDate = null;
            cbbNhaCungCap.SelectedIndex = 0;

            // Reset lại danh sách gốc
            dgDanhSachBaoHanh.ItemsSource = _originalList;
        }

        // 5. Nút xuất báo cáo (Demo)
        private void Button_XuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng xuất Excel đang phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --- NAVIGATION ---

        private void Button_HSTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new HSTB_va_QR());

        }

        private void Button_DieuChuyen_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DC_va_BG());
        }

        private void Button_TraCuu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TCTS());
        }


        // --- LOGIC CHỨC NĂNG (Thêm vào nếu trong XAML đã có sự kiện Click) ---

        private void Button_XoaBoLoc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã xóa bộ lọc.");
        }
    }

    // --- CONVERTER MÀU SẮC NGÀY THÁNG ---
    public class DateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                DateTime today = DateTime.Now.Date;

                // Đã hết hạn -> Màu Đỏ
                if (date < today)
                    return Brushes.Red;

                // Sắp hết hạn (trong vòng 30 ngày tới) -> Màu Vàng Cam
                if ((date - today).TotalDays <= 30)
                    return Brushes.Orange;
            }
            // Còn hạn xa -> Màu Đen (hoặc xanh đậm tùy thích)
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
