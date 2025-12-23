using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class ChiTietCongViecKTV : Window
    {
        public ObservableCollection<PhieuCongViecModel> DanhSachPhieu { get; set; }

        public ChiTietCongViecKTV(KyThuatVienModel ktv)
        {
            InitializeComponent();

            if (ktv != null)
            {
                // Hiển thị thông tin KTV lên header
                txtTenKTV.Text = ktv.TenKTV;
                txtMaNV.Text = "Mã NV: " + ktv.MaNV;
                txtChuyenMon.Text = "Chuyên môn: " + ktv.ChuyenMon;
                txtInitials.Text = ktv.Initials;
                txtTrangThai.Text = ktv.TrangThai;
            }

            LoadCongViecSample();
            this.DataContext = this;
        }

        private void LoadCongViecSample()
        {
            var converter = new HeaderJobConverter();
            DanhSachPhieu = new ObservableCollection<PhieuCongViecModel>
            {
                new PhieuCongViecModel {
                    MaPhieu = "PCV001", TenThietBi = "Máy tính Dell OptiPlex",
                    MoTaLoi = "Thay RAM và nâng cấp SSD", MucUuTien = "Cao",
                    PriorityBg = (SolidColorBrush)converter.Convert("#FEE2E2"),
                    PriorityFg = Brushes.Red,
                    TrangThai = "Đang thực hiện", StatusColor = Brushes.Blue, StatusIcon = "ProgressWrench"
                },
                new PhieuCongViecModel {
                    MaPhieu = "PCV042", TenThietBi = "Máy in HP LaserJet",
                    MoTaLoi = "Kẹt giấy liên tục và mờ chữ", MucUuTien = "Trung bình",
                    PriorityBg = (SolidColorBrush)converter.Convert("#FEF3C7"),
                    PriorityFg = Brushes.DarkOrange,
                    TrangThai = "Chờ linh kiện", StatusColor = Brushes.Orange, StatusIcon = "ClockOutline"
                },
                new PhieuCongViecModel {
                    MaPhieu = "PCV089", TenThietBi = "Điều hòa Daikin 12000BTU",
                    MoTaLoi = "Vệ sinh định kỳ và nạp gas", MucUuTien = "Thấp",
                    PriorityBg = (SolidColorBrush)converter.Convert("#DCFCE7"),
                    PriorityFg = Brushes.Green,
                    TrangThai = "Hoàn thành", StatusColor = Brushes.Green, StatusIcon = "CheckCircleOutline"
                }
            };
            dgCongViec.ItemsSource = DanhSachPhieu;
        }

        // PHẢI CÓ HÀM NÀY ĐỂ XAML KHÔNG LỖI
        private void SidebarMenu_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_CapNhatTienDo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Mở Form cập nhật tiến độ công việc...");
        }
    }

    public class PhieuCongViecModel
    {
        public string MaPhieu { get; set; }
        public string TenThietBi { get; set; }
        public string MoTaLoi { get; set; }
        public string MucUuTien { get; set; }
        public SolidColorBrush PriorityBg { get; set; }
        public SolidColorBrush PriorityFg { get; set; }
        public string TrangThai { get; set; }
        public SolidColorBrush StatusColor { get; set; }
        public string StatusIcon { get; set; }
    }

    public class HeaderJobConverter
    {
        public object Convert(string hex) => (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
    }
}