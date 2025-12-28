namespace BTL_Nhom6.Models
{
    public class MaterialCatalogViewModel
    {
        public int MaterialID { get; set; }

        // Binding: MaDM (Tự động format VT-001)
        public string MaDM => $"VT-{MaterialID:D3}";

        // Binding: TenDM
        public string TenDM { get; set; }

        // Binding: MoTa
        public string MoTa { get; set; }

        // Binding: DinhMuc
        public int DinhMuc { get; set; }

        // Binding: DonVi
        public string DonVi { get; set; }

        // Các thuộc tính ẩn dùng cho chức năng Sửa
        public int UnitID { get; set; }
        public decimal DonGia { get; set; }
        public int CurrentStock { get; set; }
    }
}