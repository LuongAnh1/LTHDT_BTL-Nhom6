using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Quan_Tri_He_Thong;

namespace BTL_Nhom6
{
    public partial class Trang_Chu : Window
    {
        public Trang_Chu()
        {
            InitializeComponent();
        }

        // Cho phép kéo thả cửa sổ khi click chuột trái vào vùng trống
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
        // Truy cập form TDMK_va_TTCN
        private void Button_QTHT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Khởi tạo đối tượng cửa sổ mục tiêu
                // Dùng tên đầy đủ namespace như bạn yêu cầu
                var formQuanTri = new QLND_va_PQ();

                // Gán trạng thái (Normal/Maximized) của cửa sổ hiện tại cho cửa sổ mới
                formQuanTri.WindowState = this.WindowState;
                // 2. Hiển thị form mới
                formQuanTri.Show();

                // 3. Đóng form Trang Chủ hiện tại (để chuyển hẳn sang form kia)
                this.Close();

                // LƯU Ý: Nếu bạn chỉ muốn mở form kia lên dạng popup (không đóng trang chủ)
                // thì dùng lệnh: formQuanTri.ShowDialog(); và bỏ dòng this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Có lỗi khi mở form: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(); // Thoát ứng dụng
        }

        private void Button_Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized; // Thu nhỏ cửa sổ
        }

        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            // Xử lý logic đăng xuất (mở lại form đăng nhập)
            Dang_Nhap loginWindow = new Dang_Nhap();
            loginWindow.Show();
            this.Close();
        }
        private void Button_Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                // Nếu đang phóng to thì thu về bình thường
                this.WindowState = WindowState.Normal;
                iconMaximize.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize; // Đổi icon về hình ô vuông đơn
                btnMaximize.ToolTip = "Phóng to";
            }
            else
            {
                // Nếu đang bình thường thì phóng to
                this.WindowState = WindowState.Maximized;
                iconMaximize.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore; // Đổi icon về hình 2 ô vuông chồng nhau
                btnMaximize.ToolTip = "Khôi phục";
            }
        }
    }
}