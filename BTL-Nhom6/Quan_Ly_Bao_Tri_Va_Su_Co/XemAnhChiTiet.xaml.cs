using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BTL_Nhom6.Quan_Ly_Bao_Tri_Va_Su_Co
{
    public partial class XemAnhChiTiet : Window
    {
        public XemAnhChiTiet(string imageUrl)
        {
            InitializeComponent();
            LoadImage(imageUrl);
        }

        private void LoadImage(string url)
        {
            try
            {
                // Tạo BitmapImage từ đường dẫn URL hoặc File
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Tải ảnh vào bộ nhớ ngay
                bitmap.EndInit();

                imgFull.Source = bitmap;
            }
            catch (Exception)
            {
                MessageBox.Show("Không thể tải ảnh này (Đường dẫn lỗi hoặc file không tồn tại).", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}