using BTL_Nhom6.Helper; // Để lấy tên User hiện tại
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml;
using MySql.Data.MySqlClient;


namespace BTL_Nhom6.Services
{
    // 1. Model để hiển thị lên DataGrid
    public class LogEntry
    {
        public string Time { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
    }

    public static class LoggerService
    {
        // Đường dẫn file log: nằm trong thư mục Debug/bin
        private static string logFilePath = "system_logs.txt";

        // 2. Hàm Ghi Log (Gọi hàm này ở bất cứ đâu có sự kiện quan trọng)
        public static void WriteLog(string action)
        {
            try
            {
                // Format dòng log: Thời gian | Người dùng | Hành động
                string logLine = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss}|{UserSession.CurrentUserName ?? "Unknown"}|{action}";

                // Ghi chèn thêm vào cuối file (Append)
                File.AppendAllLines(logFilePath, new[] { logLine });
            }
            catch
            {
                // Lỗi ghi log thì bỏ qua, không làm crash app
            }
        }

        // 3. Hàm Đọc Log (Để hiển thị lên Grid)
        public static List<LogEntry> GetLogs()
        {
            var list = new List<LogEntry>();

            if (!File.Exists(logFilePath)) return list;

            try
            {
                // Đọc tất cả dòng, đảo ngược để dòng mới nhất lên đầu
                var lines = File.ReadAllLines(logFilePath).Reverse();

                foreach (var line in lines)
                {
                    var parts = line.Split('|');
                    if (parts.Length >= 3)
                    {
                        list.Add(new LogEntry
                        {
                            Time = parts[0],
                            UserName = parts[1],
                            Action = parts[2]
                        });
                    }
                }
            }
            catch { }

            return list;
        }
    }

    // 2. Service để Backup Database
    public class BackupService
    {
        // Hàm Backup ra file .sql chuẩn
        public bool BackupDatabase(string filePath)
        {
            try
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;

                        // Khởi tạo đối tượng MySqlBackup
                        using (MySqlBackup mb = new MySqlBackup(cmd))
                        {
                            conn.Open();

                            // Thực hiện Export ra file theo đường dẫn
                            mb.ExportToFile(filePath);

                            conn.Close();
                        }
                    }
                }
                return true; // Thành công
            }
            catch (Exception ex)
            {
                // Bạn có thể ghi log lỗi chi tiết ở đây nếu cần
                // Console.WriteLine(ex.Message);
                return false; // Thất bại
            }
        }
    }
}