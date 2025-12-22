using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class CommonErrorService
    {
        // 1. Lấy tất cả lỗi
        public List<CommonError> GetAllErrors()
        {
            List<CommonError> list = new List<CommonError>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM CommonErrors ORDER BY ErrorID DESC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new CommonError
                        {
                            ErrorID = Convert.ToInt32(reader["ErrorID"]),
                            ErrorName = reader["ErrorName"].ToString(),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : "",
                            Solution = reader["Solution"] != DBNull.Value ? reader["Solution"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm lỗi mới
        public void AddError(CommonError error)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO CommonErrors (ErrorName, Description, Solution) VALUES (@Name, @Desc, @Sol)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", error.ErrorName);
                cmd.Parameters.AddWithValue("@Desc", error.Description ?? "");
                cmd.Parameters.AddWithValue("@Sol", error.Solution ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // 3. Sửa lỗi
        public void UpdateError(CommonError error)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE CommonErrors SET ErrorName = @Name, Description = @Desc, Solution = @Sol WHERE ErrorID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", error.ErrorID);
                cmd.Parameters.AddWithValue("@Name", error.ErrorName);
                cmd.Parameters.AddWithValue("@Desc", error.Description ?? "");
                cmd.Parameters.AddWithValue("@Sol", error.Solution ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // 4. Xóa lỗi
        public void DeleteError(int id)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM CommonErrors WHERE ErrorID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}