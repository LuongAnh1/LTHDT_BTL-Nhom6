using System;

namespace BTL_Nhom6.Models
{
    // DTO cho DataGrid
    public class IncidentDTO
    {
        public int MaSC { get; set; }          // RequestID
        public string ThietBi { get; set; }    // DeviceName
        public string LoaiLoi { get; set; }    // ProblemDescription
        public DateTime Ngay { get; set; }     // RequestDate
        public string MucDo { get; set; }      // Priority (Low, Medium, High...)
    }

    // DTO cho Biểu đồ cột (Custom Bar Chart)
    public class BarChartDTO
    {
        public string Label { get; set; }      // Tên cột (Ví dụ: Tên thiết bị)
        public int GiaTriThuc { get; set; }    // Số lượng lỗi thực tế
        public double HeightValue { get; set; } // Chiều cao cột hiển thị trên UI (Pixel)
    }
}