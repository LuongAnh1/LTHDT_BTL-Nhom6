using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Services
{
    public class RequestImagesService
    {
        // Lấy danh sách ảnh theo ID yêu cầu
        public List<RequestImage> GetImagesByRequestId(int requestId)
        {
            List<RequestImage> list = new List<RequestImage>();
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string sql = "SELECT * FROM RequestImages WHERE RequestID = @RequestID";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@RequestID", requestId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new RequestImage
                        {
                            ImageID = Convert.ToInt32(reader["ImageID"]),
                            RequestID = Convert.ToInt32(reader["RequestID"]),
                            ImageUrl = reader["ImageUrl"].ToString(),
                            UploadedAt = Convert.ToDateTime(reader["UploadedAt"])
                        });
                    }
                }
            }
            return list;
        }
    }
}