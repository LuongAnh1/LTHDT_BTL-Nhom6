using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class LocationService
    {
        // 1. Lấy danh sách vị trí (Kèm theo tên của vị trí cha)
        public List<Location> GetAllLocations()
        {
            List<Location> list = new List<Location>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Kỹ thuật SELF JOIN: Nối bảng Locations với chính nó để lấy tên cha
                string sql = @"
                    SELECT 
                        t1.LocationID, 
                        t1.LocationName, 
                        t1.Description, 
                        t1.ParentLocationID,
                        t2.LocationName AS ParentName 
                    FROM Locations t1
                    LEFT JOIN Locations t2 ON t1.ParentLocationID = t2.LocationID
                    ORDER BY t1.LocationID ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Location
                        {
                            LocationID = Convert.ToInt32(reader["LocationID"]),
                            LocationName = reader["LocationName"].ToString(),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : "",

                            // Xử lý Null cho ParentID
                            ParentLocationID = reader["ParentLocationID"] != DBNull.Value ? Convert.ToInt32(reader["ParentLocationID"]) : (int?)null,

                            // Xử lý hiển thị tên cha: Nếu null thì hiển thị dấu "-" hoặc "Gốc"
                            ParentLocationName = reader["ParentName"] != DBNull.Value ? reader["ParentName"].ToString() : "-"
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm vị trí mới
        public void AddLocation(Location loc)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Locations (LocationName, Description, ParentLocationID) VALUES (@Name, @Desc, @ParentID)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Name", loc.LocationName);
                cmd.Parameters.AddWithValue("@Desc", loc.Description ?? "");

                // Nếu ParentLocationID là null (hoặc 0/null tùy logic combobox của bạn), thì lưu DBNull
                if (loc.ParentLocationID == null || loc.ParentLocationID == 0)
                    cmd.Parameters.AddWithValue("@ParentID", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ParentID", loc.ParentLocationID);

                cmd.ExecuteNonQuery();
            }
        }

        // 3. Cập nhật vị trí (Sửa)
        public void UpdateLocation(Location loc)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Locations SET LocationName = @Name, Description = @Desc, ParentLocationID = @ParentID WHERE LocationID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@ID", loc.LocationID);
                cmd.Parameters.AddWithValue("@Name", loc.LocationName);
                cmd.Parameters.AddWithValue("@Desc", loc.Description ?? "");

                if (loc.ParentLocationID == null || loc.ParentLocationID == 0)
                    cmd.Parameters.AddWithValue("@ParentID", DBNull.Value);
                else
                    cmd.Parameters.AddWithValue("@ParentID", loc.ParentLocationID);

                cmd.ExecuteNonQuery();
            }
        }

        // 4. Xóa vị trí
        // Lưu ý: Nếu vị trí này đang là cha của vị trí khác, SQL có thể báo lỗi Foreign Key Constraint.
        // Bạn nên kiểm tra xem nó có con không trước khi xóa, hoặc dùng try-catch ở tầng UI.
        public void DeleteLocation(int locationId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Locations WHERE LocationID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", locationId);
                cmd.ExecuteNonQuery();
            }
        }

        // 5. Tìm kiếm (Theo tên hoặc mã - optional cho ô tìm kiếm trên giao diện)
        public List<Location> SearchLocations(string keyword)
        {
            List<Location> list = new List<Location>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT t1.*, t2.LocationName AS ParentName 
                    FROM Locations t1
                    LEFT JOIN Locations t2 ON t1.ParentLocationID = t2.LocationID
                    WHERE t1.LocationName LIKE @Keyword OR t1.LocationID LIKE @Keyword";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Location
                        {
                            LocationID = Convert.ToInt32(reader["LocationID"]),
                            LocationName = reader["LocationName"].ToString(),
                            Description = reader["Description"].ToString(),
                            ParentLocationID = reader["ParentLocationID"] != DBNull.Value ? Convert.ToInt32(reader["ParentLocationID"]) : (int?)null,
                            ParentLocationName = reader["ParentName"] != DBNull.Value ? reader["ParentName"].ToString() : "-"
                        });
                    }
                }
            }
            return list;
        }
    }
}