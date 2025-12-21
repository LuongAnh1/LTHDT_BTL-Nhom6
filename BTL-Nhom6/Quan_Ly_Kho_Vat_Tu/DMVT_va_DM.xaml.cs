using BTL_Nhom6.Helper;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Quan_Ly_Kho_Vat_Tu
{
    public partial class DMVT_va_DM : Window
    {
        public DMVT_va_DM()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var listData = new List<VatTu>
            {
                new VatTu { MaDM = "VT-001", TenDM = "Dầu máy nén khí", MoTa = "Dầu chuyên dụng cho máy nén khí trục vít", DinhMuc = "20", DonVi = "Lít / Tháng" },
                new VatTu { MaDM = "VT-002", TenDM = "Lọc dầu", MoTa = "Bộ lọc tách dầu máy nén", DinhMuc = "2", DonVi = "Cái / Quý" },
                new VatTu { MaDM = "VT-003", TenDM = "Mỡ bôi trơn", MoTa = "Mỡ chịu nhiệt cao độ", DinhMuc = "5", DonVi = "Kg / Tháng" },
                new VatTu { MaDM = "VT-004", TenDM = "Vòng bi 6205", MoTa = "Vòng bi rãnh sâu SKF", DinhMuc = "10", DonVi = "Cái / Năm" }
            };
            dgVatTu.ItemsSource = listData;
        }

        #region ĐIỀU HƯỚNG TABS

        // 1. Danh mục (Trang hiện tại)
        private void Button_DanhMuc_Click(object sender, RoutedEventArgs e)
        {
            // Đang ở trang này rồi thì không làm gì cả
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

        // 4. Thẻ kho
        private void Button_TheKho_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new LSGD());
        }

        #endregion
    }

    public class VatTu
    {
        public string MaDM { get; set; }
        public string TenDM { get; set; }
        public string MoTa { get; set; }
        public string DinhMuc { get; set; }
        public string DonVi { get; set; }
    }
}