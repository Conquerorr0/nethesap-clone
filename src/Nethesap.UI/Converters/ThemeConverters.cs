using System;
using System.Globalization;
using System.Windows.Data;

namespace Nethesap.UI.Converters
{
    public class BoolToThemeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDarkTheme)
            {
                return isDarkTheme ? "Koyu Tema" : "Açık Tema";
            }
            
            return "Açık Tema";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 