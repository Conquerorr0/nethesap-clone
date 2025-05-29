using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Nethesap.UI.Converters
{
    public class BalanceToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (balance > 0)
                {
                    // Positive balance (customer owes money)
                    return new SolidColorBrush(Color.FromRgb(198, 40, 40)); // Red
                }
                else if (balance < 0)
                {
                    // Negative balance (we owe money to customer)
                    return new SolidColorBrush(Color.FromRgb(46, 125, 50)); // Green
                }
                else
                {
                    // Zero balance
                    return new SolidColorBrush(Color.FromRgb(33, 33, 33)); // Dark Gray
                }
            }

            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 