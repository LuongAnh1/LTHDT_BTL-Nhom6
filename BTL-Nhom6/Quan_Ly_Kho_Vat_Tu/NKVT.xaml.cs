using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class NKVT : Window
    {
        public NKVT()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var data = new List<PhieuNhap>
            {
                new PhieuNhap { MaPhieu = "PN-2023-085", NgayNhap = "25/10/2023", NhaCungCap = "Cty TNHH Công Nghệ Việt", DiaChi = "HCM - Quận 1", TongSL = 150, TrangThai = "Hoàn thành" },
                new PhieuNhap { MaPhieu = "PN-2023-084", NgayNhap = "24/10/2023", NhaCungCap = "Vật Liệu Xây Dựng Bình An", DiaChi = "Đà Nẵng", TongSL = 500, TrangThai = "Đang xử lý" },
                new PhieuNhap { MaPhieu = "PN-2023-083", NgayNhap = "22/10/2023", NhaCungCap = "Nhà Cung Cấp Thành Đạt", DiaChi = "Hà Nội", TongSL = 20, TrangThai = "Chờ duyệt" },
                new PhieuNhap { MaPhieu = "PN-2023-082", NgayNhap = "20/10/2023", NhaCungCap = "Kho Tổng Miền Nam", DiaChi = "Bình Dương", TongSL = 1200, TrangThai = "Đã hủy" },
            };
            dgNhapKho.ItemsSource = data;
        }

        #region ĐIỀU HƯỚNG TABS

        // 1. Danh mục
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DMVT_va_DM());
        }

        // 2. Nhập kho (Trang hiện tại)
        private void Button_NhapKho_Click(object sender, RoutedEventArgs e) { }

        // 3. Xuất kho
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new XKVT());
        }

        // 4. Thẻ kho
        private void Button_TheKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LSGD());
        }

        #endregion
    }

    public class PhieuNhap
    {
        public string MaPhieu { get; set; }
        public string NgayNhap { get; set; }
        public string NhaCungCap { get; set; }
        public string DiaChi { get; set; }
        public int TongSL { get; set; }
        public string TrangThai { get; set; }
    }
}