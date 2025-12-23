using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class DeviceAssignmentService
    {
        // 1. Lấy lịch sử cấp phát của một thiết bị cụ thể
        public List<DeviceAssignment> GetHistoryByDevice(string deviceCode)
        {
            List<DeviceAssignment> list = new List<DeviceAssignment>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT da.*, 
                           u.FullName, u.Username, 
                           d.DeviceName 
                    FROM DeviceAssignments da
                    JOIN Users u ON da.UserID = u.UserID
                    JOIN Devices d ON da.DeviceCode = d.DeviceCode
                    WHERE da.DeviceCode = @Code
                    ORDER BY da.AssignedDate DESC"; // Mới nhất lên đầu

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", deviceCode);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceAssignment
                        {
                            AssignmentID = Convert.ToInt32(reader["AssignmentID"]),
                            DeviceCode = reader["DeviceCode"].ToString(),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            AssignedDate = (DateTime)reader["AssignedDate"],
                            ReturnDate = reader["ReturnDate"] != DBNull.Value ? (DateTime?)reader["ReturnDate"] : null,
                            Note = reader["Note"].ToString(),

                            // Các trường hiển thị
                            DeviceName = reader["DeviceName"].ToString(),
                            UserFullName = reader["FullName"].ToString(),
                            Username = reader["Username"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 2. Kiểm tra thiết bị có đang Rảnh (Available) để cấp phát không?
        public bool IsDeviceAvailable(string deviceCode)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Nếu tìm thấy dòng nào có ReturnDate là NULL -> Nghĩa là đang có người giữ
                string sql = "SELECT COUNT(*) FROM DeviceAssignments WHERE DeviceCode = @Code AND ReturnDate IS NULL";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", deviceCode);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count == 0; // Nếu count = 0 nghĩa là không ai giữ -> Available
            }
        }

        // 3. Cấp phát thiết bị (Giao máy)
        public void AssignDevice(DeviceAssignment assignment)
        {
            // Kiểm tra logic trước khi lưu
            if (!IsDeviceAvailable(assignment.DeviceCode))
            {
                throw new Exception("Thiết bị này đang được người khác sử dụng. Vui lòng thu hồi trước khi cấp mới!");
            }

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO DeviceAssignments (DeviceCode, UserID, AssignedDate, ReturnDate, Note) 
                               VALUES (@DevCode, @User, @Date, NULL, @Note)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DevCode", assignment.DeviceCode);
                cmd.Parameters.AddWithValue("@User", assignment.UserID);
                cmd.Parameters.AddWithValue("@Date", assignment.AssignedDate);
                cmd.Parameters.AddWithValue("@Note", assignment.Note);

                cmd.ExecuteNonQuery();
            }
        }

        // 4. Thu hồi thiết bị (Trả máy)
        public void ReturnDevice(string deviceCode)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Cập nhật ReturnDate = NOW cho bản ghi đang mở (ReturnDate IS NULL)
                string sql = @"UPDATE DeviceAssignments 
                               SET ReturnDate = NOW() 
                               WHERE DeviceCode = @Code AND ReturnDate IS NULL";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", deviceCode);

                int rows = cmd.ExecuteNonQuery();
                if (rows == 0)
                {
                    throw new Exception("Thiết bị này hiện không do ai giữ, không thể thu hồi!");
                }
            }
        }

        // 5. Lấy thông tin người đang giữ hiện tại (nếu có)
        public DeviceAssignment GetCurrentAssignment(string deviceCode)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT da.*, u.FullName, u.Username, d.DeviceName 
                               FROM DeviceAssignments da
                               JOIN Users u ON da.UserID = u.UserID
                               JOIN Devices d ON da.DeviceCode = d.DeviceCode
                               WHERE da.DeviceCode = @Code AND da.ReturnDate IS NULL
                               LIMIT 1";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", deviceCode);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new DeviceAssignment
                        {
                            AssignmentID = Convert.ToInt32(reader["AssignmentID"]),
                            DeviceCode = reader["DeviceCode"].ToString(),
                            UserID = Convert.ToInt32(reader["UserID"]),
                            AssignedDate = (DateTime)reader["AssignedDate"],
                            ReturnDate = null,
                            Note = reader["Note"].ToString(),
                            UserFullName = reader["FullName"].ToString(),
                            Username = reader["Username"].ToString(),
                            DeviceName = reader["DeviceName"].ToString()
                        };
                    }
                }
            }
            return null; // Không ai giữ
        }
    }
}