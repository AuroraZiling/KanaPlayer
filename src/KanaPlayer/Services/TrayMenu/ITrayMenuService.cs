using System.Threading.Tasks;
using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Services.TrayMenu;

public interface ITrayMenuService
{
    void ChangeTooltipText(string toolTipText);
    void SwitchPlaybackMode(PlaybackMode playbackMode, bool save);
    void TogglePlayStatus();
    Task LoadPlayPreviousAsync();
    Task LoadPlayForwardAsync();
}