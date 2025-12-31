using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows; // Thư viện để hiện MessageBox

namespace BTL_Nhom6.Services
{
    public class WorkOrderService
    {
        // ==========================================================
        // 1. Lấy danh sách việc của KTV theo ID
        // ==========================================================
        public List<WorkOrderViewModel> GetWorkOrdersByTechId(int userId)
        {
            List<WorkOrderViewModel> list = new List<WorkOrderViewModel>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
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
                            item.WorkOrderID = Convert.ToInt32(reader["WorkOrderID"]);
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
                    MessageBox.Show("Lỗi GetWorkOrdersByTechId: " + ex.Message);
                }
            }
            return list;
        }

        // ==========================================================
        // 2. Lấy danh sách trạng thái
        // ==========================================================
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

        // ==========================================================
        // 3. Tạo Phiếu công việc mới
        // ==========================================================
        public bool CreateWorkOrder(WorkOrder wo)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
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
                    MessageBox.Show("Lỗi CreateWO: " + ex.Message);
                    return false;
                }
            }
        }

        // ==========================================================
        // 4. Cập nhật tiến độ WorkOrder
        // ==========================================================
        public bool UpdateWorkOrder(int workOrderId, int newStatusId, string solution)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        MySqlCommand cmd = new MySqlCommand("", conn, trans);

                        // 4.1. Cập nhật bảng WorkOrders
                        string sqlUpdateWO = @"UPDATE WorkOrders 
                                               SET StatusID = @Stat, 
                                                   Solution = @Sol,
                                                   EndDate = CASE WHEN @Stat = 3 THEN NOW() ELSE EndDate END
                                               WHERE WorkOrderID = @ID";

                        cmd.CommandText = sqlUpdateWO;
                        cmd.Parameters.AddWithValue("@Stat", newStatusId);
                        cmd.Parameters.AddWithValue("@Sol", solution ?? "");
                        cmd.Parameters.AddWithValue("@ID", workOrderId);
                        cmd.ExecuteNonQuery();

                        // 4.2. Đồng bộ trạng thái sang MaintenanceRequests (nếu có)
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT RequestID FROM WorkOrders WHERE WorkOrderID = @WOID";
                        cmd.Parameters.AddWithValue("@WOID", workOrderId);

                        object reqIdObj = cmd.ExecuteScalar();
                        if (reqIdObj != null && reqIdObj != DBNull.Value)
                        {
                            int requestId = Convert.ToInt32(reqIdObj);
                            string reqStatus = "";

                            if (newStatusId == 3) reqStatus = "Completed";
                            else if (newStatusId == 4) reqStatus = "Pending";

                            if (!string.IsNullOrEmpty(reqStatus))
                            {
                                cmd.Parameters.Clear();
                                string sqlUpdateReq = (reqStatus == "Completed")
                                    ? "UPDATE MaintenanceRequests SET Status = @RStat, ActualCompletion = NOW() WHERE RequestID = @RID"
                                    : "UPDATE MaintenanceRequests SET Status = @RStat WHERE RequestID = @RID";

                                cmd.CommandText = sqlUpdateReq;
                                cmd.Parameters.AddWithValue("@RStat", reqStatus);
                                cmd.Parameters.AddWithValue("@RID", requestId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Lỗi UpdateWO: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        // ==========================================================
        // 5. Lấy danh sách WO để nghiệm thu
        // ==========================================================
        public List<WorkOrderViewModel> GetWorkOrdersForAcceptance()
        {
            List<WorkOrderViewModel> list = new List<WorkOrderViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT wo.WorkOrderID, wo.DeviceCode, d.DeviceName, u.FullName AS TenKTV, stt.StatusName
                               FROM WorkOrders wo
                               JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                               JOIN Users u ON wo.TechnicianID = u.UserID
                               JOIN WorkOrderStatus stt ON wo.StatusID = stt.StatusID
                               WHERE wo.StatusID != 3 
                               ORDER BY wo.WorkOrderID DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new WorkOrderViewModel
                        {
                            WorkOrderID = Convert.ToInt32(reader["WorkOrderID"]),
                            TenThietBi = reader["DeviceName"].ToString(),
                            MoTaLoi = $"WO-{reader["WorkOrderID"]:D4} | {reader["DeviceName"]}",
                            MucUuTien = reader["TenKTV"].ToString(),
                            TrangThai = reader["StatusName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // ==========================================================
        // 6. LƯU NGHIỆM THU
        // ==========================================================
        public bool SaveAcceptance(int workOrderId, List<MaterialViewModel> materials, bool isCloseTicket,
                                   decimal laborCost, decimal transportCost, decimal otherCost, string otherDesc)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // Xóa chi tiết cũ
                    cmd.CommandText = "DELETE FROM WorkOrderDetails WHERE WorkOrderID = @WOID";
                    cmd.Parameters.AddWithValue("@WOID", workOrderId);
                    cmd.ExecuteNonQuery();

                    // Thêm chi tiết mới
                    if (materials != null && materials.Count > 0)
                    {
                        cmd.Parameters.Clear();
                        StringBuilder sb = new StringBuilder();
                        sb.Append("INSERT INTO WorkOrderDetails (WorkOrderID, MaterialID, QuantityUsed, UnitPrice) VALUES ");

                        List<string> rows = new List<string>();
                        for (int i = 0; i < materials.Count; i++)
                        {
                            rows.Add($"(@wo_{i}, @mat_{i}, @qty_{i}, @price_{i})");
                            cmd.Parameters.AddWithValue($"@wo_{i}", workOrderId);
                            cmd.Parameters.AddWithValue($"@mat_{i}", materials[i].MaterialID);
                            cmd.Parameters.AddWithValue($"@qty_{i}", materials[i].SoLuong);
                            cmd.Parameters.AddWithValue($"@price_{i}", materials[i].DonGia);
                        }
                        sb.Append(string.Join(",", rows));
                        cmd.CommandText = sb.ToString();
                        cmd.ExecuteNonQuery();
                    }

                    // Cập nhật chi phí tổng
                    cmd.Parameters.Clear();
                    string sqlUpdate = @"UPDATE WorkOrders 
                                         SET StatusID = @Stat, 
                                             EndDate = CASE WHEN @Stat = 3 THEN NOW() ELSE EndDate END,
                                             LaborCost = @Labor,
                                             TransportCost = @Trans,
                                             OtherCost = @Other,
                                             OtherCostDescription = @Desc
                                         WHERE WorkOrderID = @WOID";

                    cmd.CommandText = sqlUpdate;
                    cmd.Parameters.AddWithValue("@Stat", isCloseTicket ? 3 : 2);
                    cmd.Parameters.AddWithValue("@Labor", laborCost);
                    cmd.Parameters.AddWithValue("@Trans", transportCost);
                    cmd.Parameters.AddWithValue("@Other", otherCost);
                    cmd.Parameters.AddWithValue("@Desc", otherDesc ?? "");
                    cmd.Parameters.AddWithValue("@WOID", workOrderId);

                    cmd.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Lỗi SaveAcceptance: " + ex.Message);
                    return false;
                }
            }
        }

        // ==========================================================
        // 7. Lấy chi tiết vật tư
        // ==========================================================
        public List<MaterialViewModel> GetWorkOrderDetails(int workOrderId)
        {
            List<MaterialViewModel> list = new List<MaterialViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT wod.MaterialID, m.MaterialName, u.UnitName, 
                                      wod.QuantityUsed, wod.UnitPrice
                               FROM WorkOrderDetails wod
                               JOIN Materials m ON wod.MaterialID = m.MaterialID
                               JOIN Units u ON m.UnitID = u.UnitID
                               WHERE wod.WorkOrderID = @WOID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WOID", workOrderId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaterialViewModel
                        {
                            MaterialID = Convert.ToInt32(reader["MaterialID"]),
                            TenVatTu = reader["MaterialName"].ToString(),
                            DonVi = reader["UnitName"].ToString(),
                            SoLuong = Convert.ToInt32(reader["QuantityUsed"]),
                            DonGia = Convert.ToDecimal(reader["UnitPrice"])
                        });
                    }
                }
            }
            return list;
        }

        // ==========================================================
        // 8. Lấy chi phí phụ
        // ==========================================================
        public WorkOrderViewModel GetWorkOrderCosts(int workOrderId)
        {
            WorkOrderViewModel result = null;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT LaborCost, TransportCost, OtherCost, OtherCostDescription FROM WorkOrders WHERE WorkOrderID = @WOID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WOID", workOrderId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new WorkOrderViewModel
                        {
                            LaborCost = reader["LaborCost"] != DBNull.Value ? Convert.ToDecimal(reader["LaborCost"]) : 0,
                            TransportCost = reader["TransportCost"] != DBNull.Value ? Convert.ToDecimal(reader["TransportCost"]) : 0,
                            OtherCost = reader["OtherCost"] != DBNull.Value ? Convert.ToDecimal(reader["OtherCost"]) : 0,
                            OtherCostDescription = reader["OtherCostDescription"] != DBNull.Value ? reader["OtherCostDescription"].ToString() : ""
                        };
                    }
                }
            }
            return result;
        }

        // ==========================================================
        // 9. [ĐÃ CÓ TRỞ LẠI] Xóa Phiếu (Dành cho KTV/Admin)
        // ==========================================================
        public bool DeleteWorkOrder(int workOrderId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Chỉ xóa được nếu phiếu đang ở trạng thái cho phép (ví dụ: Hủy - 5)
                // Hoặc xóa thẳng nếu không cần check
                string sql = "DELETE FROM WorkOrders WHERE WorkOrderID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", workOrderId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // ==========================================================
        // 10. [QUAN TRỌNG] Lấy danh sách Phân xưởng từ bảng LOCATIONS
        // ==========================================================
        public List<string> GetDanhSachPhanXuong()
        {
            List<string> list = new List<string>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Lấy LocationName từ bảng Locations
                    string sql = "SELECT LocationName FROM Locations ORDER BY LocationName";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(reader["LocationName"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Báo lỗi nếu không lấy được danh sách phân xưởng
                    MessageBox.Show("Lỗi lấy danh sách Phân xưởng: " + ex.Message);
                }
            }
            return list;
        }

        // ==========================================================
        // 11. [QUAN TRỌNG] Lấy Dữ liệu Báo Cáo (ĐÃ THÊM BÁO LỖI)
        // ==========================================================
        public List<ChiPhiDTO> GetReportData(int? month = null, int? year = null)
        {
            List<ChiPhiDTO> list = new List<ChiPhiDTO>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    StringBuilder queryBuilder = new StringBuilder();

                    // CÂU SQL CHUẨN:
                    // 1. JOIN Devices (qua DeviceCode)
                    // 2. JOIN Locations (qua d.LocationID)

                    queryBuilder.Append(@"
                        SELECT * FROM (
                            -- 1. VẬT TƯ
                            SELECT 
                                CONCAT('WO-', wo.WorkOrderID) AS MaPhieu, 
                                'Vật tư' AS LoaiChiPhi, 
                                m.MaterialName AS NoiDung, 
                                COALESCE(wo.EndDate, wo.StartDate) AS Ngay, 
                                (wod.QuantityUsed * wod.UnitPrice) AS SoTien,
                                l.LocationName AS TenPhanXuong,  
                                wo.StatusID
                            FROM WorkOrders wo
                            JOIN WorkOrderDetails wod ON wo.WorkOrderID = wod.WorkOrderID
                            JOIN Materials m ON wod.MaterialID = m.MaterialID
                            JOIN Devices d ON wo.DeviceCode = d.DeviceCode 
                            LEFT JOIN Locations l ON d.LocationID = l.LocationID

                            UNION ALL

                            -- 2. NHÂN CÔNG
                            SELECT 
                                CONCAT('WO-', wo.WorkOrderID) AS MaPhieu, 
                                'Nhân công' AS LoaiChiPhi, 
                                'Chi phí nhân công' AS NoiDung, 
                                COALESCE(wo.EndDate, wo.StartDate) AS Ngay, 
                                wo.LaborCost AS SoTien,
                                l.LocationName AS TenPhanXuong, 
                                wo.StatusID
                            FROM WorkOrders wo
                            JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                            LEFT JOIN Locations l ON d.LocationID = l.LocationID
                            WHERE wo.LaborCost > 0

                            UNION ALL

                            -- 3. CHI PHÍ KHÁC
                            SELECT 
                                CONCAT('WO-', wo.WorkOrderID) AS MaPhieu, 
                                'Chi phí khác' AS LoaiChiPhi, 
                                COALESCE(wo.OtherCostDescription, 'Phụ phí') AS NoiDung, 
                                COALESCE(wo.EndDate, wo.StartDate) AS Ngay, 
                                (wo.TransportCost + wo.OtherCost) AS SoTien,
                                l.LocationName AS TenPhanXuong, 
                                wo.StatusID
                            FROM WorkOrders wo
                            JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                            LEFT JOIN Locations l ON d.LocationID = l.LocationID
                            WHERE (wo.TransportCost > 0 OR wo.OtherCost > 0)
                        ) AS T
                        WHERE T.StatusID = 3 "); // CHỈ LẤY PHIẾU ĐÃ HOÀN THÀNH

                    // Thêm điều kiện lọc thời gian
                    if (month.HasValue) queryBuilder.Append(" AND MONTH(T.Ngay) = @Month ");
                    if (year.HasValue) queryBuilder.Append(" AND YEAR(T.Ngay) = @Year ");

                    queryBuilder.Append(" ORDER BY T.Ngay DESC");

                    MySqlCommand cmd = new MySqlCommand(queryBuilder.ToString(), conn);

                    if (month.HasValue) cmd.Parameters.AddWithValue("@Month", month.Value);
                    if (year.HasValue) cmd.Parameters.AddWithValue("@Year", year.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new ChiPhiDTO
                            {
                                MaPhieu = reader["MaPhieu"].ToString(),
                                LoaiChiPhi = reader["LoaiChiPhi"].ToString(),
                                NoiDung = reader["NoiDung"].ToString(),
                                Ngay = reader["Ngay"] != DBNull.Value ? Convert.ToDateTime(reader["Ngay"]) : DateTime.MinValue,
                                SoTien = reader["SoTien"] != DBNull.Value ? Convert.ToDecimal(reader["SoTien"]) : 0,
                                TenPhanXuong = reader["TenPhanXuong"] != DBNull.Value ? reader["TenPhanXuong"].ToString() : "Không xác định"
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // [QUAN TRỌNG] Hiện lỗi lên màn hình để biết tại sao không kết nối được
                    MessageBox.Show("Lỗi lấy dữ liệu báo cáo: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return list;
        }
    }
}