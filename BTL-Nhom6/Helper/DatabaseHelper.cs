using System;
using MySql.Data.MySqlClient;
using System.Data;

namespace BTL_Nhom6.Helper
{
    public class DatabaseHelper
    {
        // CẤU HÌNH KẾT NỐI TIDB CLOUD
        // Bạn hãy copy chính xác thông tin từ nút "Connect" trên trang TiDB
        private static string _server = "gateway01.ap-southeast-1.prod.aws.tidbcloud.com"; // Thay bằng Host của bạn
        private static string _port = "4000"; // TiDB mặc định là 4000
        private static string _database = "csdl"; // Tên database bạn đặt
        private static string _uid = "m6w4RepQ5YviZkQ.root"; // Username kiểu lạ của TiDB
        private static string _password = "yY7BSsyGZEEd581I";

        // Tạo chuỗi kết nối đầy đủ
        // Thêm SslMode=Required vì TiDB bắt buộc bảo mật
        private static string _connectionString = $"Server={_server};Port={_port};Database={_database};User Id={_uid};Password={_password};SslMode=Required;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public static bool TestConnection(out string message)
        {
            // Dùng using để đảm bảo connection luôn được đóng sau khi kiểm tra xong
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    message = "Kết nối thành công tới TiDB Cloud!";
                    return true;
                }
                catch (MySqlException sqlEx)
                {
                    // Bắt lỗi riêng của MySQL để dễ debug
                    message = $"Lỗi MySQL (Code: {sqlEx.Number}): {sqlEx.Message}";
                    return false;
                }
                catch (Exception ex)
                {
                    message = $"Lỗi hệ thống: {ex.Message}";
                    return false;
                }
            }
        }
    }
}