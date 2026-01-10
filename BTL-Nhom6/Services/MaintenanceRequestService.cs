using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class DeviceSelection
    {
        public string DeviceCode { get; set; }
        public string DeviceName { get; set; }
    }

    public class MaintenanceRequestService
    {
        // 1. Lấy danh sách yêu cầu (Kèm Filter và Search)
        // statusFilter: "All", "Pending", "Approved"...
        // keyword: Tìm theo tên thiết bị hoặc mô tả lỗi
        // Trong file MaintenanceRequestService.cs

        // Trong file MaintenanceRequestService.cs

        public List<MaintenanceRequest> GetRequests(string statusFilter = "Trạng thái", string keyword = "", int? userId = null)
        {
            List<MaintenanceRequest> list = new List<MaintenanceRequest>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // [SQL]
                string sql = @"
                SELECT r.*, 
                       d.DeviceName, 
                       u_req.FullName AS RequesterName,
                       u_tech.FullName AS TechnicianName
                FROM MaintenanceRequests r
                LEFT JOIN Devices d ON r.DeviceCode = d.DeviceCode
                LEFT JOIN Users u_req ON r.RequestedBy = u_req.UserID
                LEFT JOIN WorkOrders wo ON r.RequestID = wo.RequestID
                LEFT JOIN Users u_tech ON wo.TechnicianID = u_tech.UserID
                WHERE 1=1 ";

                // --- BƯỚC 0: Filter theo User ID ---
                if (userId.HasValue)
                {
                    sql += " AND r.RequestedBy = @UID ";
                }

                // --- BƯỚC 1: Filter Trạng thái ---
                string dbStatus = "";
                if (statusFilter == "Đang chờ xử lý") dbStatus = "Pending";
                else if (statusFilter == "Đang thực hiện") dbStatus = "Approved";
                else if (statusFilter == "Hoàn thành") dbStatus = "Completed";
                else if (statusFilter == "Từ chối") dbStatus = "Rejected";

                if (!string.IsNullOrEmpty(dbStatus)) sql += " AND r.Status = @Status";

                // --- BƯỚC 2: Search ---
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += " AND (r.ProblemDescription LIKE @Keyword OR d.DeviceName LIKE @Keyword OR r.DeviceCode LIKE @Keyword)";
                }

                sql += " ORDER BY r.RequestDate DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                if (userId.HasValue) cmd.Parameters.AddWithValue("@UID", userId.Value);
                if (!string.IsNullOrEmpty(dbStatus)) cmd.Parameters.AddWithValue("@Status", dbStatus);
                if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var req = new MaintenanceRequest
                        {
                            RequestID = Convert.ToInt32(reader["RequestID"]),
                            DeviceCode = reader["DeviceCode"].ToString(),
                            RequestedBy = Convert.ToInt32(reader["RequestedBy"]),
                            RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                            ActualCompletion = reader["ActualCompletion"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ActualCompletion"]) : null,
                            Priority = reader["Priority"].ToString(),
                            Status = reader["Status"].ToString(),
                            ProblemDescription = reader["ProblemDescription"].ToString(),

                            // --- SỬA Ở ĐÂY: Gán vào RequesterName và TechnicianName ---
                            // (Thay vì gán vào NguoiYeuCau/NguoiXuLy đang bị lỗi Read-only)
                            DeviceName = reader["DeviceName"] != DBNull.Value ? reader["DeviceName"].ToString() : "Unknown",
                            RequesterName = reader["RequesterName"] != DBNull.Value ? reader["RequesterName"].ToString() : "Unknown",
                            TechnicianName = reader["TechnicianName"] != DBNull.Value ? reader["TechnicianName"].ToString() : ""
                        };

                        list.Add(req);
                    }
                }
            }
            return list;
        }

        // 2. Tạo yêu cầu bảo trì MỚI (User) + Lưu ảnh (Transaction)
        public bool CreateRequest(MaintenanceRequest req, List<string> imageUrls)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Bước 1: Thêm Request
                    string sqlReq = @"INSERT INTO MaintenanceRequests (DeviceCode, RequestedBy, Priority, Status, ProblemDescription, RequestDate) 
                                      VALUES (@Device, @User, @Priority, 'Pending', @Desc, NOW());
                                      SELECT LAST_INSERT_ID();";

                    MySqlCommand cmdReq = new MySqlCommand(sqlReq, conn, transaction);
                    cmdReq.Parameters.AddWithValue("@Device", req.DeviceCode);
                    cmdReq.Parameters.AddWithValue("@User", req.RequestedBy);
                    cmdReq.Parameters.AddWithValue("@Priority", req.Priority);
                    cmdReq.Parameters.AddWithValue("@Desc", req.ProblemDescription);

                    // Lấy ID vừa tạo
                    int newRequestId = Convert.ToInt32(cmdReq.ExecuteScalar());

                    // Bước 2: Thêm ảnh (nếu có)
                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        string sqlImg = "INSERT INTO RequestImages (RequestID, ImageUrl) VALUES (@ReqID, @Url)";
                        foreach (string url in imageUrls)
                        {
                            MySqlCommand cmdImg = new MySqlCommand(sqlImg, conn, transaction);
                            cmdImg.Parameters.AddWithValue("@ReqID", newRequestId);
                            cmdImg.Parameters.AddWithValue("@Url", url);
                            cmdImg.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Error: " + ex.Message); // Log lỗi
                    return false;
                }
            }
        }

        // 3. Admin Duyệt hoặc Từ chối yêu cầu
        public bool UpdateStatus(int requestId, string newStatus)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE MaintenanceRequests SET Status = @Status WHERE RequestID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // newStatus phải là: 'Approved' hoặc 'Rejected' hoặc 'Completed'
                cmd.Parameters.AddWithValue("@Status", newStatus);
                cmd.Parameters.AddWithValue("@ID", requestId);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 4. Xóa yêu cầu (Chỉ cho phép khi Pending)
        public bool DeleteRequest(int requestId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction()) // <--- Bắt buộc phải có Transaction
                {
                    try
                    {
                        MySqlCommand cmd = new MySqlCommand("", conn, transaction);

                        // 1. Xóa WorkOrders (Phiếu công việc) liên quan trước
                        // Vì mục Completed chắc chắn có phiếu công việc đi kèm
                        cmd.CommandText = "DELETE FROM WorkOrders WHERE RequestID = @ID";
                        cmd.Parameters.AddWithValue("@ID", requestId);
                        cmd.ExecuteNonQuery();

                        // 2. Xóa Ảnh minh chứng (nếu bảng ảnh ko có ON DELETE CASCADE trong SQL)
                        cmd.CommandText = "DELETE FROM RequestImages WHERE RequestID = @ID";
                        cmd.ExecuteNonQuery();

                        // 3. Sau đó mới xóa Yêu cầu
                        cmd.CommandText = "DELETE FROM MaintenanceRequests WHERE RequestID = @ID";
                        int rows = cmd.ExecuteNonQuery();

                        transaction.Commit();
                        return rows > 0;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<DeviceSelection> GetDevicesForSelection()
        {
            List<DeviceSelection> list = new List<DeviceSelection>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT DeviceCode, DeviceName FROM Devices"; // Giả sử bảng Devices có cột này
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new DeviceSelection
                        {
                            DeviceCode = reader["DeviceCode"].ToString(),
                            DeviceName = reader["DeviceName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // Cập nhật thông tin yêu cầu (Dùng cho cả Admin duyệt và User sửa)
        public bool UpdateRequest(MaintenanceRequest req)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Cập nhật: Trạng thái, Ưu tiên, Mô tả, Ngày hoàn thành (nếu có)
                string sql = @"UPDATE MaintenanceRequests 
                       SET Priority = @Priority, 
                           Status = @Status, 
                           ProblemDescription = @Desc,
                           ActualCompletion = @ActualCompletion
                       WHERE RequestID = @ID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // Map lại giá trị ENUM cho đúng với Database
                // Lưu ý: req.Status đang chứa giá trị Tiếng Anh (Pending, Approved...) từ Model
                cmd.Parameters.AddWithValue("@Priority", req.Priority);
                cmd.Parameters.AddWithValue("@Status", req.Status);
                cmd.Parameters.AddWithValue("@Desc", req.ProblemDescription);
                cmd.Parameters.AddWithValue("@ID", req.RequestID);

                // Nếu trạng thái là Hoàn thành (Completed) -> Cập nhật ngày thực tế, ngược lại để NULL
                if (req.Status == "Completed")
                    cmd.Parameters.AddWithValue("@ActualCompletion", DateTime.Now);
                else
                    cmd.Parameters.AddWithValue("@ActualCompletion", DBNull.Value);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Hàm kiểm tra xem RequestID này đã có WorkOrder nào chưa
        public bool HasWorkOrder(int requestId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM WorkOrders WHERE RequestID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", requestId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
    }
}