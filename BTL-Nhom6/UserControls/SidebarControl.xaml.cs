using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Bao_Cao_Thong_Ke;
using BTL_Nhom6.Enums;
using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co;
using BTL_Nhom6.Quan_Ly_Kho_Vat_Tu;
using BTL_Nhom6.Quan_Lý_Thiet_Bi;
using BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc;
using BTL_Nhom6.Quan_Tri_He_Thong;
// Đảm bảo bạn đã có các Window tương ứng, nếu để trong thư mục Views thì thêm: using BTL_Nhom6.Views;

namespace BTL_Nhom6.UserControls
{
    public partial class SidebarControl : UserControl
    {
        public SidebarControl()
        {
            InitializeComponent();
        }

        #region Dependency Property (Giữ nguyên logic của bạn)

        public static readonly DependencyProperty ActiveItemProperty =
            DependencyProperty.Register(
                "ActiveItem",
                typeof(SidebarItem),
                typeof(SidebarControl),
                new PropertyMetadata(SidebarItem.None));

        public SidebarItem ActiveItem
        {
            get { return (SidebarItem)GetValue(ActiveItemProperty); }
            set { SetValue(ActiveItemProperty, value); }
        }

        #endregion

        #region Helper Method (Hàm hỗ trợ chuyển trang)

        /// <summary>
        /// Hàm chung để xử lý việc chuyển Form
        /// </summary>
        /// <param name="nextWindow">Cửa sổ muốn mở</param>
        /// <param name="targetItem">Loại menu tương ứng (để kiểm tra nếu đang ở trang đó rồi thì không load lại)</param>
        private void NavigateTo(Window nextWindow, SidebarItem targetItem)
        {
            // 1. Nếu đang ở đúng trang đó rồi thì không làm gì cả (tránh load lại gây giật lag)
            if (ActiveItem == targetItem)
            {
                // Tuy nhiên, nextWindow vừa được new ở bên ngoài, cần đóng nó lại ngay để tránh rò rỉ bộ nhớ
                nextWindow.Close();
                return;
            }

            // 2. Lấy cửa sổ hiện tại (Cửa sổ cha chứa UserControl này)
            Window currentWindow = Window.GetWindow(this);

            // 3. Gọi NavigationHelper của bạn để xử lý
            if (currentWindow != null)
            {
                NavigationHelper.Navigate(currentWindow, nextWindow);
            }
        }

        #endregion

        #region Event Handlers (Xử lý từng nút bấm)

        // 1. Trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Trang_Chu(), SidebarItem.Home);
        }

        // 2. Quản trị hệ thống
        private void Button_QTHT_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new QLND_va_PQ(), SidebarItem.System);
        }

        // 3. Quản lý thông tin danh mục
        private void Button_QLTTDM_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new QLVTPB(), SidebarItem.Category);
        }

        // 4. Quản lý thiết bị
        private void Button_QLTB_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new HSTB_va_QR(), SidebarItem.Device);
        }

        // 5. Quản lý quy trình bảo trì
        private void Button_QLQTBT_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new QLYCBT(), SidebarItem.Maintenance);
        }

        // 6. Quản lý kho
        private void Button_QLK_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new DMVT_va_DM(), SidebarItem.Inventory);
        }

        // 7. Báo cáo thống kê
        private void Button_BCTK_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Home_BCTK(), SidebarItem.Report);
        }

        // 8. Đăng xuất (Xử lý riêng một chút)
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?",
                                         "Xác nhận",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Window currentWindow = Window.GetWindow(this);
                // Thay 'Dang_Nhap' bằng tên class Form đăng nhập của bạn
                NavigationHelper.Navigate(currentWindow, new Dang_Nhap());
            }
        }

        #endregion
    }
}