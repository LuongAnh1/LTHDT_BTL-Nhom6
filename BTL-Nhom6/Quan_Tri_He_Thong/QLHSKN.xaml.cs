using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;
// Import namespace của Helper
using BTL_Nhom6.Helper;
using BTL_Nhom6.Models;
using BTL_Nhom6.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;


namespace BTL_Nhom6.Quan_Tri_He_Thong
{
    public partial class QLHSKN : Window
    {
        public bool IsAdminOrManager { get; set; }

        
        // Khai báo các Service
        private SkillService _skillService = new SkillService();
        private UserService _userService = new UserService(); // Tái sử dụng UserService có sẵn

        // List dữ liệu
        // Lưu ý: Dùng List cũng được, nhưng ObservableCollection tốt hơn nếu muốn UI tự cập nhật realtime mà không cần gán lại ItemsSource
        // Ở đây mình dùng List cho đơn giản giống code mẫu của bạn.

        public QLHSKN()
        {
            InitializeComponent();

            // Xác định quyền ngay khi khởi tạo
            int roleId = UserSession.CurrentRoleID;
            IsAdminOrManager = (roleId == 1 || roleId == 2);

            // Gán DataContext cho chính Window này để XAML có thể Binding biến IsAdminOrManager
            this.DataContext = this;

            ApplyPermissions();
            this.Loaded += QLHSKN_Loaded;
        }

        private void ApplyPermissions()
        {
            int roleId = UserSession.CurrentRoleID;

            // --- TRƯỜNG HỢP: KHÁCH HÀNG (ID = 11) ---
            if (roleId == 11)
            {
                // 1. Ẩn các tab quản trị
                if (btnTabQLND != null) btnTabQLND.Visibility = Visibility.Collapsed;
                if (btnTabSkill != null) btnTabSkill.Visibility = Visibility.Collapsed;
                if (btnTabLog != null) btnTabLog.Visibility = Visibility.Collapsed;
            }

            // --- TRƯỜNG HỢP: NHÂN VIÊN (ID = 3,4,5...) ---
            else if (roleId != 1) // Không phải Admin
            {
                // Nhân viên không được xem Nhật ký hệ thống
                if (btnTabLog != null) btnTabLog.Visibility = Visibility.Collapsed;

                // Nhân viên chỉ được xem thông tin cá nhân, không được quản lý người khác
                if (btnTabQLND != null) btnTabQLND.Visibility = Visibility.Collapsed;

                // ======================================================
                // A. CỘT TRÁI (DANH SÁCH KỸ NĂNG) - KHÔNG ĐƯỢC THÊM/SỬA/XÓA
                // ======================================================

                // 1. Ẩn nút Thêm mới
                if (btnAddSkill != null) btnAddSkill.Visibility = Visibility.Collapsed;

                // 2. Vô hiệu hóa ListView bên trái (để không bấm được nút Sửa/Xóa bên trong)
                // Cách này nhanh nhất: Toàn bộ danh sách sẽ mờ đi và không tương tác được
                if (lvSkills != null) lvSkills.IsEnabled = false;

                // ======================================================
                // B. CỘT PHẢI (GÁN KỸ NĂNG) - CHỈ ĐƯỢC XEM, CÓ THỂ CUỘN
                // ======================================================

                // 1. Ẩn nút Lưu
                if (btnSaveUserSkills != null) btnSaveUserSkills.Visibility = Visibility.Collapsed;

                // 2. Vô hiệu hóa việc tick chọn và combobox, NHƯNG VẪN CHO PHÉP CUỘN (SCROLL)
                // Thay vì disable cả ListView (sẽ không cuộn được), ta sẽ xử lý ở ItemTemplate trong XAML 
                // hoặc dùng vòng lặp disable từng item (phức tạp).
                // Cách tốt nhất là dùng "IsHitTestVisible" nhưng nó cũng chặn cuộn.

                // GIẢI PHÁP: Disable các control con bên trong (CheckBox, ComboBox) thông qua Binding ở XAML
                // (Xem phần cập nhật XAML bên dưới)

                // 3. Khóa chọn người dùng (Chỉ xem được chính mình)
                if (cboUsers != null) cboUsers.IsEnabled = false;
            }
        }

        // Sự kiện Loaded của Window
        private void QLHSKN_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSkillsList(); // Load cột trái
            LoadUsersComboBox(); // Load combobox cột phải
        }

