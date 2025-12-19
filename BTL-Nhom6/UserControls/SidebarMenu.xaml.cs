using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Tri_He_Thong; // Namespace chứa NavigationHelper
// using BTL_Nhom6.Views; // Mở comment nếu các Window nằm trong thư mục Views

namespace BTL_Nhom6.UserControls
{
    public partial class SidebarMenu : UserControl
    {
        public SidebarMenu()
        {
            InitializeComponent();
        }

        // ============================================================
        // 1. DEPENDENCY PROPERTY: CurrentItem
        // Giúp XAML nhận biết trang nào đang active để đổi màu nút
        // ============================================================
        public string CurrentItem
        {
            get { return (string)GetValue(CurrentItemProperty); }
            set { SetValue(CurrentItemProperty, value); }
        }

        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register("CurrentItem", typeof(string), typeof(SidebarMenu), new PropertyMetadata(string.Empty));

        // ============================================================
        // 2. SỰ KIỆN CLICK MENU (Chuyển trang)
        // ============================================================
        private void Button_Menu_Click(object sender, RoutedEventArgs e)
        {
            // Lấy nút vừa bấm
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            string tag = btn.Tag.ToString();

            // Lấy cửa sổ hiện tại đang chứa UserControl này
            Window currentWindow = Window.GetWindow(this);
            Window nextWindow = null;

            // Kiểm tra xem user có đang bấm vào chính trang hiện tại không
            // Nếu đúng thì không làm gì cả (để tránh reload lại trang)
            if (CurrentItem == tag) return;

            // Dựa vào Tag để khởi tạo Window tương ứng
            // LƯU Ý: Bạn cần thay thế tên Class Window (ví dụ: MainWindow, DevicesWindow...) 
            // cho đúng với tên class thực tế trong dự án của bạn.
            switch (tag)
            {
                case "Home":
                    nextWindow = new Trang_Chu(); // Ví dụ: Trang chủ
                    break;

                case "System":
                    // nextWindow = new QuanTriHeThongWindow(); 
                    nextWindow = new QLND_va_PQ(); // Ví dụ: Trang chủ
                    break;

                case "Roles":
                    // nextWindow = new PhanQuyenWindow();
                    MessageBox.Show("Chức năng Phân quyền đang phát triển");
                    break;

                case "Devices":
                    // nextWindow = new QuanLyThietBiWindow();
                    MessageBox.Show("Chức năng Quản lý thiết bị đang phát triển");
                    break;

                case "Maintenance":
                    // nextWindow = new BaoTriWindow();
                    MessageBox.Show("Chức năng Bảo trì đang phát triển");
                    break;

                default:
                    break;
            }

            // Thực hiện điều hướng nếu nextWindow đã được khởi tạo
            if (nextWindow != null)
            {
                // Gọi hàm helper bạn đã cung cấp
                NavigationHelper.Navigate(currentWindow, nextWindow);
            }
        }

        // ============================================================
        // 3. SỰ KIỆN ĐĂNG XUẤT
        // ============================================================
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            Window currentWindow = Window.GetWindow(this);

            // Khởi tạo màn hình Đăng Nhập
            // Đảm bảo class Dang_Nhap tồn tại trong namespace BTL_Nhom6
            Dang_Nhap loginWindow = new Dang_Nhap();

            NavigationHelper.Navigate(currentWindow, loginWindow);
        }
    }
}