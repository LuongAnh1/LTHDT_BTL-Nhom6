using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class UnitWindow : Window
    {
        public ProductUnit ResultUnit { get; private set; }

        public UnitWindow(ProductUnit existingUnit = null)
        {
            InitializeComponent();
            if (existingUnit != null)
            {
                lblTitle.Text = "CẬP NHẬT ĐƠN VỊ";
                ResultUnit = existingUnit;
                txtUnitName.Text = existingUnit.UnitName;
                txtDescription.Text = existingUnit.Description;
            }
            else
            {
                lblTitle.Text = "THÊM ĐƠN VỊ MỚI";
                ResultUnit = new ProductUnit();
            }
            txtUnitName.Focus();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUnitName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đơn vị!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ResultUnit.UnitName = txtUnitName.Text.Trim();
            ResultUnit.Description = txtDescription.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}