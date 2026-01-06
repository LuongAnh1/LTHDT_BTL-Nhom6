// Import namespace của Helper để sử dụng lớp điều hướng
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects; // Thêm thư viện này để dùng BlurEffect

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class QLVTPB : Window
    {
        // Khởi tạo Service
        private LocationService _locationService = new LocationService();
        private List<Location> _allLocations;
        private DeviceService _deviceService = new DeviceService();


        // Biến kiểm tra quyền (để dùng lại nhiều chỗ)
        private bool _canEdit = false;

        public QLVTPB()
        {
            InitializeComponent();

            // 1. Phân quyền trước khi load dữ liệu
            ApplyPermissions();

            LoadData();
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
                if (btnAdd != null) btnAdd.Visibility = Visibility.Collapsed;

                // 2. Ẩn cột "HÀNH ĐỘNG" (Sửa/Xóa) trong DataGrid
                // Giả sử cột Hành động là cột cuối cùng
                if (dgLocations.Columns.Count > 0)
                {
                    dgLocations.Columns[dgLocations.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
        }

        // Hàm lấy dữ liệu từ DB và đổ vào DataGrid
        private void LoadData()
        {
            try
            {
                _allLocations = _locationService.GetAllLocations();
                dgLocations.ItemsSource = _allLocations;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }


        //1. Sự kiện khi bấm nút Sửa
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!_canEdit) return; // Chặn nếu không có quyền
            // Lấy nút bấm và dữ liệu dòng tương ứng
            Button btn = sender as Button;
            if (btn != null && btn.Tag is Location selectedLocation)
            {
                // BƯỚC 1: TẠO HIỆU ỨNG MỜ
                BlurEffect blurObj = new BlurEffect();
                blurObj.Radius = 15;
                this.Effect = blurObj;

                // BƯỚC 2: MỞ FORM CON (Mode Sửa)
                // Truyền đối tượng cần sửa vào constructor
                LocationWindow form = new LocationWindow(selectedLocation);

                bool? result = form.ShowDialog();

                // BƯỚC 3: GỠ BỎ HIỆU ỨNG
                this.Effect = null;

                // BƯỚC 4: LOAD LẠI DỮ LIỆU
                if (result == true)
                {
                    LoadData();
                    MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        //2. Sự kiện khi bấm nút Xóa
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!_canEdit) return; // Chặn nếu không có quyền
            Button btn = sender as Button;
            if (btn == null || !(btn.Tag is Location selectedLocation))
                return;

            try
            {
                // 1. KIỂM TRA LOCATION CÒN DEVICE KHÔNG
                int deviceCount = _deviceService.CountDevicesByLocation(selectedLocation.LocationID);

                if (deviceCount > 0)
                {
                    MessageBox.Show(
                        $"Không thể xóa vị trí '{selectedLocation.LocationName}'.\n" +
                        $"Hiện đang có {deviceCount} thiết bị thuộc vị trí này.\n\n" +
                        "Vui lòng chuyển hoặc xóa các thiết bị trước.",
                        "Không thể xóa",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );
                    return;
                }

                // 2. HỎI XÁC NHẬN XÓA
                var result = MessageBox.Show(
                    $"Bạn có chắc muốn xóa '{selectedLocation.LocationName}'?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result != MessageBoxResult.Yes)
                    return;

                // 3. THỰC HIỆN XÓA
                _locationService.DeleteLocation(selectedLocation.LocationID);
                LoadData();

                MessageBox.Show(
                    "Xóa vị trí thành công!",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi xóa vị trí: " + ex.Message,
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        //3.  Sự kiện nút Thêm mới (Góc trên phải giao diện)

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!_canEdit) return; // Chặn nếu không có quyền
            // BƯỚC 1: TẠO HIỆU ỨNG MỜ
            BlurEffect blurObj = new BlurEffect();
            blurObj.Radius = 15;
            this.Effect = blurObj;

            // BƯỚC 2: MỞ FORM CON (Mode Thêm mới)
            // Gọi constructor không tham số như đã định nghĩa ở LocationWindow
            LocationWindow form = new LocationWindow();

            // ShowDialog trả về true/false/null. 
            // Nếu bên form con gọi this.DialogResult = true thì biến result sẽ là true.
            bool? result = form.ShowDialog();

            // BƯỚC 3: GỠ BỎ HIỆU ỨNG
            this.Effect = null;

            // BƯỚC 4: LOAD LẠI DỮ LIỆU NẾU LƯU THÀNH CÔNG
            if (result == true)
            {
                LoadData();
                MessageBox.Show("Thêm mới vị trí thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        // 4. Sự kiện khi thay đổi nội dung tìm kiếm
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allLocations == null) return;

            string code = txtSearchCode.Text.Trim().ToLowerInvariant();
            string name = txtSearchName.Text.Trim().ToLowerInvariant();

            var filtered = _allLocations.Where(l =>
                (string.IsNullOrEmpty(code) ||
                 l.LocationID.ToString().ToLowerInvariant().Contains(code)) &&

                (string.IsNullOrEmpty(name) ||
                 l.LocationName.ToLowerInvariant().Contains(name))
            ).ToList();

            dgLocations.ItemsSource = filtered;
        }


        // ==========================================================
        // SỰ KIỆN CHUYỂN TAB TRONG QUẢN LÝ DANH MỤC
        // ==========================================================

        // Chuyển tới Tab: Quản lý loại thiết bị & Model
        private void Button_QLLTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLLTB_va_Model());
        }

        // Chuyển tới Tab: Nhà cung cấp & Báo giá linh kiện
        private void Button_NCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NCC_va_BGLK());
        }

        // Chuyển tới Tab: Từ điển dữ liệu chung
        private void Button_TDDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        // ==========================================================
        // CÁC SỰ KIỆN ĐIỀU HƯỚNG CHUNG (SIDEBAR/SYSTEM)
        // ==========================================================

        // Quay lại trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Trang_Chu());
        }

        // Đăng xuất khỏi hệ thống
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            // Có thể thêm xác nhận đăng xuất nếu muốn
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                NavigationHelper.Navigate(this, new Dang_Nhap());
            }
        }
    }
}