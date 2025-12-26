using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class WorkOrderService
    {
        // 1. Lấy danh sách việc của KTV theo ID (Dùng cho form ChiTietCongViecKTV)
        public List<WorkOrderViewModel> GetWorkOrdersByTechId(int userId)
        {
            List<WorkOrderViewModel> list = new List<WorkOrderViewModel>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Câu lệnh SQL: Kết hợp dữ liệu từ nhiều bảng để lấy thông tin hiển thị
                    // Sử dụng COALESCE để lấy Mô tả lỗi ưu tiên từ Request -> Schedule -> Solution
                    string sql = @"
                        SELECT 
                            wo.WorkOrderID,
                            d.DeviceName AS TenThietBi,
                            COALESCE(r.ProblemDescription, s.TaskName, wo.Solution, 'Công việc khác') AS MoTaLoi,
                            COALESCE(r.Priority, 'Medium') AS MucUuTien,
                            stt.StatusName AS TrangThai
                        FROM WorkOrders wo
                        JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                        JOIN WorkOrderStatus stt ON wo.StatusID = stt.StatusID
                        LEFT JOIN MaintenanceRequests r ON wo.RequestID = r.RequestID
                        LEFT JOIN MaintenanceSchedules s ON wo.ScheduleID = s.ScheduleID
                        WHERE wo.TechnicianID = @UID
                        ORDER BY wo.StartDate DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UID", userId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            WorkOrderViewModel item = new WorkOrderViewModel();

                            // Ánh xạ dữ liệu từ DataReader vào Model
                            item.WorkOrderID = Convert.ToInt32(reader["WorkOrderID"]);

                            // Kiểm tra DBNull cho an toàn (dù SQL đã xử lý)
                            item.TenThietBi = reader["TenThietBi"] != DBNull.Value ? reader["TenThietBi"].ToString() : "N/A";
                            item.MoTaLoi = reader["MoTaLoi"] != DBNull.Value ? reader["MoTaLoi"].ToString() : "";
                            item.MucUuTien = reader["MucUuTien"] != DBNull.Value ? reader["MucUuTien"].ToString() : "Medium";
                            item.TrangThai = reader["TrangThai"] != DBNull.Value ? reader["TrangThai"].ToString() : "Unknown";

                            list.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu cần thiết
                    Console.WriteLine("Lỗi GetWorkOrdersByTechId: " + ex.Message);
                }
            }
            return list;
        }

        // 2. Lấy danh sách trạng thái (Cho ComboBox nếu cần)
        public List<WorkOrderStatus> GetAllStatuses()
        {
            List<WorkOrderStatus> list = new List<WorkOrderStatus>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM WorkOrderStatus";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new WorkOrderStatus
                        {
                            StatusID = Convert.ToInt32(reader["StatusID"]),
                            StatusName = reader["StatusName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 3. Tạo Phiếu công việc mới (Giữ nguyên logic cũ dùng Transaction)
        public bool CreateWorkOrder(WorkOrder wo)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // A. Insert vào bảng WorkOrders
                    string sql = @"INSERT INTO WorkOrders 
                                   (DeviceCode, RequestID, ScheduleID, TechnicianID, StatusID, StartDate) 
                                   VALUES 
                                   (@Dev, @Req, @Sch, @Tech, @Stat, @Start)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@Dev", wo.DeviceCode);
                    cmd.Parameters.AddWithValue("@Req", wo.RequestID.HasValue ? wo.RequestID.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sch", wo.ScheduleID.HasValue ? wo.ScheduleID.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tech", wo.TechnicianID.HasValue ? wo.TechnicianID.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Stat", wo.StatusID);
                    cmd.Parameters.AddWithValue("@Start", wo.StartDate ?? DateTime.Now);

                    cmd.ExecuteNonQuery();

                    // B. Cập nhật trạng thái Request (Nếu có RequestID)
                    if (wo.RequestID.HasValue)
                    {
                        string sqlUpdateReq = "UPDATE MaintenanceRequests SET Status = 'Approved' WHERE RequestID = @RID";
                        MySqlCommand cmdReq = new MySqlCommand(sqlUpdateReq, conn, transaction);
                        cmdReq.Parameters.AddWithValue("@RID", wo.RequestID.Value);
                        cmdReq.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Lỗi CreateWO: " + ex.Message);
                    return false;
                }
            }
        }
    }
}