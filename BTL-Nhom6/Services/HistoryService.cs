using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class HistoryService
    {
        // Hàm lấy lịch sử giao dịch (có bộ lọc)
        public List<TransactionViewModel> GetHistory(string materialName, DateTime? fromDate, DateTime? toDate)
        {
            List<TransactionViewModel> list = new List<TransactionViewModel>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // Câu SQL sử dụng UNION ALL để gộp Nhập và Xuất
                // 1. Lấy NHẬP KHO
                string sqlImport = @"
                    SELECT 
                        t.TransactionID, t.TransactionDate, i.ReceiptCode AS MaPhieu, 'Nhập kho' AS Loai, 
                        t.Quantity, u.UnitName, 
                        s.SupplierName AS NguoiLienQuan, s.Address AS BoPhan, i.Note AS GhiChu, m.MaterialName
                    FROM MaterialTransactions t
                    JOIN ImportReceipts i ON t.ReceiptID = i.ReceiptID
                    JOIN Suppliers s ON i.SupplierID = s.SupplierID
                    JOIN Materials m ON t.MaterialID = m.MaterialID
                    JOIN Units u ON m.UnitID = u.UnitID
                    WHERE t.TransactionType = 'IMPORT' 
                      AND i.Status = 'Completed'"; // Chỉ lấy phiếu đã hoàn thành

                // 2. Lấy XUẤT KHO
                string sqlExport = @"
                    SELECT 
                        t.TransactionID, t.TransactionDate, e.ExportCode AS MaPhieu, 'Xuất kho' AS Loai, 
                        t.Quantity, u.UnitName,
                        us.FullName AS NguoiLienQuan, r.RoleName AS BoPhan, e.Note AS GhiChu, m.MaterialName
                    FROM MaterialTransactions t
                    JOIN ExportReceipts e ON t.ExportID = e.ExportID
                    JOIN Users us ON e.ReceiverID = us.UserID
                    LEFT JOIN Roles r ON us.RoleID = r.RoleID
                    JOIN Materials m ON t.MaterialID = m.MaterialID
                    JOIN Units u ON m.UnitID = u.UnitID
                    WHERE t.TransactionType = 'EXPORT' 
                      AND e.Status = 'Completed'"; // Chỉ lấy phiếu đã hoàn thành

                // Gộp 2 câu lệnh
                string finalSql = $"SELECT * FROM ({sqlImport} UNION ALL {sqlExport}) AS CombinedTable WHERE 1=1";

                // Thêm điều kiện lọc
                if (!string.IsNullOrEmpty(materialName))
                {
                    finalSql += " AND MaterialName LIKE @MatName"; // Lọc theo Tên vật tư (để người dùng dễ tìm hơn Mã)
                }
                if (fromDate.HasValue)
                {
                    finalSql += " AND TransactionDate >= @FromDate";
                }
                if (toDate.HasValue)
                {
                    finalSql += " AND TransactionDate <= @ToDate";
                }

                // Sắp xếp giảm dần theo ngày
                finalSql += " ORDER BY TransactionDate DESC";

                MySqlCommand cmd = new MySqlCommand(finalSql, conn);
                if (!string.IsNullOrEmpty(materialName)) cmd.Parameters.AddWithValue("@MatName", "%" + materialName + "%");
                if (fromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate", fromDate.Value.Date); // Lấy đầu ngày
                if (toDate.HasValue) cmd.Parameters.AddWithValue("@ToDate", toDate.Value.Date.AddDays(1).AddTicks(-1)); // Lấy cuối ngày

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new TransactionViewModel
                        {
                            TransactionID = Convert.ToInt32(reader["TransactionID"]),
                            NgayRaw = Convert.ToDateTime(reader["TransactionDate"]),
                            MaPhieu = reader["MaPhieu"].ToString(),
                            LoaiGiaoDich = reader["Loai"].ToString(),
                            SoLuong = Convert.ToInt32(reader["Quantity"]),
                            DonVi = reader["UnitName"].ToString(),
                            NguoiLienQuan = reader["NguoiLienQuan"].ToString(),
                            BoPhan = reader["BoPhan"] != DBNull.Value ? reader["BoPhan"].ToString() : "",
                            GhiChu = reader["GhiChu"] != DBNull.Value ? reader["GhiChu"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }
    }
}