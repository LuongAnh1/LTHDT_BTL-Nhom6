using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32; // Để dùng OpenFileDialog
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using BTL_Nhom6.Helper; // Để dùng UserSession (nếu có)

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class TaoYeuCauMoi : Window
    {
        private MaintenanceRequestService _service = new MaintenanceRequestService();
        private List<string> _selectedImagePaths = new List<string>(); // Lưu đường dẫn ảnh tạm

        public TaoYeuCauMoi()
        {
            InitializeComponent();
            LoadDevices();
        }

        // Load danh sách thiết bị vào ComboBox
        private void LoadDevices()
        {
            try
            {
                var devices = _service.GetDevicesForSelection();
                cboThietBi.ItemsSource = devices;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách thiết bị: " + ex.Message);
            }
        }

        // Xử lý kéo thả cửa sổ
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        // Nút Hủy
        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Nút Chọn Ảnh
        private void BtnChonAnh_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true; // Cho phép chọn nhiều ảnh
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedImagePaths.AddRange(openFileDialog.FileNames);
                lblAnhDaChon.Text = $"Đã chọn {_selectedImagePaths.Count} ảnh";
                lblAnhDaChon.Foreground = System.Windows.Media.Brushes.Green;
            }
        }

        // Nút Lưu
        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (cboThietBi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn thiết bị!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMoTa.Text))
            {
                MessageBox.Show("Vui lòng nhập mô tả lỗi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMoTa.Focus();
                return;
            }

            try
            {
                // 2. Tạo đối tượng Model
                MaintenanceRequest req = new MaintenanceRequest();
                req.DeviceCode = cboThietBi.SelectedValue.ToString();
                req.ProblemDescription = txtMoTa.Text;

                // Lấy Priority từ Tag của ComboBoxItem
                ComboBoxItem selectedPriority = (ComboBoxItem)cboMucUuTien.SelectedItem;
                req.Priority = selectedPriority.Tag.ToString();

                // Lấy User ID đang đăng nhập (Giả sử bạn có class lưu session)
                // Nếu chưa có, tạm thời điền cứng số 1
                req.RequestedBy = UserSession.CurrentUserID;

                // 3. Gọi Service lưu
                bool result = _service.CreateRequest(req, _selectedImagePaths);

                if (result)
                {
                    MessageBox.Show("Tạo yêu cầu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // Đóng form và trả về true để form cha reload data
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi lưu yêu cầu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}