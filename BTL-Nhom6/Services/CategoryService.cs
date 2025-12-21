using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using System;

namespace BTL_Nhom6.Services
{
    public class CategoryService
    {
        public List<Category> GetAllCategories()
        {
            List<Category> list = new List<Category>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Categories ORDER BY CategoryName ASC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Category
                        {
                            CategoryID = Convert.ToInt32(reader["CategoryID"]),
                            CategoryName = reader["CategoryName"].ToString(),
                            Description = reader["Description"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // Thêm Loại
        public void AddCategory(Category cat)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Categories (CategoryName, Description) VALUES (@Name, @Desc)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", cat.CategoryName);
                cmd.Parameters.AddWithValue("@Desc", cat.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // Sửa Loại
        public void UpdateCategory(Category cat)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Categories SET CategoryName=@Name, Description=@Desc WHERE CategoryID=@ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", cat.CategoryID);
                cmd.Parameters.AddWithValue("@Name", cat.CategoryName);
                cmd.Parameters.AddWithValue("@Desc", cat.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // Xóa
        public void DeleteCategory(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Câu lệnh DELETE
                string sql = "DELETE FROM Categories WHERE CategoryID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}