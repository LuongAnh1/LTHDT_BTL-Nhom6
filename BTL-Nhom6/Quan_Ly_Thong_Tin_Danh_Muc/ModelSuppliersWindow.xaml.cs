using System;
using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class ModelSuppliersWindow : Window
    {
        public SupplierQuoteDTO Result { get; private set; }

        private readonly SupplierQuoteDTOService _service = new SupplierQuoteDTOService();

        public ModelSuppliersWindow(SupplierQuoteDTO existing = null)
        {
            InitializeComponent();

            cbModels.ItemsSource = _service.GetAllModels();
            cbSuppliers.ItemsSource = _service.GetAllSuppliers();

            if (existing != null)
            {
                lblTitle.Text = "CẬP NHẬT BÁO GIÁ";
                Result = existing;

                cbModels.SelectedValue = existing.ModelID;
                cbSuppliers.SelectedValue = existing.SupplierID;
                txtPrice.Text = existing.Price.ToString();
                dpDate.SelectedDate = existing.LastSupplyDate;

                // KHÓA KHÓA CHÍNH
                cbModels.IsEnabled = false;
                cbSuppliers.IsEnabled = false;
            }
            else
            {
                lblTitle.Text = "THÊM BÁO GIÁ";
                Result = new SupplierQuoteDTO
                {
                    LastSupplyDate = DateTime.Now
                };
                dpDate.SelectedDate = DateTime.Now;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbModels.SelectedValue == null || cbSuppliers.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn linh kiện và nhà cung cấp.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Đơn giá không hợp lệ.");
                return;
            }

            Result.ModelID = (int)cbModels.SelectedValue;
            Result.SupplierID = (int)cbSuppliers.SelectedValue;
            Result.Price = price;
            Result.LastSupplyDate = dpDate.SelectedDate ?? DateTime.Now;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
