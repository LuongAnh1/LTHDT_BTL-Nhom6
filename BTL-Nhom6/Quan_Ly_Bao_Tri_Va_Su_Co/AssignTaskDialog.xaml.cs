using System;
using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class AssignTaskDialog : Window
    {
        // Service
        private readonly UserService _userService = new UserService();
        private readonly WorkOrderService _woService = new WorkOrderService();

        // Dữ liệu đầu vào
        private string _deviceCode;
        private int? _requestId;
        private int? _scheduleId;

        /// <summary>
        /// Constructor nhận thông tin từ Form cha
        /// </summary>
        /// <param name="deviceCode">Mã thiết bị</param>
        /// <param name="deviceName">Tên thiết bị</param>
        /// <param name="description">Mô tả lỗi hoặc tên việc</param>
        /// <param name="reqId">ID Yêu cầu (Nếu có)</param>
        /// <param name="schId">ID Lịch (Nếu có)</param>
        public AssignTaskDialog(string deviceCode, string deviceName, string description, int? reqId = null, int? schId = null)
        {
            InitializeComponent();

            // Lưu dữ liệu vào biến
            _deviceCode = deviceCode;
            _requestId = reqId;
            _scheduleId = schId;

            // Hiển thị lên giao diện
            lblDeviceCode.Text = deviceCode;
            lblDeviceName.Text = deviceName;
            lblDescription.Text = description;

            // Mặc định ngày bắt đầu là hôm nay
            dpStartDate.SelectedDate = DateTime.Now;

            LoadTechnicians();
        }

        private void LoadTechnicians()
        {
            try
            {
                // Gọi hàm lấy danh sách User với RoleID = 3 (Kỹ thuật viên)
                // Hàm này bạn đã có trong UserService: GetAllUsers(string keyword, int roleID)
                var technicians = _userService.GetAllUsers("", 3);

                cboTechnician.ItemsSource = technicians;

                // Nếu không có thợ nào
                if (technicians.Count == 0)
                {
                    MessageBox.Show("Chưa có nhân viên kỹ thuật nào trong hệ thống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách kỹ thuật viên: " + ex.Message);
            }
        }

        // Cho phép kéo thả cửa sổ (Vì WindowStyle=None)
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (cboTechnician.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn kỹ thuật viên thực hiện!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                cboTechnician.IsDropDownOpen = true;
                return;
            }

            if (dpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Vui lòng chọn ngày bắt đầu!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 2. Tạo đối tượng WorkOrder
                WorkOrder newWO = new WorkOrder
                {
                    DeviceCode = _deviceCode,
                    RequestID = _requestId,
                    ScheduleID = _scheduleId,
                    TechnicianID = (int)cboTechnician.SelectedValue,
                    StatusID = 1, // 1: Mới tạo (Mặc định) - Dựa trên bảng WorkOrderStatus của bạn
                    StartDate = dpStartDate.SelectedDate,
                    EndDate = dpEndDate.SelectedDate, // Có thể null
                    Solution = txtNote.Text.Trim()    // Dùng trường Solution để lưu ghi chú ban đầu (hoặc tạo trường Note riêng nếu DB có)
                };

                // 3. Gọi Service để lưu
                bool result = _woService.CreateWorkOrder(newWO);

                if (result)
                {
                    MessageBox.Show("Phân công công việc thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true; // Trả về true để form cha biết đường reload lại lưới
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi tạo phiếu công việc. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}