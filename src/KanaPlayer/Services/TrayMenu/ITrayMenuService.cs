using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Services.TrayMenu;

public interface ITrayMenuService
{
    void ChangeTooltipText(string toolTipText);
    public void SwitchPlaybackMode(PlaybackMode playbackMode, bool saveConfiguration);
}