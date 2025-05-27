using System;
using System.Globalization;
using System.Windows.Data;

namespace Nethesap.UI.Converters;

public class MathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return 0;

        double doubleValue;
        
        if (!double.TryParse(value.ToString(), out doubleValue))
            return 0;

        string param = parameter.ToString();
        
        if (param.StartsWith("@VALUE"))
        {
            if (param.Contains("+"))
                return doubleValue + double.Parse(param.Replace("@VALUE+", ""));
            else if (param.Contains("-"))
                return doubleValue - double.Parse(param.Replace("@VALUE-", ""));
            else if (param.Contains("*"))
                return doubleValue * double.Parse(param.Replace("@VALUE*", ""));
            else if (param.Contains("/"))
                return doubleValue / double.Parse(param.Replace("@VALUE/", ""));
        }
        
        return doubleValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 