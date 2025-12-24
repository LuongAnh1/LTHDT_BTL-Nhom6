using System;
using System.Collections.ObjectModel;
using System.Windows;
using BTL_Nhom6.Helper; // Namespace giả định cho NavigationHelper
using BTL_Nhom6.Models; // Nếu bạn tách Model ra file riêng, hoặc để chung bên dưới như cũ

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    public partial class TKTSL_va_SC : Window
    {
        // 1. Dữ liệu cho Bảng danh sách sự cố
        public ObservableCollection<SuCoModel> DanhSachSuCo { get; set; }

        // 2. Dữ liệu cho Biểu đồ cột
        public ObservableCollection<BieuDoCotModel> DuLieuBieuDo { get; set; }

        // --- CẤU HÌNH BIỂU ĐỒ ---
        // Chiều cao tối đa của vùng vẽ (trừ đi margin của Grid trong XAML)
        private const double MAX_GRAPH_HEIGHT = 250;
        // Giá trị trục Y cao nhất (khớp với số 20 trên trục Y trong XAML)
        private const double MAX_Y_VALUE = 20;

        public TKTSL_va_SC()
        {
            InitializeComponent();

            // Khởi tạo các collection trước khi LoadData
            DanhSachSuCo = new ObservableCollection<SuCoModel>();
            DuLieuBieuDo = new ObservableCollection<BieuDoCotModel>();

            LoadData();

            // Gán DataContext để Binding hoạt động
            this.DataContext = this;
        }

        private void LoadData()
        {
            // 1. Tải dữ liệu Bảng (Giữ nguyên mẫu của bạn)
            DanhSachSuCo.Clear();
            DanhSachSuCo.Add(new SuCoModel { MaSC = "SC001", ThietBi = "Máy bơm B-01", LoaiLoi = "Lỗi cơ khí - Rò rỉ dầu", Ngay = new DateTime(2023, 10, 25), MucDo = "High" });
            DanhSachSuCo.Add(new SuCoModel { MaSC = "SC002", ThietBi = "Hệ thống HVAC-03", LoaiLoi = "Lỗi điện - Mất nguồn", Ngay = new DateTime(2023, 10, 24), MucDo = "High" });
            DanhSachSuCo.Add(new SuCoModel { MaSC = "SC003", ThietBi = "Dây chuyền SX-A", LoaiLoi = "Lỗi phần mềm - Treo hệ thống", Ngay = new DateTime(2023, 10, 24), MucDo = "Medium" });
            DanhSachSuCo.Add(new SuCoModel { MaSC = "SC004", ThietBi = "Máy bơm B-02", LoaiLoi = "Lỗi cơ khí - Động cơ quá nhiệt", Ngay = new DateTime(2023, 10, 23), MucDo = "High" });
            DanhSachSuCo.Add(new SuCoModel { MaSC = "SC005", ThietBi = "Hệ thống HVAC-01", LoaiLoi = "Lỗi cơ khí - Quạt không chạy", Ngay = new DateTime(2023, 10, 22), MucDo = "Low" });

            // 2. Tải dữ liệu Biểu đồ ban đầu (Có tính toán chiều cao)
            UpdateChartData();
        }

        // Hàm hỗ trợ thêm cột vào biểu đồ và tính chiều cao pixel tự động
        private void AddChartColumn(string label, int value)
        {
            // Công thức: (Giá trị thực / Max trục Y) * Chiều cao khung vẽ
            double pixelHeight = (value / MAX_Y_VALUE) * MAX_GRAPH_HEIGHT;

            // Giới hạn không cho vượt quá khung (nếu dữ liệu > 20)
            if (pixelHeight > MAX_GRAPH_HEIGHT) pixelHeight = MAX_GRAPH_HEIGHT;

            DuLieuBieuDo.Add(new BieuDoCotModel
            {
                Label = label,
                GiaTriThuc = value,
                HeightValue = pixelHeight
            });
        }

        // Hàm giả lập dữ liệu ban đầu
        private void UpdateChartData()
        {
            DuLieuBieuDo.Clear();
            AddChartColumn("Lỗi cơ khí", 17);
            AddChartColumn("Lỗi điện", 12);
            AddChartColumn("Lỗi phần mềm", 9);
            AddChartColumn("Lỗi vận hành", 6);
            AddChartColumn("Khác", 3);
        }

        private void BtnApDung_Click(object sender, RoutedEventArgs e)
        {
            // XỬ LÝ KHI BẤM NÚT ÁP DỤNG
            // 1. Xóa dữ liệu cũ
            DuLieuBieuDo.Clear();

            // 2. Tạo dữ liệu ngẫu nhiên để mô phỏng sự thay đổi (Thực tế sẽ lấy từ DB theo ngày/tháng)
            Random rnd = new Random();

            AddChartColumn("Lỗi cơ khí", rnd.Next(5, 20));
            AddChartColumn("Lỗi điện", rnd.Next(2, 15));
            AddChartColumn("Lỗi phần mềm", rnd.Next(1, 10));
            AddChartColumn("Lỗi vận hành", rnd.Next(3, 12));
            AddChartColumn("Khác", rnd.Next(0, 5));

            // MessageBox.Show("Đã cập nhật dữ liệu báo cáo!", "Thông báo");
        }

        private void BtnXuatBaoCao_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang xuất file Excel...", "Thông báo");
        }

        #region NAVIGATION TABS
        private void Button_ChiPhiVatTu_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCCPVT()); }
        private void Button_HieuSuatBaoTri_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCHSBT()); }
        private void Button_NangSuatKTV_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCNSKTV()); }
        private void Button_BaoHanhNCC_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTDBH_va_NCC()); }
        private void Button_TinhTrangThietBi_Click(object sender, RoutedEventArgs e) { NavigationHelper.Navigate(this, new BCTTTB()); }
        private void Button_ThongKeLoi_Click(object sender, RoutedEventArgs e) { /* Trang hiện tại */ }
        #endregion
    }

    // --- Models ---
    public class SuCoModel
    {
        public string MaSC { get; set; }
        public string ThietBi { get; set; }
        public string LoaiLoi { get; set; }
        public DateTime Ngay { get; set; }
        public string MucDo { get; set; } // High, Medium, Low
    }

    public class BieuDoCotModel
    {
        public string Label { get; set; }

        // Thêm thuộc tính này để lưu giá trị thật (dùng hiển thị Tooltip hoặc text)
        public int GiaTriThuc { get; set; }

        // Chiều cao hiển thị trên giao diện (đã quy đổi sang pixel)
        public double HeightValue { get; set; }
    }
}