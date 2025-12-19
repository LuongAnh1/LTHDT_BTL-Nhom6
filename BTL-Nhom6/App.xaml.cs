using System.Windows;
using BTL_Nhom6.Helper; // Nhớ using namespace chứa DatabaseHelper

namespace BTL_Nhom6
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Kiểm tra kết nối MySQL trước
            string loiNhan = "";
            bool ketNoiThanhCong = DatabaseHelper.TestConnection(out loiNhan);

            if (ketNoiThanhCong)
            {
                // 2. Nếu OK -> Mở màn hình Đăng nhập
                Dang_Nhap loginWindow = new Dang_Nhap();
                loginWindow.Show();
            }
            else
            {
                // 3. Nếu Lỗi -> Hiện thông báo và Tắt App luôn
                MessageBox.Show($"Không thể kết nối đến Cơ sở dữ liệu!\n\nChi tiết lỗi: {loiNhan}",
                                "Lỗi Hệ Thống",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                // Tắt ứng dụng
                this.Shutdown();
            }
        }
    }
}