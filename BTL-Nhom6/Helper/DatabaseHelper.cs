using System;
using MySql.Data.MySqlClient; // Đảm bảo đã cài gói MySQL.Data
using System.Data;

namespace BTL_Nhom6.Helper
{
    public class DatabaseHelper
    {
        // Thay đổi thông tin này khớp với MySQL của bạn
        private static string _connectionString = "Server=localhost;Database=csdl;User Id=root;Password=A10king2005;Port=3306;";

        // Hàm lấy Connection (để dùng cho các lệnh truy vấn sau này)
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        // Hàm kiểm tra kết nối (chỉ dùng để test lúc mở app)
        public static bool TestConnection(out string message)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open(); // Thử mở kết nối
                    message = "Kết nối thành công!";
                    return true;
                }
                catch (Exception ex)
                {
                    message = ex.Message; // Lấy nội dung lỗi
                    return false;
                }
            }
        }
    }
}