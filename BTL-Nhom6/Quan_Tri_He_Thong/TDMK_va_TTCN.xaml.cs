using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    public partial class TDMK_va_TTCN : Window
    {
        public TDMK_va_TTCN()
        {
            InitializeComponent();
        }
        // Chuyển tới trang Quản Lý Người Dùng và Phân Quyền
        private void QLND_va_PQ(object sender, RoutedEventArgs e)
        {
            try
            {
                // Khởi tạo form QLND_va_PQ
                var homeQLND_va_PQ = new QLND_va_PQ();

                // Gán trạng thái (Normal/Maximized) của cửa sổ hiện tại cho cửa sổ mới
                homeQLND_va_PQ.WindowState = this.WindowState;
                homeQLND_va_PQ.Show();

                // Đóng form hiện tại
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi quay về trang chủ: " + ex.Message);
            }
        }
        // Quay lại trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Khởi tạo form Trang Chủ (nằm ở namespace gốc BTL_Nhom6)
                var homeWindow = new Trang_Chu();

                // Gán trạng thái (Normal/Maximized) của cửa sổ hiện tại cho cửa sổ mới
                homeWindow.WindowState = this.WindowState;
                // Hiển thị Trang Chủ
                homeWindow.Show();

                // Đóng form hiện tại (Quản trị hệ thống)
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi quay về trang chủ: " + ex.Message);
            }
        }

        // --- Xử lý sự kiện Window ---
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                iconMaximize.Kind = PackIconKind.WindowMaximize;
                btnMaximize.ToolTip = "Phóng to";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                iconMaximize.Kind = PackIconKind.WindowRestore;
                btnMaximize.ToolTip = "Khôi phục";
            }
        }
    }
}