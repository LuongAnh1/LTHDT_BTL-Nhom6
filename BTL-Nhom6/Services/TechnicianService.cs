using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq; // Để dùng hàm Where cho bộ lọc

namespace BTL_Nhom6.Services
{
    public class TechnicianService
    {
        // Hàm lấy danh sách KTV kèm thống kê (Viết lại không dùng Dapper)
        public List<TechnicianViewModel> GetTechnicianStats(string keyword = "", string statusFilter = "Tất cả")
        {
            List<TechnicianViewModel> list = new List<TechnicianViewModel>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    // SQL Logic giữ nguyên
                    string sql = @"
                        SELECT 
                            u.UserID,
                            u.FullName AS TenKTV,
                            u.Email,
                            u.IsActive,
                            
                            -- Đếm công việc đang làm (StatusID: 1=Mới, 2=Đang làm)
                            (SELECT COUNT(*) FROM WorkOrders wo 
                             WHERE wo.TechnicianID = u.UserID AND wo.StatusID IN (1, 2)) AS CongViecCho,

                            -- Lấy danh sách kỹ năng
                            (SELECT GROUP_CONCAT(s.SkillName SEPARATOR ', ') 
                             FROM TechnicianSkills ts 
                             JOIN Skills s ON ts.SkillID = s.SkillID 
                             WHERE ts.UserID = u.UserID) AS ChuyenMon

                        FROM Users u
                        WHERE u.RoleID = 3 
                        AND (@Key = '' OR u.FullName LIKE @Search OR u.Username LIKE @Search)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    // Thêm tham số
                    cmd.Parameters.AddWithValue("@Key", keyword);
                    cmd.Parameters.AddWithValue("@Search", "%" + keyword + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tech = new TechnicianViewModel();

                            // Map dữ liệu thủ công
                            tech.UserID = Convert.ToInt32(reader["UserID"]);
                            tech.TenKTV = reader["TenKTV"].ToString();
                            tech.Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";

                            // Xử lý Boolean
                            tech.IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);

                            // Xử lý số lượng (Count trả về Int64/Long)
                            tech.CongViecCho = reader["CongViecCho"] != DBNull.Value ? Convert.ToInt32(reader["CongViecCho"]) : 0;

                            // Xử lý chuỗi kỹ năng
                            tech.ChuyenMon = reader["ChuyenMon"] != DBNull.Value ? reader["ChuyenMon"].ToString() : "Chưa có chuyên môn";

                            list.Add(tech);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            // Xử lý lọc Client-side (như cũ)
            if (statusFilter != "Tất cả")
            {
                if (statusFilter == "Đang hoạt động") return list.Where(x => x.IsActive).ToList();
                if (statusFilter == "Đang bận") return list.Where(x => x.CongViecCho > 0).ToList();
                if (statusFilter == "Nghỉ phép") return list.Where(x => !x.IsActive).ToList();
            }

            return list;
        }
    }
}