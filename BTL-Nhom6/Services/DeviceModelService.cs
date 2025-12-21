using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using System;

namespace BTL_Nhom6.Services
{
    public class DeviceModelService
    {
        // Lấy Model theo ID Loại (Dùng cho Master-Detail)
        // Nếu categoryId = 0 hoặc -1 thì lấy tất cả
        public List<DeviceModel> GetModels(int categoryId = 0, string keyword = "")
        {
            List<DeviceModel> list = new List<DeviceModel>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Khởi tạo câu lệnh SQL cơ bản
                string sql = @"
            SELECT m.*, c.CategoryName 
            FROM DeviceModels m
            JOIN Categories c ON m.CategoryID = c.CategoryID
            WHERE 1=1"; // Kỹ thuật 1=1 giúp dễ dàng nối chuỗi AND phía sau

                // 1. Lọc theo Category (nếu có chọn)
                if (categoryId > 0)
                {
                    sql += " AND m.CategoryID = @CatID";
                }

                // 2. Lọc theo Từ khóa (nếu người dùng có nhập)
                if (!string.IsNullOrEmpty(keyword))
                {
                    // Tìm theo Tên Model HOẶC Hãng sản xuất
                    sql += " AND (m.ModelName LIKE @Keyword OR m.Manufacturer LIKE @Keyword)";
                }

                sql += " ORDER BY m.ModelName ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // Thêm tham số
                if (categoryId > 0)
                    cmd.Parameters.AddWithValue("@CatID", categoryId);

                if (!string.IsNullOrEmpty(keyword))
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%"); // Thêm % để tìm kiếm gần đúng

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceModel
                        {
                            ModelID = Convert.ToInt32(reader["ModelID"]),
                            ModelName = reader["ModelName"].ToString(),
                            Manufacturer = reader["Manufacturer"].ToString(),
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            Description = reader["Description"].ToString(),
                            CategoryName = reader["CategoryName"].ToString()
                        });
                    }
                }
            }
            return list;
        }
        
        // Thêm hàm DeleteModel, AddModel...
        public void DeleteModel(int id)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                new MySqlCommand($"DELETE FROM DeviceModels WHERE ModelID={id}", conn).ExecuteNonQuery();
            }
        }

        // Thêm Model
        public void AddModel(DeviceModel model)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO DeviceModels (ModelName, Manufacturer, CategoryID, Description) VALUES (@Name, @Man, @CatID, @Desc)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", model.ModelName);
                cmd.Parameters.AddWithValue("@Man", model.Manufacturer);
                cmd.Parameters.AddWithValue("@CatID", model.CategoryID);
                cmd.Parameters.AddWithValue("@Desc", model.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // Sửa Model
        public void UpdateModel(DeviceModel model)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE DeviceModels SET ModelName=@Name, Manufacturer=@Man, CategoryID=@CatID, Description=@Desc WHERE ModelID=@ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", model.ModelID);
                cmd.Parameters.AddWithValue("@Name", model.ModelName);
                cmd.Parameters.AddWithValue("@Man", model.Manufacturer);
                cmd.Parameters.AddWithValue("@CatID", model.CategoryID);
                cmd.Parameters.AddWithValue("@Desc", model.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // Kiểm tra xem Category có Model con không
        public bool CheckCategoryHasModels(int categoryId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Đếm số lượng Model thuộc CategoryID này
                string sql = "SELECT COUNT(*) FROM DeviceModels WHERE CategoryID = @ID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", categoryId);

                long count = Convert.ToInt64(cmd.ExecuteScalar());

                return count > 0; // Trả về True nếu đã có Model
            }
        }
    }
}