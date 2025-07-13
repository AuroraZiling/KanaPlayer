namespace KanaPlayer.Services.Theme;
using RgbColor = Avalonia.Media.Color;

public interface IThemeService
{
    static RgbColor KanaNotificationInformationColor;
    static RgbColor KanaNotificationNoticeColor;
    static RgbColor KanaNotificationSuccessColor;
    static RgbColor KanaNotificationWarningColor;
    static RgbColor KanaNotificationCriticalColor;
    
    public void SetThemeColor(RgbColor accentRgbColor, bool applyTextForegroundAsThemeColor = false);
}