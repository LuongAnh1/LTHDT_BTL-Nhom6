using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BTL_Nhom6.Services
{
    public class TechnicianService
    {
        public List<TechnicianViewModel> GetTechnicianStats(string keyword = "", string statusFilter = "Tất cả")
        {
            List<TechnicianViewModel> list = new List<TechnicianViewModel>();

            using (var conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();

                    // --- SỬA CÂU SQL TẠI ĐÂY ---
                    // Thay vì "WHERE u.RoleID = 3", ta dùng "WHERE u.RoleID IN (3, 4, 5)"
                    // Hoặc dùng LIKE '%Kỹ thuật viên%' nếu muốn linh động hơn
                    string sql = @"
                        SELECT 
                            u.UserID,
                            u.FullName AS TenKTV,
                            u.Email,
                            u.IsActive,
                            
                            -- Đếm công việc đang làm
                            (SELECT COUNT(*) FROM WorkOrders wo 
                             WHERE wo.TechnicianID = u.UserID 
                            AND wo.StatusID NOT IN (3, 5)) AS CongViecCho,

                            -- Lấy danh sách kỹ năng
                            (SELECT GROUP_CONCAT(s.SkillName SEPARATOR ', ') 
                             FROM TechnicianSkills ts 
                             JOIN Skills s ON ts.SkillID = s.SkillID 
                             WHERE ts.UserID = u.UserID) AS ChuyenMon

                        FROM Users u
                        JOIN Roles r ON u.RoleID = r.RoleID
                        -- Lấy Role 3, 4, 5 HOẶC tên role có chữ 'Kỹ thuật viên'
                        WHERE (u.RoleID IN (3, 4, 5) OR r.RoleName LIKE '%Kỹ thuật viên%')
                        AND (@Key = '' OR u.FullName LIKE @Search OR u.Username LIKE @Search)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Key", keyword);
                    cmd.Parameters.AddWithValue("@Search", "%" + keyword + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var tech = new TechnicianViewModel();
                            tech.UserID = Convert.ToInt32(reader["UserID"]);
                            tech.TenKTV = reader["TenKTV"].ToString();
                            tech.Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";
                            tech.IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);
                            tech.CongViecCho = reader["CongViecCho"] != DBNull.Value ? Convert.ToInt32(reader["CongViecCho"]) : 0;
                            tech.ChuyenMon = reader["ChuyenMon"] != DBNull.Value ? reader["ChuyenMon"].ToString() : "Chưa cập nhật";

                            list.Add(tech);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            // Bộ lọc phía Client
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