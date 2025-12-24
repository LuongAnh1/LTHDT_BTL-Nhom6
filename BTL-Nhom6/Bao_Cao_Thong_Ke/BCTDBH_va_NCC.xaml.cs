using System;
using System.Collections.ObjectModel;
using System.Windows;
using BTL_Nhom6.Helper; 

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class BCTDBH_va_NCC : Window
    {
        // Khai báo các danh sách để Binding dữ liệu ra giao diện
        public ObservableCollection<ThietBiBaoHanh> DanhSachHetHanBaoHanh { get; set; }
        public ObservableCollection<NhaCungCap> DanhSachNCC { get; set; }

        public BCTDBH_va_NCC()
        {
            InitializeComponent();
            LoadData();

            // QUAN TRỌNG: Dòng này giúp XAML nhận diện được các biến dữ liệu
            this.DataContext = this;
        }

        private void LoadData()
        {
            // 1. Tạo dữ liệu mẫu cho Bảng: Thiết bị sắp hết hạn
            DanhSachHetHanBaoHanh = new ObservableCollection<ThietBiBaoHanh>
            {
                new ThietBiBaoHanh { MaTB = "CNC-01", TenTB = "Máy phay CNC Vertical", NgayMua = new DateTime(2023, 1, 15), NgayHetHan = new DateTime(2025, 1, 15) },
                new ThietBiBaoHanh { MaTB = "COOL-05", TenTB = "Hệ thống làm mát công nghiệp", NgayMua = new DateTime(2023, 2, 10), NgayHetHan = new DateTime(2025, 2, 10) },
                new ThietBiBaoHanh { MaTB = "HYD-02", TenTB = "Bơm thủy lực áp suất cao", NgayMua = new DateTime(2023, 3, 20), NgayHetHan = new DateTime(2025, 3, 20) },
                new ThietBiBaoHanh { MaTB = "ROBOT-A", TenTB = "Cánh tay robot lắp ráp", NgayMua = new DateTime(2024, 1, 5), NgayHetHan = new DateTime(2026, 1, 5) }
            };

            // 2. Tạo dữ liệu mẫu cho Cards: Nhà cung cấp
            DanhSachNCC = new ObservableCollection<NhaCungCap>
            {
                new NhaCungCap {
                    TenCongTy = "Công ty TNHH Thiết bị A",
                    Email = "contact@thietbia.com",
                    SoDienThoai = "0901234567",
                    DanhGia = "Tốt",
                    MoTa = "Hỗ trợ kỹ thuật nhanh chóng, cung cấp vật tư chính hãng, giá cả hợp lý. Thời gian phản hồi trung bình 2 giờ."
                },
                new NhaCungCap {
                    TenCongTy = "Công ty CP Kỹ thuật B",
                    Email = "info@kythuatb.vn",
                    SoDienThoai = "0987654321",
                    DanhGia = "Trung bình",
                    MoTa = "Chất lượng dịch vụ ổn định nhưng đôi khi giao hàng chậm. Cần cải thiện thời gian phản hồi yêu cầu."
                },
                new NhaCungCap {
                    TenCongTy = "Nhà phân phối C",
                    Email = "sales@nppc.com",
                    SoDienThoai = "02838123456",
                    DanhGia = "Tốt",
                    MoTa = "Giá cả cạnh tranh, đa dạng sản phẩm. Thủ tục thanh toán và giao hàng linh hoạt."
                }
            };
        }

        // --- KHẮC PHỤC LỖI CS1061 TẠI ĐÂY ---
        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            // Logic xử lý khi bấm nút Áp dụng
            MessageBox.Show("Đã áp dụng bộ lọc dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region ĐIỀU HƯỚNG TABS (Navigation)

        // Bạn cần thay thế 'new TenClassWindow()' bằng đúng tên các Window trong project của bạn

        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCCPVT());
        }

        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCHSBT());
        }

        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCNSKTV());
        }

        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e)
        {
            // Đang ở trang hiện tại, không làm gì
        }

        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new BCTTTB());
        }

        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TKTSL_va_SC());
        }

        #endregion
    }

    // --- CÁC CLASS MODEL DỮ LIỆU ---
    // Cần có các class này để DataGrid và ItemsControl hiểu được Binding

    public class ThietBiBaoHanh
    {
        public string MaTB { get; set; }
        public string TenTB { get; set; }
        public DateTime NgayMua { get; set; }
        public DateTime NgayHetHan { get; set; }
    }

    public class NhaCungCap
    {
        public string TenCongTy { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DanhGia { get; set; } // "Tốt", "Trung bình", "Kém"
        public string MoTa { get; set; }

        // Thuộc tính tính toán để hiển thị gọn trên thẻ
        public string LienHe => $"{Email} | {SoDienThoai}";
    }
}