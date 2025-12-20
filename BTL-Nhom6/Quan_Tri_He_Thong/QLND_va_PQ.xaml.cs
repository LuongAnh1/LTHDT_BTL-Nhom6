using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    /// <summary>
    /// Interaction logic for QLND_va_PQ.xaml
    /// </summary>
    public partial class QLND_va_PQ : Window
    {
        
        private UserService _userService;

        public QLND_va_PQ()
        {
            InitializeComponent();
            _userService = new UserService();

            // Đăng ký sự kiện Loaded để khi mở form lên là load dữ liệu ngay
            this.Loaded += QLND_va_PQ_Loaded;
        }
        // --- Xử lý dữ liệu ---
        // 1. Hàm chạy khi Form vừa mở
        private void QLND_va_PQ_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            // Gán sự kiện cho nút tìm kiếm và ô text tìm kiếm
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        // 2. Hàm tải dữ liệu lên Grid
        private void LoadData(string keyword = "")
        {
            try
            {
                // Gọi Service lấy List<User>
                var listUsers = _userService.GetAllUsers(keyword);
                dgUsers.ItemsSource = listUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // 3. Xử lý tìm kiếm khi gõ phím
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData(txtSearch.Text);
        }

        // 4. Xử lý nút "Thêm người dùng mới"
        private void btnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // --- BƯỚC 1: TẠO HIỆU ỨNG MỜ ---
            System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect();
            blurObj.Radius = 15; // Độ nhòe (càng cao càng mờ, tầm 10-20 là đẹp)

            // Áp dụng hiệu ứng lên cửa sổ hiện tại (Form cha)
            this.Effect = blurObj;

            // --- BƯỚC 2: MỞ FORM CON ---
            UserWindow form = new UserWindow(null);

            // Dùng ShowDialog() để code dừng lại tại đây cho đến khi form con đóng
            form.ShowDialog();

            // --- BƯỚC 3: GỠ BỎ HIỆU ỨNG ---
            this.Effect = null; // Trả lại trạng thái bình thường

            // --- BƯỚC 4: LOAD LẠI DỮ LIỆU ---
            if (form.IsSuccess)
            {
                LoadData();
            }
        }

        // 5. Xử lý nút Xóa (Nút thùng rác trong Grid)
        // Lưu ý: Bạn cần đặt tên sự kiện Click cho nút trong XAML (xem phần dưới)
        private void btnDeleteRow_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dòng dữ liệu hiện tại
            Button btn = sender as Button;
            if (btn != null && btn.DataContext is User userToDelete)
            {
                if (MessageBox.Show($"Bạn có chắc muốn xóa nhân viên {userToDelete.FullName}?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (_userService.DeleteUser(userToDelete.UserID))
                    {
                        MessageBox.Show("Xóa thành công!");
                        LoadData(); // Load lại bảng
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!");
                    }
                }
            }
        }

        // 6. Xử lý nút Sửa (Nút bút chì)
        private void btnEditRow_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.DataContext is User userToEdit)
            {
                // --- BƯỚC 1: TẠO HIỆU ỨNG MỜ ---
                System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect();
                blurObj.Radius = 15;
                this.Effect = blurObj;

                // --- BƯỚC 2: MỞ FORM CON ---
                UserWindow form = new UserWindow(userToEdit);
                form.ShowDialog();

                // --- BƯỚC 3: GỠ BỎ HIỆU ỨNG ---
                this.Effect = null;

                // --- BƯỚC 4: LOAD LẠI DỮ LIỆU ---
                if (form.IsSuccess)
                {
                    LoadData();
                }
            }
        }
        
        // 7. Xử lý Toggle Switch (Active/Inactive)
        private void chkStatus_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            if (chk != null && chk.DataContext is User user)
            {
                bool newStatus = chk.IsChecked ?? false;
                _userService.UpdateStatus(user.UserID, newStatus);
            }
        }

        // --- Xử lý sự kiện Button ---
        // Chuyển tới trang Thay đổi Mật Khẩu và Thông Tin Cá Nhân
        private void Button_TDMK_va_TTCN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDMK_va_TTCN());
        }
        // Chuyển tới trang Quản lý hồ sơ kỹ năng
        private void Button_QLHSKN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLHSKN());
        }
        // Chuyển tới trang Nhật kỹ và sao lưu dữ liệu 
        private void Button_NK_va_SLDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NK_va_SLDL());
        }
        // Quay lại trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Trang_Chu());
        }
        // Nút đăng xuất
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Dang_Nhap());
        }
    }
}
