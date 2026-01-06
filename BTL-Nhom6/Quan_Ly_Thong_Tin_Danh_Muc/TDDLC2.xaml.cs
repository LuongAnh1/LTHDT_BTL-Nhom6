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
    public partial class TDDLC2 : Window
    {
        private readonly UnitService _unitService = new UnitService();
        private List<ProductUnit> _originalList = new List<ProductUnit>();
        // Biến kiểm tra quyền (để dùng lại nhiều chỗ)
        private bool _canEdit = false;
        public TDDLC2()
        {
            InitializeComponent();
            ApplyPermissions(); // Áp dụng phân quyền
            Loaded += TDDLC2_Loaded;
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
                if (btnAddNew != null) btnAddNew.Visibility = Visibility.Collapsed;

                // 2. Ẩn cột "HÀNH ĐỘNG" (Sửa/Xóa) trong DataGrid
                // Giả sử cột Hành động là cột cuối cùng
                if (dgDonViTinh.Columns.Count > 0)
                {
                    dgDonViTinh.Columns[dgDonViTinh.Columns.Count - 1].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void TDDLC2_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _originalList = _unitService.GetAllUnits();
                dgDonViTinh.ItemsSource = _originalList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // TÌM KIẾM
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string ma = txtSearchMa.Text.Trim().ToLower();
            string ten = txtSearchTen.Text.Trim().ToLower();

            if (_originalList == null) return;

            var filtered = _originalList.Where(x =>
                (string.IsNullOrEmpty(ma) || x.UnitID.ToString().Contains(ma)) &&
                (string.IsNullOrEmpty(ten) || x.UnitName.ToLower().Contains(ten))
            ).ToList();

            dgDonViTinh.ItemsSource = filtered;
        }

        // THÊM MỚI (Có Blur)
        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blur = new BlurEffect { Radius = 15 };
            this.Effect = blur;
            try
            {
                UnitWindow window = new UnitWindow();
                if (window.ShowDialog() == true)
                {
                    _unitService.AddUnit(window.ResultUnit);
                    LoadData();
                    MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Effect = null;
            }
        }

        // SỬA (Có Blur)
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int id = (int)btn.Tag;
                var item = _originalList.FirstOrDefault(x => x.UnitID == id);
                if (item != null)
                {
                    BlurEffect blur = new BlurEffect { Radius = 15 };
                    this.Effect = blur;
                    try
                    {
                        UnitWindow window = new UnitWindow(item);
                        if (window.ShowDialog() == true)
                        {
                            _unitService.UpdateUnit(window.ResultUnit);
                            LoadData();
                            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        this.Effect = null;
                    }
                }
            }
        }

        // XÓA
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int id = (int)btn.Tag;

                // --- 1. KIỂM TRA RÀNG BUỘC DỮ LIỆU ---
                // Gọi hàm kiểm tra vừa viết bên Service
                if (_unitService.IsUnitInUse(id))
                {
                    MessageBox.Show(
                        "Không thể xóa đơn vị này!\n\nLý do: Đơn vị tính này đang được sử dụng cho các Vật tư trong kho.\nVui lòng gỡ bỏ hoặc thay đổi đơn vị tính của các vật tư liên quan trước khi xóa.",
                        "Cảnh báo ràng buộc dữ liệu",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return; // Dừng lại, không thực hiện xóa
                }

                // --- 2. XÁC NHẬN XÓA ---
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa Đơn vị tính có ID: {id} không?",
                                    "Xác nhận xóa",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // --- 3. THỰC HIỆN XÓA ---
                        _unitService.DeleteUnit(id);

                        // Load lại bảng
                        LoadData();
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        // Phòng trường hợp lỗi khác (mất kết nối mạng, DB lỗi...)
                        MessageBox.Show("Đã xảy ra lỗi khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        #region Chuyển đổi Danh mục lớn
        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new QLVTPB());
        private void Button_QLLTB_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new QLLTB_va_Model());
        private void Button_NCC_Click(object sender, RoutedEventArgs e) => NavigationHelper.Navigate(this, new NCC_va_BGLK());
        #endregion

        #region Chuyển đổi Tab con
        private void Button_TrangThai_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC()); // Về trang 1

        private void Button_Loi_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new TDDLC3()); // Sang trang 3
        #endregion
    }
}