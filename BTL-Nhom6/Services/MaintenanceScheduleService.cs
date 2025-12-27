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
        public List<MaintenanceSchedule> GetDueSchedules(int daysLookAhead = 30)
        {
            List<MaintenanceSchedule> list = new List<MaintenanceSchedule>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Logic: Lấy các lịch đang Active VÀ Ngày đến hạn <= (Hôm nay + số ngày quy định)
                // Sắp xếp ưu tiên ngày gần nhất lên đầu
                string sql = @"
                    SELECT s.*, d.DeviceName 
                    FROM MaintenanceSchedules s
                    LEFT JOIN Devices d ON s.DeviceCode = d.DeviceCode
                    WHERE s.Status = 'Active' 
                    AND s.NextMaintenanceDate <= DATE_ADD(CURRENT_DATE, INTERVAL @days DAY)
                    ORDER BY s.NextMaintenanceDate ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@days", daysLookAhead);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaintenanceSchedule
                        {
                            ScheduleID = Convert.ToInt32(reader["ScheduleID"]),
                            DeviceCode = reader["DeviceCode"].ToString(),
                            TaskName = reader["TaskName"].ToString(),
                            FrequencyDays = Convert.ToInt32(reader["FrequencyDays"]),
                            LastMaintenanceDate = reader["LastMaintenanceDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LastMaintenanceDate"]) : null,
                            NextMaintenanceDate = Convert.ToDateTime(reader["NextMaintenanceDate"]),
                            Status = reader["Status"].ToString(),

                            // Map dữ liệu JOIN
                            DeviceName = reader["DeviceName"] != DBNull.Value ? reader["DeviceName"].ToString() : "Unknown"
                        });
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