using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;

namespace KanaPlayer.ViewModels;

public partial class MainViewModel(IConfigurationService<SettingsModel> configurationService, IPlayerManager playerManager)
    : ViewModelBase
{
    public IPlayerManager PlayerManager => playerManager;

    [ObservableProperty]
    public partial double Volume { get; set; } = configurationService.Settings.CommonSettings.BehaviorHistory.Volume;

    partial void OnVolumeChanged(double value)
    {
        PlayerManager.Volume = value;
        configurationService.Settings.CommonSettings.BehaviorHistory.Volume = value;
        configurationService.Save();
    }

    [ObservableProperty]
    public partial PlaybackMode PlaybackMode { get; set; } =
        configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;

    [RelayCommand]
    private void SwitchPlaybackModes()
    {
        PlaybackMode = (PlaybackMode)((int)(PlaybackMode + 1) % (int)PlaybackMode.MaxValue);
        PlayerManager.PlaybackMode = PlaybackMode;
        configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode = PlaybackMode;
        configurationService.Save();
    }

    [RelayCommand]
    private void TogglePlay()
    {
        if (PlayerManager.Status == PlayStatus.Playing)
            PlayerManager.Pause();
        else
            PlayerManager.Play();
    }
}