using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using BTL_Nhom6.Helper; // Đảm bảo đúng namespace của NavigationHelper

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class NCC_va_BGLK : Window
    {
        private readonly SupplierService _supplierService = new SupplierService();
        private readonly SupplierQuoteDTOService _quoteService = new SupplierQuoteDTOService();


        // Danh sách gốc cho 2 bảng
        private List<Supplier> _listSuppliers = new List<Supplier>();
        private List<SupplierQuoteDTO> _listQuotes = new List<SupplierQuoteDTO>();

        // Biến kiểm tra quyền (để dùng lại nhiều chỗ)
        private bool _canEdit = false;

        public NCC_va_BGLK()
        {
            InitializeComponent();

            ApplyPermissions(); // Áp dụng phân quyền khi khởi tạo

            Loaded += QLNCC_Loaded;
        }

        // --- HÀM PHÂN QUYỀN ---
        private void ApplyPermissions()
        {
            int roleId = UserSession.CurrentRoleID;

            // Quy định: Chỉ Admin (1) và Quản lý (2) mới được Thêm/Sửa/Xóa
            if (roleId == 1 || roleId == 2)
            {
                _canEdit = true;
            }
            else
            {
                _canEdit = false; // Nhân viên thường, Khách hàng...
            }

            // Nếu không có quyền sửa -> Ẩn các nút thao tác
            if (!_canEdit)
            {
                // 1. Ẩn nút Thêm mới (Cần đặt x:Name="btnAdd" trong XAML)
                if (btnAddSupplier != null) btnAddSupplier.Visibility = Visibility.Collapsed;
                if (bntAddQote != null) bntAddQote.Visibility = Visibility.Collapsed;

                // 2. Ẩn cột "HÀNH ĐỘNG" (Sửa/Xóa) trong DataGrid
                // Giả sử cột Hành động là cột cuối cùng
                if (dgSuppliers.Columns.Count > 0)
                {
                    dgSuppliers.Columns[dgSuppliers.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
                if (dgQuotes.Columns.Count > 0)
                {
                    dgQuotes.Columns[dgQuotes.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void QLNCC_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAllData();
        }

        private void LoadAllData()
        {
            try
            {
                // 1. Load bảng NCC
                _listSuppliers = _supplierService.GetAllSuppliers();
                dgSuppliers.ItemsSource = _listSuppliers;

                // 2. Load bảng Báo Giá (JOIN từ Service)
                _listQuotes = _supplierService.GetAllQuotes();
                dgQuotes.ItemsSource = _listQuotes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // --- XỬ LÝ TÌM KIẾM BẢNG TRÊN (NCC) ---
        private void txtSearchNCC_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearchNCC.Text.ToLower();
            var filtered = _listSuppliers.Where(x => x.SupplierName.ToLower().Contains(keyword) || x.Phone.Contains(keyword)).ToList();
            dgSuppliers.ItemsSource = filtered;
        }

        // --- XỬ LÝ TÌM KIẾM BẢNG DƯỚI (BÁO GIÁ) ---
        private void txtSearchQuote_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearchQuote.Text.ToLower();
            var filtered = _listQuotes.Where(x =>
                x.ModelName.ToLower().Contains(keyword) ||
                x.SupplierName.ToLower().Contains(keyword)
            ).ToList();
            dgQuotes.ItemsSource = filtered;
        }

        // THÊM MỚI NHÀ CUNG CẤP
        private void btnAddNewSupplier_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blur = new BlurEffect { Radius = 15 };
            this.Effect = blur;
            try
            {
                // Gọi form SupplierWindow đã làm ở bước trước
                SupplierWindow win = new SupplierWindow();
                if (win.ShowDialog() == true)
                {
                    _supplierService.AddSupplier(win.ResultSupplier);
                    LoadAllData(); // Refresh cả trang
                }
            }
            finally { this.Effect = null; }
        }

        // Sự kiện khi nhấn nút Sửa nhà cung cấp
        private void btnEditSupplier_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blur = new BlurEffect { Radius = 15 };
            this.Effect = blur;

            try
            {
                var button = sender as Button;
                if (button == null) return;

                int id = (int)button.Tag;

                // Tìm NCC trong danh sách gốc
                Supplier existing = _listSuppliers.FirstOrDefault(s => s.SupplierID == id);
                if (existing == null) return;

                // Mở cửa sổ sửa
                SupplierWindow win = new SupplierWindow(existing);
                if (win.ShowDialog() == true)
                {
                    _supplierService.UpdateSupplier(win.ResultSupplier);
                    LoadAllData();
                }
            }
            finally
            {
                this.Effect = null;
            }
        }


        // Sự kiện khi nhấn nút Xóa nhà cung cấp
        private void btnDeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            int id = (int)button.Tag;
            // Tìm nhà cung cấp được chọn
            Supplier existing = _listSuppliers.FirstOrDefault(s => s.SupplierID == id);
            if (existing == null) return;

            // Kiểm tra xem có báo giá hoặc model nào liên kết với nhà cung cấp này không
            bool hasQuotes = _listQuotes.Any(q => q.SupplierID == id); // cần có SupplierID trong DTO
            if (hasQuotes)
            {
                MessageBox.Show("Không thể xóa nhà cung cấp này vì đã có báo giá liên kết.",
                                "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Hoặc nếu có liên kết khác: 
            // bool hasModels = _service.HasRelatedModel(existing.SupplierID);
            // if (hasModels) { ... }

            // Xác nhận xóa
            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa nhà cung cấp \"{existing.SupplierName}\" không?",
                                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                _supplierService.DeleteSupplier(existing.SupplierID);
                LoadAllData();
            }
        }

        // THÊM MỚI BÁO GIÁ
        private void btnAddQuote_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blur = new BlurEffect { Radius = 15 };
            this.Effect = blur;

            try
            {
                ModelSuppliersWindow win = new ModelSuppliersWindow();
                if (win.ShowDialog() == true)
                {
                    // kiểm tra trùng khóa ghép (nên có ở UI)
                    var existed = _quoteService.GetQuote(
                        win.Result.ModelID,
                        win.Result.SupplierID
                    );

                    if (existed != null)
                    {
                        MessageBox.Show(
                            "Báo giá cho Model và Nhà cung cấp này đã tồn tại.",
                            "Trùng dữ liệu",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        return;
                    }

                    _quoteService.AddQuote(win.Result);
                    LoadAllData();
                }
            }
            finally
            {
                this.Effect = null;
            }
        }

        // Chỉnh sửa báo giá
        private void btnEditQuote_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blur = new BlurEffect { Radius = 15 };
            this.Effect = blur;

            try
            {
                var dto = (sender as Button)?.Tag as SupplierQuoteDTO;
                if (dto == null) return;

                // Lấy entity thật từ DB theo khóa ghép
                var entity = _quoteService.GetQuote(dto.ModelID, dto.SupplierID);
                if (entity == null) return;

                ModelSuppliersWindow win = new ModelSuppliersWindow(entity);
                if (win.ShowDialog() == true)
                {
                    _quoteService.UpdateQuote(win.Result);
                    LoadAllData();
                }
            }
            finally
            {
                this.Effect = null;
            }
        }

        // Xóa báo giá
        private void btnDeleteQuote_Click(object sender, RoutedEventArgs e)
        {
            var dto = (sender as Button)?.Tag as SupplierQuoteDTO;
            if (dto == null) return;

            if (MessageBox.Show(
                $"Xóa báo giá:\n{dto.ModelName} - {dto.SupplierName} ?",
                "Xác nhận",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            _quoteService.DeleteQuote(dto.ModelID, dto.SupplierID);
            LoadAllData();
        }


        #region Điều hướng Tab chính

        // Xử lý nút Quản lý vị trí phòng ban
        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLVTPB());
        }

        // Xử lý nút Quản lý loại thiết bị
        private void Button_QLLTB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLLTB_va_Model());
        }

        // Xử lý nút Từ điển dữ liệu chung (Chuyển sang TDDLC.xaml)
        private void Button_TDDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        #endregion

        #region Các hành động khác (Thêm, Sửa, Xóa NCC)
        // Bạn có thể viết các hàm xử lý dữ liệu nhà cung cấp tại đây
        #endregion

        
    }
}