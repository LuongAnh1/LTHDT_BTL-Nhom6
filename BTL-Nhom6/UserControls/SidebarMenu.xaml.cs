using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc;
using BTL_Nhom6.Quan_Tri_He_Thong;
using BTL_Nhom6.Quan_Ly_Kho_Vat_Tu;
using BTL_Nhom6.Quan_Ly_Thiet_Bi;
using BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co;// Namespace chứa NavigationHelper
using BTL_Nhom6.Bao_Cao_Thong_Ke;// Namespace chứa NavigationHelper

// using BTL_Nhom6.Views; // Mở comment nếu các Window nằm trong thư mục Views

namespace BTL_Nhom6.UserControls
{
    public partial class SidebarMenu : UserControl
    {
        public SidebarMenu()
        {
            InitializeComponent();

            // Gọi hàm phân quyền khi Control được tải xong
            this.Loaded += SidebarMenu_Loaded;
        }

        // ============================================================
        // HÀM PHÂN QUYỀN HIỂN THỊ MENU
        // ============================================================
        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
            int roleId = UserSession.CurrentRoleID;

            // Nếu là KHÁCH HÀNG (ID = 11)
            if (roleId == 11)
            {
                // Ẩn Quản lý thông tin danh mục
                if (btnQLTTDM != null) btnQLTTDM.Visibility = Visibility.Collapsed;

                // Ẩn Quản lý kho vật tư
                if (btnQLKVT != null) btnQLKVT.Visibility = Visibility.Collapsed;

                // Ẩn Báo cáo thống kê
                if (btnBCTK != null) btnBCTK.Visibility = Visibility.Collapsed;

                // (Tùy chọn) Ẩn thêm Quản trị hệ thống nếu muốn
                // if (btnQTHT != null) btnQTHT.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Nếu là Nhân viên/Admin thì hiện lại (đề phòng trường hợp đăng xuất rồi đăng nhập lại user khác)
                if (btnQLTTDM != null) btnQLTTDM.Visibility = Visibility.Visible;
                if (btnQLKVT != null) btnQLKVT.Visibility = Visibility.Visible;
                if (btnBCTK != null) btnBCTK.Visibility = Visibility.Visible;
            }
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
            var item = sender as SidebarItem;
            if (item == null || string.IsNullOrEmpty(item.NavTag)) return;

            string tag = item.NavTag;

            if (CurrentItem == tag) return;

            Window currentWindow = Window.GetWindow(this);
            Window nextWindow = null;
            int roleId = UserSession.CurrentRoleID;
            switch (tag)
            {
                case "Home":
                    nextWindow = new Trang_Chu();
                    break;

                case "QTHT":
                    // Logic điều hướng thông minh đã làm ở bước trước
                    if (roleId == 11) nextWindow = new TDMK_va_TTCN();
                    else if (roleId != 1 && roleId != 2) nextWindow = new QLHSKN();
                    else nextWindow = new QLND_va_PQ();
                    break;

                case "QLTTDM":

                    if (roleId == 11) return; // Nếu là KHÁCH HÀNG thì không cho vào
                    nextWindow = new QLVTPB();
                    break;

                case "QLTB":
                    nextWindow = new HSTB_va_QR();
                    break;

                case "QLQTBT":
                    nextWindow = new QLYCBT();
                    break;

                case "QLKVT":
                    // Chặn ở tầng Code (bảo mật lớp 2) đề phòng UI chưa ẩn kịp
                    if (UserSession.CurrentRoleID == 11) return; // Nếu là KHÁCH HÀNG thì không cho vào

                    nextWindow = new DMVT_va_DM();
                    break;

                case "BCTK":
                    if (UserSession.CurrentRoleID == 11) return; // Nếu là KHÁCH HÀNG thì không cho vào
                    nextWindow = new BCCPVT();
                    break;

                default:
                    break;
            }

            if (nextWindow != null)
            {
                NavigationHelper.Navigate(currentWindow, nextWindow);
            }
        }

        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            Window currentWindow = Window.GetWindow(this);

            // Xóa session khi đăng xuất
            UserSession.Clear();

            Dang_Nhap loginWindow = new Dang_Nhap();
            NavigationHelper.Navigate(currentWindow, loginWindow);
        }
    }
}