using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.PlayerManager;
using Lucide.Avalonia;

namespace KanaPlayer.Converters;

public class FavoriteTypeToIconKindConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FavoriteType favoriteType)
        {
            if (favoriteType.HasFlag(FavoriteType.Folder))
                return LucideIconKind.FolderHeart;
            if (favoriteType.HasFlag(FavoriteType.Collection))
                return LucideIconKind.LibraryBig;
        }

        throw new ArgumentOutOfRangeException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}