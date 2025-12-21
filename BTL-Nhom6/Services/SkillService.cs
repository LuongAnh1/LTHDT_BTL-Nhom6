using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient; // SỬ DỤNG THƯ VIỆN MYSQL
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;       // SỬ DỤNG HELPER CỦA BẠN

namespace BTL_Nhom6.Services
{
    public class SkillService
    {
        // 1. Lấy danh sách tất cả kỹ năng (Cho cột trái)
        public List<Skill> GetAllSkills()
        {
            List<Skill> list = new List<Skill>();

            // Dùng DatabaseHelper.GetConnection() thay vì tự tạo connection string
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM Skills";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Skill
                        {
                            // Convert.ToInt32 an toàn hơn ép kiểu trực tiếp (int)
                            SkillID = Convert.ToInt32(reader["SkillID"]),
                            SkillName = reader["SkillName"].ToString(),
                            Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : ""
                        });
                    }
                }
            }
            return list;
        }

        // 2. Thêm kỹ năng mới
        public void AddSkill(Skill skill)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "INSERT INTO Skills (SkillName, Description) VALUES (@Name, @Desc)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", skill.SkillName);
                cmd.Parameters.AddWithValue("@Desc", skill.Description ?? ""); // Xử lý nếu null thì lưu chuỗi rỗng
                cmd.ExecuteNonQuery();
            }
        }

        // 3. Xóa kỹ năng
        public void DeleteSkill(int skillId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "DELETE FROM Skills WHERE SkillID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", skillId);
                cmd.ExecuteNonQuery();
            }
        }

        // 4. Lấy danh sách kỹ năng CỦA MỘT USER (Cho cột phải)
        public List<TechnicianSkillViewModel> GetSkillsByUser(int userId)
        {
            List<TechnicianSkillViewModel> list = new List<TechnicianSkillViewModel>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // LEFT JOIN để lấy tất cả skill và check xem user có skill đó chưa
                string sql = @"
                        SELECT s.SkillID, s.SkillName, ts.Level 
                        FROM Skills s
                        LEFT JOIN TechnicianSkills ts ON s.SkillID = ts.SkillID AND ts.UserID = @UserID";

                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new TechnicianSkillViewModel
                        {
                            SkillID = Convert.ToInt32(reader["SkillID"]),
                            SkillName = reader["SkillName"].ToString(),
                        };

                        if (reader["Level"] != DBNull.Value)
                        {
                            item.IsSelected = true;
                            item.Level = reader["Level"].ToString();
                        }
                        else
                        {
                            item.IsSelected = false;
                            item.Level = "Cơ bản";
                        }
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        // 5. Lưu (Cập nhật) kỹ năng cho User (Có Transaction)
        public bool SaveUserSkills(int userId, List<TechnicianSkillViewModel> skillList)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                // Sử dụng MySqlTransaction
                MySqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Bước 1: Xóa kỹ năng cũ
                    string deleteSql = "DELETE FROM TechnicianSkills WHERE UserID = @UserID";
                    MySqlCommand deleteCmd = new MySqlCommand(deleteSql, conn, transaction);
                    deleteCmd.Parameters.AddWithValue("@UserID", userId);
                    deleteCmd.ExecuteNonQuery();

                    // Bước 2: Thêm kỹ năng mới được chọn
                    string insertSql = "INSERT INTO TechnicianSkills (UserID, SkillID, Level) VALUES (@UserID, @SkillID, @Level)";

                    foreach (var item in skillList)
                    {
                        if (item.IsSelected)
                        {
                            MySqlCommand insertCmd = new MySqlCommand(insertSql, conn, transaction);
                            insertCmd.Parameters.AddWithValue("@UserID", userId);
                            insertCmd.Parameters.AddWithValue("@SkillID", item.SkillID);
                            insertCmd.Parameters.AddWithValue("@Level", item.Level ?? "Cơ bản");
                            insertCmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit(); // Xác nhận lưu
                    return true;
                }
                catch
                {
                    transaction.Rollback(); // Gặp lỗi thì hoàn tác
                    return false;
                }
            }
        }

        //6. Cập nhật kỹ năng
        public void UpdateSkill(Skill skill)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "UPDATE Skills SET SkillName = @Name, Description = @Desc WHERE SkillID = @ID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Name", skill.SkillName);
                cmd.Parameters.AddWithValue("@Desc", skill.Description ?? "");
                cmd.Parameters.AddWithValue("@ID", skill.SkillID);
                cmd.ExecuteNonQuery();
            }
        }
    }
}