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

            // Kiểm tra nếu đang ở trang đó rồi thì không load lại (trừ trường hợp muốn refresh)
            if (CurrentItem == tag) return;

            Window currentWindow = Window.GetWindow(this);
            Window nextWindow = null;

            switch (tag)
            {
                case "Home":
                    nextWindow = new Trang_Chu();
                    break;

                case "QTHT":
                    // --- SỬA LOGIC ĐIỀU HƯỚNG DỰA TRÊN QUYỀN ---
                    int roleId = UserSession.CurrentRoleID;

                    if (roleId == 11) // Khách hàng
                    {
                        // Khách hàng vào QTHT -> Chuyển đến Thay đổi mật khẩu & TTCN
                        nextWindow = new TDMK_va_TTCN();
                    }
                    else if (roleId != 1) // Nhân viên (Không phải Admin)
                    {
                        // Nhân viên (KTV, Thủ kho, Quản lý...) -> Chuyển đến Hồ sơ kỹ năng
                        nextWindow = new QLHSKN();
                    }
                    else // Admin (RoleID = 1)
                    {
                        // Admin -> Vào trang Quản lý người dùng & Phân quyền đầy đủ
                        nextWindow = new QLND_va_PQ();
                    }
                    break;

                case "QLTTDM":
                    nextWindow = new QLVTPB();
                    break;

                case "QLTB":
                    nextWindow = new HSTB_va_QR();
                    break;

                case "QLQTBT":
                    nextWindow = new QLYCBT();
                    break;

                case "QLKVT":
                    // Có thể thêm logic chặn Khách hàng vào kho nếu cần
                    if (UserSession.CurrentRoleID == 11)
                    {
                        MessageBox.Show("Bạn không có quyền truy cập Kho vật tư.", "Thông báo");
                        return;
                    }
                    nextWindow = new DMVT_va_DM();
                    break;

                case "BCTK":
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