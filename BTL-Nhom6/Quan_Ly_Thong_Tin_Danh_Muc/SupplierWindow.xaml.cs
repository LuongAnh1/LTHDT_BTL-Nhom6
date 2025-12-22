using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class SupplierWindow : Window
    {
        public Supplier ResultSupplier { get; private set; }

        public SupplierWindow(Supplier existing = null)
        {
            InitializeComponent();
            if (existing != null)
            {
                lblTitle.Text = "CẬP NHẬT NHÀ CUNG CẤP";
                ResultSupplier = existing;
                txtSupplierName.Text = existing.SupplierName;
                txtContactPerson.Text = existing.ContactPerson;
                txtPhone.Text = existing.Phone;
                txtAddress.Text = existing.Address;
            }
            else
            {
                lblTitle.Text = "THÊM NHÀ CUNG CẤP";
                ResultSupplier = new Supplier();
            }
            txtSupplierName.Focus();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSupplierName.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên nhà cung cấp!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSupplierName.Focus();
                return;
            }

            ResultSupplier.SupplierName = txtSupplierName.Text.Trim();
            ResultSupplier.ContactPerson = txtContactPerson.Text.Trim();
            ResultSupplier.Phone = txtPhone.Text.Trim();
            ResultSupplier.Address = txtAddress.Text.Trim();

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