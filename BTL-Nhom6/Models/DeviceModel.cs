namespace BTL_Nhom6.Models
{
    public class DeviceModel
    {
        public int ModelID { get; set; }
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public int CategoryID { get; set; }
        public string Description { get; set; }

        // Thuộc tính phụ hiển thị tên Loại (Do JOIN bảng)
        public string CategoryName { get; set; }
    }
}