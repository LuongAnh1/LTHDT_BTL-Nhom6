using BTL_Nhom6.Helper;
using BTL_Nhom6.Quan_Ly_Kho_Vat_Tu;
using LiveCharts;
using LiveCharts.Wpf; // Thư viện LiveCharts
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media; // Cần thiết để dùng Color, Brush

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    /// <summary>
    /// Logic tương tác cho màn hình Báo cáo Hiệu suất Bảo trì
    /// </summary>
    public partial class BCHSBT : Window, INotifyPropertyChanged
    {
        // =============================================================
        // 1. KHAI BÁO CÁC BIẾN DỮ LIỆU (BINDING PROPERTIES)
        // =============================================================

        #region Dữ liệu Thống kê (Số & Chữ)

        private int _soPhieuDungHan;
        public int SoPhieuDungHan
        {
            get => _soPhieuDungHan;
            set { _soPhieuDungHan = value; OnPropertyChanged(); }
        }

        private int _soPhieuTreHan;
        public int SoPhieuTreHan
        {
            get => _soPhieuTreHan;
            set { _soPhieuTreHan = value; OnPropertyChanged(); }
        }

        private int _tyLeHoanThanh;
        public int TyLeHoanThanh
        {
            get => _tyLeHoanThanh;
            set { _tyLeHoanThanh = value; OnPropertyChanged(); }
        }

        // Danh sách cho ComboBox
        public ObservableCollection<string> DanhSachThietBi { get; set; }
        public ObservableCollection<string> DanhSachKyThuatVien { get; set; }

        #endregion

        #region Dữ liệu Biểu đồ (LiveCharts)

        // Biểu đồ Tròn (Donut)
        private SeriesCollection _donutChartData;
        public SeriesCollection DonutChartData
        {
            get => _donutChartData;
            set { _donutChartData = value; OnPropertyChanged(); }
        }

        // Biểu đồ Đường (Line - MTTR)
        private SeriesCollection _mttrChartSeries;
        public SeriesCollection MttrChartSeries
        {
            get => _mttrChartSeries;
            set { _mttrChartSeries = value; OnPropertyChanged(); }
        }

        // Nhãn trục hoành cho biểu đồ đường (Tháng 1, Tháng 2...)
        private ObservableCollection<string> _mttrLabels;
        public ObservableCollection<string> MttrLabels
        {
            get => _mttrLabels;
            set { _mttrLabels = value; OnPropertyChanged(); }
        }

        #endregion

        // =============================================================
        // 2. KHỞI TẠO & LOAD DỮ LIỆU
        // =============================================================
        public BCHSBT()
        {
            InitializeComponent();

            // Bước 1: Load dữ liệu mẫu vào các biến
            LoadSampleData();

            // Bước 2: Gán DataContext để XAML nhận diện được dữ liệu
            this.DataContext = this;
        }

        private void LoadSampleData()
        {
            // --- A. Dữ liệu ComboBox ---
            DanhSachThietBi = new ObservableCollection<string>
            {
                "Tất cả thiết bị",
                "Máy cắt CNC - Model X1",
                "Hệ thống băng tải Line 2",
                "Robot hàn tự động ABB",
                "Máy nén khí trung tâm"
            };

            DanhSachKyThuatVien = new ObservableCollection<string>
            {
                "Tất cả KTV",
                "Nguyễn Văn An",
                "Trần Thị Bích",
                "Lê Hoàng Nam",
                "Phạm Quốc Việt"
            };

            // --- B. Dữ liệu Số liệu thống kê & Biểu đồ Tròn ---
            int dungHan = 125;
            int treHan = 22;

            SoPhieuDungHan = dungHan;
            SoPhieuTreHan = treHan;

            // Tính % (tránh chia cho 0)
            int tong = dungHan + treHan;
            TyLeHoanThanh = tong > 0 ? (int)((double)dungHan / tong * 100) : 0;

            // Cấu hình Biểu đồ Tròn (Pie/Donut)
            DonutChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Đúng hạn",
                    Values = new ChartValues<double> { dungHan },
                    Fill = new SolidColorBrush(Color.FromRgb(30, 64, 175)), // Màu #1E40AF (Xanh đậm)
                    StrokeThickness = 0,
                    DataLabels = false
                },
                new PieSeries
                {
                    Title = "Trễ hạn",
                    Values = new ChartValues<double> { treHan },
                    Fill = new SolidColorBrush(Color.FromRgb(239, 246, 255)), // Màu #EFF6FF (Xanh nhạt)
                    StrokeThickness = 0,
                    DataLabels = false
                }
            };

            // --- C. Dữ liệu Biểu đồ Đường (MTTR) ---

            // 1. Nhãn trục X
            MttrLabels = new ObservableCollection<string>
            {
                "Thg 1", "Thg 2", "Thg 3", "Thg 4", "Thg 5", "Thg 6"
            };

            // 2. Tạo màu Gradient cho vùng dưới đường kẻ
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            gradientBrush.GradientStops.Add(new GradientStop(Color.FromRgb(30, 64, 175), 0)); // Màu chính
            gradientBrush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));         // Trong suốt

            // 3. Cấu hình Series đường
            MttrChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "MTTR",
                    // Dữ liệu mẫu (Giờ): 2h, 4h, 7h...
                    Values = new ChartValues<double> { 2, 4, 7, 5, 3, 6 },
                    
                    // Đường kẻ (Stroke)
                    Stroke = new SolidColorBrush(Color.FromRgb(30, 64, 175)),
                    StrokeThickness = 3,
                    
                    // Vùng phủ màu (Fill)
                    Fill = gradientBrush,
                    
                    // Điểm tròn (Point)
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    PointForeground = Brushes.White // Điểm màu trắng, viền tự lấy theo Stroke
                }
            };
        }

        // =============================================================
        // 3. CÁC HÀM CHUYỂN TRANG (NAVIGATION)
        // =============================================================

        // 1. Chi phí & Vật tư
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCCPVT());
        }

        // 2. Hiệu suất bảo trì (Trang hiện tại - Không làm gì)
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e)
        {
            
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
            NavigationHelper.Navigate(this, new BCTTTB());
        }

        // 6. Thống kê Lỗi
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TKTSL_va_SC());
        }

        // =============================================================
        // 4. HỖ TRỢ BINDING (INotifyPropertyChanged)
        // =============================================================
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Thêm hàm này vào trong class BCHSBT (trước dấu đóng ngoặc } cuối cùng)

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            // Mô phỏng việc load lại dữ liệu khi bấm nút "Áp dụng"
            // Bạn có thể viết logic random dữ liệu hoặc gọi API ở đây

            // Ví dụ: Random lại số liệu thống kê
            Random r = new Random();

            // Cập nhật số liệu (Giả sử bạn đang dùng các biến này cho BCHSBT)
            SoPhieuDungHan = r.Next(50, 200);
            SoPhieuTreHan = r.Next(5, 50);
            int tong = SoPhieuDungHan + SoPhieuTreHan;
            TyLeHoanThanh = tong > 0 ? (int)((double)SoPhieuDungHan / tong * 100) : 0;

            // Cập nhật biểu đồ (nếu có)
            // Ví dụ cập nhật Donut Chart nếu bạn đang dùng nó
            if (DonutChartData != null && DonutChartData.Count >= 2)
            {
                DonutChartData[0].Values[0] = (double)SoPhieuDungHan;
                DonutChartData[1].Values[0] = (double)SoPhieuTreHan;
            }

            MessageBox.Show("Đã cập nhật dữ liệu báo cáo!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}