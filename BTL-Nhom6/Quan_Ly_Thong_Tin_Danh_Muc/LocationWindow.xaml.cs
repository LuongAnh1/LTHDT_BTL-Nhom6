using System.Windows;
using System.Windows.Input;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;

namespace BTL_Nhom6.Quan_Ly_Thong_Tin_Danh_Muc
{
    public partial class LocationWindow : Window
    {
        private LocationService _service = new LocationService();
        private Location _currentLocation = null; // Biến lưu vị trí đang sửa (nếu có)

        // Constructor 1: Dùng cho THÊM MỚI
        public LocationWindow()
        {
            InitializeComponent();
            LoadParents();
            lblTitle.Text = "THÊM VỊ TRÍ MỚI";
        }

        // Constructor 2: Dùng cho CẬP NHẬT (Sửa)
        public LocationWindow(Location loc)
        {
            InitializeComponent();
            LoadParents();

            // Đổ dữ liệu cũ vào form
            _currentLocation = loc;
            lblTitle.Text = "CẬP NHẬT VỊ TRÍ";
            txtLocationName.Text = loc.LocationName;
            txtDescription.Text = loc.Description;
            cboParent.SelectedValue = loc.ParentLocationID;
        }

        // Load danh sách vị trí để chọn làm cha
        private void LoadParents()
        {
            var list = _service.GetAllLocations();

            // Nếu đang sửa, loại bỏ chính nó khỏi danh sách cha (A không thể là cha của A)
            if (_currentLocation != null)
            {
                list.RemoveAll(x => x.LocationID == _currentLocation.LocationID);
            }

            cboParent.ItemsSource = list;
        }

        // Xử lý nút Lưu
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(txtLocationName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên vị trí!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLocationName.Focus();
                return;
            }

            try
            {
                // 2. Tạo đối tượng Location từ form
                Location loc = new Location();
                loc.LocationName = txtLocationName.Text.Trim();
                loc.Description = txtDescription.Text.Trim();

                // Lấy ID cha (nếu không chọn thì là null)
                if (cboParent.SelectedValue != null)
                    loc.ParentLocationID = (int)cboParent.SelectedValue;
                else
                    loc.ParentLocationID = null;

                // 3. Gọi Service để lưu vào DB
                if (_currentLocation == null)
                {
                    // Thêm mới
                    _service.AddLocation(loc);
                }
                else
                {
                    // Cập nhật (giữ nguyên ID cũ)
                    loc.LocationID = _currentLocation.LocationID;
                    _service.UpdateLocation(loc);
                }

                // 4. Đóng form và trả về True
                this.DialogResult = true;
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Nút Hủy
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Nút xóa chọn ComboBox (Đặt về null - Vị trí gốc)
        private void BtnClearCombo_Click(object sender, RoutedEventArgs e)
        {
            cboParent.SelectedIndex = -1;
        }

        // Cho phép kéo thả cửa sổ (vì WindowStyle=None)
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}