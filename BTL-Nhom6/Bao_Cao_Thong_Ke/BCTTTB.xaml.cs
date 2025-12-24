using BTL_Nhom6.Helper;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public class ThietBiStatus
    {
        public string MaTB { get; set; }
        public string TenTB { get; set; }
        public string TrangThai { get; set; } // Tốt, Hỏng, Thanh lý
    }

    public partial class BCTTTB : Window, INotifyPropertyChanged
    {
        private SeriesCollection _statusSeries;
        public SeriesCollection StatusSeries
        {
            get => _statusSeries;
            set { _statusSeries = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<ThietBiStatus> _danhSachThietBi;
        public ObservableCollection<ThietBiStatus> DanhSachThietBi
        {
            get => _danhSachThietBi;
            set { _danhSachThietBi = value; NotifyPropertyChanged(); }
        }

        public BCTTTB()
        {
            InitializeComponent();
            this.DataContext = this;
            LoadData();
        }

        private void LoadData()
        {
            // 1. Dữ liệu biểu đồ tròn (Pie Chart)
            StatusSeries = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Tốt",
                    Values = new ChartValues<double> { 70 },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#22C55E"), // Xanh lá
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Hỏng",
                    Values = new ChartValues<double> { 20 },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#EF4444"), // Đỏ
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Thanh lý",
                    Values = new ChartValues<double> { 10 },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#EAB308"), // Vàng
                    DataLabels = true
                }
            };

            // 2. Dữ liệu bảng chi tiết
            DanhSachThietBi = new ObservableCollection<ThietBiStatus>
            {
                new ThietBiStatus { MaTB = "CNC-001", TenTB = "Máy phay CNC 5 trục", TrangThai = "Tốt" },
                new ThietBiStatus { MaTB = "RB-003", TenTB = "Robot hàn tự động", TrangThai = "Hỏng" },
                new ThietBiStatus { MaTB = "BC-002", TenTB = "Băng chuyền sản phẩm", TrangThai = "Tốt" },
                new ThietBiStatus { MaTB = "CNC-004", TenTB = "Máy tiện CNC", TrangThai = "Tốt" },
                new ThietBiStatus { MaTB = "KD-007", TenTB = "Máy đóng gói", TrangThai = "Thanh lý" }
            };
        }

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã lọc dữ liệu thiết bị!", "Thông báo");
            // Thực hiện logic filter ở đây
        }

        // --- NAVIGATION BUTTONS ---
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCCPVT());
        }

        // 2. Hiệu suất bảo trì (Trang hiện tại - Không làm gì)
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCHSBT());
        }

        // 3. Năng suất KTV
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCNSKTV());
        }

        // 4. Bảo hành & NCC
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCTDBH_va_NCC());
        }

        // 5. Tình trạng thiết bị
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e)
        {
           
        }

        // 6. Thống kê Lỗi
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TKTSL_va_SC());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}