using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCCPVT : Window
    {
        public BCCPVT()
        {
            InitializeComponent();
            LoadData();

            // Xử lý kéo thả cửa sổ (vì WindowStyle="None")
            this.MouseDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) this.DragMove(); };
        }

        private void LoadData()
        {
            var listData = new List<ChiPhiModel>
            {
                new ChiPhiModel { MaPhieu = "PBT-00123", LoaiChiPhi = "Nhân công", NoiDung = "Bảo trì định kỳ máy dập A-01", Ngay = "25/10/2023", SoTien = "2,500,000" },
                new ChiPhiModel { MaPhieu = "PXK-00451", LoaiChiPhi = "Vật tư", NoiDung = "Xuất kho vòng bi cho PBT-00123", Ngay = "25/10/2023", SoTien = "1,200,000" },
                new ChiPhiModel { MaPhieu = "PBT-00124", LoaiChiPhi = "Nhân công", NoiDung = "Sửa chữa hệ thống điện xưởng 2", Ngay = "26/10/2023", SoTien = "3,000,000" },
                new ChiPhiModel { MaPhieu = "PXK-00452", LoaiChiPhi = "Vật tư", NoiDung = "Xuất kho dây điện, aptomat", Ngay = "26/10/2023", SoTien = "5,500,000" },
                new ChiPhiModel { MaPhieu = "PBT-00125", LoaiChiPhi = "Nhân công", NoiDung = "Bảo trì hệ thống băng chuyền", Ngay = "27/10/2023", SoTien = "1,800,000" },
                new ChiPhiModel { MaPhieu = "PXK-00453", LoaiChiPhi = "Vật tư", NoiDung = "Dầu bôi trơn công nghiệp (50L)", Ngay = "28/10/2023", SoTien = "4,200,000" },
            };
            dgChiPhi.ItemsSource = listData;
        }

        #region ĐIỀU HƯỚNG TABS (BÁO CÁO THỐNG KÊ)

        // 1. Chi phí vật tư (Trang hiện tại)
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e)
        {
            // Đang ở trang này nên không làm gì
        }

        // 2. Hiệu suất bảo trì
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCHSBT());
        }

        // 3. Năng suất Kỹ thuật viên
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCNSKTV());
        }

        // 4. Theo dõi bảo hành & Nhà cung cấp
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCTDBH_va_NCC());
        }

        // 5. Tình trạng thiết bị
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCTTTB());
        }

        // 6. Thống kê lỗi & Sự cố
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TKTSL_va_SC());
        }

        #endregion

        // Sự kiện xuất báo cáo (nếu cần)
        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tính năng xuất báo cáo đang được phát triển!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Model dữ liệu cho DataGrid
    public class ChiPhiModel
    {
        public string MaPhieu { get; set; }
        public string LoaiChiPhi { get; set; }
        public string NoiDung { get; set; }
        public string Ngay { get; set; }
        public string SoTien { get; set; }
    }
}