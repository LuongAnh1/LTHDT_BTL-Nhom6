using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class DeviceService
    {
        // 1. Lấy danh sách tất cả thiết bị (Kèm thông tin chi tiết qua JOIN)
        public List<Device> GetAllDevices()
        {
            List<Device> list = new List<Device>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // JOIN 4 bảng để lấy tên thay vì ID
                string sql = @"
                    SELECT d.*, 
                           m.ModelName, 
                           l.LocationName, 
                           s.StatusName, 
                           sup.SupplierName
                    FROM Devices d
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
                    LEFT JOIN Locations l ON d.LocationID = l.LocationID
                    LEFT JOIN DeviceStatus s ON d.StatusID = s.StatusID
                    LEFT JOIN Suppliers sup ON d.SupplierID = sup.SupplierID
                    ORDER BY d.DeviceCode ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Device
                        {
                            DeviceCode = reader["DeviceCode"].ToString(),
                            DeviceName = reader["DeviceName"].ToString(),
                            ModelID = Convert.ToInt32(reader["ModelID"]),
                            SerialNumber = reader["SerialNumber"] != DBNull.Value ? reader["SerialNumber"].ToString() : "",
                            LocationID = Convert.ToInt32(reader["LocationID"]),
                            StatusID = Convert.ToInt32(reader["StatusID"]),

                            // Xử lý ngày tháng (Nullable)
                            PurchaseDate = reader["PurchaseDate"] != DBNull.Value ? (DateTime?)reader["PurchaseDate"] : null,
                            WarrantyExpiry = reader["WarrantyExpiry"] != DBNull.Value ? (DateTime?)reader["WarrantyExpiry"] : null,
                            SupplierID = reader["SupplierID"] != DBNull.Value ? (int?)reader["SupplierID"] : null,

                            // Các trường hiển thị
                            ModelName = reader["ModelName"] != DBNull.Value ? reader["ModelName"].ToString() : "N/A",
                            LocationName = reader["LocationName"] != DBNull.Value ? reader["LocationName"].ToString() : "N/A",
                            StatusName = reader["StatusName"] != DBNull.Value ? reader["StatusName"].ToString() : "N/A",
                            SupplierName = reader["SupplierName"] != DBNull.Value ? reader["SupplierName"].ToString() : "-"
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm thiết bị mới
        public void AddDevice(Device dev)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"INSERT INTO Devices 
                               (DeviceCode, DeviceName, ModelID, SerialNumber, LocationID, StatusID, PurchaseDate, WarrantyExpiry, SupplierID) 
                               VALUES 
                               (@Code, @Name, @Model, @Serial, @Loc, @Status, @BuyDate, @Warranty, @Sup)";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                AddParameters(cmd, dev);
                cmd.ExecuteNonQuery();
            }
        }

        // 3. Cập nhật thiết bị
        public void UpdateDevice(Device dev)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"UPDATE Devices SET 
                               DeviceName=@Name, ModelID=@Model, SerialNumber=@Serial, 
                               LocationID=@Loc, StatusID=@Status, 
                               PurchaseDate=@BuyDate, WarrantyExpiry=@Warranty, SupplierID=@Sup
                               WHERE DeviceCode=@Code";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                AddParameters(cmd, dev);
                cmd.ExecuteNonQuery();
            }
        }

        // 4. Xóa thiết bị
        public void DeleteDevice(string deviceCode)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Devices WHERE DeviceCode = @Code";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Code", deviceCode);
                cmd.ExecuteNonQuery();
            }
        }

        // --- QUAN TRỌNG: Hàm kiểm tra dùng cho form Model ---
        // 5. Đếm số lượng thiết bị thuộc về một Model nào đó
        public int CountDevicesByModel(int modelId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Devices WHERE ModelID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", modelId);

                // Trả về số lượng dòng tìm thấy
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // Hàm phụ trợ add tham số (để đỡ viết lại code)
        private void AddParameters(MySqlCommand cmd, Device dev)
        {
            cmd.Parameters.AddWithValue("@Code", dev.DeviceCode);
            cmd.Parameters.AddWithValue("@Name", dev.DeviceName);
            cmd.Parameters.AddWithValue("@Model", dev.ModelID);
            cmd.Parameters.AddWithValue("@Serial", dev.SerialNumber ?? "");
            cmd.Parameters.AddWithValue("@Loc", dev.LocationID);
            cmd.Parameters.AddWithValue("@Status", dev.StatusID);
            cmd.Parameters.AddWithValue("@BuyDate", dev.PurchaseDate.HasValue ? dev.PurchaseDate.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Warranty", dev.WarrantyExpiry.HasValue ? dev.WarrantyExpiry.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Sup", dev.SupplierID.HasValue ? dev.SupplierID.Value : (object)DBNull.Value);
        }
    }
}