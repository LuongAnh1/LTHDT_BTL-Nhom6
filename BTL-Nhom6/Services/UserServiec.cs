using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using System.Data;

namespace BTL_Nhom6.Services
{
    public class UserService
    {
        // 1. Lấy danh sách Roles
        // Đã lấy ở RoleService.cs

        // 2. Lấy danh sách Users
        // Cập nhật hàm GetAllUsers để lấy thêm IsActive và hỗ trợ Tìm kiếm
        public List<User> GetAllUsers(string keyword = "", int roleID = 0)
        {
            List<User> list = new List<User>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Thêm điều kiện: (@rid = 0 OR u.RoleID = @rid)
                    // Nghĩa là nếu truyền vào 0 thì bỏ qua điều kiện này, nếu khác 0 thì lọc chính xác
                    string sql = @"SELECT u.*, r.RoleName 
                           FROM Users u 
                           JOIN Roles r ON u.RoleID = r.RoleID
                           WHERE (@key = '' OR u.FullName LIKE @search OR u.Username LIKE @search)
                           AND (@rid = 0 OR u.RoleID = @rid)
                           ORDER BY u.UserID DESC";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@key", keyword);
                    cmd.Parameters.AddWithValue("@search", "%" + keyword + "%");
                    cmd.Parameters.AddWithValue("@rid", roleID); // Tham số mới

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new User()
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                Username = reader["Username"].ToString(),
                                // PasswordHash không cần hiển thị lên grid thì có thể bỏ qua hoặc để trống để bảo mật
                                PasswordHash = "",
                                FullName = reader["FullName"] != DBNull.Value ? reader["FullName"].ToString() : "",
                                Phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "",
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                                RoleID = Convert.ToInt32(reader["RoleID"]),
                                RoleName = reader["RoleName"].ToString(),
                                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi: " + ex.Message);
                }
            }
            return list;
        }
        // 3. Thêm User mới (Cập nhật thêm IsActive)
        public bool AddUser(User u)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // Thêm cột IsActive vào câu lệnh INSERT
                    string sql = @"INSERT INTO Users (Username, PasswordHash, FullName, Phone, Email, RoleID, IsActive) 
                           VALUES (@user, @pass, @name, @phone, @email, @rid, @active)";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@user", u.Username);
                    cmd.Parameters.AddWithValue("@pass", u.PasswordHash); // Lưu ý: Nên mã hóa MD5/BCrypt thực tế
                    cmd.Parameters.AddWithValue("@name", u.FullName);
                    cmd.Parameters.AddWithValue("@phone", u.Phone);
                    cmd.Parameters.AddWithValue("@email", u.Email);
                    cmd.Parameters.AddWithValue("@rid", u.RoleID);
                    cmd.Parameters.AddWithValue("@active", u.IsActive); // True/False

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi thêm: " + ex.Message);
                    return false;
                }
            }
        }

        // 4. Cập nhật User (Logic thông minh: Mật khẩu trống thì không đổi)
        public bool UpdateUser(User u)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string sql;

                    // Nếu PasswordHash rỗng (người dùng không nhập pass mới), ta không update cột PasswordHash
                    if (string.IsNullOrEmpty(u.PasswordHash))
                    {
                        sql = @"UPDATE Users SET FullName=@name, Phone=@phone, Email=@email, RoleID=@rid, IsActive=@active
                        WHERE UserID=@id";
                    }
                    else
                    {
                        // Nếu có nhập pass mới thì update cả pass
                        sql = @"UPDATE Users SET FullName=@name, Phone=@phone, Email=@email, RoleID=@rid, IsActive=@active, PasswordHash=@pass
                        WHERE UserID=@id";
                    }

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", u.FullName);
                    cmd.Parameters.AddWithValue("@phone", u.Phone);
                    cmd.Parameters.AddWithValue("@email", u.Email);
                    cmd.Parameters.AddWithValue("@rid", u.RoleID);
                    cmd.Parameters.AddWithValue("@active", u.IsActive);
                    cmd.Parameters.AddWithValue("@id", u.UserID);

                    if (!string.IsNullOrEmpty(u.PasswordHash))
                    {
                        cmd.Parameters.AddWithValue("@pass", u.PasswordHash);
                    }

                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi sửa: " + ex.Message);
                    return false;
                }
            }
        }

        // 5. Xóa User (Giữ nguyên logic)
        public bool DeleteUser(int userId)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string sql = "DELETE FROM Users WHERE UserID = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        //6. Hàm cập nhật trạng thái Active (Khi gạt nút)
        public bool UpdateStatus(int userId, bool isActive)
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string sql = "UPDATE Users SET IsActive = @stt WHERE UserID = @id";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@stt", isActive);
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }
    }
}