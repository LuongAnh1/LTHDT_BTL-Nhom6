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
using Microsoft.Win32;      // SaveFileDialog
using ClosedXML.Excel;      // Xuất Excel
using System.Diagnostics;   // Process.Start

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCNSKTV : Window, INotifyPropertyChanged
    {
        private readonly ReportService _reportService;
        private readonly UserService _userService;

        public SeriesCollection TopKtvSeries { get; set; }
        public List<string> KtvLabels { get; set; }
        public List<NangSuatKTVDTO> Top3KtvList { get; set; }
        public List<BaoCaoCongViecDTO> DanhSachCongViec { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public BCNSKTV()
        {
            InitializeComponent();
            DataContext = this;

            _reportService = new ReportService();
            _userService = new UserService();

            LoadComboBoxData();
            LoadDashboardData();
            LoadGridData();
        }

        private void LoadComboBoxData()
        {
            var locations = _reportService.GetLocations();
            cboPhanXuong.Items.Clear();
            cboPhanXuong.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = 0, IsSelected = true });
            foreach (var loc in locations)
            {
                cboPhanXuong.Items.Add(new ComboBoxItem { Content = loc.Value, Tag = loc.Key });
            }

            var cats = _reportService.GetCategories();
            cboLoaiTB.Items.Clear();
            cboLoaiTB.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = 0, IsSelected = true });
            foreach (var c in cats)
            {
                cboLoaiTB.Items.Add(new ComboBoxItem { Content = c.Value, Tag = c.Key });
            }

            var techs = _userService.GetTechnicians();
            cboKTV.Items.Clear();
            cboKTV.Items.Add(new ComboBoxItem { Content = "Tất cả", Tag = 0, IsSelected = true });
            foreach (var t in techs)
            {
                cboKTV.Items.Add(new ComboBoxItem { Content = t.FullName, Tag = t.UserID });
            }
        }

        private void LoadDashboardData()
        {
            var data = _reportService.GetNangSuatKTV(10);

            Top3KtvList = data.Take(3).ToList();
            OnPropertyChanged(nameof(Top3KtvList));

            var chartData = data.Take(5).ToList();
            TopKtvSeries = new SeriesCollection
            {
                new RowSeries
                {
                    Title = "Số lượng công việc",
                    Values = new ChartValues<int>(chartData.Select(x => x.TongCongViec)),
                    Fill = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#3B82F6"),
                    DataLabels = true
                }
            };
            KtvLabels = chartData.Select(x => x.TenKTV).ToList();

            OnPropertyChanged(nameof(TopKtvSeries));
            OnPropertyChanged(nameof(KtvLabels));
        }

        private void LoadGridData()
        {
            DateTime? date = dpNgay.SelectedDate;
            int locId = 0;
            if (cboPhanXuong.SelectedItem is ComboBoxItem itemLoc) locId = Convert.ToInt32(itemLoc.Tag);
            int catId = 0;
            if (cboLoaiTB.SelectedItem is ComboBoxItem itemCat) catId = Convert.ToInt32(itemCat.Tag);
            int techId = 0;
            if (cboKTV.SelectedItem is ComboBoxItem itemTech) techId = Convert.ToInt32(itemTech.Tag);

            DanhSachCongViec = _reportService.GetBaoCaoCongViec(date, locId, catId, techId);
            dgCongViec.ItemsSource = DanhSachCongViec;
        }

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            LoadGridData();
        }

        private void BtnXemChiTiet_Click(object sender, RoutedEventArgs e)
        {
            var fullList = _reportService.GetNangSuatKTV(100);
            ChiTietXepHang popup = new ChiTietXepHang(fullList);
            popup.ShowDialog();
        }

        // --- CHỨC NĂNG XUẤT EXCEL (ĐÃ SỬA LỖI) ---
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            if (DanhSachCongViec == null || DanhSachCongViec.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu công việc để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_NangSuat_KTV_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Lưu báo cáo năng suất"
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
                // SHEET 1: CHI TIẾT CÔNG VIỆC
                var ws1 = workbook.Worksheets.Add("Chi tiết công việc");

                var title1 = ws1.Range("A1:E1");
                title1.Merge().Value = "BÁO CÁO CHI TIẾT CÔNG VIỆC KỸ THUẬT VIÊN";
                title1.Style.Font.Bold = true;
                title1.Style.Font.FontSize = 14;
                title1.Style.Font.FontColor = XLColor.DarkBlue;
                title1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                string dateStr = dpNgay.SelectedDate.HasValue ? dpNgay.SelectedDate.Value.ToString("dd/MM/yyyy") : "Tất cả thời gian";
                ws1.Range("A2:E2").Merge().Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} | Phạm vi: {dateStr}";
                ws1.Range("A2:E2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row1 = 4;
                ws1.Cell(row1, 1).Value = "Kỹ Thuật Viên";
                ws1.Cell(row1, 2).Value = "Mã Công Việc";
                ws1.Cell(row1, 3).Value = "Mô Tả";
                ws1.Cell(row1, 4).Value = "Trạng Thái";
                ws1.Cell(row1, 5).Value = "Độ Ưu Tiên";

                var header1 = ws1.Range(row1, 1, row1, 5);
                header1.Style.Fill.BackgroundColor = XLColor.RoyalBlue;
                header1.Style.Font.FontColor = XLColor.White;
                header1.Style.Font.Bold = true;
                header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row1++;
                foreach (var item in DanhSachCongViec)
                {
                    ws1.Cell(row1, 1).Value = item.TenKTV;
                    ws1.Cell(row1, 2).Value = item.MaCV;
                    ws1.Cell(row1, 3).Value = item.MoTa;
                    ws1.Cell(row1, 4).Value = item.TrangThai;
                    ws1.Cell(row1, 5).Value = item.DoUuTien;

                    var cellStatus = ws1.Cell(row1, 4);
                    if (item.TrangThai == "Hoàn thành") cellStatus.Style.Font.FontColor = XLColor.Green;
                    else if (item.TrangThai == "Mới") cellStatus.Style.Font.FontColor = XLColor.Blue;
                    else cellStatus.Style.Font.FontColor = XLColor.Orange;

                    var cellPriority = ws1.Cell(row1, 5);
                    if (item.DoUuTien == "Cao") cellPriority.Style.Font.FontColor = XLColor.Red;

                    row1++;
                }

                ws1.Range(4, 1, row1 - 1, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws1.Range(4, 1, row1 - 1, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws1.Columns().AdjustToContents();

                // SHEET 2: TỔNG HỢP TOP KTV
                var ws2 = workbook.Worksheets.Add("Tổng hợp năng suất");

                ws2.Range("A1:C1").Merge().Value = "TOP KỸ THUẬT VIÊN XUẤT SẮC NHẤT";
                ws2.Range("A1:C1").Style.Font.Bold = true;

                // [ĐÃ SỬA LỖI Ở ĐÂY] Thêm .Font trước .FontSize
                ws2.Range("A1:C1").Style.Font.FontSize = 14;

                ws2.Range("A1:C1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row2 = 3;
                ws2.Cell(row2, 1).Value = "Tên Kỹ Thuật Viên";
                ws2.Cell(row2, 2).Value = "Tổng Công Việc";
                ws2.Cell(row2, 3).Value = "Kỹ Năng Chính";

                var header2 = ws2.Range(row2, 1, row2, 3);
                header2.Style.Fill.BackgroundColor = XLColor.Orange;
                header2.Style.Font.FontColor = XLColor.White;
                header2.Style.Font.Bold = true;

                row2++;
                if (Top3KtvList != null)
                {
                    foreach (var ktv in Top3KtvList)
                    {
                        ws2.Cell(row2, 1).Value = ktv.TenKTV;
                        ws2.Cell(row2, 2).Value = ktv.TongCongViec;
                        string skills = (ktv.DanhSachKyNang != null) ? string.Join(", ", ktv.DanhSachKyNang) : "";
                        ws2.Cell(row2, 3).Value = skills;
                        row2++;
                    }
                }

                ws2.Range(3, 1, row2 - 1, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws2.Range(3, 1, row2 - 1, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws2.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCCPVT()); }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCHSBT()); }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTDBH_va_NCC()); }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTTTB()); }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new TKTSL_va_SC()); }
    }
}