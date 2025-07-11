using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KanaPlayer.Core.Models.PlayerManager;
using Lucide.Avalonia;

namespace KanaPlayer.Converters;

public class PlaybackModeToIconKindConverter: IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is PlaybackMode playbackMode)
        {
            return playbackMode switch
            {
                PlaybackMode.Shuffle => LucideIconKind.Shuffle,
                PlaybackMode.RepeatAll => LucideIconKind.Repeat,
                PlaybackMode.RepeatOne => LucideIconKind.Repeat1,
                PlaybackMode.Sequential => LucideIconKind.ListEnd,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        throw new ArgumentOutOfRangeException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotImplementedException();
}