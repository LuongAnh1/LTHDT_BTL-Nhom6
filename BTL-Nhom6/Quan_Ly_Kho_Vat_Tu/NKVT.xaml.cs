using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Để dùng KeyEventArgs
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co; // Để điều hướng nếu cần

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class NKVT : Window
    {
        private readonly ImportService _service = new ImportService();

        public NKVT()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            dgNhapKho.ItemsSource = _service.GetImportList(keyword);
        }

        // Sự kiện tìm kiếm khi nhấn Enter
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadData();
            }
        }

        // Nút Tạo phiếu nhập mới
        private void Button_TaoPhieu_Click(object sender, RoutedEventArgs e)
        {
            // Hiệu ứng mờ
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            this.Effect = blur;

            TaoPhieuNhapDialog dlg = new TaoPhieuNhapDialog();
            if (dlg.ShowDialog() == true)
            {
                // Load lại danh sách sau khi thêm thành công
                LoadData();
            }

            this.Effect = null;
        }

        // Các nút hành động trong bảng (Xem, Sửa, Xóa)
        private void Button_Action_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var item = btn.DataContext as ImportViewModel;
            string action = btn.Tag.ToString();

            switch (action)
            {
                case "APPROVE":
                    HandleApprove(item);
                    break;
                case "VIEW":
                    HandleView(item);
                    break;
                case "EDIT":
                    HandleEdit(item);
                    break;
                case "DELETE":
                    HandleDelete(item);
                    break;
            }
        }

        // --- XỬ LÝ CHI TIẾT ---

        // Hàm xử lý Duyệt
        private void HandleApprove(ImportViewModel item)
        {
            var result = MessageBox.Show($"Xác nhận duyệt nhập kho phiếu: {item.MaPhieu}?\nSố lượng sẽ được cộng vào tồn kho.",
                                         "Duyệt phiếu",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                bool success = _service.ApproveReceipt(item.ReceiptID);
                if (success)
                {
                    MessageBox.Show("Đã duyệt và nhập kho thành công!", "Thông báo");
                    LoadData(); // Load lại để thấy trạng thái chuyển sang Hoàn thành
                }
                else
                {
                    MessageBox.Show("Lỗi khi duyệt phiếu.", "Lỗi");
                }
            }
        }


        private void HandleView(ImportViewModel item)
        {
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            this.Effect = blur;

            // Gọi Constructor xem chi tiết (isViewMode = true)
            TaoPhieuNhapDialog dlg = new TaoPhieuNhapDialog(item.ReceiptID, true);
            dlg.ShowDialog();

            this.Effect = null;
        }

        private void HandleEdit(ImportViewModel item)
        {
            if (item.StatusRaw == "Completed")
            {
                MessageBox.Show("Phiếu đã hoàn thành, không thể sửa!", "Cảnh báo");
                return;
            }

            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            this.Effect = blur;

            // Gọi Constructor sửa (isViewMode = false)
            TaoPhieuNhapDialog dlg = new TaoPhieuNhapDialog(item.ReceiptID, false);
            if (dlg.ShowDialog() == true)
            {
                LoadData(); // Load lại bảng sau khi sửa
            }

            this.Effect = null;
        }

        private void HandleDelete(ImportViewModel item)
        {
            // Logic: Xóa (Thực chất là Hủy phiếu)
            if (item.StatusRaw == "Cancelled")
            {
                return; // Đã hủy rồi thì thôi
            }

            if (item.StatusRaw == "Completed")
            {
                MessageBox.Show("Phiếu đã hoàn thành nhập kho. Việc hủy phiếu này cần quyền Admin để hoàn trả tồn kho!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Tùy nghiệp vụ, thường thì Completed không cho xóa đơn giản
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn HỦY phiếu nhập: {item.MaPhieu}?\nThao tác này không thể hoàn tác.",
                                         "Xác nhận hủy",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Gọi Service xóa (Hàm DeleteImport bạn đã có trong ImportService)
                bool success = _service.DeleteImport(item.ReceiptID);

                if (success)
                {
                    MessageBox.Show("Đã hủy phiếu nhập thành công!", "Thông báo");
                    LoadData(); // Load lại bảng
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi hủy phiếu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region Navigation
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DMVT_va_DM());
        }
        private void Button_NhapKho_Click(object sender, RoutedEventArgs e) { /* Trang hiện tại */ }
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new XKVT()); 
        }
        private void Button_TheKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LSGD()); 
        }
        #endregion
    }
}