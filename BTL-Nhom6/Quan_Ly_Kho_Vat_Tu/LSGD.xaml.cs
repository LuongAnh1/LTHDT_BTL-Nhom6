using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class LSGD : Window
    {
        public LSGD()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var data = new List<GiaoDichKho>
            {
                new GiaoDichKho { Ngay = "25/10/2023", MaPhieu = "PX-2023-112", LoaiGiaoDich = "Xuất kho", SoLuong = "- 20", DonVi = "Cái", NguoiLienQuan = "Nguyễn Văn A", BoPhan = "Tổ bảo trì máy nén", GhiChu = "Bảo dưỡng định kỳ máy nén khí số 2" },
                new GiaoDichKho { Ngay = "20/10/2023", MaPhieu = "PN-2023-085", LoaiGiaoDich = "Nhập kho", SoLuong = "+ 50", DonVi = "Cái", NguoiLienQuan = "Cty TNHH Công Nghệ Việt", BoPhan = "Nhà cung cấp", GhiChu = "Nhập hàng theo đơn đặt hàng PO-2023-099" },
                new GiaoDichKho { Ngay = "15/10/2023", MaPhieu = "PX-2023-105", LoaiGiaoDich = "Xuất kho", SoLuong = "- 5", DonVi = "Cái", NguoiLienQuan = "Trần Thị B", BoPhan = "Bộ phận sản xuất", GhiChu = "Thay thế linh kiện hỏng máy đóng gói" },
                new GiaoDichKho { Ngay = "10/10/2023", MaPhieu = "PN-2023-080", LoaiGiaoDich = "Nhập kho", SoLuong = "+ 100", DonVi = "Cái", NguoiLienQuan = "Kho Tổng Miền Nam", BoPhan = "Điều chuyển kho", GhiChu = "Điều chuyển nội bộ" },
                new GiaoDichKho { Ngay = "01/10/2023", MaPhieu = "KK-2023-10", LoaiGiaoDich = "Kiểm kê", SoLuong = "0", DonVi = "Cái", NguoiLienQuan = "Ban Kiểm Kê", BoPhan = "Định kỳ", GhiChu = "Số lượng thực tế khớp với sổ sách (25 cái)" },
            };
            dgLichSu.ItemsSource = data;
        }

        #region ĐIỀU HƯỚNG TABS

        // 1. Danh mục
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DMVT_va_DM());
        }

        // 2. Nhập kho
        private void Button_NhapKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NKVT());
        }

        // 3. Xuất kho
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new XKVT());
        }

        // 4. Thẻ kho (Trang hiện tại)
        private void Button_TheKho_Click(object sender, RoutedEventArgs e) { }

        #endregion
    }

    public class GiaoDichKho
    {
        public string Ngay { get; set; }
        public string MaPhieu { get; set; }
        public string LoaiGiaoDich { get; set; }
        public string SoLuong { get; set; }
        public string DonVi { get; set; }
        public string NguoiLienQuan { get; set; }
        public string BoPhan { get; set; }
        public string GhiChu { get; set; }

        public string MauSoLuong
        {
            get
            {
                if (!string.IsNullOrEmpty(SoLuong))
                {
                    if (SoLuong.Trim().StartsWith("+")) return "#2E7D32";
                    if (SoLuong.Trim().StartsWith("-")) return "#EF6C00";
                }
                return "#333333";
            }
        }
    }
}