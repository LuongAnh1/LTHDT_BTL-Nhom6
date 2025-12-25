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
        public List<MaintenanceRequest> GetRequests(string statusFilter = "Trạng thái", string keyword = "")
        {
            List<MaintenanceRequest> list = new List<MaintenanceRequest>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
            SELECT r.*, d.DeviceName, u.FullName 
            FROM MaintenanceRequests r
            LEFT JOIN Devices d ON r.DeviceCode = d.DeviceCode
            LEFT JOIN Users u ON r.RequestedBy = u.UserID
            WHERE 1=1 ";

                // --- BƯỚC 1: Xử lý Logic Filter (Mapping từ Tiếng Việt -> Tiếng Anh DB) ---
                string dbStatus = "";
                if (statusFilter == "Đang chờ xử lý") dbStatus = "Pending";
                else if (statusFilter == "Đang thực hiện") dbStatus = "Approved";
                else if (statusFilter == "Hoàn thành") dbStatus = "Completed";
                else if (statusFilter == "Từ chối") dbStatus = "Rejected";

                // Nếu có chọn status hợp lệ thì nối thêm điều kiện vào SQL
                if (!string.IsNullOrEmpty(dbStatus))
                {
                    sql += " AND r.Status = @Status";
                }

                // --- BƯỚC 2: Xử lý Search ---
                if (!string.IsNullOrEmpty(keyword))
                {
                    sql += " AND (r.ProblemDescription LIKE @Keyword OR d.DeviceName LIKE @Keyword OR r.DeviceCode LIKE @Keyword)";
                }

                sql += " ORDER BY r.RequestDate DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);

                // --- BƯỚC 3: Thêm tham số (QUAN TRỌNG: Chỉ thêm 1 lần) ---

                // Sửa lỗi: Chỉ thêm @Status khi dbStatus có dữ liệu
                if (!string.IsNullOrEmpty(dbStatus))
                {
                    cmd.Parameters.AddWithValue("@Status", dbStatus);
                }

                if (!string.IsNullOrEmpty(keyword))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                }

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaintenanceRequest
                        {
                            RequestID = Convert.ToInt32(reader["RequestID"]),
                            DeviceCode = reader["DeviceCode"].ToString(),
                            RequestedBy = Convert.ToInt32(reader["RequestedBy"]),
                            RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                            ActualCompletion = reader["ActualCompletion"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ActualCompletion"]) : null,
                            Priority = reader["Priority"].ToString(),
                            Status = reader["Status"].ToString(),
                            ProblemDescription = reader["ProblemDescription"].ToString(),

                            // Các trường JOIN
                            DeviceName = reader["DeviceName"] != DBNull.Value ? reader["DeviceName"].ToString() : "Unknown Device",
                            RequesterName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : "Unknown User"
                        });
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
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Check status trước khi xóa nếu cần
                string sql = "DELETE FROM MaintenanceRequests WHERE RequestID = @ID AND Status = 'Pending'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", requestId);
                return cmd.ExecuteNonQuery() > 0;
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
    }
}