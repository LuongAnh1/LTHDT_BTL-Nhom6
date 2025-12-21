using BTL_Nhom6.Helper;       // Gọi đến DatabaseHelper và UserSession
using BTL_Nhom6.Services;     // Gọi đến LoggerService
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
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    // 2. Cập nhật SQL: Lấy thêm cột IsActive để kiểm tra xem có bị khóa không
                    string sql = "SELECT UserID, Username, FullName, RoleID, IsActive FROM Users WHERE Username = @user AND PasswordHash = @pass";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        // Lưu ý: Nếu database đã mã hóa pass thì nhớ mã hóa biến password ở đây trước khi truyền vào
                        cmd.Parameters.AddWithValue("@pass", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // --- KIỂM TRA TRẠNG THÁI HOẠT ĐỘNG ---
                                // (Dựa trên cột IsActive bạn đã thêm ở các bước trước)
                                bool isActive = true; // Mặc định là true nếu DB cũ chưa có cột này

                                // Kiểm tra xem cột IsActive có tồn tại trong kết quả trả về không để tránh lỗi
                                try { isActive = Convert.ToBoolean(reader["IsActive"]); } catch { }

                                if (!isActive)
                                {
                                    MessageBox.Show("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Quản trị viên!", "Truy cập bị từ chối", MessageBoxButton.OK, MessageBoxImage.Stop);
                                    return; // Dừng lại, không cho đăng nhập
                                }

                                // --- ĐĂNG NHẬP THÀNH CÔNG ---

                                // 3. Lưu thông tin vào Session
                                UserSession.CurrentUserID = reader.GetInt32("UserID");
                                UserSession.CurrentUserName = reader.GetString("Username");
                                UserSession.CurrentRoleID = reader.GetInt32("RoleID");

                                if (!reader.IsDBNull(reader.GetOrdinal("FullName")))
                                {
                                    UserSession.CurrentFullName = reader.GetString("FullName");
                                }

                                // ============================================================
                                // [QUAN TRỌNG] GHI LOG HỆ THỐNG
                                // Phải gọi sau khi đã gán UserSession.CurrentUserName để log biết ai đang đăng nhập
                                // ============================================================
                                LoggerService.WriteLog("Đăng nhập vào hệ thống");


                                // 4. Mở màn hình chính
                                Trang_Chu main = new Trang_Chu();
                                main.Show();

                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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