using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models; // Đã có DTO trong Models
using System.Text;
using System.Data;

namespace BTL_Nhom6.Services
{
    // ==========================================
    // KHU VỰC DTO RIÊNG (Chưa có trong Models thì để ở đây)
    // ==========================================
    public class PerformanceDataDTO
    {
        public int RequestID { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? ActualCompletion { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public string Status { get; set; }
    }

    public class TechnicianDTO
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
    }

    // ==========================================
    // CLASS CHÍNH: REPORT SERVICE
    // (Lần trước bạn bị thiếu dòng này nên gây lỗi)
    // ==========================================
    public class ReportService
    {
        // ---------------------------------------------------------
        // PHẦN 1: CÁC HÀM LẤY DỮ LIỆU CŨ
        // ---------------------------------------------------------

        // 1. Lấy danh sách Kỹ thuật viên
        public List<TechnicianDTO> GetTechnicians()
        {
            List<TechnicianDTO> list = new List<TechnicianDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"SELECT u.UserID, u.FullName 
                       FROM Users u
                       JOIN Roles r ON u.RoleID = r.RoleID
                       WHERE r.RoleName LIKE '%Kỹ thuật viên%' 
                          OR r.RoleName LIKE '%Technician%'
                       ORDER BY u.FullName ASC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TechnicianDTO
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : "User " + reader["UserID"]
                        });
                    }
                }
            }
            return list;
        }

        // 2. Lấy dữ liệu thô (MTTR, Tỷ lệ hoàn thành)
        // SỬA LẠI HÀM NÀY TRONG ReportService.cs
        public List<PerformanceDataDTO> GetPerformanceRawData(DateTime fromDate, DateTime toDate, int? categoryId, int? techId)
        {
            List<PerformanceDataDTO> list = new List<PerformanceDataDTO>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // [THAY ĐỔI] Truy vấn trực tiếp từ WorkOrders thay vì MaintenanceRequests
                // Vì WorkOrders mới có StartDate/EndDate chính xác để tính MTTR
                string sql = @"
                    SELECT 
                        wo.WorkOrderID AS RequestID,       -- Dùng ID phiếu làm ID định danh
                        wo.StartDate AS RequestDate,       -- Ngày bắt đầu sửa
                        wo.EndDate AS ActualCompletion,    -- Ngày thực tế hoàn thành
                        stt.StatusName AS Status,          -- Trạng thái (Completed...)
                        
                        -- Giả định Deadline là 24h sau khi bắt đầu (để tính tỷ lệ đúng hạn)
                        DATE_ADD(wo.StartDate, INTERVAL 24 HOUR) AS ScheduledEndDate 
                    FROM WorkOrders wo
                    JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
                    JOIN WorkOrderStatus stt ON wo.StatusID = stt.StatusID
                    WHERE wo.StatusID = 3  -- Chỉ lấy phiếu ĐÃ HOÀN THÀNH
                    AND wo.EndDate >= @From AND wo.EndDate <= @To 
                ";

                // Bộ lọc Loại thiết bị
                if (categoryId.HasValue && categoryId.Value > 0)
                    sql += " AND m.CategoryID = @CatID ";

                // Bộ lọc Kỹ thuật viên
                if (techId.HasValue && techId.Value > 0)
                    sql += " AND wo.TechnicianID = @TechID ";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@From", fromDate);
                cmd.Parameters.AddWithValue("@To", toDate); // Lưu ý: toDate đã được xử lý là cuối ngày ở Code-behind

                if (categoryId.HasValue && categoryId.Value > 0)
                    cmd.Parameters.AddWithValue("@CatID", categoryId.Value);

                if (techId.HasValue && techId.Value > 0)
                    cmd.Parameters.AddWithValue("@TechID", techId.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new PerformanceDataDTO
                        {
                            RequestID = Convert.ToInt32(reader["RequestID"]),
                            RequestDate = Convert.ToDateTime(reader["RequestDate"]),
                            ActualCompletion = reader["ActualCompletion"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ActualCompletion"]) : null,
                            ScheduledEndDate = reader["ScheduledEndDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ScheduledEndDate"]) : null,
                            Status = "Completed" // Vì đã lọc StatusID = 3 nên chắc chắn là Completed
                        });
                    }
                }
            }
            return list;
        }

        // ---------------------------------------------------------
        // PHẦN 2: CÁC HÀM MỚI CHO GIAO DIỆN BCNSKTV
        // ---------------------------------------------------------

        // 3. Lấy danh sách công việc chi tiết
        public List<BaoCaoCongViecDTO> GetBaoCaoCongViec(DateTime? fromDate, int? locationId, int? categoryId, int? techId)
        {
            List<BaoCaoCongViecDTO> list = new List<BaoCaoCongViecDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                StringBuilder sql = new StringBuilder();

                sql.Append(@"SELECT 
                                u.FullName AS TenKTV,
                                wo.WorkOrderID AS MaCV,
                                COALESCE(req.ProblemDescription, wo.Solution, 'Bảo trì định kỳ') AS MoTa,
                                ws.StatusName AS TrangThai,
                                COALESCE(req.Priority, 'Medium') AS DoUuTien,
                                wo.StartDate
                             FROM WorkOrders wo
                             JOIN Users u ON wo.TechnicianID = u.UserID
                             JOIN WorkOrderStatus ws ON wo.StatusID = ws.StatusID
                             LEFT JOIN MaintenanceRequests req ON wo.RequestID = req.RequestID
                             LEFT JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                             LEFT JOIN Locations l ON d.LocationID = l.LocationID
                             LEFT JOIN DeviceModels dm ON d.ModelID = dm.ModelID
                             LEFT JOIN Categories c ON dm.CategoryID = c.CategoryID
                             WHERE 1=1 ");

                // --- Xử lý Bộ lọc ---
                if (fromDate.HasValue)
                    sql.Append(" AND DATE(wo.StartDate) = DATE(@Date) ");

                if (locationId.HasValue && locationId.Value > 0)
                    sql.Append(" AND l.LocationID = @LocID ");

                if (categoryId.HasValue && categoryId.Value > 0)
                    sql.Append(" AND c.CategoryID = @CatID ");

                if (techId.HasValue && techId.Value > 0)
                    sql.Append(" AND wo.TechnicianID = @TechID ");

                sql.Append(" ORDER BY wo.WorkOrderID DESC");

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);

                if (fromDate.HasValue) cmd.Parameters.AddWithValue("@Date", fromDate.Value);
                if (locationId.HasValue) cmd.Parameters.AddWithValue("@LocID", locationId.Value);
                if (categoryId.HasValue) cmd.Parameters.AddWithValue("@CatID", categoryId.Value);
                if (techId.HasValue) cmd.Parameters.AddWithValue("@TechID", techId.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BaoCaoCongViecDTO
                        {
                            TenKTV = reader["TenKTV"].ToString(),
                            MaCV = Convert.ToInt32(reader["MaCV"]),
                            MoTa = reader["MoTa"].ToString(),
                            TrangThai = reader["TrangThai"].ToString(),
                            DoUuTien = reader["DoUuTien"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 4. Lấy thống kê năng suất
        // 4. Lấy thống kê năng suất (SỬA LẠI ĐOẠN SQL)
        public List<NangSuatKTVDTO> GetNangSuatKTV(int topCount = 10)
        {
            List<NangSuatKTVDTO> list = new List<NangSuatKTVDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // [ĐÃ SỬA] Thay điều kiện RoleName bằng RoleID = 2 (Technician)
                // Hoặc dùng OR để bắt cả 2 trường hợp tên
                string sql = @"SELECT u.UserID, u.FullName, COUNT(wo.WorkOrderID) AS TongViec
                               FROM Users u
                               JOIN Roles r ON u.RoleID = r.RoleID
                               LEFT JOIN WorkOrders wo ON u.UserID = wo.TechnicianID 
                                    AND wo.StatusID = 3 -- Chỉ đếm phiếu ĐÃ HOÀN THÀNH
                               WHERE (r.RoleID = 2 OR r.RoleName LIKE '%Technician%' OR r.RoleName LIKE '%Kỹ thuật viên%')
                                 AND u.IsActive = 1
                               GROUP BY u.UserID, u.FullName
                               ORDER BY TongViec DESC
                               LIMIT @Top";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Top", topCount);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new NangSuatKTVDTO
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            TenKTV = reader["FullName"].ToString(),
                            TongCongViec = Convert.ToInt32(reader["TongViec"])
                        });
                    }
                }

                // (Giữ nguyên đoạn lấy kỹ năng phía dưới...)
                foreach (var ktv in list)
                {
                    ktv.DanhSachKyNang = GetSkillsByUserId(ktv.UserID, conn);
                }
            }
            return list;
        }

        // 5. Helper lấy kỹ năng
        private List<string> GetSkillsByUserId(int userId, MySqlConnection openConn)
        {
            List<string> skills = new List<string>();
            string sql = @"SELECT s.SkillName 
                           FROM TechnicianSkills ts
                           JOIN Skills s ON ts.SkillID = s.SkillID
                           WHERE ts.UserID = @UID";

            using (var cmd = new MySqlCommand(sql, openConn))
            {
                cmd.Parameters.AddWithValue("@UID", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        skills.Add(reader["SkillName"].ToString());
                    }
                }
            }
            return skills;
        }

        // 6. Lấy danh sách Phân Xưởng
        public Dictionary<int, string> GetLocations()
        {
            var dict = new Dictionary<int, string>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT LocationID, LocationName FROM Locations ORDER BY LocationName", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dict.Add(Convert.ToInt32(reader["LocationID"]), reader["LocationName"].ToString());
                    }
                }
                catch { }
            }
            return dict;
        }

        // 7. Lấy danh sách Loại thiết bị
        public Dictionary<int, string> GetCategories()
        {
            var dict = new Dictionary<int, string>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT CategoryID, CategoryName FROM Categories ORDER BY CategoryName", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dict.Add(Convert.ToInt32(reader["CategoryID"]), reader["CategoryName"].ToString());
                    }
                }
                catch { }
            }
            return dict;
        }
    } // <-- Dấu ngoặc đóng Class ReportService
} // <-- Dấu ngoặc đóng Namespace