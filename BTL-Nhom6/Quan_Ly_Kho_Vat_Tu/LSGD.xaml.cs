using System;
using System.Windows;
using System.Windows.Controls;
using BTL_Nhom6.Services;
using BTL_Nhom6.Models;
using BTL_Nhom6.Helper;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class LSGD : Window
    {
        private readonly HistoryService _service = new HistoryService();

        private bool _canEdit = false; // Biến kiểm soát quyền Thêm/Sửa/Xóa

        public LSGD()
        {
            InitializeComponent();

            ApplyPermissions();

            // SỬA TẠI ĐÂY: Lùi ngày bắt đầu về năm 2023 hoặc 2000 để thấy dữ liệu mẫu
            var now = DateTime.Now;
            dpTuNgay.SelectedDate = new DateTime(2023, 1, 1); // <--- Sửa dòng này
            dpDenNgay.SelectedDate = now;

            LoadData();
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
                // Ẩn nút Tab Nhập kho
                if (btnTabNhapKho != null)
                {
                    btnTabNhapKho.Visibility = Visibility.Collapsed;
                }

            }
        }

        private void LoadData()
        {
            string matName = txtSearchMaVT.Text.Trim(); // Ô nhập Mã/Tên vật tư
            DateTime? from = dpTuNgay.SelectedDate;
            DateTime? to = dpDenNgay.SelectedDate;

            try
            {
                var data = _service.GetHistory(matName, from, to);
                dgLichSu.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử: " + ex.Message);
            }
        }

        // Sự kiện nút Lọc dữ liệu
        // LƯU Ý: Trong XAML bạn cần thêm Click="BtnFilter_Click" cho nút Lọc
        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        #region ĐIỀU HƯỚNG TABS

        // 1. Danh mục
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DMVT_va_DM());
        }

        // 2. Nhập kho
        private void Button_NhapKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new NKVT());
        }

        // 3. Xuất kho
        private void Button_XuatKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new XKVT());
        }

        // 4. Thẻ kho (Trang hiện tại)
        private void Button_TheKho_Click(object sender, RoutedEventArgs e) { }

        #endregion
    }
}