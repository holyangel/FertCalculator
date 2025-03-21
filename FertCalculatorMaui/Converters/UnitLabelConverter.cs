using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FertCalculatorMaui.Converters
{
    public class UnitLabelConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool useImperial)
            {
                // Check if this is for the units type label
                if (parameter is string paramStr && paramStr == "label")
                {
                    return useImperial ? "Imperial (per gallon)" : "Metric (per liter)";
                }

                // For quantity units
                return useImperial ? "g/Gal" : "g/L";
            }

            return "g/L"; // Default to metric
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
