using System;
using System.Collections.ObjectModel; // Dùng ObservableCollection để DataGrid tự cập nhật
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class KKVT_va_NT : Window
    {
        private readonly WorkOrderService _woService = new WorkOrderService();

        // Dùng ObservableCollection để khi thêm/xóa item, giao diện tự thay đổi
        private ObservableCollection<MaterialViewModel> _listVatTu = new ObservableCollection<MaterialViewModel>();

        public KKVT_va_NT()
        {
            InitializeComponent();

            // Gán Source cho DataGrid
            dgVatTu.ItemsSource = _listVatTu;

            // Load danh sách Phiếu công việc vào ComboBox
            LoadWorkOrders();
        }

        private void LoadWorkOrders()
        {
            // Hàm GetWorkOrdersForAcceptance cần được định nghĩa trong WorkOrderService như hướng dẫn trước
            cboWorkOrder.ItemsSource = _woService.GetWorkOrdersForAcceptance();

            // Cài đặt hiển thị cho ComboBox
            cboWorkOrder.DisplayMemberPath = "MoTaLoi";
            cboWorkOrder.SelectedValuePath = "WorkOrderID";
        }

        // 1. Sự kiện khi chọn 1 Phiếu công việc
        private void cboWorkOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var wo = cboWorkOrder.SelectedItem as WorkOrderViewModel;
            if (wo != null)
            {
                // 1. Hiển thị thông tin tóm tắt
                lblStatus.Text = $"Trạng thái: {wo.TrangThai}";
                lblTech.Text = $"KTV: {wo.MucUuTien}";

                // 2. LOAD VẬT TƯ ĐÃ KÊ KHAI (Đổ dữ liệu vào bảng dgVatTu)
                _listVatTu.Clear();
                var details = _woService.GetWorkOrderDetails(wo.WorkOrderID);

                foreach (var item in details)
                {
                    // Đăng ký sự kiện tính tổng tiền khi sửa số lượng (quan trọng)
                    item.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(MaterialViewModel.ThanhTien))
                            UpdateTotal();
                    };
                    _listVatTu.Add(item);
                }

                // 3. LOAD CHI PHÍ PHỤ (Đổ dữ liệu vào các TextBox)
                var costs = _woService.GetWorkOrderCosts(wo.WorkOrderID);
                if (costs != null)
                {
                    txtNhanCong.Text = costs.LaborCost > 0 ? costs.LaborCost.ToString("N0") : "";
                    txtVanChuyen.Text = costs.TransportCost > 0 ? costs.TransportCost.ToString("N0") : "";
                    txtChiPhiKhac.Text = costs.OtherCost > 0 ? costs.OtherCost.ToString("N0") : "";
                    txtMoTaKhac.Text = costs.OtherCostDescription;
                }
                else
                {
                    // Reset nếu không có dữ liệu
                    txtNhanCong.Text = "";
                    txtVanChuyen.Text = "";
                    txtChiPhiKhac.Text = "";
                    txtMoTaKhac.Text = "";
                }

                // 4. Cập nhật lại tổng tiền
                UpdateTotal();
            }
        }

        // 2. Sự kiện bấm nút Thêm Vật Tư
        private void Button_ThemVatTu_Click(object sender, RoutedEventArgs e)
        {
            // Làm mờ nền
            this.Effect = new BlurEffect { Radius = 10 };

            // Mở Dialog chọn vật tư (Cần có file ChonVatTuDialog.xaml)
            ChonVatTuDialog dialog = new ChonVatTuDialog();
            if (dialog.ShowDialog() == true)
            {
                var selected = dialog.SelectedMaterial;

                // Kiểm tra xem vật tư này đã có trong bảng chưa
                var existItem = _listVatTu.FirstOrDefault(x => x.MaterialID == selected.MaterialID);
                if (existItem != null)
                {
                    existItem.SoLuong++; // Tăng số lượng nếu đã có
                }
                else
                {
                    // Đăng ký sự kiện PropertyChanged để khi sửa số lượng thì cập nhật Tổng tiền
                    selected.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == nameof(MaterialViewModel.ThanhTien))
                            UpdateTotal();
                    };
                    _listVatTu.Add(selected);
                }
                UpdateTotal();
            }

            this.Effect = null;
        }

        // 3. Sự kiện bấm nút Xóa dòng vật tư
        private void Button_XoaVatTu_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var item = btn.DataContext as MaterialViewModel;
            if (item != null)
            {
                _listVatTu.Remove(item);
                UpdateTotal();
            }
        }

        // Hàm tính tổng tiền
        private void UpdateTotal()
        {
            decimal total = _listVatTu.Sum(x => x.ThanhTien);
            lblTongTienVatTu.Text = $"{total:N0} VNĐ";
        }

        // 4. Sự kiện nút Hủy bỏ (FIX LỖI CỦA BẠN)
        private void Button_HuyBo_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn hủy bỏ và làm mới màn hình?",
                                         "Xác nhận",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                cboWorkOrder.SelectedItem = null;
                cboWorkOrder.Text = "";
                lblStatus.Text = "Trạng thái: ---";
                lblTech.Text = "KTV: ---";
                _listVatTu.Clear();
                UpdateTotal();
            }
        }

        // 5. SỰ KIỆN NÚT LƯU TẠM
        private void Button_LuuTam_Click(object sender, RoutedEventArgs e)
        {
            // Gọi hàm lưu chung, tham số false nghĩa là KHÔNG đóng phiếu
            SaveData(false);
        }

        // 6. SỰ KIỆN NÚT HOÀN THÀNH (Sửa lại hàm cũ để gọi hàm chung)
        private void Button_HoanThanh_Click(object sender, RoutedEventArgs e)
        {
            // Gọi hàm lưu chung, tham số true nghĩa là ĐÓNG phiếu luôn
            SaveData(true);
        }

        // HÀM XỬ LÝ LƯU CHUNG (Private Helper)
        private void SaveData(bool isCloseTicket)
        {
            // A. Kiểm tra chọn phiếu
            if (cboWorkOrder.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn phiếu công việc!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int woId = (int)cboWorkOrder.SelectedValue;

            // B. Lấy dữ liệu từ TextBox (Xử lý chuỗi rỗng và dấu phẩy nếu có)
            decimal laborCost = ParseCurrency(txtNhanCong.Text);
            decimal transportCost = ParseCurrency(txtVanChuyen.Text);
            decimal otherCost = ParseCurrency(txtChiPhiKhac.Text);
            string otherDesc = txtMoTaKhac.Text.Trim();

            // C. Gọi Service
            bool success = _woService.SaveAcceptance(
                woId,
                _listVatTu.ToList(),
                isCloseTicket, // Truyền biến này vào để quyết định trạng thái (2 hay 3)
                laborCost,
                transportCost,
                otherCost,
                otherDesc
            );

            // D. Thông báo kết quả
            if (success)
            {
                string msg = isCloseTicket ? "Đã nghiệm thu và đóng phiếu thành công!" : "Đã lưu thông tin tạm thời thành công!";
                MessageBox.Show(msg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Nếu đóng phiếu -> Load lại list (phiếu sẽ mất khỏi list vì list chỉ hiện phiếu chưa đóng)
                // Nếu lưu tạm -> Vẫn giữ nguyên để sửa tiếp hoặc Load lại để cập nhật số liệu chuẩn
                LoadWorkOrders();

                if (isCloseTicket)
                {
                    ResetForm(); // Xóa trắng màn hình nếu đã đóng
                }
                else
                {
                    // Nếu lưu tạm, giữ nguyên màn hình nhưng load lại data để đảm bảo đồng bộ
                    // (Hoặc có thể giữ nguyên không làm gì)
                }
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi lưu dữ liệu!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm phụ để convert chuỗi tiền tệ (ví dụ "100,000") sang số decimal an toàn
        private decimal ParseCurrency(string input)
        {
            if (string.IsNullOrEmpty(input)) return 0;
            // Loại bỏ dấu phẩy phân cách ngàn (nếu người dùng copy paste vào) hoặc chữ cái
            string clean = new string(input.Where(c => char.IsDigit(c)).ToArray());
            if (decimal.TryParse(clean, out decimal result))
            {
                return result;
            }
            return 0;
        }

        // Hàm phụ để reset form cho gọn
        private void ResetForm()
        {
            _listVatTu.Clear();
            lblStatus.Text = "Trạng thái: ---";
            lblTech.Text = "KTV: ---";
            cboWorkOrder.SelectedItem = null;

            txtNhanCong.Text = "";
            txtVanChuyen.Text = "";
            txtChiPhiKhac.Text = "";
            txtMoTaKhac.Text = "";

            UpdateTotal();
        }

        #region Navigation (Chuyển trang)
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new QLYCBT());
        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new LKH_va_DP());
        private void Button_CapNhat_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new CNPCV());
        #endregion
    }
}