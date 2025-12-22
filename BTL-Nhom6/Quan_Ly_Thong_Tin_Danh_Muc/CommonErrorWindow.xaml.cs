using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class CommonErrorWindow : Window
    {
        public CommonError ResultError { get; private set; }

        public CommonErrorWindow(CommonError existing = null)
        {
            InitializeComponent();
            if (existing != null)
            {
                lblTitle.Text = "CẬP NHẬT LỖI";
                ResultError = existing;
                txtErrorName.Text = existing.ErrorName;
                txtDescription.Text = existing.Description;
                txtSolution.Text = existing.Solution;
            }
            else
            {
                lblTitle.Text = "THÊM LỖI MỚI";
                ResultError = new CommonError();
            }
            txtErrorName.Focus();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) this.DragMove(); }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtErrorName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên lỗi!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ResultError.ErrorName = txtErrorName.Text.Trim();
            ResultError.Description = txtDescription.Text.Trim();
            ResultError.Solution = txtSolution.Text.Trim();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) { this.DialogResult = false; this.Close(); }
    }
}