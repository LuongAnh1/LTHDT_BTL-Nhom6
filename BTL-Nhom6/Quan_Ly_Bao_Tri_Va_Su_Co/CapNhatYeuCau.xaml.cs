using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class CapNhatYeuCau : Window
    {
        private MaintenanceRequest _request;
        private MaintenanceRequestService _service = new MaintenanceRequestService();

        // [LƯU Ý] Đảm bảo bạn đã có class này, nếu chưa có hãy tạo nó (xem phần B bên dưới)
        private RequestImagesService _imgService = new RequestImagesService();

        // Constructor nhận vào đối tượng MaintenanceRequest từ DataGrid
        public CapNhatYeuCau(MaintenanceRequest req)
        {
            InitializeComponent();
            _request = req;
            LoadDataToForm();
            LoadImages();
        }

        private void LoadDataToForm()
        {
            if (_request == null) return;

            // 1. Điền dữ liệu Readonly
            txtThietBi.Text = $"{_request.DeviceName} ({_request.DeviceCode})";
            txtNguoiYeuCau.Text = _request.NguoiYeuCau;
            txtMoTa.Text = _request.MoTaLoi;

            // [LOGIC MỚI] Nếu đã duyệt hoặc hoàn thành -> Không cho sửa mô tả lỗi nữa
            if (_request.Status == "Approved" || _request.Status == "Completed")
            {
                txtMoTa.IsReadOnly = true;
                txtMoTa.ToolTip = "Không thể sửa mô tả khi yêu cầu đang thực hiện hoặc đã xong.";
                txtMoTa.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#F3F4F6"); // Màu xám nhạt
            }

            // Select Priority
            foreach (ComboBoxItem item in cboMucUuTien.Items)
            {
                if (item.Tag.ToString() == _request.Priority) { cboMucUuTien.SelectedItem = item; break; }
            }
            }

            // Select Status
            foreach (ComboBoxItem item in cboTrangThai.Items)
            {
                if (item.Tag.ToString() == _request.Status) { cboTrangThai.SelectedItem = item; break; }
            }
        }
        }

        // 3. Hàm lấy ảnh từ CSDL và hiển thị
        private void LoadImages()
        {
            if (_request == null) return;
            try
            {
                // Gọi Service lấy ảnh (Cần class Service chuẩn)
                List<RequestImage> images = _imgService.GetImagesByRequestId(_request.RequestID);

                if (images != null && images.Count > 0)
                {
                    icImages.ItemsSource = images;
                    lblNoImage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    icImages.ItemsSource = null;
                    lblNoImage.Visibility = Visibility.Visible;
                }
            }
            catch
            {
                // Nếu chưa có Service ảnh hoặc lỗi kết nối, tạm thời ẩn đi để không crash app
                lblNoImage.Text = "Lỗi tải ảnh minh chứng.";
                lblNoImage.Visibility = Visibility.Visible;
            }
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Lấy giá trị mới
            string newStatus = ((ComboBoxItem)cboTrangThai.SelectedItem).Tag.ToString();
            string newPriority = ((ComboBoxItem)cboMucUuTien.SelectedItem).Tag.ToString();

            // [LOGIC MỚI] Cảnh báo nếu chuyển sang Hoàn thành
            if (newStatus == "Completed" && _request.Status != "Completed")
            {
                var confirm = MessageBox.Show("Bạn xác nhận yêu cầu này đã được xử lý xong hoàn toàn?\n\nHệ thống sẽ ghi nhận Ngày hoàn tất là hôm nay.",
                    "Xác nhận hoàn thành", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm == MessageBoxResult.No) return;
            }

            // Cập nhật object
            _request.ProblemDescription = txtMoTa.Text;
            _request.Priority = newPriority;
            _request.Status = newStatus;

            // Gọi Service
            if (_service.UpdateRequest(_request))
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Lỗi khi cập nhật vào cơ sở dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) this.DragMove(); }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var imageObj = border.DataContext as RequestImage;

            if (imageObj != null && !string.IsNullOrEmpty(imageObj.ImageUrl))
            {
                // Đảm bảo bạn có form XemAnhChiTiet
                // XemAnhChiTiet viewForm = new XemAnhChiTiet(imageObj.ImageUrl);
                // viewForm.ShowDialog();
                MessageBox.Show($"Đang mở ảnh: {imageObj.ImageUrl}"); // Test tạm
            }
        }
    }
}