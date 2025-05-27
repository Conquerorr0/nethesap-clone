using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nethesap.UI.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isTrue && parameter is string colorParams)
        {
            string[] colors = colorParams.Split(',');
            string colorStr = isTrue ? colors[0] : colors.Length > 1 ? colors[1] : colors[0];
            
            if (targetType == typeof(Brush))
            {
                return new BrushConverter().ConvertFrom(colorStr) as Brush;
            }
            
            return colorStr;
        }
        
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 