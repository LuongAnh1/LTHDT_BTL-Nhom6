using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class LKH_va_DP : Window
    {
        public LKH_va_DP()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var data = new List<CongViecDieuPhoi>
            {
                new CongViecDieuPhoi { MaThietBi = "TM-SCH-T01", TenThietBi = "Thang máy Schindler", MoTaLoi = "Bảo dưỡng định kỳ", MucUuTien = "High", NguoiYeuCau = "Lê Văn C", NgayYeuCau = "20/07/2024" },
                new CongViecDieuPhoi { MaThietBi = "PCCC-KHO-02", TenThietBi = "Hệ thống chữa cháy kho", MoTaLoi = "Kiểm tra áp suất bình chữa cháy", MucUuTien = "Medium", NguoiYeuCau = "Phòng An Toàn", NgayYeuCau = "18/07/2024" },
                new CongViecDieuPhoi { MaThietBi = "HVAC-T10-03", TenThietBi = "Hệ thống HVAC tầng 10", MoTaLoi = "Kiểm tra và vệ sinh bộ lọc", MucUuTien = "Low", NguoiYeuCau = "Ban Quản lý Tòa nhà", NgayYeuCau = "15/07/2024" },
            };
            dgDieuPhoi.ItemsSource = data;
        }

        // ĐÃ THÊM: Sửa lỗi thiếu định nghĩa sự kiện Loaded trong Sidebar
        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
            // Có thể để trống nếu không xử lý gì đặc biệt
        }

        private void Button_PhanCong_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở cửa sổ chọn nhân viên kỹ thuật để phân công.", "Thông báo");
        }

        #region CHUYỂN TRANG (NAVIGATION)

        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLYCBT());
        }

        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e)
        {
            // Trang hiện tại, không cần làm gì
        }

        private void Button_CapNhat_Click(object sender, RoutedEventArgs e)
        {
            // NavigationHelper.Navigate(this, new CapNhatPhieu());
        }

        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new KKVT_va_NT());
        }

        #endregion
    }

    public class CongViecDieuPhoi
    {
        public string MaThietBi { get; set; }
        public string TenThietBi { get; set; }
        public string MoTaLoi { get; set; }
        public string MucUuTien { get; set; }
        public string NguoiYeuCau { get; set; }
        public string NgayYeuCau { get; set; }
    }
}