using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KanaPlayer.Core.Models.BiliMediaList;
using Lucide.Avalonia;

namespace KanaPlayer.Converters;

public class FavoriteTypeToIconKindConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BiliMediaListType favoriteType)
        {
            if (favoriteType.HasFlag(BiliMediaListType.Folder))
                return LucideIconKind.FolderHeart;
            if (favoriteType.HasFlag(BiliMediaListType.Collection))
                return LucideIconKind.LibraryBig;
        }

        throw new ArgumentOutOfRangeException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class FavoriteTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BiliMediaListType favoriteType)
        {
            if (favoriteType.HasFlag(BiliMediaListType.Folder))
                return "收藏夹";
            if (favoriteType.HasFlag(BiliMediaListType.Collection))
                return "合集";
        }

        throw new ArgumentOutOfRangeException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}