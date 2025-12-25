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
        private MaintenanceRequest _request; // Lưu đối tượng đang sửa
        private MaintenanceRequestService _service = new MaintenanceRequestService();
        private RequestImagesService _imgService = new RequestImagesService();

        // Constructor nhận vào đối tượng MaintenanceRequest từ DataGrid
        public CapNhatYeuCau(MaintenanceRequest req)
        {
            InitializeComponent();
            _request = req;
            LoadDataToForm();
            LoadImages(); // 2. Gọi hàm load ảnh
        }

        private void LoadDataToForm()
        {
            if (_request == null) return;

            // 1. Điền dữ liệu Readonly
            txtThietBi.Text = $"{_request.DeviceName} ({_request.DeviceCode})";
            txtNguoiYeuCau.Text = _request.NguoiYeuCau;
            txtMoTa.Text = _request.MoTaLoi;

            // 2. Select Mức ưu tiên
            foreach (ComboBoxItem item in cboMucUuTien.Items)
            {
                if (item.Tag.ToString() == _request.Priority)
                {
                    cboMucUuTien.SelectedItem = item;
                    break;
                }
            }

            // 3. Select Trạng thái
            foreach (ComboBoxItem item in cboTrangThai.Items)
            {
                if (item.Tag.ToString() == _request.Status)
                {
                    cboTrangThai.SelectedItem = item;
                    break;
                }
            }
        }

        // 3. Hàm lấy ảnh từ CSDL và hiển thị
        private void LoadImages()
        {
            if (_request == null) return;

            // Lấy danh sách ảnh từ Service
            List<RequestImage> images = _imgService.GetImagesByRequestId(_request.RequestID);

            if (images != null && images.Count > 0)
            {
                icImages.ItemsSource = images;
                lblNoImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                icImages.ItemsSource = null;
                lblNoImage.Visibility = Visibility.Visible; // Hiện thông báo nếu không có ảnh
            }
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Cập nhật lại object _request từ UI
            _request.ProblemDescription = txtMoTa.Text;

            if (cboMucUuTien.SelectedItem is ComboBoxItem itemPriority)
                _request.Priority = itemPriority.Tag.ToString();

            if (cboTrangThai.SelectedItem is ComboBoxItem itemStatus)
                _request.Status = itemStatus.Tag.ToString();

            // Gọi Service update
            bool result = _service.UpdateRequest(_request);

            if (result)
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Báo cho form cha biết để reload
                this.Close();
            }
            else
            {
                MessageBox.Show("Lỗi khi cập nhật!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // Xử lý khi bấm vào ảnh nhỏ
        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // 1. Lấy đối tượng Border (hoặc Image) bị click
            var border = sender as System.Windows.Controls.Border;

            // 2. Lấy dữ liệu (RequestImage) gắn với nó
            var imageObj = border.DataContext as RequestImage;

            if (imageObj != null && !string.IsNullOrEmpty(imageObj.ImageUrl))
            {
                // 3. Mở form xem ảnh to
                XemAnhChiTiet viewForm = new XemAnhChiTiet(imageObj.ImageUrl);
                viewForm.ShowDialog(); // ShowDialog để user phải đóng ảnh mới thao tác tiếp
            }
        }
    }
}