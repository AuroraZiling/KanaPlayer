using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Services.TrayMenu;

namespace KanaPlayer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public IPlayerManager PlayerManager { get; }

    public TimeSpan PlaybackTime
    {
        get
        {
            if (IsSeeking) return Progress * PlayerManager.Duration;
            return PlayerManager.PlaybackTime;
        }
        set => PlayerManager.PlaybackTime = value;
    }
    
    public double Progress
    {
        get
        {
            if (IsSeeking) return field;
            return field = PlayerManager.PlaybackTime.TotalSeconds / PlayerManager.Duration.TotalSeconds;
        }
        set
        {
            field = value;
            if (!IsSeeking) PlayerManager.PlaybackTime = TimeSpan.FromSeconds(value * PlayerManager.Duration.TotalSeconds);
            OnPropertyChanged(nameof(PlaybackTime));
            OnPropertyChanged();
        }
    }

    public double BufferedProgress => PlayerManager.BufferedProgress;

    public bool IsSeeking
    {
        get;
        set
        {
            if (value)
                _playbackTimeExecutionTimer.Stop();
            else
            {
                PlayerManager.PlaybackTime = TimeSpan.FromSeconds(Progress * PlayerManager.Duration.TotalSeconds);
                _playbackTimeExecutionTimer.Start();
            }
            field = value;
        }
    }

    public TimeSpan Duration => PlayerManager.Duration;

    private readonly DispatcherTimer _playbackTimeExecutionTimer;
    private readonly IConfigurationService<SettingsModel> _configurationService;

    public MainViewModel(IConfigurationService<SettingsModel> configurationService, IPlayerManager playerManager, ITrayMenuService trayMenuService)
    {
        _configurationService = configurationService;
        PlayerManager = playerManager;
        
        Volume = configurationService.Settings.CommonSettings.BehaviorHistory.Volume;
        
        PlaybackMode = configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;
        trayMenuService.SwitchPlaybackMode(PlaybackMode, false);

        _playbackTimeExecutionTimer = new DispatcherTimer(TimeSpan.FromSeconds(0.1), DispatcherPriority.Normal, delegate
        {
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(PlaybackTime));
            OnPropertyChanged(nameof(Duration));
            OnPropertyChanged(nameof(BufferedProgress));
        });
    }

    [ObservableProperty] public partial double Volume { get; set; }

    partial void OnVolumeChanged(double value)
    {
        PlayerManager.Volume = value;
        _configurationService.Settings.CommonSettings.BehaviorHistory.Volume = value;
        _configurationService.Save();
    }
    
    private double _beforeMuteVolume;
    
    [RelayCommand]
    private void Mute()
    {
        if (Volume > 0)
        {
            _beforeMuteVolume = Volume;
            Volume = 0;
        }
        else
            Volume = _beforeMuteVolume;
    }

    [ObservableProperty] public partial PlaybackMode PlaybackMode { get; set; }

    [RelayCommand]
    private void SwitchPlaybackModes()
    {
        PlaybackMode = (PlaybackMode)((int)(PlaybackMode + 1) % (int)PlaybackMode.MaxValue);
        PlayerManager.PlaybackMode = PlaybackMode;
        _configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode = PlaybackMode;
        _configurationService.Save();
    }
    
    [RelayCommand]
    private Task LoadPreviousAsync()
        => PlayerManager.LoadPrevious(true, true);

    [RelayCommand]
    private void TogglePlay()
    {
        if (PlayerManager.Status == PlayStatus.Playing)
            PlayerManager.Pause();
        else
            PlayerManager.Play();
    }
    
    [RelayCommand]
    private Task LoadForwardAsync()
        => PlayerManager.LoadForward(true, true);
}