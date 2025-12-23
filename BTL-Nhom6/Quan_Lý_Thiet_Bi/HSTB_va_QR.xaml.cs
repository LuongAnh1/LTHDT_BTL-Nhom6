using BTL_Nhom6.Helper; // Namespace chứa NavigationHelper
using BTL_Nhom6.Models; // Namespace chứa các file class Model của bạn
using BTL_Nhom6.Services; // Namespace chứa các file Service
using QRCoder; // Cần cài NuGet Package: QRCoder
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace BTL_Nhom6.Quan_Ly_Thiet_Bi
{
    // Class ViewModel để hiển thị dữ liệu lên ListView (Kết hợp Device với tên Model, Status, Location)
    public class DeviceDisplayModel
    {
        public string DeviceCode { get; set; }
        public string DeviceName { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public string LocationName { get; set; }
        public string StatusName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpiry { get; set; }
    }

    public partial class HSTB_va_QR : Window
    {
        // Khởi tạo các Service
        private readonly DeviceService _deviceService = new DeviceService();
        private readonly DeviceModelService _modelService = new DeviceModelService();
        private readonly DeviceStatusService _statusService = new DeviceStatusService();
        private readonly LocationService _locationService = new LocationService();

        // Danh sách gốc để tìm kiếm
        private List<DeviceDisplayModel> _fullDeviceList = new List<DeviceDisplayModel>();

        public HSTB_va_QR()
        {
            InitializeComponent();
            Loaded += HSTB_va_QR_Loaded;

            // Đăng ký sự kiện tìm kiếm
            txtSearchMa.TextChanged += (s, e) => FilterData();
            txtSearchTen.TextChanged += (s, e) => FilterData();
        }

        private void HSTB_va_QR_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        // Hàm tải dữ liệu và map tên (Join bảng thủ công bằng C#)
        private void LoadData()
        {
            try
            {
                var devices = _deviceService.GetAllDevices();
                var models = _modelService.GetModels(); // Giả sử hàm này lấy tất cả models
                var statusList = _statusService.GetAllDeviceStatus();
                var locations = _locationService.GetAllLocations();

                // Dùng LINQ để join dữ liệu lấy tên hiển thị
                _fullDeviceList = devices.Select(d => new DeviceDisplayModel
                {
                    DeviceCode = d.DeviceCode,
                    DeviceName = d.DeviceName,
                    SerialNumber = d.SerialNumber,
                    PurchaseDate = d.PurchaseDate,
                    WarrantyExpiry = d.WarrantyExpiry,
                    // Lấy tên Model từ ID
                    ModelName = models.FirstOrDefault(m => m.ModelID == d.ModelID)?.ModelName ?? "N/A",
                    // Lấy tên Location từ ID
                    LocationName = locations.FirstOrDefault(l => l.LocationID == d.LocationID)?.LocationName ?? "N/A",
                    // Lấy tên Status từ ID
                    StatusName = statusList.FirstOrDefault(s => s.StatusID == d.StatusID)?.StatusName ?? "N/A"
                }).ToList();

                // Gán vào ListView
                dgDevices.ItemsSource = _fullDeviceList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm lọc dữ liệu
        private void FilterData()
        {
            string keywordMa = txtSearchMa.Text.ToLower();
            string keywordTen = txtSearchTen.Text.ToLower();

            var filteredList = _fullDeviceList.Where(d =>
                (string.IsNullOrEmpty(keywordMa) || d.DeviceCode.ToLower().Contains(keywordMa)) &&
                (string.IsNullOrEmpty(keywordTen) || d.DeviceName.ToLower().Contains(keywordTen))
            ).ToList();

            dgDevices.ItemsSource = filteredList;
        }

        // --- XỬ LÝ NÚT THÊM/SỬA/XÓA ---

        // Nút Thêm mới
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Tạo hiệu ứng mờ
            BlurEffect blurObj = new BlurEffect { Radius = 15 };
            this.Effect = blurObj;

            // Mở form
            DeviceDetailWindow addWindow = new DeviceDetailWindow();
            bool? result = addWindow.ShowDialog();

            // Gỡ bỏ hiệu ứng mờ ngay sau khi form con đóng lại
            this.Effect = null;

            // Nếu người dùng bấm Lưu (DialogResult = true)
            if (result == true)
            {
                LoadData(); // Tải lại danh sách
                            // MessageBox.Show("Thêm mới thành công!"); // Thông báo này đã có trong form con rồi nên có thể bỏ qua
            }
        }

        // Nút Sửa (Trong DataGrid hoặc ListView)
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;

            string deviceCode = btn.Tag.ToString();

            // Tạo hiệu ứng mờ
            BlurEffect blurObj = new BlurEffect { Radius = 15 };
            this.Effect = blurObj;

            // Mở form truyền mã thiết bị vào
            DeviceDetailWindow editWindow = new DeviceDetailWindow(deviceCode);
            bool? result = editWindow.ShowDialog();

            // Gỡ bỏ hiệu ứng mờ
            this.Effect = null;

            // Nếu người dùng bấm Lưu
            if (result == true)
            {
                LoadData(); // Tải lại danh sách
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string deviceCode = btn.Tag.ToString();

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa thiết bị {deviceCode}?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _deviceService.DeleteDevice(deviceCode);
                LoadData();
                MessageBox.Show("Đã xóa thành công!");
            }
        }

        // --- CHỨC NĂNG IN QR CODE ---

        // 1. Sửa lại sự kiện Click
        private void BtnPrintQR_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag == null) return;

            string deviceCode = btn.Tag.ToString();
            var device = _fullDeviceList.FirstOrDefault(d => d.DeviceCode == deviceCode);
            if (device == null) return;

            string qrContent = device.DeviceCode;
            //string qrContent = $"CODE:{device.DeviceCode}|NAME:{device.DeviceName}|MODEL:{device.ModelName}";

            // Gọi hàm tạo QR mới
            BitmapImage qrImage = GenerateQR_WPF(qrContent);

            // Hiển thị
            ShowQRPreviewWindow(qrImage, device.DeviceCode);
        }

        private BitmapImage GenerateQR_WPF(string content)
        {
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

                // Dùng PngByteQRCode để lấy mảng byte thay vì Bitmap
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // Chuyển byte[] -> BitmapImage (WPF)
                using (MemoryStream stream = new MemoryStream(qrCodeBytes))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Quan trọng
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // Giúp ảnh không bị lỗi thread
                    return bitmapImage;
                }
            }
        }

        private void ShowQRPreviewWindow(BitmapImage qrImage, string title)
        {
            Window qrWindow = new Window
            {
                Title = $"QR Code - {title}",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel stack = new StackPanel { Margin = new Thickness(20) };

            System.Windows.Controls.Image img = new System.Windows.Controls.Image
            {
                Source = qrImage, // Gán trực tiếp
                Width = 200,
                Height = 200,
                Margin = new Thickness(0, 0, 0, 20)
            };

            Button btnPrint = new Button
            {
                Content = "🖨 IN NGAY",
                Height = 40,
                Background = System.Windows.Media.Brushes.Blue,
                Foreground = System.Windows.Media.Brushes.White
            };

            btnPrint.Click += (s, ev) =>
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // PrintVisual in cả cái Image Control
                    printDialog.PrintVisual(img, $"QR_{title}");
                }
            };

            stack.Children.Add(img);
            stack.Children.Add(btnPrint);
            qrWindow.Content = stack;
            qrWindow.ShowDialog();
        }

        // Helper chuyển đổi Bitmap -> ImageSource
        //private BitmapImage BitmapToImageSource(Bitmap bitmap)
        //{
        //    using (MemoryStream memory = new MemoryStream())
        //    {
        //        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        //        memory.Position = 0;
        //        BitmapImage bitmapimage = new BitmapImage();
        //        bitmapimage.BeginInit();
        //        bitmapimage.StreamSource = memory;
        //        bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmapimage.EndInit();
        //        return bitmapimage;
        //    }
        //}

        #region Điều hướng Tab (Thanh bar)

        // 1. Hồ sơ thiết bị & QR (Trang hiện tại)
        private void Button_HSTB_Click(object sender, RoutedEventArgs e) { }

        // 2. Tra cứu tài sản
        private void Button_TraCuu_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TCTS());
        }

        // 3. Theo dõi bảo hành
        private void Button_BaoHanh_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new TDBH());
        }

        // 4. Điều chuyển & Bàn giao
        private void Button_BanGiao_Click(object sender, RoutedEventArgs e)
        {
            NavigationHelper.Navigate(this, new DC_va_BG());
        }

        #endregion
    }
}