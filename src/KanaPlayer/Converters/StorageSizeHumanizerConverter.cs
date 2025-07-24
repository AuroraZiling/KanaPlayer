using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace KanaPlayer.Converters;

public class StorageSizeHumanizerConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int storageSize ? storageSize switch
        {
            >= 1024 => $"{storageSize / 1024.0:F1} GB",
            _ => $"{storageSize} MB"
        } : value;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotSupportedException();
}