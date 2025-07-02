using Avalonia;
using Avalonia.Media;
using RgbColor = Avalonia.Media.Color;

namespace KanaPlayer.Services.Theme;

public class ThemeService: IThemeService
{
    private static readonly Application Application = Application.Current!;
    
    private static void SetColorKey(string key, HsvColor hsvColor)
        => Application.Resources[key] = hsvColor.ToRgb();
    
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
    }
}