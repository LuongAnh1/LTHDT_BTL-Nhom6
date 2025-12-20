using BTL_Nhom6.Helper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
// Cách dùng
// Gọi trực tiếp luôn, không cần new
// VD: bool kq = BusinessLogicHelper.ThemVatTuVaTruKho(maPhieu, maVatTu, soLuong, ghiChu);
// Không cần usding vì là static class
namespace BTL_Nhom6.Helper
{
    internal static class BusinessLogicHelper
    {
        // Trigger "Trừ kho khi dùng vật tư"
        public static bool ThemVatTuVaTruKho(int workOrderId, int materialId, int quantity, string note)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Bắt đầu giao dịch (Transaction)
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Lệnh 1: Thêm vào bảng chi tiết (WorkOrderDetails)
                        string sqlInsert = @"INSERT INTO WorkOrderDetails (WorkOrderID, MaterialID, QuantityUsed, Note) 
                                     VALUES (@woId, @matId, @qty, @note)";
                        using (var cmd1 = new MySqlCommand(sqlInsert, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@woId", workOrderId);
                            cmd1.Parameters.AddWithValue("@matId", materialId);
                            cmd1.Parameters.AddWithValue("@qty", quantity);
                            cmd1.Parameters.AddWithValue("@note", note);
                            cmd1.ExecuteNonQuery();
                        }

                        // Lệnh 2: Trừ tồn kho trong bảng Materials (Thay thế Trigger)
                        string sqlUpdateStock = @"UPDATE Materials 
                                          SET CurrentStock = CurrentStock - @qty 
                                          WHERE MaterialID = @matId";
                        using (var cmd2 = new MySqlCommand(sqlUpdateStock, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@qty", quantity);
                            cmd2.Parameters.AddWithValue("@matId", materialId);
                            cmd2.ExecuteNonQuery();
                        }

                        // Nếu cả 2 lệnh ngon lành -> Lưu lại
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi -> Hoàn tác mọi thứ (Không thêm, không trừ kho)
                        transaction.Rollback();
                        MessageBox.Show("Lỗi: " + ex.Message);
                        return false;
                    }
                }
            }

        }

        //  Trigger "Hoàn thành công việc & Cập nhật lịch bảo trì"
        public static bool HoanThanhCongViec(int workOrderId, string solution)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Lấy thông tin ScheduleID của phiếu này trước
                        int? scheduleId = null;
                        string sqlGetSch = "SELECT ScheduleID FROM WorkOrders WHERE WorkOrderID = @woId";
                        using (var cmdGet = new MySqlCommand(sqlGetSch, conn, transaction))
                        {
                            cmdGet.Parameters.AddWithValue("@woId", workOrderId);
                            var result = cmdGet.ExecuteScalar();
                            if (result != DBNull.Value && result != null) scheduleId = Convert.ToInt32(result);
                        }

                        // Lệnh 1: Cập nhật trạng thái phiếu việc (WorkOrders)
                        string sqlUpdateWO = @"UPDATE WorkOrders 
                                       SET StatusID = (SELECT StatusID FROM WorkOrderStatus WHERE StatusName = 'Completed'), 
                                           EndDate = NOW(), 
                                           Solution = @sol 
                                       WHERE WorkOrderID = @woId";
                        using (var cmd1 = new MySqlCommand(sqlUpdateWO, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@sol", solution);
                            cmd1.Parameters.AddWithValue("@woId", workOrderId);
                            cmd1.ExecuteNonQuery();
                        }

                        // Lệnh 2: Tính ngày bảo trì tiếp theo (Thay thế Trigger)
                        if (scheduleId != null)
                        {
                            string sqlUpdateSchedule = @"UPDATE MaintenanceSchedules 
                                                 SET LastMaintenanceDate = CURDATE(), 
                                                     NextMaintenanceDate = DATE_ADD(CURDATE(), INTERVAL FrequencyDays DAY) 
                                                 WHERE ScheduleID = @schId";
                            using (var cmd2 = new MySqlCommand(sqlUpdateSchedule, conn, transaction))
                            {
                                cmd2.Parameters.AddWithValue("@schId", scheduleId);
                                cmd2.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi: " + ex.Message);
                        return false;
                    }
                }
            }
        }

        // Trigger "Tự động Duyệt Yêu cầu (Request)"
        public static bool TaoPhieuViecMoi(string deviceCode, int technicianId, int requestId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Lệnh 1: Tạo WorkOrder
                        string sqlInsertWO = @"INSERT INTO WorkOrders (DeviceCode, RequestID, TechnicianID, StatusID, StartDate)
                                       VALUES (@code, @reqId, @techId, 1, NOW())"; // Giả sử StatusID 1 là 'New'
                        using (var cmd1 = new MySqlCommand(sqlInsertWO, conn, transaction))
                        {
                            cmd1.Parameters.AddWithValue("@code", deviceCode);
                            cmd1.Parameters.AddWithValue("@reqId", requestId);
                            cmd1.Parameters.AddWithValue("@techId", technicianId);
                            cmd1.ExecuteNonQuery();
                        }

                        // Lệnh 2: Cập nhật trạng thái Request thành Approved (Thay thế Trigger)
                        string sqlUpdateReq = "UPDATE MaintenanceRequests SET Status = 'Approved' WHERE RequestID = @reqId";
                        using (var cmd2 = new MySqlCommand(sqlUpdateReq, conn, transaction))
                        {
                            cmd2.Parameters.AddWithValue("@reqId", requestId);
                            cmd2.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi: " + ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}
