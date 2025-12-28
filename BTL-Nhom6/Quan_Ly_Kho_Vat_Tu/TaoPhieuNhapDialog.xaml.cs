using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System.Windows.Controls; // Để dùng UIElement

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class TaoPhieuNhapDialog : Window
    {
        private readonly ImportService _importService = new ImportService();

        // Dùng ObservableCollection để DataGrid tự cập nhật khi thêm/xóa
        private ObservableCollection<MaterialViewModel> _listChiTiet = new ObservableCollection<MaterialViewModel>();

        private int _currentReceiptId = 0;
        private bool _isViewMode = false;

        public TaoPhieuNhapDialog()
        {
            InitializeComponent();

            // 1. Cài đặt mặc định
            txtMaPhieu.Text = $"NK-{DateTime.Now:yyMMdd}-{new Random().Next(100, 999)}"; // Mã tự sinh tạm thời
            dpNgayNhap.SelectedDate = DateTime.Now;

            // 2. Load combobox
            LoadSuppliers();

            // 3. Binding DataGrid
            dgChiTiet.ItemsSource = _listChiTiet;
        }

        // Constructor 2: XEM HOẶC SỬA
        public TaoPhieuNhapDialog(int receiptId, bool isViewMode = false)
        {
            InitializeComponent();
            InitializeData();

            _currentReceiptId = receiptId;
            _isViewMode = isViewMode;

            LoadReceiptData();

            if (_isViewMode)
            {
                SetViewMode();
            }
            else
            {
                lblTitle.Text = "CẬP NHẬT PHIẾU NHẬP";
            }
        }

        private void InitializeData()
        {
            LoadSuppliers();
            dgChiTiet.ItemsSource = _listChiTiet;
        }

        // Load dữ liệu cũ lên form
        private void LoadReceiptData()
        {
            // 1. Load Header
            var header = _importService.GetImportHeader(_currentReceiptId);
            if (header != null)
            {
                txtMaPhieu.Text = header.MaPhieu;
                dpNgayNhap.SelectedDate = header.NgayNhapRaw;
                txtGhiChu.Text = header.DiaChi; // Map tạm field DiaChi chứa Note
                cboNhaCungCap.SelectedValue = int.Parse(header.NhaCungCap); // Map tạm field NhaCungCap chứa ID
            }

            // 2. Load Chi tiết
            var details = _importService.GetImportDetails(_currentReceiptId);
            foreach (var item in details)
            {
                // QUAN TRỌNG: Đăng ký sự kiện tính tổng cho các dòng cũ
                item.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == "ThanhTien") UpdateTotal();
                };
                _listChiTiet.Add(item);
            }
            UpdateTotal(); // Tính tổng ngay khi form hiện lên
        }

        // Chế độ CHỈ XEM: Khóa các ô nhập liệu, ẩn nút Lưu
        private void SetViewMode()
        {
            lblTitle.Text = "CHI TIẾT PHIẾU NHẬP";

            // Disable Controls
            txtMaPhieu.IsReadOnly = true;
            dpNgayNhap.IsEnabled = false;
            cboNhaCungCap.IsEnabled = false;
            txtGhiChu.IsReadOnly = true;

            // Ẩn các nút thao tác
            btnThemVatTu.Visibility = Visibility.Collapsed; // Cần đặt x:Name="btnThemVatTu" cho nút Thêm dòng trong XAML
            btnLuu.Visibility = Visibility.Collapsed;       // Cần đặt x:Name="btnLuu" cho nút Lưu trong XAML

            // Grid chỉ đọc
            dgChiTiet.IsReadOnly = true;

            // Ẩn cột Xóa (Cột cuối cùng)
            dgChiTiet.Columns[dgChiTiet.Columns.Count - 1].Visibility = Visibility.Collapsed;
        }
        private void LoadSuppliers()
        {
            try
            {
                cboNhaCungCap.ItemsSource = _importService.GetSuppliers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải NCC: " + ex.Message);
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // MỞ DIALOG CHỌN VẬT TƯ (Tái sử dụng form cũ)
        private void BtnThemVatTu_Click(object sender, RoutedEventArgs e)
        {
            // Sử dụng lại ChonVatTuDialog của phần Kê khai vật tư
            // Bạn cần đảm bảo namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co đã được using
            // Hoặc chuyển ChonVatTuDialog ra thư mục chung (ví dụ UserControls hoặc Shared)

            // Ở đây tôi giả sử bạn gọi được nó từ namespace cũ
            BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co.ChonVatTuDialog dialog = new BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co.ChonVatTuDialog();

            if (dialog.ShowDialog() == true)
            {
                var selected = dialog.SelectedMaterial;

                // Kiểm tra trùng: nếu có rồi thì không thêm, hoặc cộng dồn tùy logic
                var exist = _listChiTiet.FirstOrDefault(x => x.MaterialID == selected.MaterialID);
                if (exist != null)
                {
                    exist.SoLuong++; // Tăng số lượng
                }
                else
                {
                    // QUAN TRỌNG: Đăng ký sự kiện tính tổng
                    selected.PropertyChanged += (s, args) =>
                    {
                        if (args.PropertyName == "ThanhTien") UpdateTotal();
                    };
                    _listChiTiet.Add(selected);
                }
                UpdateTotal(); // Tính ngay lần đầu
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

        // LƯU DỮ LIỆU
        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (_isViewMode) return; // Chặn nếu đang xem

            // 1. Validate
            if (cboNhaCungCap.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Nhà cung cấp!", "Cảnh báo");
                return;
            }
            if (_listChiTiet.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất 1 vật tư!", "Cảnh báo");
                return;
            }

            // 2. Gọi Service
            string code = txtMaPhieu.Text;
            int supId = (int)cboNhaCungCap.SelectedValue;
            DateTime date = dpNgayNhap.SelectedDate ?? DateTime.Now;
            string note = txtGhiChu.Text;

            bool success = false;

            if (_currentReceiptId == 0)
            {
                // TẠO MỚI
                success = _importService.CreateImportReceipt(txtMaPhieu.Text, (int)cboNhaCungCap.SelectedValue, dpNgayNhap.SelectedDate ?? DateTime.Now, txtGhiChu.Text, _listChiTiet.ToList());
            }
            else
            {
                // CẬP NHẬT
                success = _importService.UpdateImportReceipt(_currentReceiptId, (int)cboNhaCungCap.SelectedValue, dpNgayNhap.SelectedDate ?? DateTime.Now, txtGhiChu.Text, _listChiTiet.ToList());
            }

            if (success)
            {
                MessageBox.Show("Lưu dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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