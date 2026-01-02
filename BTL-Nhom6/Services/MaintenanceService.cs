using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;

namespace BTL_Nhom6.Services
{
    public class MaintenanceService
    {
        // 1. Lấy danh sách sự cố chi tiết (cho DataGrid)
        public List<IncidentDTO> GetIncidentReport(DateTime? fromDate, int categoryId, string errorType)
        {
            List<IncidentDTO> list = new List<IncidentDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                System.Text.StringBuilder sql = new System.Text.StringBuilder(@"
                    SELECT r.RequestID, d.DeviceName, r.ProblemDescription, r.RequestDate, r.Priority
                    FROM MaintenanceRequests r
                    JOIN Devices d ON r.DeviceCode = d.DeviceCode
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
                    WHERE 1=1 ");

                // Lọc theo ngày (Từ ngày đã chọn trở đi)
                if (fromDate.HasValue)
                    sql.Append(" AND r.RequestDate >= @FromDate ");

                // Lọc theo loại thiết bị
                if (categoryId > 0)
                    sql.Append(" AND m.CategoryID = @CatID ");

                // Lọc theo loại lỗi (Tìm kiếm tương đối trong mô tả)
                if (!string.IsNullOrEmpty(errorType) && errorType != "Tất cả")
                {
                    // Logic: Nếu chọn "Lỗi điện" -> Tìm chữ "Điện" hoặc "Electric"
                    sql.Append(" AND (r.ProblemDescription LIKE @ErrType) ");
                }

                sql.Append(" ORDER BY r.RequestDate DESC");

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);

                if (fromDate.HasValue)
                    cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);

                if (categoryId > 0)
                    cmd.Parameters.AddWithValue("@CatID", categoryId);

                if (!string.IsNullOrEmpty(errorType) && errorType != "Tất cả")
                {
                    // Xử lý từ khóa tìm kiếm đơn giản
                    string keyword = errorType.Replace("Lỗi ", ""); // Bỏ chữ "Lỗi" đi để tìm rộng hơn
                    cmd.Parameters.AddWithValue("@ErrType", "%" + keyword + "%");
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new IncidentDTO
                        {
                            MaSC = Convert.ToInt32(reader["RequestID"]),
                            ThietBi = reader["DeviceName"].ToString(),
                            LoaiLoi = reader["ProblemDescription"].ToString(),
                            Ngay = Convert.ToDateTime(reader["RequestDate"]),
                            MucDo = reader["Priority"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 2. Lấy dữ liệu thống kê cho biểu đồ (Top 5 thiết bị hay hỏng nhất)
        public List<BarChartDTO> GetIncidentChartData(DateTime? fromDate, int categoryId, string errorType)
        {
            List<BarChartDTO> list = new List<BarChartDTO>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                System.Text.StringBuilder sql = new System.Text.StringBuilder(@"
                    SELECT d.DeviceName, COUNT(r.RequestID) as ErrorCount
                    FROM MaintenanceRequests r
                    JOIN Devices d ON r.DeviceCode = d.DeviceCode
                    LEFT JOIN DeviceModels m ON d.ModelID = m.ModelID
                    WHERE 1=1 ");

                // Áp dụng các bộ lọc giống hệt bên trên
                if (fromDate.HasValue) sql.Append(" AND r.RequestDate >= @FromDate ");
                if (categoryId > 0) sql.Append(" AND m.CategoryID = @CatID ");
                if (!string.IsNullOrEmpty(errorType) && errorType != "Tất cả")
                {
                    string keyword = errorType.Replace("Lỗi ", "");
                    sql.Append(" AND (r.ProblemDescription LIKE @ErrType) ");
                }

                // Group by và lấy Top 5-10
                sql.Append(" GROUP BY d.DeviceCode, d.DeviceName ");
                sql.Append(" ORDER BY ErrorCount DESC LIMIT 8"); // Lấy 8 cột để vừa giao diện

                MySqlCommand cmd = new MySqlCommand(sql.ToString(), conn);

                if (fromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate", fromDate.Value);
                if (categoryId > 0) cmd.Parameters.AddWithValue("@CatID", categoryId);
                if (!string.IsNullOrEmpty(errorType) && errorType != "Tất cả")
                {
                    string keyword = errorType.Replace("Lỗi ", "");
                    cmd.Parameters.AddWithValue("@ErrType", "%" + keyword + "%");
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new BarChartDTO
                        {
                            Label = reader["DeviceName"].ToString(),
                            GiaTriThuc = Convert.ToInt32(reader["ErrorCount"])
                            // HeightValue sẽ tính ở tầng Giao diện (Code-behind)
                        });
                    }
                }
            }
            return list;
        }
    }
}
