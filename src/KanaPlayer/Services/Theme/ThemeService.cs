using Avalonia;
using Avalonia.Media;
using RgbColor = Avalonia.Media.Color;

namespace KanaPlayer.Services.Theme;

public class ThemeService: IThemeService
{
    private static readonly Application Application = Application.Current!;

    #region Notification Colors

    public static readonly RgbColor KanaNotificationInformationColor = Colors.DodgerBlue;
    public static readonly RgbColor KanaNotificationSuccessColor = Colors.LimeGreen;
    public static readonly RgbColor KanaNotificationWarningColor = Colors.Orange;
    public static readonly RgbColor KanaNotificationErrorColor = Colors.OrangeRed;

    #endregion
    
    private static void SetColorKey(string key, HsvColor hsvColor)
        => Application.Resources[key] = hsvColor.ToRgb();
    
    private static void SetColorKey(string key, RgbColor rgbColor)
        => Application.Resources[key] = rgbColor;
    
    public void SetThemeColor(RgbColor accentRgbColor, bool applyTextForegroundAsThemeColor = false)
    {
        var accentHsvColor = accentRgbColor.ToHsv();
        SetColorKey("KanaAccentColor", accentHsvColor);
        
        for (var value = 5; value < 100; value+=5)
        {
            SetColorKey($"KanaAccentColor{value}", new HsvColor(
                accentHsvColor.A,
                accentHsvColor.H,
                accentHsvColor.S,
                accentHsvColor.V * value / 100d));    
        }

        if (applyTextForegroundAsThemeColor) 
            SetColorKey("KanaCommonForegroundColor", accentHsvColor);
        
        // Notification colors
        SetColorKey("KanaNotificationInformationColor", KanaNotificationInformationColor);
        SetColorKey("KanaNotificationSuccessColor", KanaNotificationSuccessColor);
        SetColorKey("KanaNotificationWarningColor", KanaNotificationWarningColor);
        SetColorKey("KanaNotificationCriticalColor", KanaNotificationErrorColor);
    }
}