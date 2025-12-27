using System.Windows;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class ChonVatTuDialog : Window
    {
        private MaterialService _service = new MaterialService();
        public MaterialViewModel SelectedMaterial { get; private set; }

        public ChonVatTuDialog()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            dgKho.ItemsSource = _service.GetAllMaterials();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void BtnChon_Click(object sender, RoutedEventArgs e)
        {
            SelectedMaterial = dgKho.SelectedItem as MaterialViewModel;
            if (SelectedMaterial != null)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một vật tư!");
            }
        }
    }
}