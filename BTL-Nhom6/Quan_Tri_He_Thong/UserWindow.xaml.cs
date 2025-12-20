using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System;
using System.Collections.Generic; // Để dùng List
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6
{
    public partial class UserWindow : Window
    {
        private UserService _userService;
        private RoleService _roleService; // 1. Khai báo thêm RoleService
        private User _currentUser;
        public bool IsSuccess { get; private set; } = false;

        // Constructor
        public UserWindow(User userToEdit = null)
        {
            InitializeComponent();
            _userService = new UserService();
            _roleService = new RoleService(); // 2. Khởi tạo RoleService
            _currentUser = userToEdit;

            LoadRoles(); // 3. GỌI HÀM LOAD ROLE TRƯỚC (QUAN TRỌNG)
            LoadFormState();

            // Cho phép kéo thả cửa sổ khi click chuột trái giữ
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }

        // --- HÀM MỚI THÊM: Tải danh sách vai trò ---
        private void LoadRoles()
        {
            try
            {
                // Lấy danh sách từ CSDL
                var roles = _roleService.GetAllRoles();

                // Gán vào ComboBox
                cboRoles.ItemsSource = roles;

                // (Không cần set DisplayMemberPath vì bạn đã set trong XAML rồi)
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách quyền: " + ex.Message);
            }
        }

        private void LoadFormState()
        {
            if (_currentUser == null) // CHẾ ĐỘ THÊM
            {
                lblTitle.Text = "THÊM NGƯỜI DÙNG";
                txtUsername.IsEnabled = true;
                lblPassHint.Visibility = Visibility.Collapsed;
                spacerPass.Visibility = Visibility.Visible;

                // Mặc định chọn vai trò đầu tiên nếu danh sách không rỗng (tùy chọn)
                if (cboRoles.Items.Count > 0) cboRoles.SelectedIndex = 0;
            }
            else // CHẾ ĐỘ SỬA
            {
                lblTitle.Text = "CẬP NHẬT THÔNG TIN";
                txtUsername.Text = _currentUser.Username;
                txtUsername.IsEnabled = false;

                txtFullName.Text = _currentUser.FullName;
                txtPhone.Text = _currentUser.Phone;
                txtEmail.Text = _currentUser.Email;

                // Việc gán SelectedValue này chỉ hoạt động NẾU ComboBox đã có ItemsSource (do hàm LoadRoles chạy trước)
                cboRoles.SelectedValue = _currentUser.RoleID;

                // chkActive.IsChecked = _currentUser.IsActive; // (Bỏ comment nếu dùng)

                lblPassHint.Visibility = Visibility.Visible;
                spacerPass.Visibility = Visibility.Collapsed;
            }
        }

        // ... (Giữ nguyên các hàm btnSave_Click và btnCancel_Click cũ của bạn) ...
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên đăng nhập!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }
            if (cboRoles.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Vai trò!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- PHẦN KIỂM TRA QUY TẮC ĐẶT TÊN ---

            var selectedRole = cboRoles.SelectedItem as Role;

            if (selectedRole != null)
            {
                // Chuyển hết về chữ thường để so sánh
                string roleNameLower = selectedRole.RoleName.Trim().ToLower();
                string usernameLower = txtUsername.Text.Trim().ToLower();

                // 1. KIỂM TRA QUẢN TRỊ VIÊN
                // Thêm điều kiện: chứa chữ "admin" HOẶC chứa chữ "quản trị"
                if (roleNameLower.Contains("admin") || roleNameLower.Contains("quản trị"))
                {
                    // Quy tắc: Phải là "admin" HOẶC bắt đầu bằng "admin_"
                    bool isValidAdmin = (usernameLower == "admin") || usernameLower.StartsWith("admin_");

                    if (!isValidAdmin)
                    {
                        MessageBox.Show("Với vai trò Quản trị viên, Tên đăng nhập phải là 'admin' hoặc bắt đầu bằng 'admin_'\n(Ví dụ: admin_kho)",
                                        "Sai quy tắc đặt tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtUsername.Focus();
                        return; // Chặn lại ngay
                    }
                }

                // 2. KIỂM TRA KHÁCH HÀNG
                // Thêm điều kiện: chứa chữ "customer" HOẶC chứa chữ "khách"
                else if (roleNameLower.Contains("customer") || roleNameLower.Contains("khách"))
                {
                    if (!usernameLower.StartsWith("customer_"))
                    {
                        MessageBox.Show("Với vai trò Khách hàng, Tên đăng nhập phải bắt đầu bằng 'customer_'\n(Ví dụ: customer_01)",
                                        "Sai quy tắc đặt tên", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtUsername.Focus();
                        return; // Chặn lại ngay
                    }
                }
            }

            // Map data
            User userModel = new User()
            {
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                RoleID = (int)cboRoles.SelectedValue,
                IsActive = chkActive.IsChecked ?? false,
                PasswordHash = txtPassword.Password
            };

            bool result = false;

            if (_currentUser == null)
            {
                // Validate Pass khi thêm mới
                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }
                result = _userService.AddUser(userModel);
            }
            else
            {
                userModel.UserID = _currentUser.UserID;
                result = _userService.UpdateUser(userModel);
            }

            if (result)
            {
                MessageBox.Show("Lưu dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                IsSuccess = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra (Trùng Username/Email hoặc mất kết nối)!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}