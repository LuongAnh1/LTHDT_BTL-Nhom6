using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class UnitService
    {
        public List<ProductUnit> GetAllUnits()
        {
            List<ProductUnit> list = new List<ProductUnit>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Units ORDER BY UnitID DESC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ProductUnit
                        {
                            UnitID = Convert.ToInt32(reader["UnitID"]),
                            UnitName = reader["UnitName"].ToString(),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }

        public void AddUnit(ProductUnit unit)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Units (UnitName, Description) VALUES (@Name, @Desc)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", unit.UnitName);
                cmd.Parameters.AddWithValue("@Desc", unit.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void UpdateUnit(ProductUnit unit)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Units SET UnitName = @Name, Description = @Desc WHERE UnitID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", unit.UnitID);
                cmd.Parameters.AddWithValue("@Name", unit.UnitName);
                cmd.Parameters.AddWithValue("@Desc", unit.Description ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteUnit(int id)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Units WHERE UnitID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        // Kiểm tra xem Đơn vị tính có đang được sử dụng trong bảng Materials không
        public bool IsUnitInUse(int unitId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Đếm số dòng trong bảng Materials có UnitID này
                string sql = "SELECT COUNT(*) FROM Materials WHERE UnitID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", unitId);

                // ExecuteScalar dùng để lấy 1 giá trị duy nhất (ở đây là số lượng đếm được)
                long count = (long)cmd.ExecuteScalar();

                // Nếu count > 0 nghĩa là đang có vật tư sử dụng đơn vị này -> Trả về True
                return count > 0;
            }
        }
    }
}