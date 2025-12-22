using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient; // Thư viện MySQL
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;       // Helper kết nối DB của bạn

namespace BTL_Nhom6.Services
{
    public class DeviceStatusService
    {
        // 1. Lấy danh sách tất cả trạng thái (Hiển thị lên bảng)
        public List<DeviceStatus> GetAllDeviceStatus()
        {
            List<DeviceStatus> list = new List<DeviceStatus>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT * FROM DeviceStatus ORDER BY StatusID DESC"; // Lấy mới nhất lên đầu
                    MySqlCommand cmd = new MySqlCommand(sql, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new DeviceStatus
                            {
                                StatusID = Convert.ToInt32(reader["StatusID"]),
                                StatusName = reader["StatusName"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : ""
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Bạn có thể log lỗi hoặc throw ra để Form bắt
                    throw new Exception("Lỗi khi lấy dữ liệu trạng thái: " + ex.Message);
                }
            }
            return list;
        }

        // 2. Thêm trạng thái mới
        public void AddDeviceStatus(DeviceStatus status)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // StatusID tự tăng nên không cần insert
                string sql = "INSERT INTO DeviceStatus (StatusName, Description) VALUES (@Name, @Desc)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Name", status.StatusName);
                cmd.Parameters.AddWithValue("@Desc", status.Description ?? "");

                cmd.ExecuteNonQuery();
            }
        }

        // 3. Cập nhật trạng thái (Dùng cho nút Sửa/Pencil)
        public void UpdateDeviceStatus(DeviceStatus status)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE DeviceStatus SET StatusName = @Name, Description = @Desc WHERE StatusID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@ID", status.StatusID);
                cmd.Parameters.AddWithValue("@Name", status.StatusName);
                cmd.Parameters.AddWithValue("@Desc", status.Description ?? "");

                cmd.ExecuteNonQuery();
            }
        }

        // 4. Xóa trạng thái (Dùng cho nút Xóa/Delete)
        public void DeleteDeviceStatus(int statusId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM DeviceStatus WHERE StatusID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@ID", statusId);

                cmd.ExecuteNonQuery();
            }
        }

        // 5. Tìm kiếm (Theo Tên hoặc Mã/ID) - Phục vụ ô tìm kiếm
        public List<DeviceStatus> SearchDeviceStatus(string keyword)
        {
            List<DeviceStatus> list = new List<DeviceStatus>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Tìm kiếm gần đúng theo tên hoặc đúng theo ID
                string sql = "SELECT * FROM DeviceStatus WHERE StatusName LIKE @Keyword OR StatusID LIKE @Keyword";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceStatus
                        {
                            StatusID = Convert.ToInt32(reader["StatusID"]),
                            StatusName = reader["StatusName"].ToString(),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }
    }
}