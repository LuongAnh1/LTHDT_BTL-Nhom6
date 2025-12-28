using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class MaintenanceScheduleService
    {
        // 1. Lấy danh sách lịch bảo trì sắp đến hạn
        // daysLookAhead: Số ngày nhìn về tương lai (ví dụ: lấy các lịch trong 7 ngày tới)
        public List<MaintenanceSchedule> GetDueSchedules(int daysAhead)
        {
            List<MaintenanceSchedule> list = new List<MaintenanceSchedule>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // LOGIC MỚI: 
                // Lấy thông tin WO gần nhất đang thực hiện (nếu có)
                // Kiểm tra WO có StatusID thuộc nhóm "Đang hoạt động" (1: Mới, 2: Đang làm, 4: Tạm hoãn)
                // Loại trừ các WO đã xong (3) hoặc Hủy (5)

                string sql = @"
        SELECT 
            s.ScheduleID, s.DeviceCode, s.TaskName, s.FrequencyDays, s.NextMaintenanceDate,
            d.DeviceName,
            -- Kiểm tra xem có phiếu nào đang chạy hay không
            (SELECT wo.WorkOrderID 
             FROM WorkOrders wo 
             WHERE wo.ScheduleID = s.ScheduleID 
               AND wo.StatusID IN (1, 2, 4) -- Chỉ lấy phiếu chưa xong
             ORDER BY wo.WorkOrderID DESC LIMIT 1) AS WorkOrderID,
             
            (SELECT u.FullName 
             FROM WorkOrders wo 
             JOIN Users u ON wo.TechnicianID = u.UserID
             WHERE wo.ScheduleID = s.ScheduleID 
               AND wo.StatusID IN (1, 2, 4)
             ORDER BY wo.WorkOrderID DESC LIMIT 1) AS NguoiThucHien

        FROM MaintenanceSchedules s
        JOIN Devices d ON s.DeviceCode = d.DeviceCode
        WHERE s.Status = 'Active' 
          AND s.NextMaintenanceDate <= DATE_ADD(CURDATE(), INTERVAL @Days DAY)
        ORDER BY s.NextMaintenanceDate ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Days", daysAhead);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new MaintenanceSchedule
                        {
                            ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                            DeviceCode = reader["DeviceCode"].ToString(),
                            DeviceName = reader["DeviceName"].ToString(),
                            TaskName = reader["TaskName"].ToString(),
                            FrequencyDays = Convert.ToInt32(reader["FrequencyDays"]),
                            NextMaintenanceDate = Convert.ToDateTime(reader["NextMaintenanceDate"]),

                            // Thêm các thuộc tính mới vào Model MaintenanceSchedule (Bạn cần bổ sung vào file Model nhé)
                            IsProcessing = reader["WorkOrderID"] != DBNull.Value, // Có phiếu đang chạy
                            TechnicianName = reader["NguoiThucHien"] != DBNull.Value ? reader["NguoiThucHien"].ToString() : ""
                        };
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        // 2. Cập nhật ngày bảo trì tiếp theo (Sẽ dùng khi hoàn thành phiếu công việc)
        public void UpdateNextDueDate(int scheduleId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Logic: Cập nhật LastDate = Hôm nay, NextDate = Hôm nay + FrequencyDays
                string sql = @"
                    UPDATE MaintenanceSchedules 
                    SET LastMaintenanceDate = CURRENT_DATE,
                        NextMaintenanceDate = DATE_ADD(CURRENT_DATE, INTERVAL FrequencyDays DAY)
                    WHERE ScheduleID = @ID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", scheduleId);
                cmd.ExecuteNonQuery();
            }
        }
    }
}