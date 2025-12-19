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
            // 1. Ép kiểu sender về SidebarItem
            var item = sender as SidebarItem;
            if (item == null || string.IsNullOrEmpty(item.NavTag)) return;

            string tag = item.NavTag;

            // Các phần xử lý logic chuyển trang giữ nguyên như cũ
            if (CurrentItem == tag) return;

            Window currentWindow = Window.GetWindow(this);
            Window nextWindow = null;

            switch (tag)
            {
                case "Home":
                    nextWindow = new Trang_Chu();
                    break;
                case "QTHT":
                    nextWindow = new QLND_va_PQ();
                    break;
                case "QLTTDM":
                    MessageBox.Show("Chức năng Quản lý thông tin danh mục đang phát triển");
                    break;
                case "QLTB":
                    MessageBox.Show("Chức năng Quản lý thiết bị đang phát triển");
                    break;
                case "QLQTBT":
                    MessageBox.Show("Chức năng Bảo trì đang phát triển");
                    break;
                case "QLKVT":
                    MessageBox.Show("Chức năng Quản lý kho vật tư đang phát triển");
                    break;
                case "BCTK":
                    MessageBox.Show("Chức năng Báo cáo thống kê đang phát triển");
                    break;
                default:
                    break;
            }

            if (nextWindow != null)
            {
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