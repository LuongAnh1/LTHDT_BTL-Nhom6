using BTL_Nhom6.Helper;       // Gọi đến DatabaseHelper và UserSession
using MySql.Data.MySqlClient; // Cần thư viện MySql.Data từ NuGet
using System;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6
{
    public partial class Dang_Nhap : Window
    {
        public Dang_Nhap()
        {
            InitializeComponent();
        }

        // Xử lý sự kiện click nút Đóng
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        // Hàm mã hóa MD5 (Thêm vào class LoginWindow hoặc Helper)
        public static string CreateMD5(string input)
        {
            // Cần: using System.Security.Cryptography;
            // Cần: using System.Text;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        // Xử lý sự kiện click nút Đăng nhập
        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            // 1. Kiểm tra rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Kết nối Database
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // Câu lệnh SQL kiểm tra đăng nhập
                    // Lưu ý: PasswordHash trong DB của bạn nếu đang lưu pass thường (ví dụ "123") thì so sánh trực tiếp.
                    // Nếu đã mã hóa (MD5/SHA) thì bạn cần mã hóa biến 'password' trước khi so sánh.
                    string sql = "SELECT UserID, Username, FullName, RoleID FROM Users WHERE Username = @user AND PasswordHash = @pass";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        // Thêm tham số để tránh lỗi SQL Injection
                        cmd.Parameters.AddWithValue("@user", username);
                        // Nếu muốn mã hóa
                        // cmd.Parameters.AddWithValue("@pass", CreateMD5(password));
                        // Nếu không mã hóa
                        cmd.Parameters.AddWithValue("@pass", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // --- ĐĂNG NHẬP THÀNH CÔNG ---

                                // 3. Lưu thông tin vào Session
                                UserSession.CurrentUserID = reader.GetInt32("UserID");
                                UserSession.CurrentUserName = reader.GetString("Username");
                                UserSession.CurrentRoleID = reader.GetInt32("RoleID");
                                // Kiểm tra null cho FullName vì trong DB cột này không NOT NULL
                                if (!reader.IsDBNull(reader.GetOrdinal("FullName")))
                                {
                                    UserSession.CurrentFullName = reader.GetString("FullName");
                                }

                                // MessageBox.Show($"Đăng nhập thành công!\nXin chào: {UserSession.CurrentFullName ?? username}", "Thông báo");

                                // 4. Mở màn hình chính (MainWindow)
                                Trang_Chu main = new Trang_Chu();
                                main.Show();

                                // Đóng cửa sổ đăng nhập hiện tại
                                this.Close();
                            }
                            else
                            {
                                // --- ĐĂNG NHẬP THẤT BẠI ---
                                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Cho phép kéo thả cửa sổ khi click chuột trái vào vùng trống
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        // Hàm xử lý khi bấm Quên mật khẩu
        private void BtnQuenMatKhau_Click(object sender, RoutedEventArgs e)
        {
            // 1. Khởi tạo màn hình Quên Mật Khẩu
            Quen_Mat_Khau formQuenMK = new Quen_Mat_Khau();

            // 2. Hiển thị màn hình Quên Mật Khẩu
            formQuenMK.Show();

            // 3. Đóng màn hình Đăng Nhập hiện tại để chuyển hoàn toàn sang trang kia
            this.Close();
        }
    }
}