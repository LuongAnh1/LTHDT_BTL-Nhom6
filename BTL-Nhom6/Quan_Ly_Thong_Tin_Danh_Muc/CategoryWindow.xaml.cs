using System.Windows;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class CategoryWindow : Window
    {
        private CategoryService _service = new CategoryService();
        private Category _currentCategory = null;

        public CategoryWindow(Category category = null)
        {
            InitializeComponent();
            _currentCategory = category;

            if (_currentCategory != null)
            {
                lblTitle.Text = "CẬP NHẬT LOẠI";
                txtCategoryName.Text = _currentCategory.CategoryName;
                txtDescription.Text = _currentCategory.Description;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Category cat = new Category
                {
                    CategoryName = txtCategoryName.Text.Trim(),
                    Description = txtDescription.Text.Trim()
                };

                if (_currentCategory == null)
                    _service.AddCategory(cat);
                else
                {
                    cat.CategoryID = _currentCategory.CategoryID;
                    _service.UpdateCategory(cat);
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