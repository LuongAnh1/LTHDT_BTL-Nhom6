using System;
using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class ThemVatTuDialog : Window
    {
        private readonly MaterialService _materialService = new MaterialService();
        private readonly UnitService _unitService = new UnitService();

        // Biến lưu ID đang sửa. Nếu = 0 nghĩa là đang Thêm mới.
        private int _editingMaterialId = 0;

        // Constructor 1: Dùng cho THÊM MỚI (Giữ nguyên)
        public ThemVatTuDialog()
        {
            InitializeComponent();
            LoadUnits();
        }

        // Constructor 2: Dùng cho CHỈNH SỬA (Mới thêm)
        public ThemVatTuDialog(MaterialCatalogViewModel oldItem) : this()
        {
            // 1. Đổi tiêu đề
            lblTitle.Text = "CẬP NHẬT VẬT TƯ";

            // 2. Lưu ID đang sửa
            _editingMaterialId = oldItem.MaterialID;

            // 3. Điền dữ liệu cũ lên Form
            txtTenVatTu.Text = oldItem.TenDM;
            cboDonVi.SelectedValue = oldItem.UnitID;
            txtDinhMuc.Text = oldItem.DinhMuc.ToString();
            txtDonGia.Text = oldItem.DonGia.ToString("G0"); // G0 để bỏ số 0 thừa
            txtMoTa.Text = oldItem.MoTa;
        }

        private void LoadUnits()
        {
            try
            {
                cboDonVi.ItemsSource = _unitService.GetAllUnits();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải đơn vị tính: " + ex.Message);
            }
        }

        // Xử lý kéo thả cửa sổ
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate (Giữ nguyên)
            if (string.IsNullOrWhiteSpace(txtTenVatTu.Text))
            {
                MessageBox.Show("Vui lòng nhập tên vật tư!", "Cảnh báo");
                return;
            }
            if (cboDonVi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính!", "Cảnh báo");
                return;
            }

            // 2. Lấy dữ liệu từ form
            int.TryParse(txtDinhMuc.Text, out int dinhMuc);
            decimal.TryParse(txtDonGia.Text, out decimal donGia);

            // 3. Tạo đối tượng Material
            Material mat = new Material
            {
                MaterialID = _editingMaterialId, // ID = 0 nếu thêm, > 0 nếu sửa
                MaterialName = txtTenVatTu.Text.Trim(),
                UnitID = (int)cboDonVi.SelectedValue,
                MinStock = dinhMuc,
                UnitPrice = donGia,
                Description = txtMoTa.Text.Trim()
            };

            // 4. Gọi Service
            try
            {
                if (_editingMaterialId == 0)
                {
                    // Logic THÊM MỚI
                    _materialService.AddMaterial(mat);
                    MessageBox.Show("Thêm vật tư thành công!", "Thông báo");
                }
                else
                {
                    // Logic CẬP NHẬT
                    _materialService.UpdateMaterial(mat);
                    MessageBox.Show("Cập nhật vật tư thành công!", "Thông báo");
                }

                this.DialogResult = true; // Báo cho form cha reload
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}