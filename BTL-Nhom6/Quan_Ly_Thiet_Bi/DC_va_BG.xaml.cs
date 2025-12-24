using BTL_Nhom6.Helper; // Đảm bảo namespace này khớp với file NavigationHelper của bạn
using System;
using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    public partial class DC_va_BG : Window
    {
        private readonly DeviceService _deviceService;
        private readonly LocationService _locationService;

        // Biến lưu ID vị trí hiện tại để kiểm tra trùng
        private int _currentLocationId = 0;

        public DC_va_BG()
        {
            InitializeComponent();
            _deviceService = new DeviceService();
            _locationService = new LocationService();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // 1. Load danh sách thiết bị vào ComboBox
                var devices = _deviceService.GetAllDevices();
                cbbThietBi.ItemsSource = devices;

                // 2. Load danh sách phòng ban vào ComboBox
                var locations = _locationService.GetAllLocations();
                cbbViTriMoi.ItemsSource = locations;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Sự kiện: Khi chọn thiết bị -> Tự động điền vị trí cũ
        private void cbbThietBi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbThietBi.SelectedItem is Device selectedDevice)
            {
                // Hiển thị tên vị trí hiện tại lên TextBox (Readonly)
                txtViTriHienTai.Text = selectedDevice.LocationName;

                // Lưu ID vị trí hiện tại
                _currentLocationId = selectedDevice.LocationID;
            }
        }

        private void Button_XacNhan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (cbbThietBi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn thiết bị!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbbViTriMoi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn vị trí mới!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Lấy dữ liệu
            string deviceCode = cbbThietBi.SelectedValue.ToString();
            int newLocationId = (int)cbbViTriMoi.SelectedValue;

            // Kiểm tra nếu vị trí mới trùng vị trí cũ
            if (newLocationId == _currentLocationId)
            {
                MessageBox.Show("Thiết bị đang ở vị trí này rồi. Vui lòng chọn vị trí khác.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 3. Thực hiện Update
            bool success = _deviceService.UpdateDeviceLocation(deviceCode, newLocationId);

            if (success)
            {
                MessageBox.Show("Cập nhật vị trí thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form và Load lại dữ liệu để cập nhật danh sách thiết bị (có LocationName mới)
                cbbThietBi.SelectedIndex = -1;
                cbbViTriMoi.SelectedIndex = -1;
                txtViTriHienTai.Text = "-";
                txtGhiChu.Text = "";

                LoadData(); // Load lại để ComboBox thiết bị cập nhật vị trí mới trong data source
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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