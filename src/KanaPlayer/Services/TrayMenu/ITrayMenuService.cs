using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Services.TrayMenu;

public interface ITrayMenuService
{
    void ChangeTooltipText(string toolTipText);
    void SwitchPlaybackMode(PlaybackMode playbackMode);
}