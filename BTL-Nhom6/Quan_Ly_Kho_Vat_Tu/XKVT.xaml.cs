using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class XKVT : Window
    {
        public XKVT()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var data = new List<PhieuXuat>
            {
                new PhieuXuat { MaPhieu = "PX-2023-001", NgayXuat = "25/10/2023", NguoiNhan = "Nguyễn Văn A", BoPhan = "Bộ phận Bảo trì", TongSL = 15, TrangThai = "Hoàn thành" },
                new PhieuXuat { MaPhieu = "PX-2023-002", NgayXuat = "25/10/2023", NguoiNhan = "Trần Thị B", BoPhan = "Kho vật tư", TongSL = 45, TrangThai = "Chờ duyệt" },
                new PhieuXuat { MaPhieu = "PX-2023-003", NgayXuat = "24/10/2023", NguoiNhan = "Lê Văn C", BoPhan = "Kỹ thuật", TongSL = 8, TrangThai = "Đang xử lý" },
            };
            dgXuatKho.ItemsSource = data;
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

        // 3. Xuất kho (Trang hiện tại)
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e) { }

        // 4. Thẻ kho
        private void Button_TheKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LSGD());
        }

        #endregion
    }

    public class PhieuXuat
    {
        public string MaPhieu { get; set; }
        public string NgayXuat { get; set; }
        public string NguoiNhan { get; set; }
        public string BoPhan { get; set; }
        public int TongSL { get; set; }
        public string TrangThai { get; set; }
    }
}