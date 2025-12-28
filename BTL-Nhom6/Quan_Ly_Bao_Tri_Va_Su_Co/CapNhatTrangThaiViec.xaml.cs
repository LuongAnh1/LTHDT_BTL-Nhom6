using System.Windows;
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using System.Linq;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class CapNhatTrangThaiViec : Window
    {
        private WorkOrderService _service = new WorkOrderService();
        private int _woId;

        public CapNhatTrangThaiViec(int woId, string maPhieu, string currentStatusName, string currentSolution)
        {
            InitializeComponent();
            _woId = woId;
            lblMaPhieu.Text = maPhieu;
            txtSolution.Text = currentSolution;

            LoadStatuses(currentStatusName);
        }

        private void LoadStatuses(string currentStatusName)
        {
            var list = _service.GetAllStatuses(); // Hàm này bạn đã có ở bước trước
            cboStatus.ItemsSource = list;

            // Tự động chọn trạng thái hiện tại
            var selected = list.FirstOrDefault(s => s.StatusName == currentStatusName);
            if (selected != null)
                cboStatus.SelectedValue = selected.StatusID;
            else
                cboStatus.SelectedIndex = 0;
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            // 1. Hiển thị hộp thoại xác nhận
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn hủy bỏ thao tác này không?\nDữ liệu đang nhập sẽ không được lưu.",
                                                      "Xác nhận hủy",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            // 2. Kiểm tra kết quả người dùng chọn
            if (result == MessageBoxResult.Yes)
            {
                // Nếu chọn Yes -> Đóng form
                this.DialogResult = false;
                this.Close();
            }
            // Nếu chọn No -> Không làm gì cả, giữ nguyên màn hình
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (cboStatus.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn trạng thái!");
                return;
            }

            int newStatusId = (int)cboStatus.SelectedValue;
            string solution = txtSolution.Text.Trim();

            if (_service.UpdateWorkOrder(_woId, newStatusId, solution))
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}