using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class DeviceStatusWindow : Window
    {
        // Property để trả kết quả về form cha
        public DeviceStatus ResultStatus { get; private set; }

        public DeviceStatusWindow(DeviceStatus existingStatus = null)
        {
            InitializeComponent();

            if (existingStatus != null)
            {
                // CHẾ ĐỘ SỬA
                lblTitle.Text = "CẬP NHẬT TRẠNG THÁI";
                ResultStatus = existingStatus;

                // Fill dữ liệu cũ lên giao diện
                txtStatusName.Text = existingStatus.StatusName;
                txtDescription.Text = existingStatus.Description;
            }
            else
            {
                // CHẾ ĐỘ THÊM MỚI
                lblTitle.Text = "THÊM TRẠNG THÁI MỚI";
                ResultStatus = new DeviceStatus();
            }

            // Tự động focus vào ô nhập tên
            txtStatusName.Focus();
        }

        // Xử lý kéo thả cửa sổ (vì WindowStyle="None")
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        // Nút Lưu
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(txtStatusName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên trạng thái!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStatusName.Focus();
                return;
            }

            // 2. Cập nhật dữ liệu vào object ResultStatus
            ResultStatus.StatusName = txtStatusName.Text.Trim();
            ResultStatus.Description = txtDescription.Text.Trim();

            // 3. Đóng form và trả về DialogResult = true
            this.DialogResult = true;
            this.Close();
        }

        // Nút Hủy
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}