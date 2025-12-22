using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class MaterialService
    {
        // 1. Lấy danh sách Vật tư (KÈM TÊN ĐƠN VỊ TÍNH)
        public List<Material> GetAllMaterials()
        {
            List<Material> list = new List<Material>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Sử dụng LEFT JOIN để lấy tên đơn vị từ bảng Units
                string sql = @"
                    SELECT m.*, u.UnitName 
                    FROM Materials m 
                    LEFT JOIN Units u ON m.UnitID = u.UnitID 
                    ORDER BY m.MaterialID DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Material
                        {
                            MaterialID = Convert.ToInt32(reader["MaterialID"]),
                            MaterialName = reader["MaterialName"].ToString(),

                            // Lấy ID từ bảng Materials
                            UnitID = reader["UnitID"] != DBNull.Value ? Convert.ToInt32(reader["UnitID"]) : 0,

                            // Lấy Tên từ bảng Units (để hiển thị)
                            UnitName = reader["UnitName"] != DBNull.Value ? reader["UnitName"].ToString() : "N/A",

                            CurrentStock = Convert.ToInt32(reader["CurrentStock"]),
                            UnitPrice = Convert.ToDecimal(reader["UnitPrice"])
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm vật tư mới (Lưu UnitID)
        public void AddMaterial(Material mat)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Materials (MaterialName, UnitID, CurrentStock, UnitPrice) VALUES (@Name, @UnitID, @Stock, @Price)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Name", mat.MaterialName);
                cmd.Parameters.AddWithValue("@UnitID", mat.UnitID); // Lưu ID
                cmd.Parameters.AddWithValue("@Stock", mat.CurrentStock);
                cmd.Parameters.AddWithValue("@Price", mat.UnitPrice);

                cmd.ExecuteNonQuery();
            }
        }

        // 3. Cập nhật vật tư
        public void UpdateMaterial(Material mat)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Materials SET MaterialName = @Name, UnitID = @UnitID, CurrentStock = @Stock, UnitPrice = @Price WHERE MaterialID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@ID", mat.MaterialID);
                cmd.Parameters.AddWithValue("@Name", mat.MaterialName);
                cmd.Parameters.AddWithValue("@UnitID", mat.UnitID);
                cmd.Parameters.AddWithValue("@Stock", mat.CurrentStock);
                cmd.Parameters.AddWithValue("@Price", mat.UnitPrice);

                cmd.ExecuteNonQuery();
            }
        }

        // Hàm Delete tương tự...
    }
}