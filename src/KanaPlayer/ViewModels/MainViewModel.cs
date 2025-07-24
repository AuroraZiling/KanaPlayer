using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Services.TrayMenu;
using NLog;

namespace KanaPlayer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(MainViewModel));
    
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
            if (PlayerManager.Duration.TotalSeconds <= 0) return 0.0;
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
    [ObservableProperty] public partial bool IsLoggedIn { get; private set; }

    public MainViewModel(IConfigurationService<SettingsModel> configurationService, IBilibiliClient bilibiliClient, IPlayerManager playerManager, ITrayMenuService trayMenuService)
    {
        _configurationService = configurationService;
        PlayerManager = playerManager;
        
        Volume = configurationService.Settings.CommonSettings.BehaviorHistory.Volume;
        
        PlaybackMode = configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;
        trayMenuService.SwitchPlaybackMode(PlaybackMode, false);
        
        bilibiliClient.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(IBilibiliClient.IsAuthenticated))
            {
                IsLoggedIn = bilibiliClient.IsAuthenticated;
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        };
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
        _configurationService.SaveDelayed();
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
        _configurationService.SaveImmediate();
        ScopedLogger.Info("切换播放模式到 {PlaybackMode}", PlaybackMode);
    }
    
    [RelayCommand]
    private Task LoadPreviousAsync()
        => PlayerManager.LoadPrevious(true, true);

    [RelayCommand]
    private void TogglePlay()
    {
        if (PlayerManager.Status == PlayStatus.Playing)
        {
            ScopedLogger.Info("暂停播放");
            PlayerManager.Pause();
        }
        else
        {
            ScopedLogger.Info("开始播放");
            PlayerManager.Play();
        }
    }
    
    [RelayCommand]
    private Task LoadForwardAsync()
        => PlayerManager.LoadForward(true, true);
}