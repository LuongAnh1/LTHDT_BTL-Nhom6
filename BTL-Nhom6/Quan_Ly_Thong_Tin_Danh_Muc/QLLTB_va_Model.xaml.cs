using BTL_Nhom6.Helper; // Đảm bảo đúng namespace của NavigationHelper
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class QLLTB_va_Model : Window
    {
        private CategoryService _catService = new CategoryService();
        private DeviceModelService _modelService = new DeviceModelService();
        private DeviceService _deviceService = new DeviceService(); // Dùng để kiểm tra ràng buộc xóa cho DeviceModel 

        // Biến lưu ID loại đang chọn (để khi thêm model mới thì tự điền loại này)
        private int _selectedCategoryId = 0;

        // Biến kiểm tra quyền (để dùng lại nhiều chỗ)
        private bool _canEdit = false;
        public QLLTB_va_Model()
        {
            InitializeComponent();
            ApplyPermissions(); // Áp dụng phân quyền
            LoadCategories();
            LoadModels(); // Load tất cả model lúc đầu
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
                // 1. Ẩn nút Thêm mới 
                if (btnAddCategory != null) btnAddCategory.Visibility = Visibility.Collapsed;
                if (btnAddModel != null) btnAddModel.Visibility = Visibility.Collapsed;

                // 2. Ẩn cột "HÀNH ĐỘNG" (Sửa/Xóa) trong DataGrid
                // Giả sử cột Hành động là cột cuối cùng
                if (dgCategories.Columns.Count > 0)
                {
                    dgCategories.Columns[dgCategories.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
                if (dgModels.Columns.Count > 0)
                {
                    dgModels.Columns[dgModels.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
        }

        // 1. Tải danh sách Loại (Bảng trên)
        private void LoadCategories()
        {
            try
            {
                dgCategories.ItemsSource = _catService.GetAllCategories();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message);
            }
        }

        // 2. Tải danh sách Model (Bảng dưới)
        private void LoadModels(int categoryId = 0, string keyword = "")
        {
            try
            {
                // Gọi hàm GetModels mới có tham số keyword
                var list = _modelService.GetModels(categoryId, keyword);
                dgModels.ItemsSource = list;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải model: " + ex.Message);
            }
        }

        // 3. SỰ KIỆN: Khi chọn 1 dòng ở bảng Loại
        private void DgCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgCategories.SelectedItem is Category selectedCat)
            {
                _selectedCategoryId = selectedCat.CategoryID;
                lblModelHeader.Text = $"Danh sách Model thuộc: {selectedCat.CategoryName}";
            }
            else
            {
                _selectedCategoryId = 0;
                lblModelHeader.Text = "Danh sách tất cả Model";
            }

            // Gọi LoadModels kèm theo từ khóa đang có trong ô tìm kiếm
            // Để khi đổi Category, vẫn giữ bộ lọc tìm kiếm hoặc reset tùy ý bạn.
            // Ở đây tôi chọn giữ lại từ khóa tìm kiếm để trải nghiệm tốt hơn.
            string currentKeyword = txtSearchModel != null ? txtSearchModel.Text : "";
            LoadModels(_selectedCategoryId, currentKeyword);
        }
        
        // 4. SỰ KIỆN NÚT THÊM LOẠI
        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            // Hiệu ứng mờ
            System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect { Radius = 15 };
            this.Effect = blurObj;

            CategoryWindow form = new CategoryWindow();
            bool? result = form.ShowDialog();

            this.Effect = null; // Gỡ mờ

            if (result == true)
            {
                LoadCategories();
                MessageBox.Show("Thêm loại thiết bị thành công!");
            }
        }

        // 5. SỰ KIỆN NÚT THÊM MODEL
        private void BtnAddModel_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect { Radius = 15 };
            this.Effect = blurObj;

            // Truyền _selectedCategoryId (biến lưu ID loại đang chọn ở bảng trên) vào
            // Để form con tự động chọn sẵn loại đó trong ComboBox
            DeviceModelWindow form = new DeviceModelWindow(_selectedCategoryId);
            bool? result = form.ShowDialog();

            this.Effect = null;

            if (result == true)
            {
                LoadModels(_selectedCategoryId); // Load lại danh sách model
                MessageBox.Show("Thêm Model thành công!");
            }
        }

        // 6. SỰ KIỆN NÚT SỬA (Nếu bạn muốn gắn vào nút bút chì)
        // Bạn nhớ thêm sự kiện Click="BtnEditCategory_Click" vào nút Sửa trong XAML của dgCategories
        private void BtnEditCategory_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is Category cat)
            {
                System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect { Radius = 15 };
                this.Effect = blurObj;

                CategoryWindow form = new CategoryWindow(cat);
                bool? result = form.ShowDialog();

                this.Effect = null;

                if (result == true) LoadCategories();
            }
        }

        // Tương tự cho Sửa Model
        private void BtnEditModel_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu dòng cần sửa từ Tag của nút bấm
            Button btn = sender as Button;
            if (btn != null && btn.Tag is DeviceModel selectedModel)
            {
                // 2. Tạo hiệu ứng làm mờ
                System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect { Radius = 15 };
                this.Effect = blurObj;

                // 3. Mở Form con với Constructor nhận vào Model (Chế độ Sửa)
                DeviceModelWindow form = new DeviceModelWindow(selectedModel);

                // 4. Chờ người dùng thao tác
                bool? result = form.ShowDialog();

                // 5. Gỡ bỏ hiệu ứng làm mờ
                this.Effect = null;

                // 6. Nếu người dùng bấm Lưu (result == true) -> Load lại bảng
                if (result == true)
                {
                    // Quan trọng: Load lại theo ID loại đang chọn hiện tại
                    // Để tránh việc đang xem "Máy tính" sửa xong lại bị reset về "Tất cả"
                    LoadModels(_selectedCategoryId);

                    MessageBox.Show("Cập nhật thông tin Model thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        // 7. Sự kiện XÓA 
        // Xóa Model (với ràng buộc kiểm tra có Device thuộc Model này hay không)
        private void BtnDeleteModel_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is DeviceModel selectedModel)
            {
                // --- 1. KIỂM TRA RÀNG BUỘC (Sử dụng DeviceService) ---
                try
                {
                    // Gọi hàm đếm số thiết bị thuộc Model này
                    int deviceCount = _deviceService.CountDevicesByModel(selectedModel.ModelID);

                    if (deviceCount > 0)
                    {
                        // Nếu có thiết bị, chặn xóa và thông báo rõ ràng
                        MessageBox.Show($"Không thể xóa Model '{selectedModel.ModelName}'!\n\n" +
                                        $"Lý do: Đang có {deviceCount} thiết bị trong kho thuộc Model này.\n" +
                                        $"Bạn cần xóa hoặc cập nhật các thiết bị đó trước.",
                                        "Ngăn chặn thao tác",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                        return; // Dừng lệnh ngay lập tức
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Lỗi khi kiểm tra dữ liệu thiết bị: " + ex.Message);
                    return;
                }

                // --- 2. HỘP THOẠI XÁC NHẬN (Chỉ hiện khi bước 1 OK) ---
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa Model '{selectedModel.ModelName}'?\nHành động này không thể hoàn tác.",
                                             "Xác nhận xóa",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _modelService.DeleteModel(selectedModel.ModelID);
                        LoadModels(_selectedCategoryId);
                        MessageBox.Show("Đã xóa Model thành công!");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                    }
                }
            }
        }
        
        // Xóa Loại (với ràng buộc kiểm tra có Model thuộc loại này hay không)
        private void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is Category selectedCat)
            {
                // --- 1. KIỂM TRA RÀNG BUỘC (Code mới tối ưu) ---
                try
                {
                    // Gọi hàm check SQL vừa viết
                    bool hasModels = _modelService.CheckCategoryHasModels(selectedCat.CategoryID);

                    if (hasModels)
                    {
                        MessageBox.Show($"Không thể xóa Loại '{selectedCat.CategoryName}'!\n\nLý do: Đang có Model thuộc loại này.\nBạn cần xóa hoặc chuyển các Model này sang loại khác trước.",
                                        "Ngăn chặn xóa",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Stop);
                        return; // Dừng ngay, không hiện hộp thoại Yes/No
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Lỗi kiểm tra dữ liệu: " + ex.Message);
                    return;
                }

                // --- 2. HỘP THOẠI XÁC NHẬN ---
                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa loại '{selectedCat.CategoryName}'?",
                                             "Xác nhận xóa",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // --- 3. THỰC HIỆN XÓA ---
                        _catService.DeleteCategory(selectedCat.CategoryID);

                        // --- 4. LOAD LẠI DỮ LIỆU ---
                        LoadCategories();

                        // Reset bảng dưới
                        _selectedCategoryId = 0;
                        LoadModels(0);

                        MessageBox.Show("Đã xóa loại thiết bị thành công!");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                    }
                }
            }
        }

        // Tìm kiếm Model theo từ khóa
        private void TxtSearchModel_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Lấy từ khóa người dùng nhập
            string keyword = txtSearchModel.Text.Trim();

            // Gọi hàm load lại dữ liệu
            // Vẫn giữ _selectedCategoryId để tìm kiếm TRONG phạm vi loại đang chọn (hoặc tất cả nếu = 0)
            LoadModels(_selectedCategoryId, keyword);
        }
        #region Điều hướng Tab chính

        // Chuyển sang Quản lý vị trí phòng ban
        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLVTPB());
        }

        // Chuyển sang Nhà cung cấp & Báo giá
        private void Button_NCC_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NCC_va_BGLK());
        }

        // Chuyển sang Từ điển dữ liệu chung (TDDLC.xaml)
        private void Button_TDDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        #endregion

        #region Các hành động khác (Search, Thêm, Sửa, Xóa)
        // Bạn có thể viết logic lọc dữ liệu cho các ô txtSearch tại đây
        #endregion
    }
}