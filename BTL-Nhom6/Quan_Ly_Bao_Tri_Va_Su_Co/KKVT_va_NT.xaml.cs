using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class KKVT_va_NT : Window
    {
        public KKVT_va_NT()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Dữ liệu mẫu cho bảng vật tư
            var data = new List<ChiTietVatTu>
            {
                new ChiTietVatTu {
                    TenVatTu = "Dầu bôi trơn CN-50",
                    DonVi = "Lít",
                    SoLuong = 2,
                    DonGia = "150,000",
                    ThanhTien = "300,000",
                    DanhSachVatTu = new List<string> { "Dầu bôi trơn CN-50", "Vòng bi SKF 6205", "Dây cáp điện 3x1.5" }
                },
                new ChiTietVatTu {
                    TenVatTu = "Khăn lau công nghiệp",
                    DonVi = "Hộp",
                    SoLuong = 1,
                    DonGia = "50,000",
                    ThanhTien = "50,000",
                    DanhSachVatTu = new List<string> { "Khăn lau công nghiệp", "Vòng bi SKF 6205" }
                }
            };

            dgVatTu.ItemsSource = data;
        }

        #region CHUYỂN TRANG (NAVIGATION)

        // 1. Quản lý Yêu cầu
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLYCBT());
        }

        // 2. Điều phối công việc
        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LKH_va_DP());
        }

        // 3. Cập nhật phiếu công việc
        private void Button_CapNhat_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new CNPCV());
        }

        // 4. Kê khai vật tư (Trang hiện tại)
        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e) { }

        #endregion
    }

    public class ChiTietVatTu
    {
        public string TenVatTu { get; set; }
        public string DonVi { get; set; }
        public int SoLuong { get; set; }
        public string DonGia { get; set; }
        public string ThanhTien { get; set; }
        public List<string> DanhSachVatTu { get; set; } // List cho ComboBox
    }
}