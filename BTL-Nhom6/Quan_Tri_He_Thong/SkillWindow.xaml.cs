using System.Windows;
using BTL_Nhom6.Models;

namespace BTL_Nhom6
{
    public partial class SkillWindow : Window
    {
        public Skill CreatedSkill { get; private set; }
        private bool _isEditMode = false; // Biến đánh dấu đang sửa hay thêm

        // Constructor có tham số mặc định là null
        public SkillWindow(Skill skillToEdit = null)
        {
            InitializeComponent();

            if (skillToEdit != null)
            {
                // --- CHẾ ĐỘ SỬA ---
                _isEditMode = true;

                // Đổi tiêu đề form
                // (Giả sử trong XAML bạn đặt x:Name="lblTitle" cho TextBlock tiêu đề "THÊM KỸ NĂNG")
                // Nếu chưa đặt tên, bạn cần vào XAML thêm x:Name="lblTitle" vào TextBlock đó nhé.
                // lblTitle.Text = "CẬP NHẬT KỸ NĂNG"; 
                this.Title = "Cập nhật kỹ năng";

                // Đổ dữ liệu cũ vào ô nhập
                txtSkillName.Text = skillToEdit.SkillName;
                txtDescription.Text = skillToEdit.Description;

                // Lưu tạm thông tin skill đang sửa
                CreatedSkill = skillToEdit;
            }
            else
            {
                // --- CHẾ ĐỘ THÊM ---
                this.Title = "Thêm kỹ năng";
                txtSkillName.Focus();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSkillName.Text))
            {
                MessageBox.Show("Tên kỹ năng không được để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtSkillName.Focus();
                return;
            }

            if (_isEditMode)
            {
                // Nếu là sửa: Chỉ cập nhật nội dung, giữ nguyên ID cũ
                CreatedSkill.SkillName = txtSkillName.Text.Trim();
                CreatedSkill.Description = txtDescription.Text?.Trim() ?? "";
            }
            else
            {
                // Nếu là thêm mới: Tạo object mới
                CreatedSkill = new Skill
                {
                    SkillName = txtSkillName.Text.Trim(),
                    Description = txtDescription.Text?.Trim() ?? ""
                };
            }

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}