using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace KanaPlayer.Converters;

public class TimestampHumanizerConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is long timestamp
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.Date.ToString("yyyy-MM-dd")
            : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotSupportedException();
}