        // --- 1. XỬ LÝ CỘT TRÁI (QUẢN LÝ KỸ NĂNG) ---
        // Load danh sách kỹ năng vào ListView bên trái
        private void LoadSkillsList()
        {
            try
            {
                var skills = _skillService.GetAllSkills();
                lvSkills.ItemsSource = skills;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải kỹ năng: " + ex.Message);
            }
        }
        // Xử lý nút Thêm kỹ năng
        private void BtnAddSkill_Click(object sender, RoutedEventArgs e)
        {
            // --- BƯỚC 1: TẠO HIỆU ỨNG MỜ ---
            System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect();
            blurObj.Radius = 15;
            this.Effect = blurObj; // Làm mờ form chính

            try
            {
                // --- BƯỚC 2: MỞ FORM CON ---
                SkillWindow skillWindow = new SkillWindow();

                // Code sẽ dừng ở đây chờ người dùng thao tác xong
                bool? result = skillWindow.ShowDialog();

                // --- BƯỚC 3: GỠ BỎ HIỆU ỨNG (Ngay sau khi form con đóng) ---
                this.Effect = null;

                // --- BƯỚC 4: XỬ LÝ DỮ LIỆU NẾU NGƯỜI DÙNG BẤM LƯU ---
                // (Trong SkillWindow.xaml.cs chúng ta đã set DialogResult = true khi lưu)
                if (result == true)
                {
                    // Lấy dữ liệu từ form con
                    var newSkill = skillWindow.CreatedSkill;

                    // Gọi Service lưu vào database
                    _skillService.AddSkill(newSkill);

                    // Reload danh sách bên trái
                    LoadSkillsList();

                    // Reload danh sách bên phải (nếu đang chọn user)
                    if (cboUsers.SelectedValue != null)
                    {
                        if (int.TryParse(cboUsers.SelectedValue.ToString(), out int userId))
                        {
                            LoadTechnicianSkills(userId);
                        }
                    }

                    // (Tuỳ chọn) MessageBox.Show("Thêm thành công!");
                }
            }
            catch (Exception ex)
            {
                // Đảm bảo gỡ bỏ hiệu ứng mờ nếu có lỗi xảy ra để người dùng còn nhìn thấy thông báo lỗi
                this.Effect = null;
                MessageBox.Show("Lỗi thêm kỹ năng: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Xử lý nút Xóa kỹ năng
        private void BtnDeleteSkill_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra quyền
            if (UserSession.CurrentRoleID != 1 && UserSession.CurrentRoleID != 2)
            {
                MessageBox.Show("Bạn không có quyền thực hiện thao tác này.", "Thông báo");
                return;
            }
            if (MessageBox.Show("Xóa kỹ năng này?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            try
            {
                Button btn = sender as Button;
                int skillId = (int)btn.Tag;

                _skillService.DeleteSkill(skillId);

                LoadSkillsList();
                if (cboUsers.SelectedValue != null)
                    LoadTechnicianSkills((int)cboUsers.SelectedValue);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa: " + ex.Message);
            }
        }
        // Xử lý nút Sửa kỹ năng
        private void BtnEditSkill_Click(object sender, RoutedEventArgs e)
        {
            // --- BƯỚC 1: LẤY DỮ LIỆU CẦN SỬA ---
            Button btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            int skillId = (int)btn.Tag;

            // Tìm đối tượng Skill trong danh sách đang hiển thị (ItemsSource của ListView/DataGrid)
            // Giả sử tên control hiển thị danh sách skill là "lvSkills" (hoặc tên bạn đang đặt)
            var listSkills = lvSkills.ItemsSource as List<Skill>;
            var skillToEdit = listSkills?.FirstOrDefault(s => s.SkillID == skillId);

            if (skillToEdit == null)
            {
                MessageBox.Show("Không tìm thấy thông tin kỹ năng!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // --- BƯỚC 2: TẠO HIỆU ỨNG MỜ ---
            System.Windows.Media.Effects.BlurEffect blurObj = new System.Windows.Media.Effects.BlurEffect();
            blurObj.Radius = 15;
            this.Effect = blurObj;

            try
            {
                // --- BƯỚC 3: MỞ FORM CON (CHẾ ĐỘ EDIT) ---
                // Truyền skillToEdit vào Constructor
                SkillWindow skillWindow = new SkillWindow(skillToEdit);

                bool? result = skillWindow.ShowDialog();

                // --- BƯỚC 4: GỠ BỎ HIỆU ỨNG ---
                this.Effect = null;

                // --- BƯỚC 5: XỬ LÝ LƯU ---
                if (result == true)
                {
                    // Lấy dữ liệu đã chỉnh sửa từ form con
                    var updatedSkill = skillWindow.CreatedSkill;

                    // Gọi Service Update
                    _skillService.UpdateSkill(updatedSkill);

                    // Reload lại giao diện
                    LoadSkillsList();

                    // Nếu user bên phải đang active, load lại luôn
                    if (cboUsers.SelectedValue != null && int.TryParse(cboUsers.SelectedValue.ToString(), out int userId))
                    {
                        LoadTechnicianSkills(userId);
                    }

                    // MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                this.Effect = null;
                MessageBox.Show("Lỗi cập nhật: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- 2. XỬ LÝ CỘT PHẢI (GÁN KỸ NĂNG CHO USER) ---
        // Load danh sách User vào ComboBox
        private void LoadUsersComboBox()
        {
            try
            {
                var allUsers = _userService.GetAllUsers("", 0);
                var filteredUsers = allUsers.Where(u => u.RoleGroup == "Nhân viên" && u.IsActive == true).ToList();

                cboUsers.ItemsSource = filteredUsers;
                cboUsers.DisplayMemberPath = "FullName";
                cboUsers.SelectedValuePath = "UserID";

                // --- LOGIC MỚI: TỰ ĐỘNG CHỌN NGƯỜI DÙNG HIỆN TẠI ---
                int currentUserId = UserSession.CurrentUserID;
                int currentRoleId = UserSession.CurrentRoleID;

                // Nếu là Nhân viên thường (Không phải Admin/Quản lý)
                if (currentRoleId != 1 && currentRoleId != 2)
                {
                    // Tìm và chọn chính mình trong danh sách
                    // (Lưu ý: cboUsers.SelectedValue mong đợi kiểu int vì SelectedValuePath="UserID")
                    cboUsers.SelectedValue = currentUserId;

                    // Gọi hàm load dữ liệu luôn (đề phòng sự kiện SelectionChanged không kích hoạt khi gán code)
                    LoadTechnicianSkills(currentUserId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải user: " + ex.Message);
            }
        }
        // Xử lý sự kiện khi chọn user trong ComboBox
        private void cboUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboUsers.SelectedValue == null) return;

            if (int.TryParse(cboUsers.SelectedValue.ToString(), out int userId))
            {
                LoadTechnicianSkills(userId);
            }
        }
        // Load danh sách kỹ năng của kỹ thuật viên (user) đã chọn
        private void LoadTechnicianSkills(int userId)
        {
            try
            {
                // Gọi Service lấy danh sách Skill kèm trạng thái tick chọn của user này
                var techSkills = _skillService.GetSkillsByUser(userId);
                lvTechSkills.ItemsSource = techSkills;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải hồ sơ: " + ex.Message);
            }
        }
        // Xử lý nút Lưu kỹ năng cho user
        private void BtnSaveUserSkills_Click(object sender, RoutedEventArgs e)
        {
            if (cboUsers.SelectedValue == null)
            {
                MessageBox.Show("Chọn nhân viên trước!");
                return;
            }

            try
            {
                int userId = (int)cboUsers.SelectedValue;

                // Lấy danh sách items hiện tại từ ListView
                // Lưu ý: ItemsSource đang chứa List<TechnicianSkillViewModel>
                var currentList = lvTechSkills.ItemsSource as List<TechnicianSkillViewModel>;

                if (currentList != null)
                {
                    bool result = _skillService.SaveUserSkills(userId, currentList);
                    if (result)
                        MessageBox.Show("Lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        MessageBox.Show("Lưu thất bại!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu: " + ex.Message);
            }
        }


        // --- Xử lý sự kiện Button ---
        
        // Chuyển tới trang Quản lý người dùng và phân quyền
        private void Button_QLND_va_PQ_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new QLND_va_PQ());
        }
        // Chuyển tới trang Thay đổi mật khẩu và thông tin cá nhân 
        private void Button_TDMK_va_TTCN_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDMK_va_TTCN());
        }
        // Chuyển tới trang Nhật ký và sao lưu dữ liệu 
        private void Button_NK_va_SLDL_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NK_va_SLDL());
        }
        // Quay lại trang chủ
        private void Button_Home_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Trang_Chu());
        }
        // Nút đăng xuất
        private void Button_Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new Dang_Nhap());
        }
    }
}
