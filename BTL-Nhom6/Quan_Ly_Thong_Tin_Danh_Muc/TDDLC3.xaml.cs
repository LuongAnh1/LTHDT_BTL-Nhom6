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
    public partial class TDDLC3 : Window
    {
        private readonly CommonErrorService _service = new CommonErrorService();
        private List<CommonError> _originalList = new List<CommonError>();

        public TDDLC3()
        {
            InitializeComponent();
            Loaded += TDDLC3_Loaded;
        }

        private void TDDLC3_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _originalList = _service.GetAllErrors();
                dgCommonErrors.ItemsSource = _originalList;
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
                (string.IsNullOrEmpty(ma) || x.ErrorID.ToString().Contains(ma)) &&
                (string.IsNullOrEmpty(ten) || x.ErrorName.ToLower().Contains(ten) ||
                 (x.Description != null && x.Description.ToLower().Contains(ten))) // Tìm cả trong mô tả
            ).ToList();

            dgCommonErrors.ItemsSource = filtered;
        }

        // THÊM MỚI
        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            BlurEffect blur = new BlurEffect { Radius = 15 };
            this.Effect = blur;
            try
            {
                CommonErrorWindow window = new CommonErrorWindow();
                if (window.ShowDialog() == true)
                {
                    _service.AddError(window.ResultError);
                    LoadData();
                    MessageBox.Show("Thêm thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { this.Effect = null; }
        }

        // SỬA
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                int id = (int)btn.Tag;
                var item = _originalList.FirstOrDefault(x => x.ErrorID == id);
                if (item != null)
                {
                    BlurEffect blur = new BlurEffect { Radius = 15 };
                    this.Effect = blur;
                    try
                    {
                        CommonErrorWindow window = new CommonErrorWindow(item);
                        if (window.ShowDialog() == true)
                        {
                            _service.UpdateError(window.ResultError);
                            LoadData();
                            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally { this.Effect = null; }
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
                if (MessageBox.Show($"Bạn chắc chắn xóa Lỗi có ID {id}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _service.DeleteError(id);
                        LoadData();
                        MessageBox.Show("Xóa thành công!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
        }

        #region Chuyển đổi Tab chính (Main Tabs)

        private void Button_QLVTPB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLVTPB());

        private void Button_QLLTB_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new QLLTB_va_Model());

        private void Button_NCC_Click(object sender, RoutedEventArgs e) =>
            NavigationHelper.Navigate(this, new NCC_va_BGLK());

        #endregion

        #region Chuyển đổi Tab con (Sub Tabs)
        private void Button_TrangThai_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC());
        }

        // Thêm hàm xử lý sự kiện Click cho nút Đơn vị tính
        private void Button_DonViTinh_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDDLC2());
        }

        #endregion
    }
}