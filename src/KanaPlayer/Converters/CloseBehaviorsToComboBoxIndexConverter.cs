using System;
using System.Globalization;
using Avalonia.Data.Converters;
using KanaPlayer.Models.SettingTypes;

namespace KanaPlayer.Converters;

public class CloseBehaviorsToComboBoxIndexConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CloseBehaviors closeBehavior)
        {
            return closeBehavior switch
            {
                CloseBehaviors.Minimize => 0,
                CloseBehaviors.TrayMenu => 1,
                CloseBehaviors.Close => 2,
                _ => null
            };
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            0 => CloseBehaviors.Minimize,
            1 => CloseBehaviors.TrayMenu,
            2 => CloseBehaviors.Close,
            _ => null
        };
    }
}