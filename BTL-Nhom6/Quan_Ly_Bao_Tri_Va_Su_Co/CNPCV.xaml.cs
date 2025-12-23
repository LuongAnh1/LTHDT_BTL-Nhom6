using BTL_Nhom6.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class CNPCV : Window
    {
        public ObservableCollection<KyThuatVienModel> DanhSachKTV { get; set; }

        public CNPCV()
        {
            InitializeComponent();
            LoadData();
            this.DataContext = this;
        }

        private void LoadData()
        {
            // Bổ sung danh sách dài để có thể cuộn (Scroll)
            DanhSachKTV = new ObservableCollection<KyThuatVienModel>
            {
                new KyThuatVienModel { Initials = "NA", TenKTV = "Nguyễn Văn A", Email = "nguyenvana@example.com", MaNV = "KT01", ChuyenMon = "Điện - Điện lạnh", CongViecCho = "3 Ưu tiên cao", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "TB", TenKTV = "Trần Thị B", Email = "tranthib@example.com", MaNV = "KT02", ChuyenMon = "Cơ khí", CongViecCho = "1 Trung bình", TrangThai = "Đang bận", StatusColor = Brushes.Orange },
                new KyThuatVienModel { Initials = "LC", TenKTV = "Lê Văn C", Email = "levanc@example.com", MaNV = "KT03", ChuyenMon = "Công nghệ thông tin", CongViecCho = "0", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "PN", TenKTV = "Phạm Thị N", Email = "phamthin@example.com", MaNV = "KT04", ChuyenMon = "Hệ thống nước", CongViecCho = "1 Ưu tiên cao", TrangThai = "Nghỉ phép", StatusColor = Brushes.Gray },
                new KyThuatVienModel { Initials = "VH", TenKTV = "Vũ Văn H", Email = "vuvanh@example.com", MaNV = "KT05", ChuyenMon = "Bảo trì tổng hợp", CongViecCho = "5 Bình thường", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "HD", TenKTV = "Hoàng Văn D", Email = "hoangvand@example.com", MaNV = "KT06", ChuyenMon = "Điện tử", CongViecCho = "2 Ưu tiên cao", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "MT", TenKTV = "Mai Anh T", Email = "maianht@example.com", MaNV = "KT07", ChuyenMon = "Cơ khí chính xác", CongViecCho = "1 Trung bình", TrangThai = "Đang bận", StatusColor = Brushes.Orange },
                new KyThuatVienModel { Initials = "DL", TenKTV = "Đỗ Văn L", Email = "dovanl@example.com", MaNV = "KT08", ChuyenMon = "Hệ thống PCCC", CongViecCho = "4 Bình thường", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "QK", TenKTV = "Quách Tuấn K", Email = "quachtuank@example.com", MaNV = "KT09", ChuyenMon = "Mạng máy tính", CongViecCho = "0", TrangThai = "Nghỉ phép", StatusColor = Brushes.Gray },
                new KyThuatVienModel { Initials = "BT", TenKTV = "Bùi Minh T", Email = "buiminht@example.com", MaNV = "KT10", ChuyenMon = "Điện công nghiệp", CongViecCho = "6 Bình thường", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "TT", TenKTV = "Trương Văn T", Email = "truongvant@example.com", MaNV = "KT11", ChuyenMon = "Hàn xì", CongViecCho = "1 Ưu tiên cao", TrangThai = "Đang bận", StatusColor = Brushes.Orange },
                new KyThuatVienModel { Initials = "NL", TenKTV = "Nguyễn Gia L", Email = "nguyengial@example.com", MaNV = "KT12", ChuyenMon = "Bảo trì thang máy", CongViecCho = "2 Trung bình", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "PH", TenKTV = "Phan Văn H", Email = "phanvanh@example.com", MaNV = "KT13", ChuyenMon = "Điện lạnh", CongViecCho = "0", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green },
                new KyThuatVienModel { Initials = "VT", TenKTV = "Võ Văn T", Email = "vovant@example.com", MaNV = "KT14", ChuyenMon = "Cấp thoát nước", CongViecCho = "3 Ưu tiên cao", TrangThai = "Đang bận", StatusColor = Brushes.Orange },
                new KyThuatVienModel { Initials = "DP", TenKTV = "Dương Văn P", Email = "duongvanp@example.com", MaNV = "KT15", ChuyenMon = "Tự động hóa", CongViecCho = "1 Bình thường", TrangThai = "Đang hoạt động", StatusColor = Brushes.Green }
            };

            dgKyThuatVien.ItemsSource = DanhSachKTV;
        }

        // --- ĐIỀU HƯỚNG TABS ---
        private void Button_QLYCBT_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLYCBT());
        }

        private void Button_DieuPhoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LKH_va_DP());
        }

        private void Button_NghiemThu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new KKVT_va_NT());
        }

        private void Button_XemCongViec_Click(object sender, RoutedEventArgs e)
        {
            var ktv = (sender as Button).DataContext as KyThuatVienModel;
            if (ktv != null)
            {
                // Truyền thông tin KTV đã chọn sang cửa sổ chi tiết
                ChiTietCongViecKTV detailWindow = new ChiTietCongViecKTV(ktv);
                detailWindow.Owner = this; // Để cửa sổ con hiện trên cửa sổ cha
                detailWindow.ShowDialog();
            }
        }

        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }

    public class KyThuatVienModel
    {
        public string Initials { get; set; }
        public string TenKTV { get; set; }
        public string Email { get; set; }
        public string MaNV { get; set; }
        public string ChuyenMon { get; set; }
        public string CongViecCho { get; set; }
        public string TrangThai { get; set; }
        public SolidColorBrush StatusColor { get; set; }
    }
}