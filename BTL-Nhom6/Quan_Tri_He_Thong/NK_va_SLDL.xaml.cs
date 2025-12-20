// Import namespace của Helper
using BTL_Nhom6.Helper;
using BTL_Nhom6.Services;
using BTL_Nhom6.UserControls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32; // Để dùng SaveFileDialog
using System;
using System.Windows;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    public partial class NK_va_SLDL : Window
    {
        private BackupService _backupService = new BackupService();

        public NK_va_SLDL()
        {
            InitializeComponent();

            // Load dữ liệu ngay khi mở form
            this.Loaded += NK_va_SLDL_Loaded;
        }

        private void NK_va_SLDL_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLogs();
            UpdateLastBackupInfo();
        }

        // --- 1. XỬ LÝ NHẬT KÝ (LOGS) ---
        private void LoadLogs()
        {
            try
            {
                // Gọi LoggerService đọc file txt
                var logs = LoggerService.GetLogs();

                // Gán vào DataGrid
                dgLogs.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải nhật ký: " + ex.Message);
            }
        }

        private void UpdateLastBackupInfo()
        {
            // Code giả lập cập nhật ngày backup gần nhất
            // Thực tế bạn có thể lưu ngày này vào AppSettings hoặc file config
            txtLastBackupDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        // --- 2. XỬ LÝ SAO LƯU (BACKUP) ---
        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // Filter chọn file .sql
            saveFileDialog.Filter = "SQL Database (*.sql)|*.sql";
            // Tên file mặc định kèm ngày giờ
            saveFileDialog.FileName = $"Backup_HeThong_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Hiển thị con trỏ xoay để báo đang xử lý
                    this.Cursor = System.Windows.Input.Cursors.Wait;

                    // Gọi Service Backup (MySqlBackup.NET)
                    bool isSuccess = _backupService.BackupDatabase(saveFileDialog.FileName);

                    // Trả lại con trỏ chuột
                    this.Cursor = System.Windows.Input.Cursors.Arrow;

                    if (isSuccess)
                    {
                        MessageBox.Show("Sao lưu dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Ghi log hệ thống
                        LoggerService.WriteLog("Thực hiện sao lưu Database (Backup)");

                        // Refresh lại bảng log để thấy dòng log vừa ghi
                        LoadLogs();

                        // Cập nhật ngày hiển thị
                        txtLastBackupDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        MessageBox.Show("Sao lưu thất bại. Vui lòng kiểm tra kết nối!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    this.Cursor = System.Windows.Input.Cursors.Arrow;
                    MessageBox.Show("Lỗi: " + ex.Message);
                }
            }
        }
        // --- Xử lý sự kiện Button ---
        // Sử dụng hàm Helper để chuyển đến form Nhập kho và xuất số liệu dữ liệu
        // Chuyển đến form Quản lý người dùng và phân quyền
        private void Button_QLND_va_PQ_Click(object sender, RoutedEventArgs e)
        {
            // Gọi hàm Helper: truyền vào (cửa sổ hiện tại, cửa sổ mới)
            NavigationHelper.Navigate(this, new QLND_va_PQ());
        }

        // Chuyển đến form Quản lý hồ sơ kỹ năng
        private void Button_QLHSKN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLHSKN());
        }
        // Chuyển đến form Thay đổi mật khẩu và thông tin cá nhân
        private void Button_TDMK_va_TTCN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDMK_va_TTCN());
        }
        // Chuyển đến form Trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Trang_Chu());
        }
        // Nút đăng xuất
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Dang_Nhap());
        }
        // --- Xử lý sự kiện Window ---
    }
}