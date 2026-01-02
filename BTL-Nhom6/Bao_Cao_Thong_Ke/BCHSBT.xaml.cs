using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
// --- THƯ VIỆN MỚI ---
using Microsoft.Win32;      // SaveFileDialog
using ClosedXML.Excel;      // Xuất Excel
using System.Diagnostics;   // Process.Start

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCHSBT : Window, INotifyPropertyChanged
    {
        // Services
        private readonly CategoryService _catService = new CategoryService();
        private readonly ReportService _reportService = new ReportService();

        // Binding Properties
        public List<Category> DanhSachThietBi { get; set; }
        public List<TechnicianDTO> DanhSachKyThuatVien { get; set; }

        // LiveCharts Properties
        public SeriesCollection MttrChartSeries { get; set; }
        public string[] MttrLabels { get; set; }

        // Donut Chart Data
        public SeriesCollection DonutChartData { get; set; }

        // Thống kê số liệu hiển thị trên giao diện
        private int _soPhieuDungHan;
        public int SoPhieuDungHan
        {
            get => _soPhieuDungHan;
            set { _soPhieuDungHan = value; OnPropertyChanged(nameof(SoPhieuDungHan)); }
        }

        private int _soPhieuTreHan;
        public int SoPhieuTreHan
        {
            get => _soPhieuTreHan;
            set { _soPhieuTreHan = value; OnPropertyChanged(nameof(SoPhieuTreHan)); }
        }

        private double _tyLeHoanThanh;
        public double TyLeHoanThanh
        {
            get => _tyLeHoanThanh;
            set { _tyLeHoanThanh = value; OnPropertyChanged(nameof(TyLeHoanThanh)); }
        }

        // [MỚI] Biến lưu trữ dữ liệu để xuất Excel
        private List<PerformanceDataDTO> _cachedReportData;

        public BCHSBT()
        {
            InitializeComponent();
            DataContext = this;
            LoadFilterData();

            // Mặc định load 6 tháng
            DateTime today = DateTime.Now;
            DateTime startDate = today.AddMonths(-6);
            DateTime endOfDay = new DateTime(today.Year, today.Month, today.Day, 23, 59, 59);

            DpStart.SelectedDate = null;
            DpEnd.SelectedDate = null;

            LoadReportData(startDate, endOfDay);
        }

        private void LoadFilterData()
        {
            var cats = _catService.GetAllCategories();
            cats.Insert(0, new Category { CategoryID = -1, CategoryName = "Tất cả" });
            DanhSachThietBi = cats;

            var techs = _reportService.GetTechnicians();
            techs.Insert(0, new TechnicianDTO { UserID = -1, FullName = "Tất cả" });
            DanhSachKyThuatVien = techs;

            OnPropertyChanged(nameof(DanhSachThietBi));
            OnPropertyChanged(nameof(DanhSachKyThuatVien));
        }

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            DateTime toDate = DpEnd.SelectedDate ?? DateTime.Now;
            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
            DateTime fromDate = DpStart.SelectedDate ?? toDate.AddMonths(-6);

            LoadReportData(fromDate, toDate);
        }

        private void LoadReportData(DateTime from, DateTime to)
        {
            int? catId = null;
            if (CboCategory.SelectedValue is int cId && cId != -1) catId = cId;

            int? techId = null;
            if (CboTechnician.SelectedValue is int tId && tId != -1) techId = tId;

            // Lấy dữ liệu
            var rawData = _reportService.GetPerformanceRawData(from, to, catId, techId);

            // [QUAN TRỌNG] Lưu lại vào biến toàn cục để dùng cho nút Xuất Excel
            _cachedReportData = rawData;

            ProcessMttrChart(rawData, from, to);
            ProcessPieChart(rawData);
        }

        private void ProcessMttrChart(List<PerformanceDataDTO> data, DateTime from, DateTime to)
        {
            var grouped = data
                .Where(x => x.ActualCompletion.HasValue)
                .GroupBy(x => new { x.RequestDate.Year, x.RequestDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Label = $"Th{g.Key.Month}/{g.Key.Year}",
                    AvgHours = g.Average(x => (x.ActualCompletion.Value - x.RequestDate).TotalHours)
                })
                .ToList();

            MttrLabels = grouped.Select(x => x.Label).ToArray();

            MttrChartSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "MTTR (Giờ)",
                    Values = new ChartValues<double>(grouped.Select(x => Math.Round(x.AvgHours, 1))),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10
                }
            };
            OnPropertyChanged(nameof(MttrLabels));
            OnPropertyChanged(nameof(MttrChartSeries));
        }

        private void ProcessPieChart(List<PerformanceDataDTO> data)
        {
            int onTime = 0;
            int late = 0;

            foreach (var item in data.Where(x => x.Status == "Completed" && x.ActualCompletion.HasValue))
            {
                DateTime actual = item.ActualCompletion.Value;
                DateTime deadline = item.ScheduledEndDate ?? item.RequestDate.AddHours(48);

                if (actual <= deadline) onTime++;
                else late++;
            }

            SoPhieuDungHan = onTime;
            SoPhieuTreHan = late;

            int totalCompleted = onTime + late;
            TyLeHoanThanh = totalCompleted > 0 ? Math.Round((double)onTime / totalCompleted * 100, 1) : 0;

            DonutChartData = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Đúng hạn",
                    Values = new ChartValues<int> { onTime },
                    Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1E40AF"),
                    DataLabels = true
                },
                new PieSeries
                {
                    Title = "Trễ hạn",
                    Values = new ChartValues<int> { late },
                    Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#EFF6FF"),
                    DataLabels = true
                }
            };
            OnPropertyChanged(nameof(DonutChartData));
        }

        // ============================================================
        // CHỨC NĂNG XUẤT EXCEL (MỚI)
        // ============================================================
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            if (_cachedReportData == null || _cachedReportData.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_HieuSuat_BaoTri_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Lưu báo cáo hiệu suất"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportToExcel(saveFileDialog.FileName);
                    var result = MessageBox.Show("Xuất báo cáo thành công! Bạn có muốn mở file ngay không?",
                                                 "Hoàn tất", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo { FileName = saveFileDialog.FileName, UseShellExecute = true });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToExcel(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                // ---------------------------------------------------------
                // SHEET 1: TỔNG HỢP (Dashboard)
                // ---------------------------------------------------------
                var ws1 = workbook.Worksheets.Add("Tổng hợp hiệu suất");

                // Tiêu đề
                var titleRange = ws1.Range("A1:E1");
                titleRange.Merge().Value = "BÁO CÁO HIỆU SUẤT BẢO TRÌ";
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Font.FontColor = XLColor.DarkBlue;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Ngày xuất
                ws1.Range("A2:E2").Merge().Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws1.Range("A2:E2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Bảng 1: Thống kê Tiến độ (Pie Chart Data)
                ws1.Cell("A4").Value = "1. TỶ LỆ HOÀN THÀNH";
                ws1.Cell("A4").Style.Font.Bold = true;

                var header1 = ws1.Range("A5:B5");
                header1.Style.Fill.BackgroundColor = XLColor.RoyalBlue;
                header1.Style.Font.FontColor = XLColor.White;
                header1.Style.Font.Bold = true;
                ws1.Cell("A5").Value = "Trạng thái";
                ws1.Cell("B5").Value = "Số lượng phiếu";

                ws1.Cell("A6").Value = "Đúng hạn";
                ws1.Cell("B6").Value = SoPhieuDungHan;
                ws1.Cell("A7").Value = "Trễ hạn";
                ws1.Cell("B7").Value = SoPhieuTreHan;
                ws1.Cell("A8").Value = "Tổng cộng";
                ws1.Cell("B8").Value = SoPhieuDungHan + SoPhieuTreHan;
                ws1.Cell("B8").Style.Font.Bold = true;

                ws1.Range("A5:B8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws1.Range("A5:B8").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Bảng 2: Thống kê MTTR theo tháng (Line Chart Data)
                ws1.Cell("D4").Value = "2. THỜI GIAN XỬ LÝ TB (MTTR)";
                ws1.Cell("D4").Style.Font.Bold = true;

                var header2 = ws1.Range("D5:E5");
                header2.Style.Fill.BackgroundColor = XLColor.Orange;
                header2.Style.Font.FontColor = XLColor.White;
                header2.Style.Font.Bold = true;
                ws1.Cell("D5").Value = "Tháng/Năm";
                ws1.Cell("E5").Value = "Giờ trung bình";

                // Tính toán lại dữ liệu MTTR để ghi vào Excel (giống logic biểu đồ)
                var mttrData = _cachedReportData
                    .Where(x => x.ActualCompletion.HasValue)
                    .GroupBy(x => new { x.RequestDate.Year, x.RequestDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        Time = $"{g.Key.Month}/{g.Key.Year}",
                        Val = Math.Round(g.Average(x => (x.ActualCompletion.Value - x.RequestDate).TotalHours), 1)
                    }).ToList();

                int mttrRow = 6;
                foreach (var item in mttrData)
                {
                    ws1.Cell(mttrRow, 4).Value = item.Time;
                    ws1.Cell(mttrRow, 5).Value = item.Val;
                    mttrRow++;
                }
                ws1.Range(5, 4, mttrRow - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws1.Range(5, 4, mttrRow - 1, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws1.Columns().AdjustToContents();

                // ---------------------------------------------------------
                // SHEET 2: CHI TIẾT (Raw Data)
                // ---------------------------------------------------------
                var ws2 = workbook.Worksheets.Add("Danh sách chi tiết");

                // Header
                int row = 1;
                ws2.Cell(row, 1).Value = "Mã Phiếu";
                ws2.Cell(row, 2).Value = "Ngày Yêu Cầu";
                ws2.Cell(row, 3).Value = "Thiết Bị";
                ws2.Cell(row, 4).Value = "Kỹ Thuật Viên";
                ws2.Cell(row, 5).Value = "Trạng Thái";
                ws2.Cell(row, 6).Value = "Ngày Hoàn Thành";
                ws2.Cell(row, 7).Value = "Thời gian xử lý (h)";

                var headerRange2 = ws2.Range(row, 1, row, 7);
                headerRange2.Style.Fill.BackgroundColor = XLColor.LightSlateGray;
                headerRange2.Style.Font.FontColor = XLColor.White;
                headerRange2.Style.Font.Bold = true;

                // Data
                row++;
                foreach (var item in _cachedReportData)
                {
                    // Giả sử PerformanceDataDTO có các trường này (bạn chỉnh lại tên property cho khớp DTO của bạn)
                    // Nếu DTO chưa có trường nào thì bạn có thể bỏ qua cột đó

                    // Ví dụ map dữ liệu:
                    ws2.Cell(row, 1).Value = "WO-" + item.GetHashCode().ToString().Substring(0, 4); // Ví dụ Mã
                    ws2.Cell(row, 2).Value = item.RequestDate;
                    ws2.Cell(row, 3).Value = "Thiết bị... (Cần join)"; // DTO này đang là Raw Data tính toán nên có thể thiếu tên TB
                    ws2.Cell(row, 4).Value = "KTV... (Cần join)";      // Tương tự
                    ws2.Cell(row, 5).Value = item.Status;

                    if (item.ActualCompletion.HasValue)
                    {
                        ws2.Cell(row, 6).Value = item.ActualCompletion.Value;
                        double hours = (item.ActualCompletion.Value - item.RequestDate).TotalHours;
                        ws2.Cell(row, 7).Value = Math.Round(hours, 2);
                    }
                    else
                    {
                        ws2.Cell(row, 6).Value = "-";
                        ws2.Cell(row, 7).Value = "-";
                    }

                    // Format ngày
                    ws2.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws2.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                    row++;
                }

                ws2.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // Navigation
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCCPVT()); }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCNSKTV()); }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTDBH_va_NCC()); }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTTTB()); }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new TKTSL_va_SC()); }
    }
}