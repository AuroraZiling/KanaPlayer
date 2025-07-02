using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace KanaPlayer.Converters;

public class NumberHumanizerConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is ulong number ? number switch
            {
                >= 100_000_000 => $"{number / 100_000_000.0:F1}亿",
                >= 10_000 => $"{number / 10_000.0:F1}万",
                _ => number.ToString()
            } : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}