using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32; // Cho SaveFileDialog
using ClosedXML.Excel; // Cho việc xuất Excel
using System.Diagnostics; // Cho Process.Start

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCTTTB : Window
    {
        // Khai báo các Service
        private readonly DeviceService _deviceService;
        private readonly LocationService _locationService;
        private readonly CategoryService _categoryService;

        // Các thuộc tính dùng để Binding ra giao diện
        public SeriesCollection StatusSeries { get; set; } // Cho biểu đồ
        public List<DeviceStatusDTO> DanhSachThietBi { get; set; } // Cho DataGrid

        public BCTTTB()
        {
            InitializeComponent();

            // Khởi tạo Service
            _deviceService = new DeviceService();
            _locationService = new LocationService();
            _categoryService = new CategoryService();

            DataContext = this;

            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilters();
            // Mặc định load tất cả (0, 0, 0)
            LoadReportData(0, 0, 0);
        }

        // 1. Load dữ liệu cho ComboBox
        private void LoadFilters()
        {
            try
            {
                // --- A. Load Vị trí ---
                var locations = _locationService.GetAllLocations();
                locations.Insert(0, new Location { LocationID = 0, LocationName = "Tất cả" });

                cboLocation.ItemsSource = locations;
                cboLocation.DisplayMemberPath = "LocationName";
                cboLocation.SelectedValuePath = "LocationID";
                cboLocation.SelectedIndex = 0;

                // --- B. Load Loại thiết bị ---
                var categories = _categoryService.GetAllCategories();
                categories.Insert(0, new Category { CategoryID = 0, CategoryName = "Tất cả" });

                cboCategory.ItemsSource = categories;
                cboCategory.DisplayMemberPath = "CategoryName";
                cboCategory.SelectedValuePath = "CategoryID";
                cboCategory.SelectedIndex = 0;

                // (Đã xóa phần load Phân xưởng vì giao diện không còn dùng nữa)
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải bộ lọc: " + ex.Message);
            }
        }

        // 2. Load dữ liệu báo cáo
        private void LoadReportData(int locationId, int categoryId, int phanXuongId)
        {
            try
            {
                // A. Lấy dữ liệu danh sách
                DanhSachThietBi = _deviceService.GetDeviceStatusList(locationId, categoryId, phanXuongId);

                // B. Lấy dữ liệu biểu đồ
                var chartData = _deviceService.GetStatusChartData(locationId, categoryId, phanXuongId);

                // Cấu hình Biểu đồ LiveCharts
                StatusSeries = new SeriesCollection();
                if (chartData != null) // Kiểm tra null để tránh lỗi
                {
                    foreach (var item in chartData)
                    {
                        StatusSeries.Add(new PieSeries
                        {
                            Title = item.StatusName,
                            Values = new ChartValues<int> { item.Quantity },
                            DataLabels = true,
                            LabelPoint = point => $"{point.Y} ({point.Participation:P0})"
                        });
                    }
                }

                // C. Cập nhật giao diện
                // Reset DataContext để UI cập nhật lại danh sách và biểu đồ mới
                DataContext = null;
                DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu báo cáo: " + ex.Message);
            }
        }

        // --- [QUAN TRỌNG] Sự kiện nút Áp dụng ---
        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy ID Vị trí
            int locId = 0;
            if (cboLocation.SelectedValue != null)
                int.TryParse(cboLocation.SelectedValue.ToString(), out locId);

            // 2. Lấy ID Loại
            int catId = 0;
            if (cboCategory.SelectedValue != null)
                int.TryParse(cboCategory.SelectedValue.ToString(), out catId);

            // 3. Gọi hàm tải dữ liệu
            // Tham số thứ 3 là 'phanXuongId', vì đã xóa ô chọn nên ta truyền 0 (nghĩa là lấy tất cả)
            LoadReportData(locId, catId, 0);
        }

        // --- CHỨC NĂNG XUẤT BÁO CÁO EXCEL ---

        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra dữ liệu
            if (DanhSachThietBi == null || DanhSachThietBi.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu thiết bị để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Mở hộp thoại lưu file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_TinhTrangThietBi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Lưu báo cáo tình trạng thiết bị"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportToExcel(saveFileDialog.FileName);

                    // 3. Hỏi người dùng có muốn mở file không
                    var result = MessageBox.Show("Xuất báo cáo thành công! Bạn có muốn mở file ngay không?",
                                                 "Hoàn tất", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        };
                        Process.Start(processStartInfo);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi xảy ra khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToExcel(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Tình trạng thiết bị");

                // --- PHẦN 1: TIÊU ĐỀ BÁO CÁO ---
                // Gộp ô A1:C1 làm tiêu đề lớn
                var titleRange = worksheet.Range("A1:C1");
                titleRange.Merge();
                titleRange.Value = "BÁO CÁO TÌNH TRẠNG THIẾT BỊ";
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 16;
                titleRange.Style.Font.FontColor = XLColor.DarkBlue;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Ngày xuất
                var dateRange = worksheet.Range("A2:C2");
                dateRange.Merge();
                dateRange.Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                dateRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dateRange.Style.Font.Italic = true;

                // Thông tin bộ lọc (Lấy text từ ComboBox)
                var filterRange = worksheet.Range("A3:C3");
                filterRange.Merge();

                string locationName = cboLocation.Text;
                if (string.IsNullOrEmpty(locationName)) locationName = "Tất cả";

                string categoryName = cboCategory.Text;
                if (string.IsNullOrEmpty(categoryName)) categoryName = "Tất cả";

                filterRange.Value = $"Khu vực: [{locationName}] - Loại thiết bị: [{categoryName}]";
                filterRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // --- PHẦN 2: HEADER CỦA BẢNG ---
                int row = 5;
                worksheet.Cell(row, 1).Value = "Mã Thiết Bị";
                worksheet.Cell(row, 2).Value = "Tên Thiết Bị";
                worksheet.Cell(row, 3).Value = "Trạng Thái";

                // Style cho Header (Nền xanh, chữ trắng)
                var headerRange = worksheet.Range(row, 1, row, 3);
                headerRange.Style.Fill.BackgroundColor = XLColor.Teal; // Màu khác một chút so với report kia cho đẹp
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // --- PHẦN 3: DATA ---
                row++;
                foreach (var item in DanhSachThietBi)
                {
                    worksheet.Cell(row, 1).Value = item.MaTB;
                    worksheet.Cell(row, 2).Value = item.TenTB;
                    worksheet.Cell(row, 3).Value = item.TrangThai;

                    // Tô màu chữ dựa trên trạng thái (Giống logic trong XAML)
                    var statusCell = worksheet.Cell(row, 3);
                    statusCell.Style.Font.Bold = true;
                    statusCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (item.TrangThai == "Tốt")
                    {
                        statusCell.Style.Font.FontColor = XLColor.Green;
                        statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7"); // Xanh nhạt
                    }
                    else if (item.TrangThai == "Hỏng")
                    {
                        statusCell.Style.Font.FontColor = XLColor.Red;
                        statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2"); // Đỏ nhạt
                    }
                    else if (item.TrangThai == "Thanh lý")
                    {
                        statusCell.Style.Font.FontColor = XLColor.DarkGoldenrod; // Màu vàng đất
                        statusCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF9C3"); // Vàng nhạt
                    }

                    row++;
                }

                // --- PHẦN 4: KẺ KHUNG & CANH CHỈNH ---
                var tableRange = worksheet.Range(5, 1, row - 1, 3);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                worksheet.Columns().AdjustToContents(); // Tự động giãn cột

                workbook.SaveAs(filePath);
            }
        }

        // --- Các nút điều hướng ---
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCCPVT()); }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCHSBT()); }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCNSKTV()); }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTDBH_va_NCC()); }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new TKTSL_va_SC()); }
    }
}