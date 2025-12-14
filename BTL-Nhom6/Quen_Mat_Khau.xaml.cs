using System.Windows;
using System.Windows.Input;
// using BTL_Nhom6.Helper; // Nhớ mở comment nếu dùng DatabaseHelper

namespace BTL_Nhom6
{
    public partial class Quen_Mat_Khau : Window
    {
        public Quen_Mat_Khau()
        {
            InitializeComponent();
        }

        // Kéo thả cửa sổ
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Đóng cửa sổ
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Quay lại đăng nhập
        private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            Dang_Nhap loginWindow = new Dang_Nhap();
            loginWindow.Show();
            this.Close();
        }

        // Xử lý đổi mật khẩu
        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string newPass = txtNewPass.Password;
            string confirmPass = txtConfirmPass.Password;

            // Validate cơ bản
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(newPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass != confirmPass)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // --- GỌI SQL ĐỂ CẬP NHẬT ---
            // DatabaseHelper.UpdatePassword(username, newPass);

            MessageBox.Show("Đổi mật khẩu thành công! Vui lòng đăng nhập lại.", "Thông báo");

            // Chuyển về trang đăng nhập sau khi đổi thành công
            BtnBackToLogin_Click(sender, e);
        }
    }
}