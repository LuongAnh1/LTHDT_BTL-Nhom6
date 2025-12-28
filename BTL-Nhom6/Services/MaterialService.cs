using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class MaterialService
    {
        // 1. Lấy tất cả vật tư (Dùng cho form chọn vật tư)
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
                            SoLuong = 1
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm vật tư mới (Đã bổ sung MinStock và Description)
        public void AddMaterial(Material mat)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Sửa câu lệnh SQL thêm cột Description và MinStock
                string sql = @"INSERT INTO Materials 
                               (MaterialName, UnitID, CurrentStock, UnitPrice, Description, MinStock) 
                               VALUES 
                               (@Name, @UnitID, @Stock, @Price, @Desc, @MinStock)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Name", mat.MaterialName);
                cmd.Parameters.AddWithValue("@UnitID", mat.UnitID);
                cmd.Parameters.AddWithValue("@Stock", mat.CurrentStock);
                cmd.Parameters.AddWithValue("@Price", mat.UnitPrice);

                // Thêm tham số mới
                cmd.Parameters.AddWithValue("@Desc", mat.Description ?? ""); // Tránh lỗi null
                cmd.Parameters.AddWithValue("@MinStock", mat.MinStock);

                cmd.ExecuteNonQuery();
            }
        }

        // 3. Cập nhật vật tư (Đã bổ sung MinStock và Description)
        public void UpdateMaterial(Material mat)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Sửa câu lệnh SQL cập nhật cột Description và MinStock
                string sql = @"UPDATE Materials 
                               SET MaterialName = @Name, 
                                   UnitID = @UnitID, 
                                   CurrentStock = @Stock, 
                                   UnitPrice = @Price,
                                   Description = @Desc,
                                   MinStock = @MinStock
                               WHERE MaterialID = @ID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@ID", mat.MaterialID);
                cmd.Parameters.AddWithValue("@Name", mat.MaterialName);
                cmd.Parameters.AddWithValue("@UnitID", mat.UnitID);
                cmd.Parameters.AddWithValue("@Stock", mat.CurrentStock);
                cmd.Parameters.AddWithValue("@Price", mat.UnitPrice);

                // Thêm tham số mới
                cmd.Parameters.AddWithValue("@Desc", mat.Description ?? "");
                cmd.Parameters.AddWithValue("@MinStock", mat.MinStock);

                cmd.ExecuteNonQuery();
            }
        }

        // 4. Lấy danh sách cho Form Danh mục (Giữ nguyên vì đã đúng)
        public List<MaterialCatalogViewModel> GetCatalog(string searchCode = "", string searchName = "")
        {
            List<MaterialCatalogViewModel> list = new List<MaterialCatalogViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT m.MaterialID, m.MaterialName, m.Description, m.MinStock, 
                           u.UnitName, m.UnitID, m.CurrentStock, m.UnitPrice
                    FROM Materials m
                    JOIN Units u ON m.UnitID = u.UnitID
                    WHERE (@Code = '' OR m.MaterialID = @CodeInt)
                      AND (@Name = '' OR m.MaterialName LIKE @NameSearch)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                int codeInt = 0;
                string cleanCode = searchCode.ToLower().Replace("vt-", "").Replace("vt", "");
                int.TryParse(cleanCode, out codeInt);

                cmd.Parameters.AddWithValue("@Code", searchCode);
                cmd.Parameters.AddWithValue("@CodeInt", codeInt);
                cmd.Parameters.AddWithValue("@Name", searchName);
                cmd.Parameters.AddWithValue("@NameSearch", "%" + searchName + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaterialCatalogViewModel
                        {
                            MaterialID = Convert.ToInt32(reader["MaterialID"]),
                            TenDM = reader["MaterialName"].ToString(),
                            MoTa = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : "",
                            DinhMuc = reader["MinStock"] != DBNull.Value ? Convert.ToInt32(reader["MinStock"]) : 0,
                            DonVi = reader["UnitName"].ToString(),
                            UnitID = Convert.ToInt32(reader["UnitID"]),
                            CurrentStock = Convert.ToInt32(reader["CurrentStock"]),
                            DonGia = Convert.ToDecimal(reader["UnitPrice"])
                        });
                    }
                }
            }
            return list;
        }

        // 5. Xóa vật tư (Giữ nguyên)
        public bool DeleteMaterial(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string sql = "DELETE FROM Materials WHERE MaterialID = @ID";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@ID", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}