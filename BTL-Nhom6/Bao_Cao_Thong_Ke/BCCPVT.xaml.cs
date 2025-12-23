using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Kho_Vat_Tu;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    // Class Model cho DataGrid
    public class ChiPhiDTO
    {
        public string MaPhieu { get; set; }
        public string LoaiChiPhi { get; set; } // "Nhân công" hoặc "Vật tư"
        public string NoiDung { get; set; }
        public string Ngay { get; set; }
        public decimal SoTien { get; set; }

        // Property phụ để hiển thị định dạng tiền tệ
        public string SoTienString => SoTien.ToString("N0");
    }

    /// <summary>
    /// Interaction logic for BCCPVT.xaml
    /// </summary>
    public partial class BCCPVT : Window, INotifyPropertyChanged
    {
        // ================= BIẾN BINDING =================

        #region Chart Properties
        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set { _seriesCollection = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _labels;
        public ObservableCollection<string> Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(); }
        }

        // Formatter để hiển thị trục Y (Ví dụ: 20,000,000 -> 20M)
        public Func<double, string> CurrencyFormatter { get; set; }
        #endregion

        #region DataGrid Properties
        private ObservableCollection<ChiPhiDTO> _danhSachChiPhi;
        public ObservableCollection<ChiPhiDTO> DanhSachChiPhi
        {
            get => _danhSachChiPhi;
            set { _danhSachChiPhi = value; OnPropertyChanged(); UpdateTotal(); }
        }

        private string _tongTienHienThi;
        public string TongTienHienThi
        {
            get => _tongTienHienThi;
            set { _tongTienHienThi = value; OnPropertyChanged(); }
        }
        #endregion

        // ================= KHỞI TẠO =================

        public BCCPVT()
        {
            InitializeComponent();
            DataContext = this;

            // Định dạng trục Y: Chuyển số về dạng triệu (M)
            CurrencyFormatter = value => (value / 1000000).ToString("N0") + "M";

            LoadDataBanDau();
        }

        private void LoadDataBanDau()
        {
            // 1. Dữ liệu Biểu đồ (Mặc định)
            Labels = new ObservableCollection<string> { "T5/2024", "T6/2024", "T7/2024", "T8/2024", "T9/2024", "T10/2024" };

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Nhân công",
                    Values = new ChartValues<double> { 5000000, 7000000, 4500000, 8000000, 6500000, 9000000 },
                    Fill = (Brush)Application.Current.Resources["PrimaryBlueBrush"], // Màu xanh đậm
                    MaxColumnWidth = 35
                },
                new ColumnSeries
                {
                    Title = "Vật tư",
                    Values = new ChartValues<double> { 8000000, 10000000, 6000000, 12000000, 9000000, 11000000 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#93C5FD")), // Màu xanh nhạt
                    MaxColumnWidth = 35
                }
            };

            // 2. Dữ liệu DataGrid (Mặc định)
            DanhSachChiPhi = new ObservableCollection<ChiPhiDTO>
            {
                new ChiPhiDTO { MaPhieu = "CP-2410-001", LoaiChiPhi = "Nhân công", NoiDung = "Bảo trì máy cắt CNC - Line 1", Ngay = "20/10/2024", SoTien = 2500000 },
                new ChiPhiDTO { MaPhieu = "CP-2410-002", LoaiChiPhi = "Vật tư", NoiDung = "Thay dầu nhớt máy nén khí", Ngay = "21/10/2024", SoTien = 1200000 },
                new ChiPhiDTO { MaPhieu = "CP-2410-003", LoaiChiPhi = "Vật tư", NoiDung = "Mua linh kiện cảm biến nhiệt", Ngay = "22/10/2024", SoTien = 5600000 },
                new ChiPhiDTO { MaPhieu = "CP-2410-004", LoaiChiPhi = "Nhân công", NoiDung = "Sửa chữa băng tải đóng gói", Ngay = "23/10/2024", SoTien = 3000000 },
                new ChiPhiDTO { MaPhieu = "CP-2410-005", LoaiChiPhi = "Vật tư", NoiDung = "Bộ lọc khí thay thế", Ngay = "25/10/2024", SoTien = 850000 }
            };
        }

        // Tính tổng tiền hiển thị ở Footer
        private void UpdateTotal()
        {
            if (DanhSachChiPhi != null)
            {
                decimal total = DanhSachChiPhi.Sum(x => x.SoTien);
                TongTienHienThi = total.ToString("N0");
            }
        }

        // ================= XỬ LÝ SỰ KIỆN =================

        // Sự kiện: Khi bấm nút "Áp dụng" ở bộ lọc
        // -> Sẽ random dữ liệu để mô phỏng biểu đồ động
        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            Random r = new Random();

            // 1. Cập nhật biểu đồ với số liệu ngẫu nhiên
            foreach (var series in SeriesCollection)
            {
                var values = new ChartValues<double>();
                for (int i = 0; i < 6; i++)
                {
                    // Random từ 2 triệu đến 15 triệu
                    values.Add(r.Next(20, 150) * 100000);
                }
                series.Values = values;
            }

            // 2. Cập nhật DataGrid
            DanhSachChiPhi.Clear();
            int soDong = r.Next(3, 8); // Tạo từ 3 đến 8 dòng
            for (int i = 0; i < soDong; i++)
            {
                bool isNhanCong = r.Next(0, 2) == 0;
                DanhSachChiPhi.Add(new ChiPhiDTO
                {
                    MaPhieu = $"CP-NEW-{r.Next(100, 999)}",
                    LoaiChiPhi = isNhanCong ? "Nhân công" : "Vật tư",
                    NoiDung = isNhanCong ? "Chi phí nhân công phát sinh" : "Mua vật tư thay thế mới",
                    Ngay = DateTime.Now.AddDays(-r.Next(0, 30)).ToString("dd/MM/yyyy"),
                    SoTien = r.Next(5, 100) * 100000
                });
            }
            UpdateTotal();
        }

        // --- NAVIGATION BUTTONS (Chuyển trang) ---

        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e)
        {
            // Trang hiện tại
        }

        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCHSBT());
        }

        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCNSKTV());
        }

        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCTDBH_va_NCC());
        }

        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCTTTB());
        }

        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TKTSL_va_SC());
        }

        // ================= INotifyPropertyChanged =================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}