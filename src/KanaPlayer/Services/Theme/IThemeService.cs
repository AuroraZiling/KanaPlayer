namespace KanaPlayer.Services.Theme;
using RgbColor = Avalonia.Media.Color;

public interface IThemeService
{
    public void SetThemeColor(RgbColor accentRgbColor, bool applyTextForegroundAsThemeColor = false);
}