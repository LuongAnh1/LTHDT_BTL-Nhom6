using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class SupplierQuoteDTOService
    {
        #region LOAD COMBOBOX

        // Lấy danh sách Nhà cung cấp
        public List<Supplier> GetAllSuppliers()
        {
            List<Supplier> list = new List<Supplier>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT SupplierID, SupplierName FROM Suppliers";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Supplier
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = reader["SupplierName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // Lấy danh sách Model / Linh kiện
        public List<DeviceModel> GetAllModels()
        {
            List<DeviceModel> list = new List<DeviceModel>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT ModelID, ModelName FROM DeviceModels";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceModel
                        {
                            ModelID = Convert.ToInt32(reader["ModelID"]),
                            ModelName = reader["ModelName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        #endregion

        #region QUOTE CRUD (KHÓA GHÉP)

        // Lấy 1 báo giá theo (ModelID, SupplierID)
        public SupplierQuoteDTO GetQuote(int modelId, int supplierId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT * FROM ModelSuppliers
                               WHERE ModelID = @ModelID
                               AND SupplierID = @SupplierID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ModelID", modelId);
                cmd.Parameters.AddWithValue("@SupplierID", supplierId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new SupplierQuoteDTO
                        {
                            ModelID = modelId,
                            SupplierID = supplierId,
                            Price = Convert.ToDecimal(reader["Price"]),
                            LastSupplyDate = Convert.ToDateTime(reader["LastSupplyDate"])
                        };
                    }
                }
            }
            return null;
        }

        // Thêm báo giá mới
        public void AddQuote(SupplierQuoteDTO quote)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO ModelSuppliers
                               (ModelID, SupplierID, Price, LastSupplyDate)
                               VALUES (@ModelID, @SupplierID, @Price, @Date)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ModelID", quote.ModelID);
                cmd.Parameters.AddWithValue("@SupplierID", quote.SupplierID);
                cmd.Parameters.AddWithValue("@Price", quote.Price);
                cmd.Parameters.AddWithValue("@Date", quote.LastSupplyDate);

                cmd.ExecuteNonQuery();
            }
        }

        // Cập nhật báo giá (CHỈ sửa Price, Date)
        public void UpdateQuote(SupplierQuoteDTO quote)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE ModelSuppliers
                               SET Price = @Price,
                                   LastSupplyDate = @Date
                               WHERE ModelID = @ModelID
                               AND SupplierID = @SupplierID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ModelID", quote.ModelID);
                cmd.Parameters.AddWithValue("@SupplierID", quote.SupplierID);
                cmd.Parameters.AddWithValue("@Price", quote.Price);
                cmd.Parameters.AddWithValue("@Date", quote.LastSupplyDate);

                cmd.ExecuteNonQuery();
            }
        }

        // Xóa báo giá
        public void DeleteQuote(int modelId, int supplierId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"DELETE FROM ModelSuppliers
                               WHERE ModelID = @ModelID
                               AND SupplierID = @SupplierID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ModelID", modelId);
                cmd.Parameters.AddWithValue("@SupplierID", supplierId);

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region CHECK NGHIỆP VỤ

        // Kiểm tra trùng báo giá (khi thêm)
        public bool ExistsQuote(int modelId, int supplierId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT COUNT(*) FROM ModelSuppliers
                               WHERE ModelID=@ModelID AND SupplierID=@SupplierID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ModelID", modelId);
                cmd.Parameters.AddWithValue("@SupplierID", supplierId);

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        #endregion
    }
}
