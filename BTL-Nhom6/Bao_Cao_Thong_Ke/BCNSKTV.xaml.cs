using BTL_Nhom6.Helper;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    // Class DTO cho bảng Công việc (Phần dưới cùng)
    public class CongViecKTV
    {
        public string TenKTV { get; set; }
        public string MaCV { get; set; }
        public string MoTa { get; set; }
        public string TrangThai { get; set; }
        public string DoUuTien { get; set; }
    }

    // Class DTO cho Top 3 Kỹ thuật viên (Bảng bên phải)
    public class KtvThanhTich
    {
        public string TenKTV { get; set; }
        public int TongCongViec { get; set; }
        public List<string> DanhSachKyNang { get; set; }
    }

    public partial class BCNSKTV : Window, INotifyPropertyChanged
    {
        // ================= DỮ LIỆU BINDING =================

        #region Chart Properties
        private SeriesCollection _topKtvSeries;
        public SeriesCollection TopKtvSeries
        {
            get => _topKtvSeries;
            set { _topKtvSeries = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<string> _ktvLabels;
        public ObservableCollection<string> KtvLabels
        {
            get => _ktvLabels;
            set { _ktvLabels = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region DataGrid Properties
        private ObservableCollection<CongViecKTV> _danhSachCongViec;
        public ObservableCollection<CongViecKTV> DanhSachCongViec
        {
            get => _danhSachCongViec;
            set { _danhSachCongViec = value; NotifyPropertyChanged(); }
        }
        #endregion

        #region Top 3 KTV Properties
        private ObservableCollection<KtvThanhTich> _top3KtvList;
        public ObservableCollection<KtvThanhTich> Top3KtvList
        {
            get => _top3KtvList;
            set { _top3KtvList = value; NotifyPropertyChanged(); }
        }
        #endregion

        // ================= KHỞI TẠO =================

        public BCNSKTV()
        {
            InitializeComponent();
            this.DataContext = this; // Gán nguồn dữ liệu
            LoadData();
        }

        private void LoadData()
        {
            // 1. Dữ liệu Biểu đồ (Top KTV)
            KtvLabels = new ObservableCollection<string> { "Phạm Đức Huy", "Trần Thị Bình", "Lê Minh Tuấn", "Nguyễn Văn An" };

            TopKtvSeries = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "Công việc hoàn thành",
                    Values = new ChartValues<double> { 65, 72, 80, 95 },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#2563EB"),
                    DataLabels = true,
                    LabelPoint = point => point.X.ToString()
                }
            };

            // 2. Dữ liệu Bảng (Trạng thái công việc)
            DanhSachCongViec = new ObservableCollection<CongViecKTV>
            {
                new CongViecKTV { TenKTV = "Nguyễn Văn An", MaCV = "CV-2023-112", MoTa = "Kiểm tra động cơ máy ép #3", TrangThai = "Đang thực hiện", DoUuTien = "Cao" },
                new CongViecKTV { TenKTV = "Trần Thị Bình", MaCV = "CV-2023-115", MoTa = "Thay thế cảm biến nhiệt lò hơi", TrangThai = "Mới", DoUuTien = "Cao" },
                new CongViecKTV { TenKTV = "Lê Minh Tuấn", MaCV = "CV-2023-118", MoTa = "Bảo trì hệ thống băng chuyền XA-02", TrangThai = "Đang thực hiện", DoUuTien = "Trung bình" },
                new CongViecKTV { TenKTV = "Nguyễn Văn An", MaCV = "CV-2023-109", MoTa = "Hiệu chỉnh máy cắt laser", TrangThai = "Hoàn thành", DoUuTien = "Thấp" }
            };

            // 3. Dữ liệu Top 3 KTV (Bảng bên phải)
            Top3KtvList = new ObservableCollection<KtvThanhTich>
            {
                new KtvThanhTich {
                    TenKTV = "Nguyễn Văn An",
                    TongCongViec = 95,
                    DanhSachKyNang = new List<string> { "Sửa CNC", "Điện công nghiệp", "PLC" }
                },
                new KtvThanhTich {
                    TenKTV = "Lê Minh Tuấn",
                    TongCongViec = 80,
                    DanhSachKyNang = new List<string> { "Khí nén", "Thủy lực" }
                },
                new KtvThanhTich {
                    TenKTV = "Trần Thị Bình",
                    TongCongViec = 72,
                    DanhSachKyNang = new List<string> { "Bảo trì dự phòng" }
                }
            };

            TopKtvSeries = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "Công việc hoàn thành",
                    Values = new ChartValues<double> { 65, 72, 80, 95 },
                    Fill = (Brush)new BrushConverter().ConvertFrom("#2563EB"), // Màu xanh dương hiện đại
        
                    // --- CHỈNH SỐ TRÊN ĐẦU CỘT TẠI ĐÂY ---
                    DataLabels = true,             // Hiện số
                    FontSize = 14,                 // Cỡ chữ số
                    FontWeight = FontWeights.Bold, // Chữ in đậm
                    Foreground = Brushes.Black,    // Màu chữ số (có thể đổi thành Brushes.White nếu muốn để số bên trong cột)
                    LabelPoint = point => point.X.ToString()
                }
            };
        }

        // ================= SỰ KIỆN =================

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã cập nhật dữ liệu theo bộ lọc!", "Thông báo");

            // Ví dụ cập nhật Chart ngẫu nhiên
            Random r = new Random();
            TopKtvSeries[0].Values = new ChartValues<double> { r.Next(50, 70), r.Next(70, 80), r.Next(80, 90), r.Next(90, 100) };

            // Cập nhật ngẫu nhiên số liệu Top 1 để test tính năng động
            if (Top3KtvList.Count > 0)
            {
                Top3KtvList[0].TongCongViec = r.Next(100, 150);
                Top3KtvList = new ObservableCollection<KtvThanhTich>(Top3KtvList); // Làm mới danh sách để UI nhận biết
            }
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

        // ================= INotifyPropertyChanged Implementation =================

        public event PropertyChangedEventHandler PropertyChanged;

        // Đã đổi tên thành NotifyPropertyChanged để tránh trùng với phương thức hệ thống của Window
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}