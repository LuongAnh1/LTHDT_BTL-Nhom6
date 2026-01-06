// Import namespace của Helper
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    public partial class TDMK_va_TTCN : Window
    {
        private UserService _userService = new UserService();

        public TDMK_va_TTCN()
        {
            InitializeComponent();
            ApplyPermissions(); // Hàm phân quyền
            // Đăng ký sự kiện Loaded để tải dữ liệu khi form mở lên
            this.Loaded += TDMK_va_TTCN_Loaded;
        }

        private void ApplyPermissions()
        {
            int roleId = UserSession.CurrentRoleID;

            // --- TRƯỜNG HỢP: KHÁCH HÀNG (ID = 11) ---
            if (roleId == 11)
            {
                // 1. Ẩn các tab quản trị
                if (btnTabQLND != null) btnTabQLND.Visibility = Visibility.Collapsed;
                if (btnTabSkill != null) btnTabSkill.Visibility = Visibility.Collapsed;
                if (btnTabLog != null) btnTabLog.Visibility = Visibility.Collapsed;

                // 2. Tab "Thay đổi mật khẩu" luôn hiện (Mặc định nó đã Visible rồi)
            }

            // --- TRƯỜNG HỢP: NHÂN VIÊN (ID = 3,4,5...) ---
            else if (roleId != 1) // Không phải Admin
            {
                // Ví dụ: Nhân viên không được xem Nhật ký hệ thống
                if (btnTabLog != null) btnTabLog.Visibility = Visibility.Collapsed;

                // Nhân viên chỉ được xem thông tin cá nhân, không được quản lý người khác
                if (btnTabQLND != null) btnTabQLND.Visibility = Visibility.Collapsed;
            }
        }
        private void TDMK_va_TTCN_Loaded(object sender, RoutedEventArgs e)
        {
            // Kiểm tra xem đã đăng nhập chưa (đề phòng)
            if (UserSession.CurrentUserID == 0)
            {
                MessageBox.Show("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

            LoadUserData();
        }

        // --- 1. LOAD DỮ LIỆU TỪ DATABASE ---
        private void LoadUserData()
        {
            try
            {
                // Lấy thông tin mới nhất từ DB dựa trên ID đang lưu trong Session
                int currentUserId = UserSession.CurrentUserID;
                User user = _userService.GetUserById(currentUserId);

                if (user != null)
                {
                    // --- MAP DỮ LIỆU VÀO CÁC Ô READONLY ---
                    // Đảm bảo bạn đã đặt x:Name trong XAML (xem phần lưu ý bên dưới)
                    if (txtUsername != null) txtUsername.Text = user.Username;
                    if (txtDisplayFullName != null) txtDisplayFullName.Text = user.FullName;

                    // Xử lý hiển thị Tên Role
                    if (txtRoleName != null)
                    {
                        // Cách 1: Nếu User có thuộc tính RoleName (do query JOIN trả về)
                        // txtRoleName.Text = user.RoleName; 

                        // Cách 2: Tạm thời check thủ công nếu Model chưa có RoleName
                        txtRoleName.Text = (user.RoleID == 1) ? "Quản trị viên" : 
                            (user.RoleID == 11) ? "Khách Hàng":
                            "Nhân viên";
                    }

                    // --- MAP DỮ LIỆU VÀO CÁC Ô CHO PHÉP SỬA ---
                    txtPhone.Text = user.Phone;
                    txtEmail.Text = user.Email;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin cá nhân: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 2. XỬ LÝ NÚT LƯU THÔNG TIN ---
        private void BtnSaveInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string newPhone = txtPhone.Text.Trim();
                string newEmail = txtEmail.Text.Trim();

                // 1. Validate dữ liệu đầu vào
                if (string.IsNullOrEmpty(newPhone))
                {
                    MessageBox.Show("Số điện thoại không được để trống.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPhone.Focus();
                    return;
                }

                // 2. Gọi Service cập nhật
                bool result = _userService.UpdatePersonalInfo(UserSession.CurrentUserID, newPhone, newEmail);

                if (result)
                {
                    MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cập nhật lại Session nếu cần (ví dụ nếu Session có lưu Email/Phone)
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại.\nCó thể Số điện thoại hoặc Email này đã được sử dụng bởi người khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 3. XỬ LÝ NÚT ĐỔI MẬT KHẨU ---
        private void BtnChangePass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentPass = pbCurrentPass.Password;
                string newPass = pbNewPass.Password;
                string confirmPass = pbConfirmPass.Password;

                // 1. Validate Input
                if (string.IsNullOrEmpty(currentPass) || string.IsNullOrEmpty(newPass))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu hiện tại và mật khẩu mới.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPass != confirmPass)
                {
                    MessageBox.Show("Mật khẩu xác nhận không khớp!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPass.Length < 6)
                {
                    MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // (Tùy chọn) Kiểm tra độ mạnh mật khẩu bằng Regex nếu cần

                // 2. Gọi Service đổi mật khẩu
                // Hàm này sẽ tự mã hóa currentPass để so sánh với DB, và mã hóa newPass để lưu
                bool result = _userService.ChangePassword(UserSession.CurrentUserID, currentPass, newPass);

                if (result)
                {
                    MessageBox.Show("Đổi mật khẩu thành công!\nVui lòng ghi nhớ mật khẩu mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Xóa trắng các ô mật khẩu
                    pbCurrentPass.Clear();
                    pbNewPass.Clear();
                    pbConfirmPass.Clear();
                }
                else
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    pbCurrentPass.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đổi mật khẩu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Chuyển tới trang Quản Lý Người Dùng và Phân Quyền
        private void Button_QLND_va_PQ_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLND_va_PQ());
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