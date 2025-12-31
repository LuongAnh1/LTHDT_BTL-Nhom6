using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class SupplierService
    {
        // 1. Lấy tất cả NCC
        public List<Supplier> GetAllSuppliers()
        {
            List<Supplier> list = new List<Supplier>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Suppliers ORDER BY SupplierID DESC";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Supplier
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = reader["SupplierName"].ToString(),
                            ContactPerson = reader["ContactPerson"] != DBNull.Value ? reader["ContactPerson"].ToString() : "",
                            Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "",
                            Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm NCC
        public void AddSupplier(Supplier sup)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Suppliers (SupplierName, ContactPerson, Phone, Address) VALUES (@Name, @Contact, @Phone, @Address)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", sup.SupplierName);
                cmd.Parameters.AddWithValue("@Contact", sup.ContactPerson ?? "");
                cmd.Parameters.AddWithValue("@Phone", sup.Phone ?? "");
                cmd.Parameters.AddWithValue("@Address", sup.Address ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // 3. Sửa NCC
        public void UpdateSupplier(Supplier sup)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Suppliers SET SupplierName = @Name, ContactPerson = @Contact, Phone = @Phone, Address = @Address WHERE SupplierID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", sup.SupplierID);
                cmd.Parameters.AddWithValue("@Name", sup.SupplierName);
                cmd.Parameters.AddWithValue("@Contact", sup.ContactPerson ?? "");
                cmd.Parameters.AddWithValue("@Phone", sup.Phone ?? "");
                cmd.Parameters.AddWithValue("@Address", sup.Address ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        // 4. Kiểm tra ràng buộc trước khi xóa
        public bool IsSupplierInUse(int supplierId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Kiểm tra ở 2 bảng: Devices (Đã từng mua máy) và ModelSuppliers (Có báo giá)
                string sql = @"SELECT 
                                (SELECT COUNT(*) FROM Devices WHERE SupplierID = @ID) + 
                                (SELECT COUNT(*) FROM ModelSuppliers WHERE SupplierID = @ID)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", supplierId);
                long count = (long)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // 5. Xóa NCC
        public void DeleteSupplier(int id)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Suppliers WHERE SupplierID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                cmd.ExecuteNonQuery();
            }
        }

        // lấy dữ liệu cho bảng thứ 2 trong form NCC_va_BGLK bằng cách JOIN bảng ModelSuppliers với DeviceModels và Suppliers
        public List<SupplierQuoteDTO> GetAllQuotes()
        {
            List<SupplierQuoteDTO> list = new List<SupplierQuoteDTO>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // JOIN 3 bảng: ModelSuppliers - DeviceModels - Suppliers
                string sql = @"
                    SELECT ms.ModelID, ms.SupplierID, ms.Price, ms.LastSupplyDate, 
                   m.ModelName, s.SupplierName
                    FROM ModelSuppliers ms
                    JOIN DeviceModels m ON ms.ModelID = m.ModelID
                    JOIN Suppliers s ON ms.SupplierID = s.SupplierID
                    ORDER BY ms.LastSupplyDate DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new SupplierQuoteDTO
                        {
                            ModelID = Convert.ToInt32(reader["ModelID"]),
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            ModelName = reader["ModelName"].ToString(),
                            SupplierName = reader["SupplierName"].ToString(),
                            Price = reader["Price"] != DBNull.Value ? Convert.ToDecimal(reader["Price"]) : 0,
                            LastSupplyDate = reader["LastSupplyDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastSupplyDate"]) : (DateTime?)null
                        });
                    }
                }
            }
            return list;
        }

        // Thêm vào class SupplierService
        public List<SupplierEvaluationDTO> GetSupplierEvaluations()
        {
            List<SupplierEvaluationDTO> list = new List<SupplierEvaluationDTO>();
            var allSuppliers = GetAllSuppliers(); // Tận dụng hàm cũ của bạn

            foreach (var sup in allSuppliers)
            {
                // LOGIC ĐÁNH GIÁ GIẢ LẬP (Bạn có thể thay bằng logic thật đếm số lần giao hàng trễ)
                // Ví dụ: ID chẵn là Tốt, ID lẻ là Trung bình
                string rating = (sup.SupplierID % 2 == 0) ? "Tốt" : "Trung bình";

                list.Add(new SupplierEvaluationDTO
                {
                    SupplierID = sup.SupplierID,
                    TenCongTy = sup.SupplierName,
                    LienHe = $"{sup.ContactPerson} - {sup.Phone}",
                    MoTa = string.IsNullOrEmpty(sup.Address) ? "Chưa cập nhật địa chỉ" : sup.Address,
                    DanhGia = rating
                });
            }
            return list;
        }
    }
}