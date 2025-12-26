using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class WorkOrderService
    {
        // 1. Lấy danh sách Trạng thái (để đổ vào ComboBox khi phân công)
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

        // 2. Tạo Phiếu công việc mới (Hàm quan trọng nhất form này)
        public bool CreateWorkOrder(WorkOrder wo)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // A. Tạo phiếu công việc
                    string sql = @"INSERT INTO WorkOrders 
                                   (DeviceCode, RequestID, ScheduleID, TechnicianID, StatusID, StartDate) 
                                   VALUES 
                                   (@Dev, @Req, @Sch, @Tech, @Stat, @Start)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn, transaction);
                    cmd.Parameters.AddWithValue("@Dev", wo.DeviceCode);
                    // Xử lý Nullable
                    cmd.Parameters.AddWithValue("@Req", wo.RequestID.HasValue ? wo.RequestID.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Sch", wo.ScheduleID.HasValue ? wo.ScheduleID.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Tech", wo.TechnicianID.HasValue ? wo.TechnicianID.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Stat", wo.StatusID); // Thường là 1 (Mới tạo) hoặc 2 (Đang làm)
                    cmd.Parameters.AddWithValue("@Start", DateTime.Now); // Ngày bắt đầu là lúc phân công

                    cmd.ExecuteNonQuery();

                    // B. Cập nhật trạng thái Request (Nếu có RequestID)
                    // Nếu đã phân công -> Chuyển Request sang 'Approved' (Đang thực hiện)
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
                    Console.WriteLine("Error CreateWO: " + ex.Message);
                    return false;
                }
            }
        }
    }
}