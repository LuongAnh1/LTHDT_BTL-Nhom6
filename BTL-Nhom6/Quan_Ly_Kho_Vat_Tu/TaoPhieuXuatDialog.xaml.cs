using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class TaoPhieuXuatDialog : Window
    {
        private readonly ExportService _exportService = new ExportService();
        private ObservableCollection<MaterialViewModel> _listChiTiet = new ObservableCollection<MaterialViewModel>();

        private int _currentExportId = 0;
        private bool _isViewMode = false;

        public TaoPhieuXuatDialog()
        {
            InitializeComponent();

            // 1. Mặc định
            txtMaPhieu.Text = $"XK-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}";
            dpNgayXuat.SelectedDate = DateTime.Now;

            // 2. Load người nhận
            LoadReceivers();

            // 3. Binding
            dgChiTiet.ItemsSource = _listChiTiet;
        }

        // Constructor 2
        public TaoPhieuXuatDialog(int exportId, bool isViewMode = false)
        {
            InitializeComponent();

            // Load data combobox trước
            try { cboNguoiNhan.ItemsSource = _exportService.GetEmployees(); } catch { }

            _currentExportId = exportId;
            _isViewMode = isViewMode;

            LoadData();

            if (_isViewMode) SetViewMode();
            else lblTitle.Text = "CẬP NHẬT PHIẾU XUẤT";
        }

        private void LoadData()
        {
            var header = _exportService.GetExportHeader(_currentExportId);
            if (header != null)
            {
                txtMaPhieu.Text = header.MaPhieu;
                dpNgayXuat.SelectedDate = header.NgayXuatRaw;
                cboNguoiNhan.SelectedValue = int.Parse(header.NguoiNhan); // NguoiNhan đang chứa ID
                txtGhiChu.Text = header.BoPhan; // BoPhan đang chứa Note
            }

            var details = _exportService.GetExportDetails(_currentExportId);
            _listChiTiet = new ObservableCollection<MaterialViewModel>(details);
            dgChiTiet.ItemsSource = _listChiTiet;

            UpdateTotal();
        }

        private void SetViewMode()
        {
            lblTitle.Text = "CHI TIẾT PHIẾU XUẤT";
            // Disable controls
            txtMaPhieu.IsReadOnly = true;
            dpNgayXuat.IsEnabled = false;
            cboNguoiNhan.IsEnabled = false;
            cboWorkOrder.IsEnabled = false;
            txtGhiChu.IsReadOnly = true;

            // Ẩn nút
            // Lưu ý: Cần đặt x:Name cho nút Thêm vật tư và nút Lưu trong XAML trước
            btnThemVatTu.Visibility = Visibility.Collapsed; 
            btnLuu.Visibility = Visibility.Collapsed;

            dgChiTiet.IsReadOnly = true;
            if (dgChiTiet.Columns.Count > 0)
            {
                dgChiTiet.Columns[dgChiTiet.Columns.Count - 1].Visibility = Visibility.Collapsed; // Ẩn cột xóa
            }
        }
        private void LoadReceivers()
        {
            try { cboNguoiNhan.ItemsSource = _exportService.GetEmployees(); }
            catch (Exception ex) { MessageBox.Show("Lỗi tải nhân viên: " + ex.Message); }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Hủy bỏ tạo phiếu xuất?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        // --- THÊM VẬT TƯ ---
        private void BtnThemVatTu_Click(object sender, RoutedEventArgs e)
        {
            // Tái sử dụng form chọn vật tư (Form này trả về MaterialViewModel có cả CurrentStock)
            BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co.ChonVatTuDialog dialog = new BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co.ChonVatTuDialog();

            if (dialog.ShowDialog() == true)
            {
                var selected = dialog.SelectedMaterial;

                // Check nếu hết hàng
                // (Note: Cần đảm bảo hàm GetAllMaterials trong MaterialService lấy cột CurrentStock)
                if (selected.CurrentStock <= 0)
                {
                    MessageBox.Show($"Vật tư '{selected.TenVatTu}' đã hết hàng trong kho!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var exist = _listChiTiet.FirstOrDefault(x => x.MaterialID == selected.MaterialID);
                if (exist != null)
                {
                    // Check tồn kho khi cộng dồn
                    if (exist.SoLuong + 1 > exist.CurrentStock)
                    {
                        MessageBox.Show("Số lượng xuất vượt quá tồn kho!", "Cảnh báo");
                        return;
                    }
                    exist.SoLuong++;
                }
                else
                {
                    // Gán số lượng mặc định là 1
                    selected.SoLuong = 1;

                    // Sự kiện tính tổng
                    selected.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == "ThanhTien") UpdateTotal();
                    };
                    _listChiTiet.Add(selected);
                }
                UpdateTotal();
            }
        }

        private void BtnXoaDong_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            var item = btn.DataContext as MaterialViewModel;
            if (item != null)
            {
                _listChiTiet.Remove(item);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            decimal total = _listChiTiet.Sum(x => x.ThanhTien);
            lblTongTien.Text = $"{total:N0} VNĐ";
        }

        private void cboNguoiNhan_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboNguoiNhan.SelectedValue != null)
            {
                int userId = (int)cboNguoiNhan.SelectedValue;
                try
                {
                    // Lấy danh sách WO đang làm của nhân viên này
                    var tasks = _exportService.GetActiveWorkOrdersByTech(userId);
                    cboWorkOrder.ItemsSource = tasks;

                    // Nếu có việc, tự động mở dropdown cho tiện (Tùy chọn)
                    if (tasks.Count > 0) cboWorkOrder.IsDropDownOpen = true;
                }
                catch { }
            }
            else
            {
                cboWorkOrder.ItemsSource = null;
            }
        }


        // --- LƯU PHIẾU ---
        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // 0. Chặn nếu đang ở chế độ xem (An toàn)
            if (_isViewMode) return;

            // 1. Validate cơ bản
            if (cboNguoiNhan.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Người nhận hàng!", "Cảnh báo");
                return;
            }
            if (_listChiTiet.Count == 0)
            {
                MessageBox.Show("Danh sách xuất đang trống!", "Cảnh báo");
                return;
            }

            // 2. Validate tồn kho (Client-side)
            foreach (var item in _listChiTiet)
            {
                // Lưu ý: Logic này đúng cho Tạo mới. 
                // Khi Sửa, CurrentStock có thể đã bị trừ rồi, nên check ở đây chỉ mang tính tương đối.
                // Việc check chính xác nhất nằm ở Service (có Transaction).
                if (item.SoLuong > item.CurrentStock)
                {
                    MessageBox.Show($"Vật tư '{item.TenVatTu}' xuất quá tồn kho!\n(Tồn: {item.CurrentStock}, Xuất: {item.SoLuong})", "Lỗi");
                    return;
                }
                if (item.SoLuong <= 0)
                {
                    MessageBox.Show($"Số lượng xuất '{item.TenVatTu}' không hợp lệ!", "Lỗi");
                    return;
                }
            }

            // Lấy WorkOrderID (có thể null)
            int? woId = null;
            if (cboWorkOrder.SelectedValue != null)
            {
                woId = (int)cboWorkOrder.SelectedValue;
            }

            string err = "";

            // 3. GỌI SERVICE (SỬA Ở ĐÂY)
            if (_currentExportId == 0)
            {
                // --- TRƯỜNG HỢP TẠO MỚI ---
                err = _exportService.CreateExportReceipt(
                    txtMaPhieu.Text,
                    (int)cboNguoiNhan.SelectedValue,
                    dpNgayXuat.SelectedDate ?? DateTime.Now,
                    txtGhiChu.Text,
                    _listChiTiet.ToList(),
                    woId
                );
            }
            else
            {
                // --- TRƯỜNG HỢP CẬP NHẬT (SỬA PHIẾU NHÁP) ---
                // Gọi hàm Update đã viết trong Service
                err = _exportService.UpdateExportReceipt(
                    _currentExportId,
                    (int)cboNguoiNhan.SelectedValue,
                    dpNgayXuat.SelectedDate ?? DateTime.Now,
                    txtGhiChu.Text,
                    _listChiTiet.ToList(),
                    woId
                );
            }

            // 4. Xử lý kết quả
            if (string.IsNullOrEmpty(err))
            {
                string msg = _currentExportId == 0 ? "Xuất kho thành công!" : "Cập nhật phiếu xuất thành công!";
                MessageBox.Show(msg, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show($"Lỗi khi lưu:\n{err}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}