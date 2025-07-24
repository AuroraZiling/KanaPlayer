using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace KanaPlayer.Converters;

public class DurationHumanizerConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        value is ulong duration ? $"{duration / 60:D2}:{duration % 60:D2}" : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotSupportedException();
}