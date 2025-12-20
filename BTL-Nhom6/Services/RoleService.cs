using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using System.Data;

namespace BTL_Nhom6.Services
{
    public class RoleService
    {
        // Lấy danh sách Roles để đổ vào ComboBox
        public List<Role> GetAllRoles()
        {
            List<Role> list = new List<Role>();

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT RoleID, RoleName FROM Roles";
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Role()
                            {
                                RoleID = Convert.ToInt32(reader["RoleID"]),
                                RoleName = reader["RoleName"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi lấy Role: " + ex.Message);
                }
            }
            return list;
        }
    }
}