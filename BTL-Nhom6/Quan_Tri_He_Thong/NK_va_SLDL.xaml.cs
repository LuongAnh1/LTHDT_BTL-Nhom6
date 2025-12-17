using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    /// <summary>
    /// Interaction logic for NK_va_SLDL.xaml
    /// </summary>
    public partial class NK_va_SLDL : Window
    {
        public NK_va_SLDL()
        {
            InitializeComponent();
        }
        // --- Xử lý sự kiện Button ---
        // Chuyển tới trang Quản lý người dùng và phân quyền
        private void Button_QLND_va_PQ_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Khởi tạo form
                var homeTDMK_va_TTCN = new QLND_va_PQ();

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
        // Chuyển tới trang Quan lý hồ sơ kỹ năng 
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
        // Chuyển tới trang Thay đổi mật khẩu và thông tin cá nhân 
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
