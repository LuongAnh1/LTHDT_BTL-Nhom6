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
        public int CurrentStock { get; set; } // Số lượng tồn kho hiện tại

        private decimal _donGia;
        public decimal DonGia
        {
            get => _donGia;
            set
            {
                _donGia = value;
                OnPropertyChanged();
                // QUAN TRỌNG: Khi sửa giá, phải báo tính lại Thành tiền
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged();
                // QUAN TRỌNG: Khi sửa số lượng, phải báo tính lại Thành tiền
                OnPropertyChanged(nameof(ThanhTien));
            }
        }

        public decimal ThanhTien => SoLuong * DonGia;

        // Sự kiện cập nhật giao diện
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int SoLuongXuat { get; set; } // Số lượng kho đã xuất cho phiếu này
    }
}