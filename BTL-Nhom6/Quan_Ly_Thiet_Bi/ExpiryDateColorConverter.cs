using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

// Chú ý: Namespace này nên là BTL_Nhom6.Converters cho gọn
namespace BTL_Nhom6.Converters
{
    public class ExpiryDateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                TimeSpan diff = date - DateTime.Now;
                if (diff.TotalDays < 0) return Brushes.Red; // Đã hết hạn
                if (diff.TotalDays <= 30) return Brushes.Orange; // Sắp hết
                if (diff.TotalDays <= 60) return Brushes.Goldenrod;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}