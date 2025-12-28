using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class ImportService
    {
        // Lấy danh sách phiếu nhập (Hỗ trợ tìm kiếm theo Mã hoặc NCC)
        public List<ImportViewModel> GetImportList(string keyword = "")
        {
            List<ImportViewModel> list = new List<ImportViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Query kết hợp bảng Receipts, Suppliers và đếm tổng số lượng từ Transactions
                string sql = @"
                    SELECT 
                        r.ReceiptID, 
                        r.ReceiptCode, 
                        r.ImportDate, 
                        r.Status,
                        s.SupplierName, 
                        s.Address,
                        -- Tính tổng số lượng vật tư trong phiếu
                        (SELECT COALESCE(SUM(Quantity), 0) 
                         FROM MaterialTransactions t 
                         WHERE t.ReceiptID = r.ReceiptID) AS TotalQty
                    FROM ImportReceipts r
                    LEFT JOIN Suppliers s ON r.SupplierID = s.SupplierID
                    WHERE (@Key = '' 
                           OR r.ReceiptCode LIKE @Search 
                           OR s.SupplierName LIKE @Search)
                    ORDER BY r.ImportDate DESC";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Key", keyword);
                cmd.Parameters.AddWithValue("@Search", "%" + keyword + "%");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ImportViewModel
                        {
                            ReceiptID = Convert.ToInt32(reader["ReceiptID"]),
                            MaPhieu = reader["ReceiptCode"].ToString(),
                            NgayNhapRaw = Convert.ToDateTime(reader["ImportDate"]),
                            StatusRaw = reader["Status"].ToString(),
                            NhaCungCap = reader["SupplierName"] != DBNull.Value ? reader["SupplierName"].ToString() : "N/A",
                            DiaChi = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : "",
                            TongSL = Convert.ToInt32(reader["TotalQty"])
                        });
                    }
                }
            }
            return list;
        }

        // Hàm xóa phiếu nhập (Optional)
        public bool DeleteImport(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Xóa chi tiết trước (MaterialTransactions)
                // Lưu ý: Thực tế không nên xóa vật lý mà chỉ đổi trạng thái Cancelled
                // Ở đây mình ví dụ đổi trạng thái Hủy
                string sql = "UPDATE ImportReceipts SET Status = 'Cancelled' WHERE ReceiptID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", id);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // 1. Lấy danh sách Nhà cung cấp
        public List<Supplier> GetSuppliers()
        {
            List<Supplier> list = new List<Supplier>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT SupplierID, SupplierName, Address FROM Suppliers";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Supplier
                        {
                            SupplierID = Convert.ToInt32(reader["SupplierID"]),
                            SupplierName = reader["SupplierName"].ToString(),
                            Address = reader["Address"] != DBNull.Value ? reader["Address"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }

        // 2. LƯU PHIẾU NHẬP MỚI (Quan trọng)
        public bool CreateImportReceipt(string receiptCode, int supplierId, DateTime importDate, string note, List<MaterialViewModel> details)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // A. Insert vào bảng ImportReceipts
                    string sqlReceipt = @"INSERT INTO ImportReceipts (ReceiptCode, SupplierID, ImportDate, Status, Note) 
                                          VALUES (@Code, @SupID, @Date, 'Pending', @Note)";

                    cmd.CommandText = sqlReceipt;
                    cmd.Parameters.AddWithValue("@Code", receiptCode);
                    cmd.Parameters.AddWithValue("@SupID", supplierId);
                    cmd.Parameters.AddWithValue("@Date", importDate);
                    cmd.Parameters.AddWithValue("@Note", note ?? "");
                    cmd.ExecuteNonQuery();

                    // Lấy ID vừa tạo
                    cmd.CommandText = "SELECT LAST_INSERT_ID()";
                    int receiptId = Convert.ToInt32(cmd.ExecuteScalar());

                    // B. Insert chi tiết vào MaterialTransactions VÀ Update Tồn kho
                    if (details != null && details.Count > 0)
                    {
                        foreach (var item in details)
                        {
                            // 1. Thêm Transaction (Lịch sử nhập)
                            string sqlTrans = @"INSERT INTO MaterialTransactions 
                                                (MaterialID, ReceiptID, TransactionType, Quantity, TransactionDate) 
                                                VALUES (@MatID, @RecID, 'IMPORT', @Qty, @Date)";

                            // Clear parameters cũ để add mới trong vòng lặp
                            cmd.Parameters.Clear();
                            cmd.CommandText = sqlTrans;
                            cmd.Parameters.AddWithValue("@MatID", item.MaterialID);
                            cmd.Parameters.AddWithValue("@RecID", receiptId);
                            cmd.Parameters.AddWithValue("@Qty", item.SoLuong);
                            cmd.Parameters.AddWithValue("@Date", importDate);
                            cmd.ExecuteNonQuery();

                            // 2. Cập nhật số lượng tồn kho (CurrentStock) và Giá nhập mới nhất (UnitPrice) trong bảng Materials
                            string sqlUpdateStock = @"UPDATE Materials 
                                                      SET CurrentStock = CurrentStock + @Qty,
                                                          UnitPrice = @NewPrice 
                                                      WHERE MaterialID = @MatID";
                            cmd.CommandText = sqlUpdateStock;
                            // Tham số @MatID, @Qty đã có ở trên
                            cmd.Parameters.AddWithValue("@NewPrice", item.DonGia);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Console.WriteLine("Lỗi CreateImportReceipt: " + ex.Message);
                    return false;
                }
            }
        }

        // 3. Lấy thông tin chung của phiếu nhập (Header)
        public ImportViewModel GetImportHeader(int receiptId)
        {
            ImportViewModel result = null;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = @"SELECT r.ReceiptID, r.ReceiptCode, r.ImportDate, r.Status, r.SupplierID, r.Note, s.SupplierName
                       FROM ImportReceipts r
                       LEFT JOIN Suppliers s ON r.SupplierID = s.SupplierID
                       WHERE r.ReceiptID = @ID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", receiptId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result = new ImportViewModel
                        {
                            ReceiptID = Convert.ToInt32(reader["ReceiptID"]),
                            MaPhieu = reader["ReceiptCode"].ToString(),
                            NgayNhapRaw = Convert.ToDateTime(reader["ImportDate"]),
                            StatusRaw = reader["Status"].ToString(),
                            NhaCungCap = reader["SupplierID"].ToString(), // Lưu ID vào biến này tạm hoặc dùng biến khác
                            DiaChi = reader["Note"].ToString() // Tạm dùng biến DiaChi để lưu Ghi chú
                        };
                    }
                }
            }
            return result;
        }

        // 4. Lấy danh sách vật tư của phiếu nhập
        public List<MaterialViewModel> GetImportDetails(int receiptId)
        {
            List<MaterialViewModel> list = new List<MaterialViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // --- SỬA 1: Bổ sung m.UnitPrice vào câu lệnh SELECT ---
                // (Nếu bạn đã thêm cột UnitPrice vào bảng MaterialTransactions để lưu lịch sử giá 
                // thì đổi m.UnitPrice thành t.UnitPrice)
                string sql = @"SELECT t.MaterialID, m.MaterialName, u.UnitName, t.Quantity, t.TransactionID, m.UnitPrice
               FROM MaterialTransactions t
               JOIN Materials m ON t.MaterialID = m.MaterialID
               JOIN Units u ON m.UnitID = u.UnitID
               WHERE t.ReceiptID = @ID AND t.TransactionType = 'IMPORT'";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", receiptId);

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

                            // --- SỬA 2: Lấy dữ liệu từ DataReader thay vì gán bằng 0 ---
                            DonGia = reader["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(reader["UnitPrice"]) : 0
                        });
                    }
                }
            }
            return list;
        }

        // 5. Cập nhật phiếu nhập (Chỉ dùng cho phiếu Pending)
        // Trong file ImportService.cs

        public bool UpdateImportReceipt(int receiptId, int supplierId, DateTime importDate, string note, List<MaterialViewModel> details)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // =================================================================================
                    // BƯỚC 1: HOÀN TRẢ TỒN KHO CŨ (Trừ đi số lượng đã nhập trước đó)
                    // =================================================================================

                    // 1.1 Lấy danh sách vật tư và số lượng cũ của phiếu này
                    cmd.CommandText = "SELECT MaterialID, Quantity FROM MaterialTransactions WHERE ReceiptID = @ID";
                    cmd.Parameters.AddWithValue("@ID", receiptId);

                    var oldItems = new List<dynamic>(); // Dùng dynamic hoặc tạo class tạm
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            oldItems.Add(new
                            {
                                MatID = Convert.ToInt32(reader["MaterialID"]),
                                Qty = Convert.ToInt32(reader["Quantity"])
                            });
                        }
                    } // Đóng reader để dùng tiếp cmd

                    // 1.2 Trừ tồn kho (Revert stock)
                    foreach (var item in oldItems)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE Materials SET CurrentStock = CurrentStock - @Qty WHERE MaterialID = @MatID";
                        cmd.Parameters.AddWithValue("@Qty", item.Qty);
                        cmd.Parameters.AddWithValue("@MatID", item.MatID);
                        cmd.ExecuteNonQuery();
                    }

                    // =================================================================================
                    // BƯỚC 2: XÓA CHI TIẾT CŨ
                    // =================================================================================
                    cmd.Parameters.Clear();
                    cmd.CommandText = "DELETE FROM MaterialTransactions WHERE ReceiptID = @ID";
                    cmd.Parameters.AddWithValue("@ID", receiptId);
                    cmd.ExecuteNonQuery();

                    // =================================================================================
                    // BƯỚC 3: CẬP NHẬT THÔNG TIN PHIẾU (HEADER)
                    // =================================================================================
                    cmd.CommandText = @"UPDATE ImportReceipts 
                                SET SupplierID = @SupID, ImportDate = @Date, Note = @Note 
                                WHERE ReceiptID = @ID";
                    cmd.Parameters.AddWithValue("@SupID", supplierId);
                    cmd.Parameters.AddWithValue("@Date", importDate);
                    cmd.Parameters.AddWithValue("@Note", note ?? "");
                    // Tham số @ID đã có sẵn từ trên
                    cmd.ExecuteNonQuery();

                    // =================================================================================
                    // BƯỚC 4: THÊM CHI TIẾT MỚI & CẬP NHẬT TỒN KHO MỚI
                    // =================================================================================
                    if (details != null && details.Count > 0)
                    {
                        foreach (var item in details)
                        {
                            // 4.1 Insert Transaction mới
                            string sqlTrans = @"INSERT INTO MaterialTransactions 
                                        (MaterialID, ReceiptID, TransactionType, Quantity, TransactionDate) 
                                        VALUES (@MatID, @RecID, 'IMPORT', @Qty, @Date)";

                            cmd.Parameters.Clear();
                            cmd.CommandText = sqlTrans;
                            cmd.Parameters.AddWithValue("@MatID", item.MaterialID);
                            cmd.Parameters.AddWithValue("@RecID", receiptId);
                            cmd.Parameters.AddWithValue("@Qty", item.SoLuong);
                            cmd.Parameters.AddWithValue("@Date", importDate);
                            cmd.ExecuteNonQuery();

                            // 4.2 Cập nhật Tồn kho (Cộng thêm) và Giá mới
                            string sqlUpdateStock = @"UPDATE Materials 
                                              SET CurrentStock = CurrentStock + @Qty,
                                                  UnitPrice = @NewPrice 
                                              WHERE MaterialID = @MatID";

                            cmd.CommandText = sqlUpdateStock;
                            // Các tham số @Qty, @MatID đã có ở trên, chỉ thêm @NewPrice
                            cmd.Parameters.AddWithValue("@NewPrice", item.DonGia);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Console.WriteLine("Lỗi UpdateImportReceipt: " + ex.Message);
                    return false;
                }
            }
        }

        // 6. Phê duyệt phiếu nhập (Chuyển từ Pending sang Completed)
        public bool ApproveReceipt(int receiptId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();
                try
                {
                    MySqlCommand cmd = new MySqlCommand("", conn, trans);

                    // 1. Lấy danh sách vật tư trong phiếu này để cộng kho
                    cmd.CommandText = @"SELECT MaterialID, Quantity 
                                FROM MaterialTransactions 
                                WHERE ReceiptID = @ID AND TransactionType = 'IMPORT'";
                    cmd.Parameters.AddWithValue("@ID", receiptId);

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

                    // 2. Cập nhật tồn kho (Cộng thêm)
                    foreach (var item in items)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE Materials SET CurrentStock = CurrentStock + @Qty WHERE MaterialID = @MatID";
                        cmd.Parameters.AddWithValue("@Qty", item.Qty);
                        cmd.Parameters.AddWithValue("@MatID", item.MatID);
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Cập nhật trạng thái phiếu sang Completed
                    cmd.Parameters.Clear();
                    cmd.CommandText = "UPDATE ImportReceipts SET Status = 'Completed' WHERE ReceiptID = @ID";
                    cmd.Parameters.AddWithValue("@ID", receiptId);
                    cmd.ExecuteNonQuery();

                    trans.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Console.WriteLine("Lỗi ApproveReceipt: " + ex.Message);
                    return false;
                }
            }
        }
    }
}