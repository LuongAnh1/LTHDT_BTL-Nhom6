using BTL_Nhom6.Helper;
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
        private readonly UserService _userService; // [MỚI] 1. Khai báo UserService

        // Biến lưu trạng thái cũ để so sánh
        private int _currentLocationId = 0;
        private int _currentUserId = 0; // [MỚI] 2. Biến lưu ID người giữ hiện tại

        public DC_va_BG()
        {
            InitializeComponent();
            _deviceService = new DeviceService();
            _locationService = new LocationService();
            _userService = new UserService(); // [MỚI] 3. Khởi tạo
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Load danh sách thiết bị
                var devices = _deviceService.GetAllDevices();
                cbbThietBi.ItemsSource = devices;

                // Load danh sách phòng ban
                var locations = _locationService.GetAllLocations();
                cbbViTriMoi.ItemsSource = locations;

                // [MỚI] 4. Load danh sách nhân viên vào ComboBox Người mới
                var users = _userService.GetAllUsers();
                cbbNguoiMoi.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Sự kiện: Khi chọn thiết bị -> Tự động điền thông tin cũ (Vị trí & Người giữ)
        private void cbbThietBi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbThietBi.SelectedItem is Device selectedDevice)
            {
                // --- Xử lý Vị trí ---
                txtViTriHienTai.Text = selectedDevice.LocationName;
                _currentLocationId = selectedDevice.LocationID;

                // --- [MỚI] 5. Xử lý Người phụ trách ---
                // Hiển thị tên người đang giữ (Lấy từ Model Device đã cập nhật ở bước trước)
                txtNguoiHienTai.Text = !string.IsNullOrEmpty(selectedDevice.CurrentUserFullName)
                                       ? selectedDevice.CurrentUserFullName
                                       : "Chưa bàn giao";

                // Lưu ID người đang giữ
                _currentUserId = selectedDevice.CurrentHolderId;

                // Reset ComboBox chọn mới về null để người dùng tự chọn nếu muốn thay đổi
                cbbViTriMoi.SelectedIndex = -1;
                cbbNguoiMoi.SelectedIndex = -1;
            }
        }

        private void Button_XacNhan_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu cơ bản
            if (cbbThietBi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn thiết bị!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Lấy dữ liệu ID
            string deviceCode = cbbThietBi.SelectedValue.ToString();

            // Logic lấy Vị trí mới: Nếu không chọn trong CBB thì giữ nguyên vị trí cũ
            int newLocationId = cbbViTriMoi.SelectedValue != null
                                ? (int)cbbViTriMoi.SelectedValue
                                : _currentLocationId;

            // Logic lấy Người nhận mới: Nếu không chọn trong CBB thì giữ nguyên người cũ
            int newUserId = cbbNguoiMoi.SelectedValue != null
                            ? (int)cbbNguoiMoi.SelectedValue
                            : _currentUserId;

            // 3. Kiểm tra xem có thay đổi gì không?
            if (newLocationId == _currentLocationId && newUserId == _currentUserId)
            {
                MessageBox.Show("Bạn chưa thay đổi Vị trí hoặc Người phụ trách nào cả.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 4. [SỬA] Gọi hàm Service cập nhật cả 2 thông tin
            // Hàm này cần 3 tham số: Mã TB, ID Vị trí Mới, ID Người Mới
            bool success = _deviceService.TransferAndHandover(deviceCode, newLocationId, newUserId);

            if (success)
            {
                string msg = "Cập nhật thành công!\n";
                if (newLocationId != _currentLocationId) msg += "- Đã điều chuyển vị trí.\n";
                if (newUserId != _currentUserId) msg += "- Đã bàn giao người phụ trách.";

                MessageBox.Show(msg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form
                ResetForm();

                // Load lại dữ liệu để cập nhật danh sách thiết bị (trạng thái mới)
                LoadData();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật Database.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm phụ trợ để xóa trắng form
        private void ResetForm()
        {
            cbbThietBi.SelectedIndex = -1;
            cbbViTriMoi.SelectedIndex = -1;
            cbbNguoiMoi.SelectedIndex = -1; // [MỚI]

            txtViTriHienTai.Text = "-";
            txtNguoiHienTai.Text = "-"; // [MỚI]
            txtGhiChu.Text = "";

            _currentLocationId = 0;
            _currentUserId = 0;
        }

        #region ĐIỀU HƯỚNG TABS
        private void Button_HSTB_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new HSTB_va_QR());
        private void Button_TraCuu_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new TCTS());
        private void Button_BaoHanh_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new TDBH());
        #endregion

        #region XỬ LÝ KHÁC
        private void Button_InBienBan_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng in biên bản đang phát triển.");
        }

        // Xem chi tiết lịch sử
        private void Button_XemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng xem lịch sử đang phát triển.");
        }

        #endregion
    }
}