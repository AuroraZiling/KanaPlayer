using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using ObservableCollections;

namespace KanaPlayer.Core.Services.Player;

public partial class PlayerManager<TSettings> : ObservableObject, IPlayerManager where TSettings : SettingsBase, new()
{
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<PlayListItemModel> PlayList =>
        field ??= _playList.ToNotifyCollectionChangedSlim();
    
    [ObservableProperty]
    public partial PlayListItemModel? CurrentPlayListItem { get; private set; }

    [ObservableProperty] public partial double Volume { get; set; }
    public PlayStatus Status => _audioPlayer.Status;

    public PlaybackMode PlaybackMode { get; set; }

    partial void OnVolumeChanged(double value)
        => _audioPlayer.Volume = value;

    private readonly ObservableList<PlayListItemModel> _playList = [];
    private Dictionary<string, string> _cookies;
    private readonly IAudioPlayer _audioPlayer;
    private readonly IBilibiliClient _bilibiliClient;
    private readonly IConfigurationService<TSettings> _configurationService;

    public PlayerManager(IConfigurationService<TSettings> configurationService, IAudioPlayer audioPlayer, IBilibiliClient bilibiliClient)
    {
        _configurationService = configurationService;
        _audioPlayer = audioPlayer;
        _bilibiliClient = bilibiliClient;
        
        _audioPlayer.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IAudioPlayer.Status))
                OnPropertyChanged(nameof(Status));
        };
        
        PlaybackMode = _configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;
        Volume = _configurationService.Settings.CommonSettings.BehaviorHistory.Volume;
        
        _bilibiliClient.TryGetCookies(out var cookies);
        _cookies = cookies;
        
        _audioPlayer.Volume = _configurationService.Settings.CommonSettings.BehaviorHistory.Volume;
    }

    public async Task LoadAsync(PlayListItemModel playListItemModel)
    {
        CurrentPlayListItem = null;
        _audioPlayer.Pause();
        ArgumentNullException.ThrowIfNull(playListItemModel);

        var index = IndexOf(playListItemModel);
        if (index < 0)
            throw new ArgumentException("The specified item is not in the playlist.", nameof(playListItemModel));

        await _audioPlayer.LoadFromAudioUrlAsync(playListItemModel.AudioBvid);
        CurrentPlayListItem = playListItemModel;
    }

    public void Play()
        => _audioPlayer.Play();

    public void Pause()
        => _audioPlayer.Pause();

    public void Append(PlayListItemModel playListItemModel)
        => _playList.Add(playListItemModel);

    public void Insert(PlayListItemModel playListItemModel, int index)
        => _playList.Insert(index, playListItemModel);

    public void Remove(PlayListItemModel playListItemModel)
        => _playList.Remove(playListItemModel);

    public int IndexOf(PlayListItemModel playListItemModel)
        => _playList.IndexOf(playListItemModel);

    public void Clear()
        => _playList.Clear();
}