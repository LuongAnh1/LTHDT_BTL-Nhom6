using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;   // Namespace chứa DTO và Models
using BTL_Nhom6.Services; // Namespace chứa Services
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;      // SaveFileDialog
using ClosedXML.Excel;      // Thư viện xuất Excel
using System.Diagnostics;   // Process.Start
using System.Collections;   // IEnumerable

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCTDBH_va_NCC : Window
    {
        private readonly DeviceService _deviceService;
        private readonly SupplierService _supplierService;
        private readonly CategoryService _categoryService;

        // Biến lưu dữ liệu để xuất Excel
        private IEnumerable _danhSachBaoHanh;
        private IEnumerable _danhSachNCC;

        public BCTDBH_va_NCC()
        {
            InitializeComponent();
            _deviceService = new DeviceService();
            _supplierService = new SupplierService();
            _categoryService = new CategoryService();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadComboBoxData();
            LoadDefaultReport();
        }

        private void LoadComboBoxData()
        {
            try
            {
                var suppliers = _supplierService.GetAllSuppliers();
                suppliers.Insert(0, new Supplier { SupplierID = 0, SupplierName = "Tất cả" });
                cboSupplier.ItemsSource = suppliers;
                cboSupplier.DisplayMemberPath = "SupplierName";
                cboSupplier.SelectedValuePath = "SupplierID";
                cboSupplier.SelectedIndex = 0;

                var categories = _categoryService.GetAllCategories();
                categories.Insert(0, new Category { CategoryID = 0, CategoryName = "Tất cả" });
                cboCategory.ItemsSource = categories;
                cboCategory.DisplayMemberPath = "CategoryName";
                cboCategory.SelectedValuePath = "CategoryID";
                cboCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu bộ lọc: " + ex.Message);
            }
        }

        private void LoadDefaultReport()
        {
            dpExpiryDate.SelectedDate = null;
            LoadWarrantyData(null, 0, 0);
            LoadSupplierEvaluation();
        }

        private void LoadWarrantyData(DateTime? dateStart, int supplierId, int categoryId)
        {
            try
            {
                var data = _deviceService.GetWarrantyReport(dateStart, supplierId, categoryId);
                _danhSachBaoHanh = data; // Lưu lại để xuất
                dgWarranty.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo bảo hành: " + ex.Message);
            }
        }

        private void LoadSupplierEvaluation()
        {
            try
            {
                var data = _supplierService.GetSupplierEvaluations();
                _danhSachNCC = data; // Lưu lại để xuất
                icSuppliers.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải đánh giá NCC: " + ex.Message);
            }
        }

        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            DateTime? selectedDate = dpExpiryDate.SelectedDate;
            int supplierId = 0;
            if (cboSupplier.SelectedValue != null) int.TryParse(cboSupplier.SelectedValue.ToString(), out supplierId);
            int categoryId = 0;
            if (cboCategory.SelectedValue != null) int.TryParse(cboCategory.SelectedValue.ToString(), out categoryId);

            LoadWarrantyData(selectedDate, supplierId, categoryId);
        }

        // --- CHỨC NĂNG XUẤT EXCEL ---
        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            if (_danhSachBaoHanh == null)
            {
                MessageBox.Show("Chưa có dữ liệu bảo hành để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"BaoCao_BaoHanh_NCC_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                Title = "Lưu báo cáo Bảo hành & NCC"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExportToExcel(saveFileDialog.FileName);
                    var result = MessageBox.Show("Xuất file thành công! Bạn có muốn mở file ngay không?",
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
                // ==========================================
                // SHEET 1: THEO DÕI BẢO HÀNH
                // ==========================================
                var ws1 = workbook.Worksheets.Add("Bảo hành thiết bị");

                var title1 = ws1.Range("A1:D1");
                title1.Merge().Value = "DANH SÁCH THIẾT BỊ SẮP HẾT HẠN BẢO HÀNH";
                title1.Style.Font.Bold = true;
                title1.Style.Font.FontSize = 14;
                title1.Style.Font.FontColor = XLColor.Red;
                title1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                string locDate = dpExpiryDate.SelectedDate.HasValue ? dpExpiryDate.SelectedDate.Value.ToString("dd/MM/yyyy") : "Tất cả";
                ws1.Range("A2:D2").Merge().Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} - Lọc theo hạn: {locDate}";
                ws1.Range("A2:D2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row1 = 4;
                ws1.Cell(row1, 1).Value = "Mã Thiết Bị";
                ws1.Cell(row1, 2).Value = "Tên Thiết Bị";
                ws1.Cell(row1, 3).Value = "Ngày Mua";
                ws1.Cell(row1, 4).Value = "Ngày Hết Hạn";

                var header1 = ws1.Range(row1, 1, row1, 4);
                header1.Style.Fill.BackgroundColor = XLColor.Orange;
                // [ĐÃ SỬA] Dùng FontColor thay vì Color
                header1.Style.Font.FontColor = XLColor.White;
                header1.Style.Font.Bold = true;
                header1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row1++;
                foreach (dynamic item in _danhSachBaoHanh)
                {
                    ws1.Cell(row1, 1).Value = item.MaTB;
                    ws1.Cell(row1, 2).Value = item.TenTB;
                    ws1.Cell(row1, 3).Value = item.NgayMua;
                    ws1.Cell(row1, 4).Value = item.NgayHetHan;

                    ws1.Cell(row1, 3).Style.DateFormat.Format = "dd/MM/yyyy";
                    ws1.Cell(row1, 4).Style.DateFormat.Format = "dd/MM/yyyy";

                    
                    ws1.Cell(row1, 4).Style.Font.FontColor = XLColor.Red;
                    ws1.Cell(row1, 4).Style.Font.Bold = true;

                    row1++;
                }

                ws1.Range(4, 1, row1 - 1, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws1.Range(4, 1, row1 - 1, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws1.Columns().AdjustToContents();

                // ==========================================
                // SHEET 2: ĐÁNH GIÁ NHÀ CUNG CẤP
                // ==========================================
                var ws2 = workbook.Worksheets.Add("Nhà cung cấp");

                var title2 = ws2.Range("A1:D1");
                title2.Merge().Value = "ĐÁNH GIÁ HIỆU SUẤT NHÀ CUNG CẤP";
                title2.Style.Font.Bold = true;
                title2.Style.Font.FontSize = 14;
                title2.Style.Font.FontColor = XLColor.DarkBlue;
                title2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row2 = 3;
                ws2.Cell(row2, 1).Value = "Tên Công Ty";
                ws2.Cell(row2, 2).Value = "Liên Hệ";
                ws2.Cell(row2, 3).Value = "Đánh Giá";
                ws2.Cell(row2, 4).Value = "Mô Tả / Ghi Chú";

                var header2 = ws2.Range(row2, 1, row2, 4);
                header2.Style.Fill.BackgroundColor = XLColor.RoyalBlue;              
                header2.Style.Font.FontColor = XLColor.White;
                header2.Style.Font.Bold = true;
                header2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row2++;
                if (_danhSachNCC != null)
                {
                    foreach (dynamic item in _danhSachNCC)
                    {
                        ws2.Cell(row2, 1).Value = item.TenCongTy;
                        ws2.Cell(row2, 2).Value = item.LienHe;
                        ws2.Cell(row2, 3).Value = item.DanhGia;
                        ws2.Cell(row2, 4).Value = item.MoTa;

                        var cellRating = ws2.Cell(row2, 3);
                        cellRating.Style.Font.Bold = true;
                        cellRating.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        if (item.DanhGia == "Tốt")
                        {
                            cellRating.Style.Font.FontColor = XLColor.Green;
                            cellRating.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCFCE7");
                        }
                        else if (item.DanhGia == "Trung bình")
                        {
                            cellRating.Style.Font.FontColor = XLColor.DarkGoldenrod;
                            cellRating.Style.Fill.BackgroundColor = XLColor.FromHtml("#FEF9C3");
                        }
                        // Thêm điều kiện nếu có loại khác (ví dụ: Kém)
                        else
                        {
                            cellRating.Style.Font.FontColor = XLColor.Black;
                        }

                        row2++;
                    }
                }

                ws2.Range(3, 1, row2 - 1, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws2.Range(3, 1, row2 - 1, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws2.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCCPVT()); }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCHSBT()); }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCNSKTV()); }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTTTB()); }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new TKTSL_va_SC()); }
    }
}