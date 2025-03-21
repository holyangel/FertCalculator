using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? new object() : null;
        }
    }
}
