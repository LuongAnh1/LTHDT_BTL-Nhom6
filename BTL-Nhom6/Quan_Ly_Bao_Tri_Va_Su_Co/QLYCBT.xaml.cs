using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;    // Import Models (MaintenanceRequest)
using BTL_Nhom6.Services;  // Import Services (MaintenanceRequestService)
using System.Windows.Media.Effects;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class QLYCBT : Window
    {
        // Khởi tạo Service
        private readonly MaintenanceRequestService _service = new MaintenanceRequestService();

        public QLYCBT()
        {
            InitializeComponent();
            LoadData(); // Load dữ liệu khi mở form
        }

        // Hàm tải dữ liệu từ Database
        private void LoadData()
        {
            // Nếu DataGrid chưa được khởi tạo (khi chạy InitializeComponent), thì dừng lại.
            if (dgYeuCauBaoTri == null) return;

            try
            {
                // 1. Lấy giá trị lọc từ ComboBox
                string statusFilter = "Trạng thái";
                if (cboFilter != null && cboFilter.SelectedItem is ComboBoxItem item)
                {
                    statusFilter = item.Content.ToString();
                }

                // 2. Lấy từ khóa tìm kiếm
                string keyword = txtSearch != null ? txtSearch.Text : "";

                // 3. Gọi Service để lấy dữ liệu thật từ SQL
                List<MaintenanceRequest> listRequests = _service.GetRequests(statusFilter, keyword);

                // 4. Đổ dữ liệu vào DataGrid
                dgYeuCauBaoTri.ItemsSource = listRequests;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Sự kiện khi thay đổi bộ lọc (ComboBox)
        private void cboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadData();
        }

        // Sự kiện khi gõ chữ vào ô tìm kiếm
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadData();
        }

        // Sự kiện click nút Tạo yêu cầu mới
        private void Button_TaoMoi_Click(object sender, RoutedEventArgs e)
        {
            // 1. Tạo hiệu ứng mờ cho cửa sổ hiện tại (QLYCBT)
            BlurEffect blurObj = new BlurEffect();
            blurObj.Radius = 15; // Độ mờ (càng cao càng mờ)
            this.Effect = blurObj;

            // 2. Khởi tạo form con
            TaoYeuCauMoi form = new TaoYeuCauMoi();

            // 3. Hiện form con (Code sẽ dừng ở dòng này cho đến khi form con đóng lại)
            bool? result = form.ShowDialog();

            // 4. Gỡ bỏ hiệu ứng mờ ngay sau khi form con đóng
            this.Effect = null;

            // 5. Kiểm tra kết quả trả về
            // Nếu form con trả về DialogResult = true (Tức là đã lưu thành công)
            if (result == true)
            {
                LoadData(); // Load lại dữ liệu trên DataGrid

                // (Tùy chọn) Hiện thông báo nếu bên form con chưa hiện
                // MessageBox.Show("Đã cập nhật danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Xử lý nút SỬA / DUYỆT
        private void Button_Edit_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dòng hiện tại
            Button btn = sender as Button;
            MaintenanceRequest selectedReq = btn.DataContext as MaintenanceRequest;

            if (selectedReq != null)
            {
                // Hiệu ứng làm mờ
                System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect();
                blurObj.Radius = 15;
                this.Effect = blurObj;

                // Mở form cập nhật và truyền object vào
                CapNhatYeuCau form = new CapNhatYeuCau(selectedReq);
                bool? result = form.ShowDialog();

                // Gỡ hiệu ứng mờ
                this.Effect = null;

                // Nếu lưu thành công thì load lại data
                if (result == true)
                {
                    LoadData();
                }
            }
        }

        // Xử lý nút XÓA
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MaintenanceRequest selectedReq = btn.DataContext as MaintenanceRequest;

            if (selectedReq != null)
            {
                // Chỉ cho xóa nếu trạng thái là Pending
                if (selectedReq.Status != "Pending")
                {
                    MessageBox.Show("Chỉ có thể xóa yêu cầu đang chờ xử lý!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc muốn xóa yêu cầu của thiết bị {selectedReq.MaThietBi}?",
                    "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (_service.DeleteRequest(selectedReq.RequestID))
                    {
                        LoadData();
                        MessageBox.Show("Đã xóa thành công.");
                    }
                    else
                    {
                        MessageBox.Show("Lỗi khi xóa.");
                    }
                }
            }
        }

        #region CHUYỂN TRANG (NAVIGATION) - GIỮ NGUYÊN CODE CŨ

        // 1. Quản lý Yêu cầu (Trang hiện tại)
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e)
        {
            // Đang ở trang này rồi nên không làm gì hoặc reload
            LoadData();
        }

        // 2. Điều phối công việc
        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LKH_va_DP());
        }

        // 3. Cập nhật phiếu công việc
        private void Button_CapNhat_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new CNPCV());
        }

        // 4. Kê khai vật tư & Nghiệm thu
        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new KKVT_va_NT());
        }

        #endregion
    }
}