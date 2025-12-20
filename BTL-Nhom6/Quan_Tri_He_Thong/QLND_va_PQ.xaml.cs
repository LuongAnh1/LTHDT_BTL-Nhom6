using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Collections.Generic; // Thêm thư viện này để dùng List

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    /// <summary>
    /// Interaction logic for QLND_va_PQ.xaml
    /// </summary>
    public partial class QLND_va_PQ : Window
    {
        
        private UserService _userService = new UserService();
        private RoleService _roleService = new RoleService();

        public QLND_va_PQ()
        {
            InitializeComponent();

            // Đăng ký sự kiện Loaded để khi mở form lên là load dữ liệu ngay
            this.Loaded += QLND_va_PQ_Loaded;
        }

        private void QLND_va_PQ_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRolesComboBox(); // Gọi hàm load combobox
            LoadData();          // Gọi hàm load lưới
        }

        // --- Xử lý dữ liệu ---
        //// 1. Hàm chạy khi Form vừa mở
        //private void QLND_va_PQ_Loaded(object sender, RoutedEventArgs e)
        //{
        //    LoadData();
        //    // Gán sự kiện cho nút tìm kiếm và ô text tìm kiếm
        //    txtSearch.TextChanged += TxtSearch_TextChanged;
        //}
        // --- Hàm Load ComboBox từ RoleService ---
        private void LoadRolesComboBox()
        {
            try
            {
                // 1. Gọi Service lấy danh sách Role
                var roles = _roleService.GetAllRoles();

                // 2. Tạo một Role "giả" để làm mục "Tất cả"
                // Lưu ý: RoleID = 0 để sau này logic lọc hiểu là lấy tất cả
                var allRole = new Role { RoleID = 0, RoleName = "Tất cả chức vụ" };

                // 3. Chèn vào đầu danh sách
                roles.Insert(0, allRole);

                // 4. Gán vào ComboBox (đã đặt tên x:Name="cboRoleName" ở bước XAML trước)
                cboRoleName.ItemsSource = roles;

                // 5. Cấu hình hiển thị (nếu chưa set trong XAML)
                cboRoleName.DisplayMemberPath = "RoleName";
                cboRoleName.SelectedValuePath = "RoleID";

                // 6. Chọn mặc định mục đầu tiên (Tất cả)
                cboRoleName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load roles: " + ex.Message);
            }
        }
        // 2. Hàm tải dữ liệu lên Grid
        private void LoadData()
        {
            try
            {
                // Lấy từ khóa tìm kiếm
                string keyword = txtSearch.Text;

                // Lấy RoleID từ ComboBox (xử lý an toàn nếu null)
                int roleID = 0;
                if (cboRoleName.SelectedValue != null && int.TryParse(cboRoleName.SelectedValue.ToString(), out int id))
                {
                    roleID = id;
                }

                // Gọi Service User (Lưu ý: Bạn phải cập nhật hàm GetAllUsers thêm tham số roleID như câu trả lời trước nhé)
                var listUsers = _userService.GetAllUsers(keyword, roleID);

                // Gán dữ liệu
                dgUsers.ItemsSource = listUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Sự kiện khi chọn thay đổi trong ComboBox
        private void cboRoleName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        // Sự kiện tìm kiếm
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        // Hàm này xử lý sự kiện khi chọn Combobox (khớp tên với XAML báo lỗi)
        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
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
