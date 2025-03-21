using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Return true if the enum value matches the parameter
            return value.ToString().Equals(parameter.ToString());
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            // Return the enum value if the boolean is true
            if ((bool)value)
                return Enum.Parse(targetType, parameter.ToString());

            return null;
        }
    }
}
