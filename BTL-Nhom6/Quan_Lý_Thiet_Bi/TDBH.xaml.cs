using BTL_Nhom6.Helper; // Nhớ using namespace chứa file NavigationService
using System.Windows;
using System.Windows.Navigation;
// using BTL_Nhom6.Quan_Ly_Thiet_Bi; // Namespace chứa các Window khác (nếu cần)

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    public partial class TDBH : Window
    {
        public TDBH()
        {
            InitializeComponent();
        }

        // --- NAVIGATION ---

        private void Button_HSTB_Click(object sender, RoutedEventArgs e)
        {
             NavigationHelper.Navigate(this, new HSTB_va_QR());
            
        }

        private void Button_DieuChuyen_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DC_va_BG());
        }

        private void Button_TraCuu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TCTS());
        }


        // --- LOGIC CHỨC NĂNG (Thêm vào nếu trong XAML đã có sự kiện Click) ---

        private void Button_Loc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã lọc dữ liệu.");
        }

        private void Button_XoaBoLoc_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã xóa bộ lọc.");
        }

        private void Button_XuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang xuất báo cáo...");
        }
    }
}