using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    /// <summary>
    /// Interaction logic for QLND_va_PQ.xaml
    /// </summary>
    public partial class QLND_va_PQ : Window
    {
        public QLND_va_PQ()
        {
            InitializeComponent();
        }
        // --- Xử lý sự kiện Button ---
        // Chuyển tới trang Thay đổi Mật Khẩu và Thông Tin Cá Nhân
        private void Button_TDMK_va_TTCN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Khởi tạo form
                var homeTDMK_va_TTCN = new TDMK_va_TTCN();

                // Gán trạng thái (Normal/Maximized) của cửa sổ hiện tại cho cửa sổ mới
                homeTDMK_va_TTCN.WindowState = this.WindowState;
                homeTDMK_va_TTCN.Show();

                // Đóng form hiện tại 
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi quay về trang chủ: " + ex.Message);
            }
        }
        // Chuyển tới trang Quản lý hồ sơ kỹ năng
        private void Button_QLHSKN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Khởi tạo form
                var homeTDMK_va_TTCN = new QLHSKN();

                // Gán trạng thái (Normal/Maximized) của cửa sổ hiện tại cho cửa sổ mới
                homeTDMK_va_TTCN.WindowState = this.WindowState;
                homeTDMK_va_TTCN.Show();

                // Đóng form hiện tại 
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi khi quay về trang chủ: " + ex.Message);
            }
        }
        // Chuyển tới trang Nhật kỹ và sao lưu dữ liệu 
        private void Button_NK_va_SLDL_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Khởi tạo form
                var homeTDMK_va_TTCN = new NK_va_SLDL();

                // Gán trạng thái (Normal/Maximized) của cửa sổ hiện tại cho cửa sổ mới
                homeTDMK_va_TTCN.WindowState = this.WindowState;
                homeTDMK_va_TTCN.Show();

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
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
                iconMaximize.Kind = PackIconKind.WindowRestore;
            }
            else
            {
                this.WindowState = WindowState.Normal;
                iconMaximize.Kind = PackIconKind.WindowMaximize;
            }
        }


    }
}
