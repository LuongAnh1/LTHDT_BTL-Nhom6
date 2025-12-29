using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace BTL_Nhom6.Services
{
    public class WorkOrderService
    {
        // 1. Lấy danh sách việc của KTV theo ID
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
                    Console.WriteLine("Lỗi GetWorkOrdersByTechId: " + ex.Message);
                }
            }
            return list;
        }

        // 2. Lấy danh sách trạng thái
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

        // 3. Tạo Phiếu công việc mới
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
                    Console.WriteLine("Lỗi CreateWO: " + ex.Message);
                    return false;
                }
            }
        }

        // 4. Cập nhật trạng thái Phiếu công việc (Dùng cho form cập nhật tiến độ)
        public bool UpdateWorkOrder(int workOrderId, int newStatusId, string solution)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        MySqlCommand cmd = new MySqlCommand("", conn, trans);

                        // 1. Cập nhật trạng thái WorkOrder (Bảng con)
                        // Cập nhật ngày kết thúc nếu trạng thái là Hoàn thành (Giả sử ID 3 là Hoàn thành)
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

                        // 2. ĐỒNG BỘ TRẠNG THÁI SANG MAINTENANCE REQUEST (Bảng cha)
                        // Bước 2.1: Lấy RequestID liên quan đến WorkOrder này
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT RequestID FROM WorkOrders WHERE WorkOrderID = @WOID";
                        cmd.Parameters.AddWithValue("@WOID", workOrderId);

                        object reqIdObj = cmd.ExecuteScalar();

                        if (reqIdObj != null && reqIdObj != DBNull.Value)
                        {
                            int requestId = Convert.ToInt32(reqIdObj);
                            string reqStatus = "";

                            // Bước 2.2: Xác định trạng thái cần update cho Request
                            // Giả sử: 3 = Hoàn thành, 4 (hoặc 5) = Hủy bỏ. Bạn cần check lại bảng WorkOrderStatus của bạn.
                            if (newStatusId == 3)
                            {
                                reqStatus = "Completed"; // Đồng bộ sang bảng Request
                            }
                            else if (newStatusId == 4 || newStatusId == 5) // Ví dụ 4 là Hủy
                            {
                                // Nếu hủy phiếu làm việc, trạng thái yêu cầu quay về Chờ xử lý để phân công người khác
                                // Hoặc chuyển sang Rejected tùy nghiệp vụ của bạn
                                reqStatus = "Pending";
                            }

                            // Bước 2.3: Thực hiện Update bảng MaintenanceRequests
                            if (!string.IsNullOrEmpty(reqStatus))
                            {
                                cmd.Parameters.Clear();
                                string sqlUpdateReq = "";

                                if (reqStatus == "Completed")
                                {
                                    // Nếu hoàn thành thì cập nhật cả ngày hoàn tất
                                    sqlUpdateReq = "UPDATE MaintenanceRequests SET Status = @RStat, ActualCompletion = NOW() WHERE RequestID = @RID";
                                }
                                else
                                {
                                    sqlUpdateReq = "UPDATE MaintenanceRequests SET Status = @RStat WHERE RequestID = @RID";
                                }

                                cmd.CommandText = sqlUpdateReq;
                                cmd.Parameters.AddWithValue("@RStat", reqStatus);
                                cmd.Parameters.AddWithValue("@RID", requestId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 3. Commit Transaction
                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }
        }

        // 5. Lấy danh sách WO để nghiệm thu
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
                               WHERE wo.StatusID != 3 -- Chưa đóng phiếu
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

        // 6. LƯU NGHIỆM THU (Đã sửa đầy đủ)
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

                    // B1: Xóa vật tư cũ
                    cmd.CommandText = "DELETE FROM WorkOrderDetails WHERE WorkOrderID = @WOID";
                    cmd.Parameters.AddWithValue("@WOID", workOrderId);
                    cmd.ExecuteNonQuery();

                    // B2: Thêm danh sách vật tư mới (Bao gồm cả UnitPrice)
                    if (materials != null && materials.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("INSERT INTO WorkOrderDetails (WorkOrderID, MaterialID, QuantityUsed, UnitPrice) VALUES ");

                        List<string> rows = new List<string>();
                        for (int i = 0; i < materials.Count; i++)
                        {
                            // Thêm tham số UnitPrice vào SQL
                            rows.Add($"(@WOID, @mat{i}, @qty{i}, @price{i})");
                            cmd.Parameters.AddWithValue($"@mat{i}", materials[i].MaterialID);
                            cmd.Parameters.AddWithValue($"@qty{i}", materials[i].SoLuong);
                            cmd.Parameters.AddWithValue($"@price{i}", materials[i].DonGia);
                        }
                        sb.Append(string.Join(",", rows));

                        cmd.CommandText = sb.ToString();
                        cmd.ExecuteNonQuery();
                    }

                    // B3: Cập nhật thông tin chi phí và trạng thái WorkOrder
                    string sqlUpdate = @"UPDATE WorkOrders 
                                         SET StatusID = @Stat, 
                                             EndDate = CASE WHEN @Stat = 3 THEN NOW() ELSE EndDate END,
                                             LaborCost = @Labor,
                                             TransportCost = @Trans,
                                             OtherCost = @Other,
                                             OtherCostDescription = @Desc
                                         WHERE WorkOrderID = @WOID";

                    cmd.CommandText = sqlUpdate;
                    // Xóa tham số cũ (để tránh trùng tên với params của vật tư nếu có) và add lại params cho update
                    // Tuy nhiên trong MySql.Data, add chồng vẫn được nếu tên khác nhau.
                    // Ở đây tốt nhất là add các tham số update riêng biệt

                    cmd.Parameters.AddWithValue("@Stat", isCloseTicket ? 3 : 2); // 3: Hoàn thành, 2: Đang làm
                    cmd.Parameters.AddWithValue("@Labor", laborCost);
                    cmd.Parameters.AddWithValue("@Trans", transportCost);
                    cmd.Parameters.AddWithValue("@Other", otherCost);
                    cmd.Parameters.AddWithValue("@Desc", otherDesc ?? "");

                    cmd.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Console.WriteLine("Lỗi SaveAcceptance: " + ex.Message);
                    return false;
                }
            }
        }

        // 7. Lấy chi tiết vật tư đã kê khai của 1 phiếu
        public List<MaterialViewModel> GetExportedMaterialsForWO(int workOrderId)
        {
            List<MaterialViewModel> list = new List<MaterialViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                            SELECT 
                                t.MaterialID, 
                                m.MaterialName, 
                                u.UnitName, 
                                SUM(t.Quantity) AS SoLuongXuat, 
                                m.UnitPrice AS DonGia
                            FROM MaterialTransactions t
                            JOIN Materials m ON t.MaterialID = m.MaterialID
                            JOIN Units u ON m.UnitID = u.UnitID
                            -- JOIN THÊM BẢNG NÀY ĐỂ CHECK TRẠNG THÁI
                            JOIN ExportReceipts e ON t.ExportID = e.ExportID
            
                            WHERE t.WorkOrderID = @WOID 
                              AND t.TransactionType = 'EXPORT'
                              AND e.Status != 'Cancelled' -- QUAN TRỌNG: Loại bỏ phiếu đã Hủy
            
                            GROUP BY t.MaterialID, m.MaterialName, u.UnitName, m.UnitPrice";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@WOID", workOrderId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int slXuat = Convert.ToInt32(reader["SoLuongXuat"]);
                        list.Add(new MaterialViewModel
                        {
                            MaterialID = Convert.ToInt32(reader["MaterialID"]),
                            TenVatTu = reader["MaterialName"].ToString(),
                            DonVi = reader["UnitName"].ToString(),
                            DonGia = Convert.ToDecimal(reader["DonGia"]),

                            SoLuongXuat = slXuat,
                            SoLuong = slXuat // Mặc định SL thực tế = SL xuất (KTV sửa giảm đi nếu dùng ít hơn)
                        });
                    }
                }
            }
            return list;
        }

        // 8. Lấy thông tin chi phí phụ (Nhân công, Vận chuyển...) của 1 phiếu
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

        // 9. Xóa Phiếu công việc (Chỉ khi trạng thái là Hủy bỏ)
        public bool DeleteWorkOrder(int workOrderId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Chỉ cho phép xóa nếu trạng thái là Hủy bỏ (StatusID = 5 - Ví dụ)
                string sql = "DELETE FROM WorkOrders WHERE WorkOrderID = @ID AND StatusID = 5";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", workOrderId);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}