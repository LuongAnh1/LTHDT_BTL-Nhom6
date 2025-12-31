using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class ExportService
    {
        // 1. Lấy danh sách phiếu xuất kho
        public List<ExportViewModel> GetExportList(string keyword = "")
        {
            List<ExportViewModel> list = new List<ExportViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        e.ExportID, e.ExportCode, e.ExportDate, e.Status,
                        u.FullName AS ReceiverName,
                        r.RoleName AS Department, -- Tạm lấy Role làm Bộ phận
                        (SELECT COALESCE(SUM(Quantity), 0) FROM MaterialTransactions t WHERE t.ExportID = e.ExportID) AS TotalQty
                    FROM ExportReceipts e
                    LEFT JOIN Users u ON e.ReceiverID = u.UserID
                    LEFT JOIN Roles r ON u.RoleID = r.RoleID
                    WHERE (@Key = '' OR e.ExportCode LIKE @Search OR u.FullName LIKE @Search)
                    ORDER BY e.ExportDate DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Key", keyword);
                cmd.Parameters.AddWithValue("@Search", "%" + keyword + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ExportViewModel
                        {
                            ExportID = Convert.ToInt32(reader["ExportID"]),
                            MaPhieu = reader["ExportCode"].ToString(),
                            NgayXuatRaw = Convert.ToDateTime(reader["ExportDate"]),
                            StatusRaw = reader["Status"].ToString(),
                            NguoiNhan = reader["ReceiverName"] != DBNull.Value ? reader["ReceiverName"].ToString() : "N/A",
                            BoPhan = reader["Department"] != DBNull.Value ? reader["Department"].ToString() : "",
                            TongSL = Convert.ToInt32(reader["TotalQty"])
                        });
                    }
                }
            }
            return list;
        }

        // 2. Lấy danh sách Nhân viên để chọn người nhận
        public List<User> GetEmployees()
        {
            List<User> list = new List<User>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT UserID, FullName, RoleID FROM Users WHERE IsActive = TRUE";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new User
                        {
                            UserID = Convert.ToInt32(reader["UserID"]),
                            FullName = reader["FullName"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        // 3. TẠO PHIẾU XUẤT(Có kiểm tra và trừ tồn kho)
        // Hàm trả về chuỗi rỗng "" nếu thành công, hoặc thông báo lỗi nếu thất bại
        public string CreateExportReceipt(string code, int receiverId, DateTime date, string note, List<MaterialViewModel> details, int? workOrderId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // A. KIỂM TRA TỒN KHO TRƯỚC KHI LƯU
                    foreach (var item in details)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT CurrentStock, MaterialName FROM Materials WHERE MaterialID = @MatID";
                        cmd.Parameters.AddWithValue("@MatID", item.MaterialID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int currentStock = Convert.ToInt32(reader["CurrentStock"]);
                                string name = reader["MaterialName"].ToString();

                                // Nếu số lượng xuất > tồn kho -> Báo lỗi ngay
                                if (item.SoLuong > currentStock)
                                {
                                    return $"Vật tư '{name}' không đủ tồn kho!\n(Tồn: {currentStock}, Cần xuất: {item.SoLuong})";
                                }
                            }
                            else return $"Vật tư ID {item.MaterialID} không tồn tại!";
                        }
                    }

                    // B. TẠO PHIẾU XUẤT (HEADER)
                    cmd.Parameters.Clear();
                    cmd.CommandText = @"INSERT INTO ExportReceipts (ExportCode, ReceiverID, ExportDate, Status, Note) 
                                VALUES (@Code, @RecID, @Date, 'Pending', @Note)";
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@RecID", receiverId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@Note", note ?? "");
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "SELECT LAST_INSERT_ID()";
                    int exportId = Convert.ToInt32(cmd.ExecuteScalar());

                    // C. THÊM CHI TIẾT & TRỪ TỒN KHO
                    foreach (var item in details)
                    {
                        // 1. Insert Transaction (Lịch sử)
                        // Lưu ý: Export thường lấy giá vốn (UnitPrice hiện tại) để tính giá trị xuất
                        cmd.Parameters.Clear();
                        cmd.CommandText = @"INSERT INTO MaterialTransactions 
                            (MaterialID, ExportID, TransactionType, Quantity, TransactionDate, UnitPrice, WorkOrderID) 
                            VALUES (@MatID, @ExpID, 'EXPORT', @Qty, @Date, @Price, @WOID)";

                        cmd.Parameters.AddWithValue("@MatID", item.MaterialID);
                        cmd.Parameters.AddWithValue("@ExpID", exportId);
                        cmd.Parameters.AddWithValue("@Qty", item.SoLuong);
                        cmd.Parameters.AddWithValue("@Date", date);
                        cmd.Parameters.AddWithValue("@Price", item.DonGia); // Giá vốn lúc xuất

                        cmd.Parameters.AddWithValue("@WOID", workOrderId.HasValue ? workOrderId.Value : (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    return ""; // Thành công (Chuỗi rỗng)
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return "Lỗi hệ thống: " + ex.Message;
                }
            }
        }

        // 4. Lấy danh sách phiếu công việc Đang thực hiện của kỹ thuật viên
        public List<WorkOrderViewModel> GetActiveWorkOrdersByTech(int techId)
        {
            List<WorkOrderViewModel> list = new List<WorkOrderViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Chỉ lấy các phiếu Đang thực hiện (StatusID = 2) của nhân viên này
                string sql = @"SELECT wo.WorkOrderID, wo.DeviceCode, d.DeviceName
                       FROM WorkOrders wo
                       JOIN Devices d ON wo.DeviceCode = d.DeviceCode
                       WHERE wo.TechnicianID = @UID AND wo.StatusID = 2";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UID", techId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new WorkOrderViewModel
                        {
                            WorkOrderID = Convert.ToInt32(reader["WorkOrderID"]),
                            // Hiển thị dạng: WO-001 | Máy lạnh Server
                            MoTaLoi = $"WO-{reader["WorkOrderID"]:D4} | {reader["DeviceName"]}"
                        });
                    }
                }
            }
            return list;
        }

        // các hàm Lấy chi tiết, Cập nhật và Hủy phiếu

        // 1. Lấy Header (Thông tin chung)
        public ExportViewModel GetExportHeader(int exportId)
        {
            ExportViewModel result = null;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT e.ExportID, e.ExportCode, e.ExportDate, e.Status, e.ReceiverID, e.Note, u.FullName
                       FROM ExportReceipts e
                       LEFT JOIN Users u ON e.ReceiverID = u.UserID
                       WHERE e.ExportID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", exportId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new ExportViewModel
                        {
                            ExportID = Convert.ToInt32(reader["ExportID"]),
                            MaPhieu = reader["ExportCode"].ToString(),
                            NgayXuatRaw = Convert.ToDateTime(reader["ExportDate"]),
                            StatusRaw = reader["Status"].ToString(),
                            // Lưu tạm ReceiverID vào NguoiNhan để load combobox (cần ép kiểu int sau)
                            NguoiNhan = reader["ReceiverID"].ToString(),
                            // Lưu Note vào BoPhan (tạm)
                            BoPhan = reader["Note"].ToString()
                        };
                    }
                }
            }
            return result;
        }

        // 2. Lấy Chi tiết vật tư
        public List<MaterialViewModel> GetExportDetails(int exportId)
        {
            List<MaterialViewModel> list = new List<MaterialViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Lấy WorkOrderID nếu có
                string sql = @"SELECT t.MaterialID, m.MaterialName, u.UnitName, t.Quantity, t.UnitPrice, t.WorkOrderID, m.CurrentStock
                       FROM MaterialTransactions t
                       JOIN Materials m ON t.MaterialID = m.MaterialID
                       JOIN Units u ON m.UnitID = u.UnitID
                       WHERE t.ExportID = @ID AND t.TransactionType = 'EXPORT'";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", exportId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new MaterialViewModel
                        {
                            MaterialID = Convert.ToInt32(reader["MaterialID"]),
                            TenVatTu = reader["MaterialName"].ToString(),
                            DonVi = reader["UnitName"].ToString(),
                            SoLuong = Convert.ToInt32(reader["Quantity"]),
                            DonGia = Convert.ToDecimal(reader["UnitPrice"]),
                            CurrentStock = Convert.ToInt32(reader["CurrentStock"]),
                            // Lưu ý: Nếu muốn bind lại WorkOrder, cần thêm thuộc tính vào ViewModel
                        });
                    }
                }
            }
            return list;
        }

        // 3. CẬP NHẬT PHIẾU XUẤT (Logic: Hoàn kho cũ -> Trừ kho mới)
        public string UpdateExportReceipt(int exportId, int receiverId, DateTime date, string note, List<MaterialViewModel> details, int? woId)
        {
            // Logic tương tự Create nhưng có bước hoàn trả kho.
            // Để đơn giản và an toàn, ta chỉ cho phép Update thông tin Header (Người nhận, Ghi chú)
            // Nếu muốn Update cả vật tư, logic rất phức tạp (như bài Import). 
            // Ở đây mình Demo Update Header thôi nhé.

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                try
                {
                    string sql = "UPDATE ExportReceipts SET ReceiverID=@Rec, ExportDate=@Date, Note=@Note WHERE ExportID=@ID";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Rec", receiverId);
                    cmd.Parameters.AddWithValue("@Date", date);
                    cmd.Parameters.AddWithValue("@Note", note ?? "");
                    cmd.Parameters.AddWithValue("@ID", exportId);
                    cmd.ExecuteNonQuery();
                    return "";
                }
                catch (Exception ex) { return ex.Message; }
            }
        }

        // 4. HỦY PHIẾU XUẤT (Hoàn trả tồn kho)
        // Trong file ExportService.cs

        public bool CancelExportReceipt(int exportId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // BƯỚC 1: Kiểm tra trạng thái hiện tại trong DB
                    cmd.CommandText = "SELECT Status FROM ExportReceipts WHERE ExportID = @ID";
                    cmd.Parameters.AddWithValue("@ID", exportId);
                    object statusObj = cmd.ExecuteScalar();

                    if (statusObj == null) return false;
                    string currentStatus = statusObj.ToString();

                    // BƯỚC 2: Chỉ hoàn trả tồn kho nếu phiếu ĐÃ HOÀN THÀNH (Completed)
                    if (currentStatus == "Completed")
                    {
                        // A. Lấy danh sách đã xuất
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT MaterialID, Quantity FROM MaterialTransactions WHERE ExportID = @ID";
                        cmd.Parameters.AddWithValue("@ID", exportId);

                        var items = new List<dynamic>();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new
                                {
                                    MID = Convert.ToInt32(reader["MaterialID"]),
                                    Qty = Convert.ToInt32(reader["Quantity"])
                                });
                            }
                        }

                        // B. Cộng lại tồn kho
                        foreach (var item in items)
                        {
                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE Materials SET CurrentStock = CurrentStock + @Qty WHERE MaterialID = @MID";
                            cmd.Parameters.AddWithValue("@Qty", item.Qty);
                            cmd.Parameters.AddWithValue("@MID", item.MID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // BƯỚC 3: Đổi trạng thái sang Cancelled (Áp dụng cho cả Pending và Completed)
                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE ExportReceipts SET Status = 'Cancelled' WHERE ExportID = @ID";
                    cmd.Parameters.AddWithValue("@ID", exportId);
                    cmd.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return false;
                }
            }
        }

        // 5. DUYỆT PHIẾU XUẤT (Trừ tồn kho chính thức)
        public bool ApproveExport(int exportId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // 1. Lấy danh sách vật tư cần xuất trong phiếu này
                    cmd.CommandText = @"SELECT MaterialID, Quantity FROM MaterialTransactions 
                                WHERE ExportID = @ID AND TransactionType = 'EXPORT'";
                    cmd.Parameters.AddWithValue("@ID", exportId);

                    var items = new List<dynamic>();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new
                            {
                                MatID = Convert.ToInt32(reader["MaterialID"]),
                                Qty = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    }

                    // 2. Thực hiện trừ tồn kho (Lúc này mới trừ thật)
                    foreach (var item in items)
                    {
                        // Kiểm tra lại tồn kho xem còn đủ không (đề phòng lúc lập phiếu thì còn, lúc duyệt thì hết)
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT CurrentStock FROM Materials WHERE MaterialID = @MatID";
                        cmd.Parameters.AddWithValue("@MatID", item.MatID);
                        int stock = Convert.ToInt32(cmd.ExecuteScalar());

                        if (stock < item.Qty)
                        {
                            throw new Exception($"Vật tư ID {item.MatID} không đủ tồn kho để duyệt (Còn: {stock}, Cần: {item.Qty})");
                        }

                        // Trừ kho
                        cmd.CommandText = "UPDATE Materials SET CurrentStock = CurrentStock - @Qty WHERE MaterialID = @MatID";
                        cmd.Parameters.AddWithValue("@Qty", item.Qty);
                        // Tham số @MatID đã có
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Cập nhật trạng thái phiếu thành Completed
                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE ExportReceipts SET Status = 'Completed' WHERE ExportID = @ID";
                    cmd.Parameters.AddWithValue("@ID", exportId);
                    cmd.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    // Bạn có thể log lỗi hoặc throw ra để UI bắt
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        // 6. XÓA PHIẾU XUẤT (Xóa vĩnh viễn, chỉ áp dụng cho phiếu đã HỦY)
        public bool DeleteExportPermanent(int exportId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Không cần Transaction phức tạp vì phiếu Hủy đã không còn ảnh hưởng tồn kho
                // Tuy nhiên do ràng buộc khóa ngoại, phải xóa chi tiết trước
                try
                {
                    string sqlDetail = "DELETE FROM MaterialTransactions WHERE ExportID = @ID";
                    MySqlCommand cmd = new MySqlCommand(sqlDetail, conn);
                    cmd.Parameters.AddWithValue("@ID", exportId);
                    cmd.ExecuteNonQuery();

                    string sqlHeader = "DELETE FROM ExportReceipts WHERE ExportID = @ID";
                    cmd.CommandText = sqlHeader;
                    cmd.ExecuteNonQuery();

                    return true;
                }
                catch { return false; }
            }
        }
    }
}