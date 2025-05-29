using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Nethesap.Domain.Entities;

namespace Nethesap.UI.Converters
{
    public class PaymentMethodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentMethod paymentMethod)
            {
                switch (paymentMethod)
                {
                    case PaymentMethod.Cash:
                        return "Nakit";
                    case PaymentMethod.CreditCard:
                        return "Kredi Kartı";
                    case PaymentMethod.BankTransfer:
                        return "Havale";
                    default:
                        return string.Empty;
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PaymentTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentType paymentType)
            {
                switch (paymentType)
                {
                    case PaymentType.Sale:
                        return "Satış";
                    case PaymentType.Refund:
                        return "İade";
                    default:
                        return string.Empty;
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PaymentTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentType paymentType)
            {
                // Sadece Satış türündeki ödemeler için İade Et butonunu göster
                return paymentType == PaymentType.Sale ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return false;

            string parameterString = parameter.ToString();
            if (Enum.IsDefined(value.GetType(), value))
            {
                string valueString = value.ToString();
                return valueString.Equals(parameterString, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null || !(value is bool))
                return Binding.DoNothing;

            string parameterString = parameter.ToString();
            bool boolValue = (bool)value;

            if (boolValue)
                return Enum.Parse(targetType, parameterString);

            return Binding.DoNothing;
        }
    }
} 