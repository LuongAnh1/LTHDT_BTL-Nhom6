using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BTL_Nhom6.Models
{
    // Kế thừa INotifyPropertyChanged để khi sửa Số lượng -> Thành tiền tự nhảy
    public class MaterialViewModel : INotifyPropertyChanged
    {
        public int MaterialID { get; set; }
        public string TenVatTu { get; set; }
        public string DonVi { get; set; } // Tên đơn vị tính (Cái, Hộp...)
        public decimal DonGia { get; set; }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ThanhTien)); // Báo cho giao diện cập nhật Thành tiền
            }
        }

        // Thành tiền = Đơn giá * Số lượng
        public decimal ThanhTien => DonGia * SoLuong;

        // Sự kiện cập nhật giao diện
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}