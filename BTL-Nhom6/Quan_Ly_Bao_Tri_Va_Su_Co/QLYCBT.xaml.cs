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

        // Xử lý nút XÓA
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            MaintenanceRequest selectedReq = btn.DataContext as MaintenanceRequest;

            if (selectedReq != null)
            {
                // --- SỬA ĐỔI TẠI ĐÂY ---
                // Logic cũ: Chỉ cho xóa Pending hoặc Rejected
                // Logic mới: Chỉ chặn xóa "Approved" (Đang thực hiện). 
                // Còn lại (Pending, Rejected, Completed) đều cho xóa.

                if (selectedReq.Status == "Approved" || selectedReq.Status == "Đang thực hiện")
                {
                    MessageBox.Show("Không thể xóa yêu cầu đang được kỹ thuật viên xử lý!\n" +
                                    "Vui lòng Hủy phiếu công việc trước hoặc đợi hoàn thành.",
                                    "Cảnh báo",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                    return;
                }

                // Cảnh báo kỹ hơn nếu xóa mục đã hoàn thành
                string warningMsg = $"Bạn có chắc chắn muốn xóa yêu cầu: {selectedReq.DeviceName}?";
                if (selectedReq.Status == "Completed")
                {
                    warningMsg += "\n\nCẢNH BÁO: Yêu cầu này ĐÃ HOÀN THÀNH. " +
                                  "Việc xóa sẽ làm mất vĩnh viễn lịch sử bảo trì và phiếu công việc liên quan!";
                }

                var result = MessageBox.Show(warningMsg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Gọi Service xóa (Hàm này phải xử lý Transaction xóa WorkOrder trước)
                    if (_service.DeleteRequest(selectedReq.RequestID))
                    {
                        LoadData();
                        MessageBox.Show("Đã xóa dữ liệu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Có lỗi xảy ra khi xóa (Có thể do ràng buộc dữ liệu).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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