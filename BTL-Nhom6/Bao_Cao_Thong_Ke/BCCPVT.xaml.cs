using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;      // SaveFileDialog
using ClosedXML.Excel;      // Xuất Excel
using System.Diagnostics;   // Process.Start

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCCPVT : Window, INotifyPropertyChanged
    {
        private readonly WorkOrderService _service;

        // --- CÁC BIẾN BINDING ---
        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set { _seriesCollection = value; OnPropertyChanged(); }
        }

        private string[] _labels;
        public string[] Labels
        {
            get => _labels;
            set { _labels = value; OnPropertyChanged(); }
        }

        public Func<double, string> CurrencyFormatter { get; set; }

        private ObservableCollection<ChiPhiDTO> _danhSachChiPhi;
        public ObservableCollection<ChiPhiDTO> DanhSachChiPhi
        {
            get => _danhSachChiPhi;
            set { _danhSachChiPhi = value; OnPropertyChanged(); }
        }

        private string _tongTienHienThi;
        public string TongTienHienThi
        {
            get => _tongTienHienThi;
            set { _tongTienHienThi = value; OnPropertyChanged(); }
        }

        private string _tieuDeBieuDo = "Biểu đồ chi phí theo thời gian";
        public string TieuDeBieuDo
        {
            get => _tieuDeBieuDo;
            set { _tieuDeBieuDo = value; OnPropertyChanged(); }
        }

        // --- BIẾN CHO COMBOBOX NĂM ---
        private ObservableCollection<int> _danhSachNam;
        public ObservableCollection<int> DanhSachNam
        {
            get => _danhSachNam;
            set { _danhSachNam = value; OnPropertyChanged(); }
        }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); }
        }

        // --- BIẾN CHO PHÂN XƯỞNG ---
        private ObservableCollection<string> _danhSachPhanXuong;
        public ObservableCollection<string> DanhSachPhanXuong
        {
            get => _danhSachPhanXuong;
            set { _danhSachPhanXuong = value; OnPropertyChanged(); }
        }

        private string _selectedPhanXuong;
        public string SelectedPhanXuong
        {
            get => _selectedPhanXuong;
            set { _selectedPhanXuong = value; OnPropertyChanged(); }
        }

        // Biến lưu dữ liệu đã lọc để xuất Excel
        private List<ChiPhiDTO> _cachedData;

        public BCCPVT()
        {
            InitializeComponent();
            _service = new WorkOrderService();
            DataContext = this;
            CurrencyFormatter = value => value.ToString("N0");
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DanhSachNam = new ObservableCollection<int>();
            int currentYear = DateTime.Now.Year;
            for (int y = currentYear - 5; y <= currentYear + 1; y++)
            {
                DanhSachNam.Add(y);
            }

            LoadComboboxPhanXuong();
            LoadData();
        }

        private void LoadComboboxPhanXuong()
        {
            try
            {
                var listPX = _service.GetDanhSachPhanXuong();
                if (listPX == null) listPX = new List<string>();
                listPX.Insert(0, "Toàn nhà máy");
                DanhSachPhanXuong = new ObservableCollection<string>(listPX);
                SelectedPhanXuong = "Toàn nhà máy";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phân xưởng: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                int year = SelectedYear ?? DateTime.Now.Year;
                TieuDeBieuDo = $"Biểu đồ chi phí năm {year}";

                var rawList = _service.GetReportData(null, year);
                if (rawList == null) rawList = new List<ChiPhiDTO>();

                var filteredList = ApplyLocalFilters(rawList);

                _cachedData = filteredList; // Lưu lại để xuất Excel

                UpdateDataGrid(filteredList);
                DrawChart(filteredList);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<ChiPhiDTO> ApplyLocalFilters(List<ChiPhiDTO> source)
        {
            var result = source.AsEnumerable();

            if (!string.IsNullOrEmpty(SelectedPhanXuong) && SelectedPhanXuong != "Toàn nhà máy")
            {
                result = result.Where(x => x.TenPhanXuong != null &&
                                           x.TenPhanXuong.Equals(SelectedPhanXuong, StringComparison.OrdinalIgnoreCase));
            }

            if (cbbLoaiChiPhi.SelectedItem is ComboBoxItem selectedItem)
            {
                string type = selectedItem.Content.ToString();
                if (type != "Tất cả")
                {
                    if (type.ToLower().Contains("vật tư"))
                        result = result.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("vật tư"));
                    else if (type.ToLower().Contains("nhân công"))
                        result = result.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("nhân công"));
                    else if (type.ToLower().Contains("khác"))
                        result = result.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("khác"));
                }
            }

            string keyword = txtTimKiem.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(keyword))
            {
                result = result.Where(x =>
                    (x.MaPhieu != null && x.MaPhieu.ToLower().Contains(keyword)) ||
                    (x.NoiDung != null && x.NoiDung.ToLower().Contains(keyword))
                );
            }

            return result.ToList();
        }

        private void UpdateDataGrid(List<ChiPhiDTO> data)
        {
            if (data == null) data = new List<ChiPhiDTO>();

            // BƯỚC 1: TÍNH TỔNG TIỀN (Tính hết: Nhân công + Vật tư + Khác)
            // Biến 'data' chứa toàn bộ dữ liệu đã lọc theo Năm/Phân Xưởng
            if (data.Any())
            {
                decimal total = data.Sum(x => x.SoTien);
                TongTienHienThi = total.ToString("N0");
            }
            else
            {
                TongTienHienThi = "0";
            }

            // BƯỚC 2: HIỂN THỊ BẢNG (Chỉ hiển thị dòng "Chi phí khác")
            // Lọc lại một lần nữa chỉ để lấy ra các khoản phát sinh đưa vào Grid
            var listChiPhiPhatSinh = data
                .Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("khác"))
                .ToList();

            DanhSachChiPhi = new ObservableCollection<ChiPhiDTO>(listChiPhiPhatSinh);
        }

        private void DrawChart(List<ChiPhiDTO> data)
        {
            var listLabels = new List<string>();
            var valuesNhanCong = new ChartValues<double>();
            var valuesVatTu = new ChartValues<double>();

            for (int m = 1; m <= 12; m++)
            {
                listLabels.Add($"T{m}");
                double sumLabor = (double)data.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("nhân công") && x.Ngay.Month == m).Sum(x => x.SoTien);
                double sumMaterial = (double)data.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("vật tư") && x.Ngay.Month == m).Sum(x => x.SoTien);

                valuesNhanCong.Add(sumLabor);
                valuesVatTu.Add(sumMaterial);
            }

            Labels = listLabels.ToArray();
            var newSeriesCollection = new SeriesCollection();
            string selectedType = "Tất cả";
            if (cbbLoaiChiPhi.SelectedItem is ComboBoxItem item)
            {
                selectedType = item.Content.ToString();
            }

            if (selectedType == "Tất cả" || selectedType.ToLower().Contains("nhân công"))
            {
                newSeriesCollection.Add(new ColumnSeries
                {
                    Title = "Nhân công",
                    Values = valuesNhanCong,
                    Fill = (Brush)new BrushConverter().ConvertFromString("#2563EB")
                });
            }

            if (selectedType == "Tất cả" || selectedType.ToLower().Contains("vật tư"))
            {
                newSeriesCollection.Add(new ColumnSeries
                {
                    Title = "Vật tư",
                    Values = valuesVatTu,
                    Fill = (Brush)new BrushConverter().ConvertFromString("#93C5FD")
                });
            }

            SeriesCollection = newSeriesCollection;
        }

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        // ============================================================
        // CHỨC NĂNG XUẤT EXCEL (ĐÃ SỬA LỖI)
        // ============================================================
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            if (_cachedData == null || _cachedData.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_ChiPhi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Lưu báo cáo chi phí"
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
                // SHEET 1: TỔNG HỢP (Theo tháng)
                var ws1 = workbook.Worksheets.Add("Tổng hợp theo tháng");

                ws1.Range("A1:D1").Merge().Value = $"BÁO CÁO TỔNG HỢP CHI PHÍ NĂM {SelectedYear ?? DateTime.Now.Year}";
                ws1.Range("A1:D1").Style.Font.Bold = true;
                ws1.Range("A1:D1").Style.Font.FontSize = 14; // .Style.Font.FontSize (Đúng)
                ws1.Range("A1:D1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws1.Range("A1:D1").Style.Font.FontColor = XLColor.DarkBlue;

                ws1.Range("A2:D2").Merge().Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} - Phân xưởng: {SelectedPhanXuong}";
                ws1.Range("A2:D2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row1 = 4;
                ws1.Cell(row1, 1).Value = "Tháng";
                ws1.Cell(row1, 2).Value = "Chi phí Nhân công";
                ws1.Cell(row1, 3).Value = "Chi phí Vật tư";
                ws1.Cell(row1, 4).Value = "Tổng cộng";

                var header1 = ws1.Range(row1, 1, row1, 4);
                header1.Style.Fill.BackgroundColor = XLColor.RoyalBlue;
                header1.Style.Font.FontColor = XLColor.White;
                header1.Style.Font.Bold = true;
                header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row1++;
                for (int m = 1; m <= 12; m++)
                {
                    decimal labor = _cachedData.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("nhân công") && x.Ngay.Month == m).Sum(x => x.SoTien);
                    decimal material = _cachedData.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("vật tư") && x.Ngay.Month == m).Sum(x => x.SoTien);

                    ws1.Cell(row1, 1).Value = $"Tháng {m}";
                    ws1.Cell(row1, 2).Value = labor;
                    ws1.Cell(row1, 3).Value = material;
                    ws1.Cell(row1, 4).Value = labor + material;

                    ws1.Cell(row1, 2).Style.NumberFormat.Format = "#,##0";
                    ws1.Cell(row1, 3).Style.NumberFormat.Format = "#,##0";
                    ws1.Cell(row1, 4).Style.NumberFormat.Format = "#,##0";

                    row1++;
                }

                ws1.Cell(row1, 1).Value = "CẢ NĂM";
                ws1.Cell(row1, 2).FormulaA1 = $"SUM(B5:B16)";
                ws1.Cell(row1, 3).FormulaA1 = $"SUM(C5:C16)";
                ws1.Cell(row1, 4).FormulaA1 = $"SUM(D5:D16)";
                ws1.Range(row1, 1, row1, 4).Style.Font.Bold = true;
                ws1.Range(row1, 2, row1, 4).Style.NumberFormat.Format = "#,##0";

                ws1.Range(4, 1, row1, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws1.Range(4, 1, row1, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws1.Columns().AdjustToContents();

                // SHEET 2: CHI TIẾT CHI PHÍ KHÁC
                var ws2 = workbook.Worksheets.Add("Chi tiết chi phí khác");

                ws2.Range("A1:E1").Merge().Value = "DANH SÁCH CHI TIẾT CÁC KHOẢN CHI PHÍ KHÁC";
                ws2.Range("A1:E1").Style.Font.Bold = true;

                // [ĐÃ SỬA LỖI TẠI ĐÂY] Thêm .Font trước .FontSize
                ws2.Range("A1:E1").Style.Font.FontSize = 14;

                ws2.Range("A1:E1").Style.Font.FontColor = XLColor.DarkGreen;
                ws2.Range("A1:E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row2 = 3;
                ws2.Cell(row2, 1).Value = "Mã Phiếu";
                ws2.Cell(row2, 2).Value = "Loại Chi Phí";
                ws2.Cell(row2, 3).Value = "Nội Dung";
                ws2.Cell(row2, 4).Value = "Ngày Ghi Nhận";
                ws2.Cell(row2, 5).Value = "Số Tiền (VNĐ)";

                var header2 = ws2.Range(row2, 1, row2, 5);
                header2.Style.Fill.BackgroundColor = XLColor.ForestGreen;
                header2.Style.Font.FontColor = XLColor.White;
                header2.Style.Font.Bold = true;

                var listChiPhiKhac = _cachedData.Where(x => x.LoaiChiPhi != null && x.LoaiChiPhi.ToLower().Contains("khác")).ToList();
                row2++;
                foreach (var item in listChiPhiKhac)
                {
                    ws2.Cell(row2, 1).Value = item.MaPhieu;
                    ws2.Cell(row2, 2).Value = item.LoaiChiPhi;
                    ws2.Cell(row2, 3).Value = item.NoiDung;
                    ws2.Cell(row2, 4).Value = item.Ngay;
                    ws2.Cell(row2, 5).Value = item.SoTien;

                    ws2.Cell(row2, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws2.Cell(row2, 5).Style.NumberFormat.Format = "#,##0";
                    row2++;
                }

                ws2.Cell(row2, 4).Value = "TỔNG CỘNG:";
                ws2.Cell(row2, 5).FormulaA1 = $"SUM(E4:E{row2 - 1})";
                ws2.Range(row2, 4, row2, 5).Style.Font.Bold = true;
                ws2.Cell(row2, 5).Style.NumberFormat.Format = "#,##0";

                ws2.Range(3, 1, row2, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws2.Range(3, 1, row2, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws2.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        // --- CÁC NÚT ĐIỀU HƯỚNG ---
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCHSBT()); }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCNSKTV()); }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTDBH_va_NCC()); }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTTTB()); }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new TKTSL_va_SC()); }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}