using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co; // Để điều hướng menu

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class XKVT : Window
    {
        private readonly ExportService _service = new ExportService();

        public XKVT()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            string keyword = txtSearch.Text.Trim();
            dgXuatKho.ItemsSource = _service.GetExportList(keyword);
        }

        // Sự kiện tìm kiếm
        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) LoadData();
        }

        // Xử lý các nút Xem/Sửa/Xóa trên bảng
        private void Button_Action_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var item = btn.DataContext as ExportViewModel;
            if (item == null) return;

            string action = btn.Tag.ToString(); // Nhớ thêm Tag="VIEW", "EDIT", "DELETE" vào XAML
            
            if (action == "APPROVE")
            {
                if (MessageBox.Show($"Duyệt phiếu xuất: {item.MaPhieu}?\nTồn kho sẽ bị trừ ngay lập tức.",
                                    "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (_service.ApproveExport(item.ExportID))
                    {
                        MessageBox.Show("Duyệt xuất kho thành công!", "Thông báo");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Lỗi khi duyệt (Có thể do không đủ tồn kho).", "Lỗi");
                    }
                }
            }
            if (action == "VIEW")
            {
                TaoPhieuXuatDialog dlg = new TaoPhieuXuatDialog(item.ExportID, true);
                dlg.ShowDialog();
            }
            else if (action == "EDIT")
            {
                // Chỉ cho sửa nếu chưa hoàn thành (Pending)
                if (item.StatusRaw == "Completed")
                {
                    MessageBox.Show("Phiếu đã xuất kho, chỉ có thể xem chi tiết!", "Thông báo");
                    return;
                }
                // THÊM MỚI: Chặn nếu đã Hủy
                if (item.StatusRaw == "Cancelled")
                {
                    MessageBox.Show("Phiếu này ĐÃ BỊ HỦY. Không thể chỉnh sửa!\nVui lòng tạo phiếu mới nếu cần.",
                                    "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                TaoPhieuXuatDialog dlg = new TaoPhieuXuatDialog(item.ExportID, false);
                if (dlg.ShowDialog() == true) LoadData();
            }
            else if (action == "DELETE")
            {
                // Xử lý nút Xóa (Hủy phiếu)
                HandleDelete(item);
            }
        }

        private void HandleDelete(ExportViewModel item)
        {
            // TRƯỜNG HỢP 1: Phiếu ĐÃ HỦY -> Cho phép XÓA VĨNH VIỄN (Dọn rác)
            if (item.StatusRaw == "Cancelled")
            {
                var confirm = MessageBox.Show($"Phiếu {item.MaPhieu} đã bị hủy.\nBạn có muốn XÓA VĨNH VIỄN khỏi cơ sở dữ liệu không?",
                                              "Xác nhận xóa vĩnh viễn",
                                              MessageBoxButton.YesNo,
                                              MessageBoxImage.Error); // Icon Error đỏ cảnh báo

                if (confirm == MessageBoxResult.Yes)
                {
                    if (_service.DeleteExportPermanent(item.ExportID))
                    {
                        MessageBox.Show("Đã xóa vĩnh viễn phiếu!", "Thông báo");
                        LoadData();
                    }
                    else
                    {
                        MessageBox.Show("Lỗi khi xóa dữ liệu.", "Lỗi");
                    }
                }
                return; // Kết thúc
            }

            // Trường hợp 2: Xử lý thông báo Hủy phiếu
            string msg = "";
            string title = "Xác nhận hủy";
            MessageBoxImage icon = MessageBoxImage.Question;

            if (item.StatusRaw == "Pending")
            {
                // Thông báo cho phiếu Chờ duyệt
                msg = $"Bạn có muốn HỦY phiếu xuất nháp: {item.MaPhieu}?\n" +
                      "Dữ liệu sẽ chuyển sang trạng thái 'Đã hủy'.";
            }
            else if (item.StatusRaw == "Completed")
            {
                // Thông báo cho phiếu Đã hoàn thành (Cảnh báo quan trọng)
                msg = $"Phiếu {item.MaPhieu} ĐÃ XUẤT KHO.\n" +
                      "Việc hủy phiếu này sẽ TỰ ĐỘNG CỘNG LẠI số lượng vào tồn kho.\n" +
                      "Bạn có chắc chắn muốn tiếp tục?";
                icon = MessageBoxImage.Warning;
            }

            if (MessageBox.Show(msg, title, MessageBoxButton.YesNo, icon) == MessageBoxResult.Yes)
            {
                if (_service.CancelExportReceipt(item.ExportID))
                {
                    string successMsg = item.StatusRaw == "Completed"
                        ? "Đã hủy phiếu và hoàn trả tồn kho thành công!"
                        : "Đã hủy phiếu xuất thành công!";

                    MessageBox.Show(successMsg, "Thông báo");
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra!", "Lỗi");
                }
            }
        }

        private void Button_TaoPhieu_Click(object sender, RoutedEventArgs e)
        {
            // Hiệu ứng mờ
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            this.Effect = blur;

            TaoPhieuXuatDialog dlg = new TaoPhieuXuatDialog();
            if (dlg.ShowDialog() == true)
            {
                LoadData(); // Load lại danh sách
            }

            this.Effect = null;
        }

        #region ĐIỀU HƯỚNG TABS

        // 1. Danh mục
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DMVT_va_DM());
        }

        // 2. Nhập kho
        private void Button_NhapKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NKVT());
        }

        // 3. Xuất kho (Trang hiện tại)
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e) { }

        // 4. Thẻ kho
        private void Button_TheKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LSGD());
        }

        #endregion
    }
}