using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using BTL_Nhom6.Helper;
using System.Linq; // Để dùng hàm Where tìm kiếm

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class ChiTietCongViecKTV : Window
    {
        // Khai báo Service
        private readonly UserService _userService = new UserService();
        private readonly WorkOrderService _woService = new WorkOrderService(); // Cần có hàm lấy việc theo ID User

        // Biến lưu trữ ID và danh sách gốc để tìm kiếm
        private int _userId;
        private List<WorkOrderViewModel> _allTasks; // Danh sách gốc để lọc

        public ChiTietCongViecKTV(int userId)
        {
            InitializeComponent();
            _userId = userId;

            // Load dữ liệu ngay khi khởi tạo
            LoadTechnicianInfo();
            LoadTasks();
        }

        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Xử lý sidebar nếu cần
        }

        // 1. Tải thông tin cá nhân KTV
        private void LoadTechnicianInfo()
        {
            var user = _userService.GetUserById(_userId);
            if (user != null)
            {
                txtTenKTV.Text = user.FullName;
                txtMaNV.Text = $"Mã NV: NV{user.UserID:D3}";

                // Lấy 2 chữ cái đầu
                string initials = "NV";
                if (!string.IsNullOrEmpty(user.FullName))
                {
                    var parts = user.FullName.Trim().Split(' ');
                    if (parts.Length == 1) initials = parts[0].Substring(0, 1).ToUpper();
                    else initials = (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
                }
                txtInitials.Text = initials;

                // Nếu bạn có lưu chuyên môn trong User hoặc cần lấy từ bảng khác
                // Ở đây mình ví dụ gán cứng hoặc lấy từ hàm GetUserSkills nếu có
                // txtChuyenMon.Text = "Chuyên môn: " + _userService.GetUserSkills(_userId);
            }
        }

        // 2. Tải danh sách công việc
        private void LoadTasks()
        {
            // Gọi Service lấy danh sách việc của User này
            // Bạn cần thêm hàm GetWorkOrdersByTechId vào WorkOrderService
            _allTasks = _woService.GetWorkOrdersByTechId(_userId);

            // Hiển thị lên lưới
            dgCongViec.ItemsSource = _allTasks;
        }

        // 3. Sự kiện tìm kiếm
        private void TxtSearchTask_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allTasks == null) return;

            string keyword = txtSearchTask.Text.ToLower().Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                dgCongViec.ItemsSource = _allTasks;
            }
            else
            {
                // Lọc theo Mã phiếu hoặc Tên thiết bị
                var filtered = _allTasks.Where(x =>
                    x.MaPhieu.ToLower().Contains(keyword) ||
                    x.TenThietBi.ToLower().Contains(keyword)
                ).ToList();

                dgCongViec.ItemsSource = filtered;
            }
        }

        // 4. Nút Quay lại
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Quay lại trang CNPCV
            NavigationHelper.Navigate(this, new CNPCV());
        }

        // 5. Nút Cập nhật trên lưới
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var task = btn.DataContext as WorkOrderViewModel;

            if (task != null)
            {
                // Mở form cập nhật trạng thái công việc (sẽ làm sau)
                MessageBox.Show($"Cập nhật phiếu: {task.MaPhieu}");
            }
        }

    }
}