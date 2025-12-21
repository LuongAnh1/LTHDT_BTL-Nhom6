using System.Windows;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class DeviceModelWindow : Window
    {
        private DeviceModelService _modelService = new DeviceModelService();
        private CategoryService _catService = new CategoryService();
        private DeviceModel _currentModel = null;

        // Constructor 1: Thêm mới (Có thể truyền categoryId đang chọn ở ngoài vào để select sẵn)
        public DeviceModelWindow(int preSelectedCategoryId = 0)
        {
            InitializeComponent();
            LoadCategories();
            lblTitle.Text = "THÊM MODEL MỚI";

            // Nếu có ID loại truyền vào, tự động chọn
            if (preSelectedCategoryId > 0)
                cboCategory.SelectedValue = preSelectedCategoryId;
        }

        // Constructor 2: Sửa
        public DeviceModelWindow(DeviceModel model)
        {
            InitializeComponent();
            LoadCategories();
            _currentModel = model;

            lblTitle.Text = "CẬP NHẬT MODEL";
            txtModelName.Text = model.ModelName;
            txtManufacturer.Text = model.Manufacturer;
            txtDescription.Text = model.Description;
            cboCategory.SelectedValue = model.CategoryID;
        }

        private void LoadCategories()
        {
            cboCategory.ItemsSource = _catService.GetAllCategories();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModelName.Text) || cboCategory.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập Tên Model và chọn Loại thiết bị!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DeviceModel model = new DeviceModel
                {
                    ModelName = txtModelName.Text.Trim(),
                    Manufacturer = txtManufacturer.Text.Trim(),
                    CategoryID = (int)cboCategory.SelectedValue,
                    Description = txtDescription.Text.Trim()
                };

                if (_currentModel == null)
                    _modelService.AddModel(model);
                else
                {
                    model.ModelID = _currentModel.ModelID;
                    _modelService.UpdateModel(model);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.ChangedButton == System.Windows.Input.MouseButton.Left) this.DragMove(); }
    }
}