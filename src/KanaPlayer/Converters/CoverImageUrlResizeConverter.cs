using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace KanaPlayer.Converters;

public class CoverImageUrlResizeConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) 
        => value is string imageUrl ? $"{imageUrl}@320w_180h_1c_!web-home-common-cover" : null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}