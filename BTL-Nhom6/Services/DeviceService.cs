using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BTL_Nhom6.Services
{
    public class DeviceService
    {
        // 1. Lấy danh sách (Đã sửa SQL để lấy thêm u.UserID)
        public List<Device> GetAllDevices()
        {
            List<Device> list = new List<Device>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // [SỬA] Thêm u.UserID AS CurrentHolderId
                string sql = @"
                    SELECT d.*, 
                           m.ModelName, l.LocationName, s.StatusName, 
                           sup.SupplierName, sup.Phone AS SupplierPhone, sup.ContactPerson AS SupplierContactPerson,
                           u.FullName AS CurrentUserFullName,
                           u.UserID AS CurrentHolderId 
                    FROM Devices d
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
                    LEFT JOIN Locations l ON d.LocationID = l.LocationID
                    LEFT JOIN DeviceStatus s ON d.StatusID = s.StatusID
                    LEFT JOIN Suppliers sup ON d.SupplierID = sup.SupplierID
                    LEFT JOIN DeviceAssignments da ON d.DeviceCode = da.DeviceCode AND da.ReturnDate IS NULL
                    LEFT JOIN Users u ON da.UserID = u.UserID
                    ORDER BY d.DeviceCode ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapReaderToDevice(reader));
                    }
                }
            }
            return list;
        }

        // 2. Tìm kiếm (Đã sửa SQL để lấy thêm u.UserID)
        public List<Device> FindDevices(string keyword, int? locationId, int? statusId, string userKeyword)
        {
            List<Device> list = new List<Device>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                StringBuilder sql = new StringBuilder(@"
                    SELECT d.*, 
                           m.ModelName, l.LocationName, s.StatusName, 
                           sup.SupplierName, sup.Phone AS SupplierPhone, sup.ContactPerson AS SupplierContactPerson,
                           u.FullName AS CurrentUserFullName,
                           u.UserID AS CurrentHolderId
                    FROM Devices d
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
                    LEFT JOIN Locations l ON d.LocationID = l.LocationID
                    LEFT JOIN DeviceStatus s ON d.StatusID = s.StatusID
                    LEFT JOIN Suppliers sup ON d.SupplierID = sup.SupplierID
                    LEFT JOIN DeviceAssignments da ON d.DeviceCode = da.DeviceCode AND da.ReturnDate IS NULL
                    LEFT JOIN Users u ON da.UserID = u.UserID
                    WHERE 1=1 ");

                if (!string.IsNullOrEmpty(keyword))
                    sql.Append(" AND (d.DeviceName LIKE @Kw OR d.DeviceCode LIKE @Kw) ");

                if (locationId.HasValue && locationId.Value > 0)
                    sql.Append(" AND d.LocationID = @LocID ");

                if (statusId.HasValue && statusId.Value > 0)
                    sql.Append(" AND d.StatusID = @StatID ");

                if (!string.IsNullOrEmpty(userKeyword))
                    sql.Append(" AND u.FullName LIKE @UserKw ");

                sql.Append(" ORDER BY d.DeviceCode ASC");

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);
                if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@Kw", "%" + keyword + "%");
                if (locationId.HasValue && locationId.Value > 0) cmd.Parameters.AddWithValue("@LocID", locationId.Value);
                if (statusId.HasValue && statusId.Value > 0) cmd.Parameters.AddWithValue("@StatID", statusId.Value);
                if (!string.IsNullOrEmpty(userKeyword)) cmd.Parameters.AddWithValue("@UserKw", "%" + userKeyword + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapReaderToDevice(reader));
                    }
                }
            }
            return list;
        }

        // 3. Hàm Map (Đã bổ sung Map CurrentHolderId)
        private Device MapReaderToDevice(MySqlDataReader reader)
        {
            return new Device
            {
                DeviceCode = reader["DeviceCode"].ToString(),
                DeviceName = reader["DeviceName"].ToString(),
                ModelID = Convert.ToInt32(reader["ModelID"]),
                SerialNumber = reader["SerialNumber"] != DBNull.Value ? reader["SerialNumber"].ToString() : "",
                LocationID = Convert.ToInt32(reader["LocationID"]),
                StatusID = Convert.ToInt32(reader["StatusID"]),

                PurchaseDate = reader["PurchaseDate"] != DBNull.Value ? (DateTime?)reader["PurchaseDate"] : null,
                WarrantyExpiry = reader["WarrantyExpiry"] != DBNull.Value ? (DateTime?)reader["WarrantyExpiry"] : null,
                SupplierID = reader["SupplierID"] != DBNull.Value ? (int?)reader["SupplierID"] : null,

                ModelName = reader["ModelName"] != DBNull.Value ? reader["ModelName"].ToString() : "N/A",
                LocationName = reader["LocationName"] != DBNull.Value ? reader["LocationName"].ToString() : "N/A",
                StatusName = reader["StatusName"] != DBNull.Value ? reader["StatusName"].ToString() : "N/A",
                SupplierName = reader["SupplierName"] != DBNull.Value ? reader["SupplierName"].ToString() : "-",
                SupplierPhone = HasColumn(reader, "SupplierPhone") && reader["SupplierPhone"] != DBNull.Value ? reader["SupplierPhone"].ToString() : "",
                SupplierContactPerson = HasColumn(reader, "SupplierContactPerson") && reader["SupplierContactPerson"] != DBNull.Value ? reader["SupplierContactPerson"].ToString() : "",

                // [MỚI] Map ID và Tên người dùng
                CurrentUserFullName = reader["CurrentUserFullName"] != DBNull.Value ? reader["CurrentUserFullName"].ToString() : "Chưa bàn giao",
                CurrentHolderId = HasColumn(reader, "CurrentHolderId") && reader["CurrentHolderId"] != DBNull.Value ? Convert.ToInt32(reader["CurrentHolderId"]) : 0
            };
        }

        // 3.1 Hàm phụ trợ: Kiểm tra cột có tồn tại trong Reader không (tránh lỗi nếu query thiếu cột)
        private bool HasColumn(MySqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        // 4. Thêm thiết bị mới
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

        // 5. Cập nhật thiết bị
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

        // 6. Xóa thiết bị
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
        // 7. Đếm số lượng thiết bị thuộc về một Model nào đó
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
        
        // 8. Đếm số thiết bị thuộc về một Location
        public int CountDevicesByLocation(int locationId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Devices WHERE LocationID = @LocID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@LocID", locationId);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        // 10. Hàm lấy danh sách thiết bị sắp hết hạn trong khoảng 'days' ngày tới
        public List<Device> GetDevicesExpiringSoon(int days)
        {
            List<Device> list = new List<Device>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
            SELECT d.*, 
                   m.ModelName, l.LocationName, s.StatusName, 
                   sup.SupplierName, sup.Phone AS SupplierPhone, sup.ContactPerson AS SupplierContactPerson,
                   u.FullName AS CurrentUserFullName,
                   u.UserID AS CurrentHolderId
            FROM Devices d
            LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
            LEFT JOIN Locations l ON d.LocationID = l.LocationID
            LEFT JOIN DeviceStatus s ON d.StatusID = s.StatusID
            LEFT JOIN Suppliers sup ON d.SupplierID = sup.SupplierID
            LEFT JOIN DeviceAssignments da ON d.DeviceCode = da.DeviceCode AND da.ReturnDate IS NULL
            LEFT JOIN Users u ON da.UserID = u.UserID
            WHERE d.WarrantyExpiry IS NOT NULL 
              AND d.WarrantyExpiry >= CURRENT_DATE()
              AND d.WarrantyExpiry <= DATE_ADD(CURRENT_DATE(), INTERVAL @days DAY)
            ORDER BY d.WarrantyExpiry ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@days", days);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapReaderToDevice(reader));
                    }
                }
            }
            return list;
        }

        // 9. Hàm phụ trợ add tham số (để đỡ viết lại code)
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

        // Thêm vào class DeviceService
        // Trong file Services/DeviceService.cs

        // Trong file DeviceService.cs

        // Đổi tên tham số từ expiryLimit thành startDate cho đúng ý nghĩa
        public List<DeviceWarrantyDTO> GetWarrantyReport(DateTime? startDate, int? supplierId, int? categoryId)
        {
            List<DeviceWarrantyDTO> list = new List<DeviceWarrantyDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Xử lý nếu ngày không được chọn thì lấy ngày hiện tại
                DateTime dateFrom = startDate.HasValue ? startDate.Value : DateTime.Now;

                System.Text.StringBuilder sql = new System.Text.StringBuilder(@"
                    SELECT d.DeviceCode, d.DeviceName, d.PurchaseDate, d.WarrantyExpiry
                    FROM Devices d
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID 
                    WHERE 1=1 ");

                // --- LOGIC MỚI: TÌM TRONG 30 NGÀY TỚI ---

                // 1. Chặn dưới: Ngày hết hạn phải >= Ngày bạn chọn
                sql.Append(" AND d.WarrantyExpiry >= @DateFrom ");

                // 2. Chặn trên: Ngày hết hạn phải <= Ngày bạn chọn + 30 ngày
                sql.Append(" AND d.WarrantyExpiry <= DATE_ADD(@DateFrom, INTERVAL 30 DAY) ");

                // --- CÁC BỘ LỌC KHÁC ---
                if (supplierId.HasValue && supplierId.Value > 0)
                    sql.Append(" AND d.SupplierID = @SupID ");

                if (categoryId.HasValue && categoryId.Value > 0)
                    sql.Append(" AND m.CategoryID = @CatID ");

                sql.Append(" ORDER BY d.WarrantyExpiry ASC");

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);

                // Thêm tham số ngày bắt đầu
                cmd.Parameters.AddWithValue("@DateFrom", dateFrom);

                if (supplierId.HasValue && supplierId.Value > 0)
                    cmd.Parameters.AddWithValue("@SupID", supplierId.Value);

                if (categoryId.HasValue && categoryId.Value > 0)
                    cmd.Parameters.AddWithValue("@CatID", categoryId.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceWarrantyDTO
                        {
                            MaTB = reader["DeviceCode"].ToString(),
                            TenTB = reader["DeviceName"].ToString(),
                            NgayMua = reader["PurchaseDate"] != DBNull.Value ? (DateTime?)reader["PurchaseDate"] : null,
                            NgayHetHan = reader["WarrantyExpiry"] != DBNull.Value ? (DateTime?)reader["WarrantyExpiry"] : null
                        });
                    }
                }
            }
            return list;
        }
        public bool TransferAndHandover(string deviceCode, int newLocationId, int newUserId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Bắt đầu Transaction để đảm bảo cả 2 lệnh cùng thành công hoặc cùng thất bại
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conn;
                        cmd.Transaction = transaction;

                        // BƯỚC 1: Cập nhật vị trí trong bảng Devices
                        cmd.CommandText = "UPDATE Devices SET LocationID = @LocID WHERE DeviceCode = @Code";
                        cmd.Parameters.AddWithValue("@LocID", newLocationId);
                        cmd.Parameters.AddWithValue("@Code", deviceCode);
                        cmd.ExecuteNonQuery();

                        // BƯỚC 2: Cập nhật người phụ trách trong bảng DeviceAssignments
                        // Logic: Tìm dòng đang active (ReturnDate IS NULL) để update người mới
                        cmd.Parameters.Clear(); // Xóa tham số cũ
                        cmd.CommandText = @"UPDATE DeviceAssignments 
                                            SET UserID = @UserID, AssignedDate = NOW() 
                                            WHERE DeviceCode = @Code AND ReturnDate IS NULL";
                        cmd.Parameters.AddWithValue("@UserID", newUserId);
                        cmd.Parameters.AddWithValue("@Code", deviceCode);

                        int rowsAssignments = cmd.ExecuteNonQuery();

                        // Nếu không có dòng nào được update (nghĩa là thiết bị này chưa từng được giao cho ai - mới nhập)
                        // Thì ta phải INSERT dòng mới
                        if (rowsAssignments == 0)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = @"INSERT INTO DeviceAssignments (DeviceCode, UserID, AssignedDate) 
                                                VALUES (@Code, @UserID, NOW())";
                            cmd.Parameters.AddWithValue("@Code", deviceCode);
                            cmd.Parameters.AddWithValue("@UserID", newUserId);
                            cmd.ExecuteNonQuery();
                        }

                        // Hoàn tất transaction
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Nếu lỗi thì rollback lại mọi thứ
                        transaction.Rollback();
                        Console.WriteLine("Lỗi TransferAndHandover: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        // 11. Lấy danh sách chi tiết kèm Trạng thái (để đổ vào DataGrid)
        // Sửa dòng khai báo để nhận 3 tham số
        public List<DeviceStatusDTO> GetDeviceStatusList(int? locationId, int? categoryId, int? phanXuongId)
        {
            List<DeviceStatusDTO> list = new List<DeviceStatusDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                System.Text.StringBuilder sql = new System.Text.StringBuilder(@"
            SELECT d.DeviceCode, d.DeviceName, s.StatusName
            FROM Devices d
            JOIN DeviceStatus s ON d.StatusID = s.StatusID
            LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
            JOIN Locations l ON d.LocationID = l.LocationID  -- JOIN thêm bảng Locations
            WHERE 1=1 ");

                // 1. Lọc Vị trí cụ thể (Con)
                if (locationId.HasValue && locationId.Value > 0)
                    sql.Append(" AND d.LocationID = @LocID ");

                // 2. Lọc Loại thiết bị
                if (categoryId.HasValue && categoryId.Value > 0)
                    sql.Append(" AND m.CategoryID = @CatID ");

                // 3. Lọc Phân xưởng (Chính là lọc theo ParentLocationID của vị trí)
                if (phanXuongId.HasValue && phanXuongId.Value > 0)
                {
                    // Tìm các thiết bị nằm trong vị trí mà vị trí đó thuộc về Phân xưởng này
                    sql.Append(" AND l.ParentLocationID = @PxID ");
                }

                sql.Append(" ORDER BY s.StatusName, d.DeviceName ASC");

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);

                if (locationId.HasValue && locationId.Value > 0)
                    cmd.Parameters.AddWithValue("@LocID", locationId.Value);

                if (categoryId.HasValue && categoryId.Value > 0)
                    cmd.Parameters.AddWithValue("@CatID", categoryId.Value);

                if (phanXuongId.HasValue && phanXuongId.Value > 0)
                    cmd.Parameters.AddWithValue("@PxID", phanXuongId.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceStatusDTO
                        {
                            MaTB = reader["DeviceCode"].ToString(),
                            TenTB = reader["DeviceName"].ToString(),
                            TrangThai = reader["StatusName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 12. Lấy số liệu thống kê cho Biểu đồ tròn
        public List<StatusChartDTO> GetStatusChartData(int? locationId, int? categoryId, int? phanXuongId)
        {
            List<StatusChartDTO> list = new List<StatusChartDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Query cơ bản
                System.Text.StringBuilder sql = new System.Text.StringBuilder(@"
            SELECT s.StatusName, COUNT(d.DeviceCode) AS Quantity
            FROM Devices d
            JOIN DeviceStatus s ON d.StatusID = s.StatusID
            LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
            WHERE 1=1 ");

                // 1. Lọc theo Vị trí
                if (locationId.HasValue && locationId.Value > 0)
                    sql.Append(" AND d.LocationID = @LocID ");

                // 2. Lọc theo Loại thiết bị
                if (categoryId.HasValue && categoryId.Value > 0)
                    sql.Append(" AND m.CategoryID = @CatID ");

                // 3. Lọc theo Phân xưởng (MỚI BỔ SUNG)
                if (phanXuongId.HasValue && phanXuongId.Value > 0)
                {
                    // LƯU Ý QUAN TRỌNG: Kiểm tra tên cột trong bảng Devices của bạn.
                    // Nếu cột là 'MaPX' thì giữ nguyên, nếu là 'DepartmentID' thì sửa lại dòng dưới.
                    sql.Append(" AND d.MaPX = @PxID ");
                }

                sql.Append(" GROUP BY s.StatusName ");

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);

                // Add tham số Vị trí
                if (locationId.HasValue && locationId.Value > 0)
                    cmd.Parameters.AddWithValue("@LocID", locationId.Value);

                // Add tham số Loại thiết bị
                if (categoryId.HasValue && categoryId.Value > 0)
                    cmd.Parameters.AddWithValue("@CatID", categoryId.Value);

                // Add tham số Phân xưởng (MỚI BỔ SUNG)
                if (phanXuongId.HasValue && phanXuongId.Value > 0)
                    cmd.Parameters.AddWithValue("@PxID", phanXuongId.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new StatusChartDTO
                        {
                            StatusName = reader["StatusName"].ToString(),
                            Quantity = Convert.ToInt32(reader["Quantity"])
                        });
                    }
                }
            }
            return list;
        }

        // 13. Lấy danh sách thiết bị theo Chủ sở hữu (UserID)
        public List<Device> GetDevicesByOwner(int userId)
        {
            List<Device> list = new List<Device>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Join với bảng DeviceAssignments để lấy thiết bị của User này
                string sql = @"SELECT d.* 
                       FROM Devices d
                       JOIN DeviceAssignments da ON d.DeviceCode = da.DeviceCode
                       WHERE da.UserID = @UID 
                       -- Tùy chọn: Chỉ lấy thiết bị đang mượn (chưa trả)
                       -- AND da.ReturnDate IS NULL 
                       GROUP BY d.DeviceCode"; // Group by để tránh trùng nếu mượn nhiều lần

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UID", userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapReaderToDevice(reader));
                    }
                }
            }
            return list;
        }
    }
}