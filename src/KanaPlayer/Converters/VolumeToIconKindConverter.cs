using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Lucide.Avalonia;

namespace KanaPlayer.Converters;

public class VolumeToIconKindConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double volume)
        {
            return volume switch
            {
                0 => LucideIconKind.Volume,
                < 0.5 => LucideIconKind.Volume1,
                _ => LucideIconKind.Volume2
            };
        }

        return LucideIconKind.VolumeOff;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}