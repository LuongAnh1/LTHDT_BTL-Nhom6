using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6
{
    public partial class UserWindow : Window
    {
        private UserService _userService;
        private User _currentUser;
        public bool IsSuccess { get; private set; } = false;

        // Constructor
        public UserWindow(User userToEdit = null)
        {
            InitializeComponent();
            _userService = new UserService();
            _currentUser = userToEdit;

            LoadRoles();
            LoadFormState();

            // Cho phép kéo thả cửa sổ khi click chuột trái giữ
            this.MouseLeftButtonDown += (s, e) => this.DragMove();
        }

        private void LoadRoles()
        {
            // Nếu có lỗi kết nối thì catch để không crash app
            try
            {
                cboRoles.ItemsSource = _userService.GetAllRoles();
            }
            catch
            {
                MessageBox.Show("Không tải được danh sách Quyền hạn!");
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
                // chkActive.IsChecked = false;
            }
            else // CHẾ ĐỘ SỬA
            {
                lblTitle.Text = "CẬP NHẬT THÔNG TIN";
                txtUsername.Text = _currentUser.Username;
                txtUsername.IsEnabled = false; // Khóa Username

                txtFullName.Text = _currentUser.FullName;
                txtPhone.Text = _currentUser.Phone;
                txtEmail.Text = _currentUser.Email;
                cboRoles.SelectedValue = _currentUser.RoleID;
                // chkActive.IsChecked = _currentUser.IsActive;

                // Hiển thị dòng nhắc nhở về mật khẩu
                lblPassHint.Visibility = Visibility.Visible;
                spacerPass.Visibility = Visibility.Collapsed;
            }
        }

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