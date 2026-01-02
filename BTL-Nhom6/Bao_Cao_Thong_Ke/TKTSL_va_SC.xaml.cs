using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32; 
using ClosedXML.Excel; 

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class TKTSL_va_SC : Window, INotifyPropertyChanged
    {
        // Services
        private readonly MaintenanceService _maintenanceService;
        private readonly CategoryService _categoryService;

        // Properties Binding
        private ObservableCollection<IncidentDTO> _danhSachSuCo;
        public ObservableCollection<IncidentDTO> DanhSachSuCo
        {
            get => _danhSachSuCo;
            set { _danhSachSuCo = value; OnPropertyChanged(); }
        }

        private ObservableCollection<BarChartDTO> _duLieuBieuDo;
        public ObservableCollection<BarChartDTO> DuLieuBieuDo
        {
            get => _duLieuBieuDo;
            set { _duLieuBieuDo = value; OnPropertyChanged(); }
        }

        public TKTSL_va_SC()
        {
            InitializeComponent();
            DataContext = this;

            _maintenanceService = new MaintenanceService();
            _categoryService = new CategoryService();

            DanhSachSuCo = new ObservableCollection<IncidentDTO>();
            DuLieuBieuDo = new ObservableCollection<BarChartDTO>();

            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxData();

            // 2. Đặt giá trị là null (để trống ô ngày)
            dpTuNgay.SelectedDate = null;

            // 3. Gọi hàm load dữ liệu
            // Khi ngày = null, Service sẽ tự động lấy "Tất cả thời gian"
            LoadReportData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                // Load ComboBox Loại thiết bị (cboCategory)
                // Lưu ý: Trong XAML bạn cần đặt x:Name="cboCategory" cho ComboBox loại thiết bị
                // Nếu chưa đặt tên, hãy thêm x:Name="cboCategory" vào file XAML ở cột 2

                // Giả sử tên trong XAML là cboCategory (Tôi sẽ dùng FindName hoặc bạn sửa XAML)
                // Ở đây tôi giả định bạn đã đặt tên x:Name="cboCategory" cho ComboBox thứ 2
                ComboBox cboCat = this.FindName("cboCategory") as ComboBox;
                if (cboCat == null)
                {
                    // Nếu chưa đặt tên, code này sẽ tìm theo cấu trúc VisualTree hoặc bạn tự sửa XAML thêm x:Name
                    // Tôi khuyến nghị bạn thêm x:Name="cboCategory" vào XAML
                }
                else
                {
                    var categories = _categoryService.GetAllCategories();
                    categories.Insert(0, new Category { CategoryID = 0, CategoryName = "Tất cả" });
                    cboCat.ItemsSource = categories;
                    cboCat.DisplayMemberPath = "CategoryName";
                    cboCat.SelectedValuePath = "CategoryID";
                    cboCat.SelectedIndex = 0;
                }

                // ComboBox "Phân loại lỗi" (cboErrorType) trong XAML đã fix cứng Item, ta chỉ cần lấy text khi lọc
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load bộ lọc: " + ex.Message);
            }
        }

        private void BtnApDung_Click(object sender, RoutedEventArgs e)
        {
            LoadReportData();
        }

        private void LoadReportData()
        {
            try
            {
                // 1. Lấy tham số
                // XAML: DatePicker cần có x:Name="dpTuNgay" (hoặc sửa code dưới theo tên bạn đặt)
                // Hiện tại XAML chưa có Name cho DatePicker, tôi sẽ dùng LogicalTreeHelper hoặc bạn thêm Name vào.
                // GIẢ ĐỊNH: Bạn thêm x:Name="dpTuNgay" vào DatePicker trong XAML.

                DatePicker dp = FindVisualChild<DatePicker>(this); // Hàm tìm DatePicker nếu lười đặt tên
                DateTime? fromDate = dp?.SelectedDate;

                // Lấy CategoryID
                ComboBox cboCat = FindName("cboCategory") as ComboBox; // Cần đặt tên trong XAML
                int catId = (cboCat != null && cboCat.SelectedValue != null) ? (int)cboCat.SelectedValue : 0;

                // Lấy ErrorType
                // Cần đặt x:Name="cboErrorType" cho ComboBox Phân loại lỗi
                ComboBox cboErr = FindName("cboErrorType") as ComboBox;
                string errType = "Tất cả";
                if (cboErr != null && cboErr.SelectedItem is ComboBoxItem item)
                {
                    errType = item.Content.ToString();
                }

                // 2. Load DataGrid
                var dataList = _maintenanceService.GetIncidentReport(fromDate, catId, errType);
                DanhSachSuCo = new ObservableCollection<IncidentDTO>(dataList);

                // 3. Load Chart & Tính toán chiều cao
                var chartData = _maintenanceService.GetIncidentChartData(fromDate, catId, errType);
                CalculateChartHeights(chartData);
                DuLieuBieuDo = new ObservableCollection<BarChartDTO>(chartData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        private void CalculateChartHeights(List<BarChartDTO> data)
        {
            if (data == null || data.Count == 0) return;

            // Trong XAML, trục Y max là 20. Chiều cao vùng vẽ khoảng 250px.
            // Nếu dữ liệu vượt quá 20, cột sẽ bị tràn. Ta cần logic co dãn hoặc fix cứng theo scale 20.

            double maxHeightPx = 220; // Chiều cao tối đa của cột trong Grid (Grid Height=300, trừ margin)
            double maxScaleValue = 20.0; // Giá trị đỉnh của trục Y trong XAML

            foreach (var item in data)
            {
                // Công thức: (Giá trị thực / Giá trị Max Trục Y) * Chiều cao Pixel tối đa
                double height = (item.GiaTriThuc / maxScaleValue) * maxHeightPx;

                // Giới hạn không cho vượt quá khung (nếu > 20 thì full cột)
                if (height > maxHeightPx) height = maxHeightPx;
                if (height < 5 && item.GiaTriThuc > 0) height = 5; // Cột tối thiểu để người dùng thấy có dữ liệu

                item.HeightValue = height;
            }
        }

        // Helper tìm control nếu chưa đặt tên (Tốt nhất bạn nên đặt x:Name trong XAML)
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild) return typedChild;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        // Navigation Handlers
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCCPVT()); }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCHSBT()); }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCNSKTV()); }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTDBH_va_NCC()); }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTTTB()); }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { }
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            // 1. Kiểm tra nếu không có dữ liệu thì không xuất
            if (DanhSachSuCo == null || DanhSachSuCo.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu sự cố để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Mở hộp thoại chọn nơi lưu file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_SuCo_{DateTime.Now:yyyyMMdd_HHmm}.xlsx", // Tên file mặc định kèm ngày giờ
                Title = "Lưu báo cáo thống kê lỗi"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Gọi hàm xuất Excel
                    ExportToExcel(saveFileDialog.FileName);

                    // Thông báo thành công và hỏi có muốn mở file không
                    var result = MessageBox.Show("Xuất báo cáo thành công! Bạn có muốn mở file ngay không?",
                                                 "Hoàn tất", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        var processStartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(processStartInfo);
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
                var worksheet = workbook.Worksheets.Add("Thống kê sự cố");

                // --- PHẦN 1: TIÊU ĐỀ BÁO CÁO ---
                // Gộp ô từ cột 1 đến 5 để làm tiêu đề lớn
                var titleRange = worksheet.Range("A1:E1");
                titleRange.Merge();
                titleRange.Value = "BÁO CÁO THỐNG KÊ TẦN SUẤT LỖI & SỰ CỐ THIẾT BỊ";
                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 16;
                titleRange.Style.Font.FontColor = XLColor.DarkBlue;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Thông tin ngày xuất
                var dateRange = worksheet.Range("A2:E2");
                dateRange.Merge();
                dateRange.Value = $"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}";
                dateRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dateRange.Style.Font.Italic = true;

                // Thông tin bộ lọc (Hiển thị người dùng đã lọc cái gì)
                var filterInfo = worksheet.Range("A3:E3");
                filterInfo.Merge();
                string thoiGian = dpTuNgay.SelectedDate.HasValue ? dpTuNgay.SelectedDate.Value.ToString("dd/MM/yyyy") : "Tất cả";
                string loaiTB = (cboCategory.SelectedItem as dynamic)?.CategoryName ?? cboCategory.Text ?? "Tất cả";
                // Lưu ý: dòng trên cần chỉnh lại tùy theo cách bạn bind dữ liệu vào ComboBox

                filterInfo.Value = $"Điều kiện lọc: Từ ngày [{thoiGian}] - Loại thiết bị: [{loaiTB}]";
                filterInfo.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // --- PHẦN 2: HEADER CỦA BẢNG DỮ LIỆU ---
                int row = 5; // Bắt đầu bảng từ dòng số 5
                worksheet.Cell(row, 1).Value = "Mã Sự Cố";
                worksheet.Cell(row, 2).Value = "Tên Thiết Bị";
                worksheet.Cell(row, 3).Value = "Mô Tả Lỗi";
                worksheet.Cell(row, 4).Value = "Ngày Ghi Nhận";
                worksheet.Cell(row, 5).Value = "Mức Độ Ưu Tiên";

                // Style cho Header (Nền xanh, chữ trắng, in đậm)
                var headerRange = worksheet.Range(row, 1, row, 5);
                headerRange.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // --- PHẦN 3: ĐỔ DỮ LIỆU VÀO ---
                row++; // Xuống dòng để bắt đầu ghi data
                foreach (var item in DanhSachSuCo)
                {
                    worksheet.Cell(row, 1).Value = item.MaSC;
                    worksheet.Cell(row, 2).Value = item.ThietBi;
                    worksheet.Cell(row, 3).Value = item.LoaiLoi;

                    // Định dạng ngày tháng
                    worksheet.Cell(row, 4).Value = item.Ngay;
                    worksheet.Cell(row, 4).Style.DateFormat.Format = "dd/MM/yyyy";

                    worksheet.Cell(row, 5).Value = item.MucDo;

                    // Tô màu chữ Mức độ ưu tiên để dễ nhìn
                    var cellMucDo = worksheet.Cell(row, 5);
                    cellMucDo.Style.Font.Bold = true;
                    if (item.MucDo == "High" || item.MucDo == "Critical")
                        cellMucDo.Style.Font.FontColor = XLColor.Red;
                    else if (item.MucDo == "Medium")
                        cellMucDo.Style.Font.FontColor = XLColor.Orange;
                    else
                        cellMucDo.Style.Font.FontColor = XLColor.Green;

                    row++;
                }

                // --- PHẦN 4: HOÀN THIỆN ---
                // Kẻ khung viền cho toàn bộ bảng dữ liệu
                var tableRange = worksheet.Range(5, 1, row - 1, 5);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Tự động giãn độ rộng cột theo nội dung
                worksheet.Columns().AdjustToContents();

                // Lưu file
                workbook.SaveAs(filePath);
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}