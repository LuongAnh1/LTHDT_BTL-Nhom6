using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using System.Windows.Controls;
using System.Windows.Media.Effects; // <--- QUAN TRỌNG: Thêm cái này để dùng BlurEffect

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class LKH_va_DP : Window
    {
        // Khởi tạo các Service
        private readonly MaintenanceRequestService _reqService = new MaintenanceRequestService();
        private readonly MaintenanceScheduleService _schService = new MaintenanceScheduleService();

        public LKH_va_DP()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // 1. Load bảng Sự cố: Lấy trạng thái 'Pending' hoặc chưa phân công
            // (Giả sử hàm GetRequests hỗ trợ lọc, hoặc bạn lấy tất rồi Filter)
            dgSuCo.ItemsSource = _reqService.GetRequests("Đang chờ xử lý", "");

            // 2. Load bảng Định kỳ: Lấy lịch trong 30 ngày tới
            dgDinhKy.ItemsSource = _schService.GetDueSchedules(30);
        }

        // ==========================================================
        // SỰ KIỆN 1: Bấm nút "Phân công" (Bảng trên - Sự cố)
        // ==========================================================
        private void BtnPhanCongSuCo_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MaintenanceRequest req = btn.DataContext as MaintenanceRequest;

            if (req != null)
            {
                // 1. Tạo hiệu ứng mờ
                BlurEffect blurObj = new BlurEffect();
                blurObj.Radius = 15;
                this.Effect = blurObj;

                // 2. Mở Form AssignTaskDialog
                AssignTaskDialog dialog = new AssignTaskDialog(
                    req.DeviceCode,
                    req.DeviceName,
                    $"SỰ CỐ: {req.ProblemDescription}", // Truyền mô tả lỗi
                    reqId: req.RequestID // Gắn ID Request
                );

                bool? result = dialog.ShowDialog();

                // 3. Gỡ bỏ hiệu ứng mờ ngay sau khi form đóng
                this.Effect = null;

                // 4. Nếu lưu thành công thì load lại dữ liệu
                if (result == true)
                {
                    LoadData();
                }
            }
        }

        // ==========================================================
        // SỰ KIỆN 2: Bấm nút "Thực hiện" (Bảng dưới - Định kỳ)
        // ==========================================================
        private void BtnThucHienDinhKy_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MaintenanceSchedule sch = btn.DataContext as MaintenanceSchedule;

            if (sch != null)
            {
                // 1. Tạo hiệu ứng mờ
                BlurEffect blurObj = new BlurEffect();
                blurObj.Radius = 15;
                this.Effect = blurObj;

                // 2. Mở Form AssignTaskDialog
                AssignTaskDialog dialog = new AssignTaskDialog(
                    sch.DeviceCode,
                    sch.DeviceName,
                    $"BẢO TRÌ ĐỊNH KỲ: {sch.TaskName}", // Truyền tên công việc
                    schId: sch.ScheduleID // Gắn ID Schedule
                );

                bool? result = dialog.ShowDialog();

                // 3. Gỡ bỏ hiệu ứng mờ
                this.Effect = null;

                // 4. Load lại dữ liệu nếu thành công
                if (result == true)
                {
                    LoadData();
                }
            }
        }

        #region CHUYỂN TRANG (NAVIGATION)

        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLYCBT());
        }

        private void Button_CapNhat_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new CNPCV());
        }

        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new KKVT_va_NT());
        }

        #endregion
    }
}