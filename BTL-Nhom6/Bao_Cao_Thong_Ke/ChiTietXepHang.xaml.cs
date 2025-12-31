using BTL_Nhom6.Models;
using System.Collections.Generic;
using System.Windows;

namespace BTL_Nhom6.Bao_Cao_Thong_Ke
{
    // Class phụ để hiển thị trên giao diện này (Kế thừa hoặc wrap DTO)
    public class XepHangViewModel : NangSuatKTVDTO
    {
        public int Rank { get; set; }
    }

    public partial class ChiTietXepHang : Window
    {
        public ChiTietXepHang(List<NangSuatKTVDTO> listData)
        {
            InitializeComponent();
            LoadData(listData);
        }

        private void LoadData(List<NangSuatKTVDTO> listData)
        {
            List<XepHangViewModel> viewList = new List<XepHangViewModel>();
            int rank = 1;

            foreach (var item in listData)
            {
                viewList.Add(new XepHangViewModel
                {
                    Rank = rank++, // Tự động tăng thứ hạng
                    UserID = item.UserID,
                    TenKTV = item.TenKTV,
                    TongCongViec = item.TongCongViec,
                    DanhSachKyNang = item.DanhSachKyNang
                });
            }

            icDanhSachKTV.ItemsSource = viewList;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}