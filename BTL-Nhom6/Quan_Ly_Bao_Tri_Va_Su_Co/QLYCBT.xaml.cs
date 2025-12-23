using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class QLYCBT : Window
    {
        public QLYCBT()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var data = new List<YeuCauBaoTri>
            {
                new YeuCauBaoTri { MucUuTien = "High", ThietBi = "Điều hòa trung tâm", MaThietBi = "DH-TT-01", MoTaLoi = "Hệ thống làm mát không hoạt động", TrangThai = "Hoàn thành", NguoiYeuCau = "Nguyễn Văn A", NguoiXuLy = "Lê Hùng", NgayYeuCau = "01/07/2024", NgayHoanTat = "02/07/2024", GhiChu = "Đã thay thế bộ lọc." },
                new YeuCauBaoTri { MucUuTien = "Medium", ThietBi = "Máy phát điện Cummins", MaThietBi = "MPD-CUM-05", MoTaLoi = "Kiểm tra định kỳ", TrangThai = "Đang thực hiện", NguoiYeuCau = "Trần Thị B", NguoiXuLy = "Nguyễn An", NgayYeuCau = "10/07/2024", NgayHoanTat = "-", GhiChu = "Đang chờ vật tư." },
                new YeuCauBaoTri { MucUuTien = "Low", ThietBi = "Thang máy Schindler", MaThietBi = "TM-SCH-T01", MoTaLoi = "Bảo dưỡng định kỳ", TrangThai = "Đang chờ xử lý", NguoiYeuCau = "Lê Văn C", NguoiXuLy = "Chưa giao", NgayYeuCau = "20/07/2024", NgayHoanTat = "-", GhiChu = "-" },
                new YeuCauBaoTri { MucUuTien = "High", ThietBi = "Hệ thống báo cháy", MaThietBi = "PCCC-BC-K01", MoTaLoi = "Đầu báo khói tầng 5 lỗi", TrangThai = "Từ chối", NguoiYeuCau = "Phạm Thị D", NguoiXuLy = "-", NgayYeuCau = "05/06/2024", NgayHoanTat = "-", GhiChu = "Yêu cầu không hợp lệ." },
                new YeuCauBaoTri { MucUuTien = "Low", ThietBi = "Server Dell R740", MaThietBi = "SRV-DELL-R740", MoTaLoi = "Bảo trì hệ thống server", TrangThai = "Hủy", NguoiYeuCau = "IT Department", NguoiXuLy = "-", NgayYeuCau = "25/07/2024", NgayHoanTat = "-", GhiChu = "Người dùng yêu cầu hủy." },
            };

            dgYeuCauBaoTri.ItemsSource = data;
        }

        #region CHUYỂN TRANG (NAVIGATION)

        // 1. Quản lý Yêu cầu (Trang hiện tại)
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e) { }

        // 2. Điều phối công việc
        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LKH_va_DP());
        }

        // 3. Cập nhật phiếu công việc (Chưa có file XAML, để trống hoặc trỏ tạm về CNPCV)
        private void Button_CapNhat_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new CNPCV());
        }

        // 4. Kê khai vật tư & Nghiệm thu
        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new KKVT_va_NT());
        }

        #endregion
    }

    public class YeuCauBaoTri
    {
        public string MucUuTien { get; set; }
        public string ThietBi { get; set; }
        public string MaThietBi { get; set; }
        public string MoTaLoi { get; set; }
        public string TrangThai { get; set; }
        public string NguoiYeuCau { get; set; }
        public string NguoiXuLy { get; set; }
        public string NgayYeuCau { get; set; }
        public string NgayHoanTat { get; set; }
        public string GhiChu { get; set; }
    }
}