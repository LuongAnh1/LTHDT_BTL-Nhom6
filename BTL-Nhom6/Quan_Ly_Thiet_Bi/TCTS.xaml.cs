using System;
using System.Windows;
using BTL_Nhom6.Helper; // Đảm bảo namespace này đúng với file NavigationHelper của bạn
using System.Collections.Generic;
using System.Windows.Controls;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using MySql.Data.MySqlClient;

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    /// <summary>
    /// Interaction logic for TCTS.xaml
    /// </summary>
    public partial class TCTS : Window
    {
        private readonly DeviceService _deviceService;

        private bool _canEdit = false; // Biến kiểm tra quyền Thêm/Sửa/Xóa
        public TCTS()
        {
            InitializeComponent();
            ApplyPermissions(); // Áp dụng phân quyền khi khởi tạo
            _deviceService = new DeviceService();
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

                // 2. Ẩn cột "HÀNH ĐỘNG" (Sửa/Xóa) trong DataGrid
                // Giả sử cột Hành động là cột cuối cùng
                if (dgDevices.Columns.Count > 0)
                {
                    dgDevices.Columns[dgDevices.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
        }
        // Sự kiện khi Window load xong
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadComboBoxData(); // Load danh sách phòng ban, trạng thái
                LoadData();         // Load danh sách thiết bị ban đầu (Toàn bộ)
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm load dữ liệu vào ComboBox (Dùng SQL trực tiếp cho nhanh, hoặc tạo Service riêng)
        private void LoadComboBoxData()
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // 1. Load Locations
                string sqlLoc = "SELECT LocationID, LocationName FROM Locations";
                MySqlCommand cmdLoc = new MySqlCommand(sqlLoc, conn);
                List<Location> locations = new List<Location> { new Location { LocationID = -1, LocationName = "Tất cả phòng ban" } };

                using (var reader = cmdLoc.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        locations.Add(new Location
                        {
                            LocationID = Convert.ToInt32(reader["LocationID"]),
                            LocationName = reader["LocationName"].ToString()
                        });
                    }
                }
                cbLocation.ItemsSource = locations;
                cbLocation.SelectedIndex = 0; // Chọn mặc định "Tất cả"

                // 2. Load Statuses
                string sqlStat = "SELECT StatusID, StatusName FROM DeviceStatus";
                MySqlCommand cmdStat = new MySqlCommand(sqlStat, conn);
                List<DeviceStatus> statuses = new List<DeviceStatus> { new DeviceStatus { StatusID = -1, StatusName = "Tất cả trạng thái" } };

                using (var reader = cmdStat.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        statuses.Add(new DeviceStatus
                        {
                            StatusID = Convert.ToInt32(reader["StatusID"]),
                            StatusName = reader["StatusName"].ToString()
                        });
                    }
                }
                cbStatus.ItemsSource = statuses;
                cbStatus.SelectedIndex = 0;
            }
        }

        // Hàm tìm kiếm và hiển thị lên DataGrid
        private void LoadData()
        {
            // Lấy giá trị từ giao diện
            string keyword = txtKeyword.Text.Trim();
            string userKw = txtUser.Text.Trim();

            int? locId = null;
            if (cbLocation.SelectedValue != null && (int)cbLocation.SelectedValue > 0)
            {
                locId = (int)cbLocation.SelectedValue;
            }

            int? statId = null;
            if (cbStatus.SelectedValue != null && (int)cbStatus.SelectedValue > 0)
            {
                statId = (int)cbStatus.SelectedValue;
            }

            // Gọi Service tìm kiếm
            List<Device> devices = _deviceService.FindDevices(keyword, locId, statId, userKw);

            // Gán vào DataGrid
            dgDevices.ItemsSource = devices;
        }

        // Sự kiện click nút Tìm kiếm
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message);
            }
        }

        // Sự kiện nút Xem chi tiết trong bảng
        private void btnViewDetail_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dòng hiện tại
            Device selectedDevice = ((FrameworkElement)sender).DataContext as Device;
            if (selectedDevice != null)
            {
                MessageBox.Show($"Xem chi tiết thiết bị: {selectedDevice.DeviceName} đang giữ bởi {selectedDevice.CurrentUserFullName}");
                // Sau này bạn có thể mở Form Chi Tiết tại đây
            }
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