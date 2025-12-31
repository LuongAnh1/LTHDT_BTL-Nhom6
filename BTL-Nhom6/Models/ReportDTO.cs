using System.Collections.Generic;

namespace BTL_Nhom6.Models
{
    // Class hứng dữ liệu cho DataGrid
    public class BaoCaoCongViecDTO
    {
        public string TenKTV { get; set; }
        public int MaCV { get; set; }
        public string MoTa { get; set; }
        public string TrangThai { get; set; }
        public string DoUuTien { get; set; }
    }

    // Class hứng dữ liệu cho Biểu đồ
    public class NangSuatKTVDTO
    {
        public int UserID { get; set; }
        public string TenKTV { get; set; }
        public int TongCongViec { get; set; }
        public List<string> DanhSachKyNang { get; set; } = new List<string>();
    }
}