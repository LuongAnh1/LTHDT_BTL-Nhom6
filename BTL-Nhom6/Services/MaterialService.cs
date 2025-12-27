using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class MaterialService
    {
        // Lấy tất cả vật tư kèm Tên đơn vị tính
        public List<MaterialViewModel> GetAllMaterials()
        {
            List<MaterialViewModel> list = new List<MaterialViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT m.MaterialID, m.MaterialName, u.UnitName, m.UnitPrice 
                               FROM Materials m
                               JOIN Units u ON m.UnitID = u.UnitID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaterialViewModel
                        {
                            MaterialID = Convert.ToInt32(reader["MaterialID"]),
                            TenVatTu = reader["MaterialName"].ToString(),
                            DonVi = reader["UnitName"].ToString(),
                            DonGia = Convert.ToDecimal(reader["UnitPrice"]),
                            SoLuong = 1 // Mặc định chọn là 1
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