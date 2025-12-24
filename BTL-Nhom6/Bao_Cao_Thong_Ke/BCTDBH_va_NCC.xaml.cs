using System;
using System.Collections.ObjectModel;
using System.Windows;
using BTL_Nhom6.Helper; // Sử dụng NavigationHelper như file mẫu của bạn

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    /// <summary>
    /// Interaction logic for BCTDBH_va_NCC.xaml
    /// </summary>
    public partial class BCTDBH_va_NCC : Window
    {
        // Khai báo các Property để Binding ra XAML
        public ObservableCollection<ThietBiBaoHanh> DanhSachHetHanBaoHanh { get; set; }
        public ObservableCollection<NhaCungCap> DanhSachNCC { get; set; }

        public BCTDBH_va_NCC()
        {
            InitializeComponent();
            LoadData();

            // Quan trọng: Gán DataContext = this để XAML có thể Binding dữ liệu từ file code-behind này
            this.DataContext = this;
        }

        private void LoadData()
        {
            // 1. Tạo dữ liệu mẫu cho Bảng: Thiết bị sắp hết hạn bảo hành
            DanhSachHetHanBaoHanh = new ObservableCollection<ThietBiBaoHanh>
            {
                new ThietBiBaoHanh { MaTB = "CNC-01", TenTB = "Máy phay CNC Vertical", NgayMua = new DateTime(2023, 1, 15), NgayHetHan = new DateTime(2025, 1, 15) },
                new ThietBiBaoHanh { MaTB = "COOL-05", TenTB = "Hệ thống làm mát công nghiệp", NgayMua = new DateTime(2023, 2, 10), NgayHetHan = new DateTime(2025, 2, 10) },
                new ThietBiBaoHanh { MaTB = "HYD-02", TenTB = "Bơm thủy lực áp suất cao", NgayMua = new DateTime(2023, 3, 20), NgayHetHan = new DateTime(2025, 3, 20) },
                new ThietBiBaoHanh { MaTB = "ROBOT-A", TenTB = "Cánh tay robot lắp ráp", NgayMua = new DateTime(2024, 1, 5), NgayHetHan = new DateTime(2026, 1, 5) }
            };

            // 2. Tạo dữ liệu mẫu cho Cards: Đánh giá nhà cung cấp
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

        // Xử lý sự kiện nút "Áp dụng" bộ lọc
        private void BtnLocDuLieu_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đã áp dụng bộ lọc dữ liệu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            // Tại đây bạn có thể viết logic lọc lại danh sách DanhSachHetHanBaoHanh dựa trên DatePicker và ComboBox
        }

        #region ĐIỀU HƯỚNG TABS (Navigation)

        // Lưu ý: Bạn cần thay thế tên các Class Window (như ChiPhiVatTu, HieuSuatBaoTri...) 
        // cho đúng với tên file thực tế trong project của bạn.

        // 1. Chi phí vật tư
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e)
        {
            // NavigationHelper.Navigate(this, new ChiPhiVatTu());
            MessageBox.Show("Chuyển đến trang: Chi phí vật tư");
        }

        // 2. Hiệu suất bảo trì
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e)
        {
            // NavigationHelper.Navigate(this, new HieuSuatBaoTri());
            MessageBox.Show("Chuyển đến trang: Hiệu suất bảo trì");
        }

        // 3. Năng suất Kỹ thuật viên
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e)
        {
            // NavigationHelper.Navigate(this, new NangSuatKTV());
            MessageBox.Show("Chuyển đến trang: Năng suất KTV");
        }

        // 4. Theo dõi bảo hành & NCC (Trang hiện tại)
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e)
        {
            // Đang ở trang này nên không làm gì
        }

        // 5. Tình trạng thiết bị
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e)
        {
            // NavigationHelper.Navigate(this, new TinhTrangThietBi());
            MessageBox.Show("Chuyển đến trang: Tình trạng thiết bị");
        }

        // 6. Thống kê lỗi
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e)
        {
            // NavigationHelper.Navigate(this, new ThongKeLoi());
            MessageBox.Show("Chuyển đến trang: Thống kê lỗi & Sự cố");
        }

        #endregion
    }

    // --- CÁC CLASS MODEL DỮ LIỆU ---

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

        // Thuộc tính tính toán (Computed Property) để hiển thị gọn trên thẻ
        public string LienHe => $"{Email} | {SoDienThoai}";
    }
}