using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nethesap.UI.Converters
{
    public class StockLevelToColorConverter : IValueConverter
    {
        // Kritik stok seviyesi
        private const int LowStockThreshold = 5;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stockQuantity)
            {
                if (stockQuantity == 0)
                {
                    // Stok yok - Kırmızı
                    return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Red
                }
                else if (stockQuantity <= LowStockThreshold)
                {
                    // Düşük stok - Sarı
                    return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Amber
                }
                else
                {
                    // Yeterli stok - Yeşil
                    return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Green 700
                }
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class StockLevelToVisibilityConverter : IValueConverter
    {
        // Kritik stok seviyesi
        private const int LowStockThreshold = 5;
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stockQuantity)
            {
                // Stok 0 veya düşük ise görünür, değilse gizli
                return (stockQuantity == 0 || stockQuantity <= LowStockThreshold) 
                    ? System.Windows.Visibility.Visible 
                    : System.Windows.Visibility.Collapsed;
            }

            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 