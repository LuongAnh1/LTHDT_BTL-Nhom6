using System;
using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;   // Namespace chứa class Device
using BTL_Nhom6.Services; // Namespace chứa các Service

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    public partial class DeviceDetailWindow : Window
    {
        // Khai báo các Service
        private readonly DeviceService _deviceService = new DeviceService();
        private readonly DeviceModelService _modelService = new DeviceModelService();
        private readonly LocationService _locationService = new LocationService();
        private readonly DeviceStatusService _statusService = new DeviceStatusService();

        private string _currentDeviceCode = null; // Lưu mã thiết bị nếu đang ở chế độ Sửa
        private bool _isEditMode = false;

        // Constructor 1: Dùng cho THÊM MỚI
        public DeviceDetailWindow()
        {
            InitializeComponent();
            LoadComboBoxData();
            lblTitle.Text = "THÊM THIẾT BỊ MỚI";
            txtDeviceCode.IsEnabled = true; // Cho phép nhập mã
        }

        // Constructor 2: Dùng cho CHỈNH SỬA
        public DeviceDetailWindow(string deviceCode)
        {
            InitializeComponent();
            LoadComboBoxData();

            _currentDeviceCode = deviceCode;
            _isEditMode = true;
            lblTitle.Text = "CẬP NHẬT THIẾT BỊ";
            txtDeviceCode.IsEnabled = false; // Không được sửa mã khóa chính

            LoadDeviceData(deviceCode);
        }

        // Hàm kéo cửa sổ
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        // Tải dữ liệu cho các ComboBox
        private void LoadComboBoxData()
        {
            try
            {
                cbModel.ItemsSource = _modelService.GetModels();
                cbLocation.ItemsSource = _locationService.GetAllLocations();
                cbStatus.ItemsSource = _statusService.GetAllDeviceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message);
            }
        }

        // Tải thông tin thiết bị cũ lên Form (chế độ Sửa)
        private void LoadDeviceData(string deviceCode)
        {
            var devices = _deviceService.GetAllDevices(); // Nên viết hàm GetDeviceById(string id) trong Service để tối ưu
            var dev = devices.Find(d => d.DeviceCode == deviceCode);

            if (dev != null)
            {
                txtDeviceCode.Text = dev.DeviceCode;
                txtDeviceName.Text = dev.DeviceName;
                txtSerialNumber.Text = dev.SerialNumber;

                cbModel.SelectedValue = dev.ModelID;
                cbLocation.SelectedValue = dev.LocationID;
                cbStatus.SelectedValue = dev.StatusID;

                dpPurchaseDate.SelectedDate = dev.PurchaseDate;
                dpWarrantyExpiry.SelectedDate = dev.WarrantyExpiry;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(txtDeviceCode.Text)) { MessageBox.Show("Vui lòng nhập Mã thiết bị!"); return; }
            if (string.IsNullOrWhiteSpace(txtDeviceName.Text)) { MessageBox.Show("Vui lòng nhập Tên thiết bị!"); return; }
            if (cbModel.SelectedValue == null) { MessageBox.Show("Vui lòng chọn Model!"); return; }
            if (cbLocation.SelectedValue == null) { MessageBox.Show("Vui lòng chọn Vị trí!"); return; }
            if (cbStatus.SelectedValue == null) { MessageBox.Show("Vui lòng chọn Trạng thái!"); return; }

            // 2. Tạo đối tượng Device
            Device dev = new Device
            {
                DeviceCode = txtDeviceCode.Text.Trim(),
                DeviceName = txtDeviceName.Text.Trim(),
                SerialNumber = txtSerialNumber.Text.Trim(),

                ModelID = (int)cbModel.SelectedValue,
                LocationID = (int)cbLocation.SelectedValue,
                StatusID = (int)cbStatus.SelectedValue,

                PurchaseDate = dpPurchaseDate.SelectedDate,
                WarrantyExpiry = dpWarrantyExpiry.SelectedDate,

                SupplierID = null // Tạm để null vì chưa có SupplierService
            };

            try
            {
                if (_isEditMode)
                {
                    // Update
                    _deviceService.UpdateDevice(dev);
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Add New
                    // Kiểm tra trùng mã trước khi thêm (Nên có hàm CheckExist trong Service)
                    // if (_deviceService.CheckExist(dev.DeviceCode)) ...

                    _deviceService.AddDevice(dev);
                    MessageBox.Show("Thêm mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Đóng cửa sổ và trả về kết quả True (để form cha reload lại data)
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}