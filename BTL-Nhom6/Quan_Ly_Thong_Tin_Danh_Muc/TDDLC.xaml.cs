using System;
using System.Collections.Generic;
using System.Linq; // Để dùng Linq Filter
using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System.Windows.Media.Effects;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class TDDLC : Window
    {
        // Khởi tạo Service
        private readonly DeviceStatusService _deviceStatusService = new DeviceStatusService();

        // List lưu dữ liệu gốc để tìm kiếm mà không cần gọi lại DB liên tục
        private List<DeviceStatus> _originalList = new List<DeviceStatus>();

        // Biến kiểm tra quyền (để dùng lại nhiều chỗ)
        private bool _canEdit = false;
        public TDDLC()
        {
            InitializeComponent();

            ApplyPermissions(); // Áp dụng phân quyền

            // Gọi hàm load dữ liệu khi mở form
            Loaded += TDDLC_Loaded;
        }

        // --- HÀM PHÂN QUYỀN ---
        private void ApplyPermissions()
        {
            int roleId = UserSession.CurrentRoleID;

            // Quy định: Chỉ Admin (1) và Quản lý (2) mới được Thêm/Sửa/Xóa
            if (roleId == 1 || roleId == 2)
            {
                _canEdit = true;
            }
            else
            {
                _canEdit = false; // Nhân viên thường, Khách hàng...
            }

            // Nếu không có quyền sửa -> Ẩn các nút thao tác
            if (!_canEdit)
            {
                // 1. Ẩn nút Thêm mới (Cần đặt x:Name="btnAdd" trong XAML)
                if (btnAddNew != null) btnAddNew.Visibility = Visibility.Collapsed;

                // 2. Ẩn cột "HÀNH ĐỘNG" (Sửa/Xóa) trong DataGrid
                // Giả sử cột Hành động là cột cuối cùng
                if (dgDeviceStatus.Columns.Count > 0)
                {
                    dgDeviceStatus.Columns[dgDeviceStatus.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
        }

        // 1. Sự kiện khi form load xong
        private void TDDLC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        // 2. Hàm tải dữ liệu từ CSDL lên DataGrid
        private void LoadData()
        {
            try
            {
                // Lấy dữ liệu từ Service
                _originalList = _deviceStatusService.GetAllDeviceStatus();

                // Gán vào DataGrid
                dgDeviceStatus.ItemsSource = _originalList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 3. Chức năng Tìm kiếm (khi gõ vào ô Mã hoặc Tên)
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lấy từ khóa tìm kiếm
            string searchMa = txtSearchMa.Text.Trim().ToLower();
            string searchTen = txtSearchTen.Text.Trim().ToLower();

            if (_originalList == null || !_originalList.Any()) return;

            // Filter danh sách gốc (LINQ)
            var filteredList = _originalList.Where(item =>
                (string.IsNullOrEmpty(searchMa) || item.StatusID.ToString().Contains(searchMa)) &&
                (string.IsNullOrEmpty(searchTen) || item.StatusName.ToLower().Contains(searchTen))
            ).ToList();

            dgDeviceStatus.ItemsSource = filteredList;
        }

        // 4. Chức năng Xóa
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Lấy ID từ thuộc tính Tag của nút Xóa
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int statusId = (int)btn.Tag;

                // Xác nhận xóa
                MessageBoxResult result = MessageBox.Show($"Bạn có chắc chắn muốn xóa trạng thái (ID: {statusId}) không?",
                                                          "Xác nhận xóa",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _deviceStatusService.DeleteDeviceStatus(statusId);
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadData(); // Load lại bảng
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Không thể xóa (có thể dữ liệu đang được sử dụng).\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        // 5. CHỨC NĂNG THÊM MỚI (CÓ BLUR)
        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            // Tạo hiệu ứng làm mờ
            BlurEffect blurObj = new BlurEffect { Radius = 15 };

            // Áp dụng hiệu ứng cho cửa sổ hiện tại (TDDLC)
            this.Effect = blurObj;

            try
            {
                DeviceStatusWindow addWindow = new DeviceStatusWindow();

                // Mở form con dạng Dialog (người dùng buộc phải xử lý form con trước)
                if (addWindow.ShowDialog() == true)
                {
                    try
                    {
                        _deviceStatusService.AddDeviceStatus(addWindow.ResultStatus);
                        LoadData();
                        MessageBox.Show("Thêm mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi thêm mới: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            finally
            {
                // Gỡ bỏ hiệu ứng làm mờ dù có lỗi hay không
                this.Effect = null;
            }
        }

        // 6. CHỨC NĂNG SỬA (CÓ BLUR)
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int statusId = (int)btn.Tag;
                var itemToEdit = _originalList.FirstOrDefault(x => x.StatusID == statusId);

                if (itemToEdit != null)
                {
                    // Tạo hiệu ứng làm mờ
                    BlurEffect blurObj = new BlurEffect { Radius = 15 };
                    this.Effect = blurObj;

                    try
                    {
                        DeviceStatusWindow editWindow = new DeviceStatusWindow(itemToEdit);

                        if (editWindow.ShowDialog() == true)
                        {
                            try
                            {
                                _deviceStatusService.UpdateDeviceStatus(editWindow.ResultStatus);
                                LoadData();
                                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Lỗi cập nhật: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    finally
                    {
                        // Gỡ bỏ hiệu ứng làm mờ
                        this.Effect = null;
                    }
                }
            }
        }

        #region Chuyển đổi Danh mục lớn (Main Tabs)

        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLVTPB());

        private void Button_QLLTB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLLTB_va_Model());

        private void Button_NCC_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new NCC_va_BGLK());

        #endregion

        #region Chuyển đổi Tab con (Sub Tabs)

        private void Button_DonViTinh_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC2());

        private void Button_LoiThuongGap_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC3());

        #endregion
    }
}