using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using ObservableCollections;

namespace KanaPlayer.Core.Services.Player;

public partial class PlayerManager<TSettings> : ObservableObject, IPlayerManager where TSettings : SettingsBase, new()
{
    public PlayerManager(IConfigurationService<TSettings> configurationService, IAudioPlayer audioPlayer, IBilibiliClient bilibiliClient)
    {
        _audioPlayer = audioPlayer;
        _bilibiliClient = bilibiliClient;

        _audioPlayer.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IAudioPlayer.Status))
                OnPropertyChanged(nameof(Status));
        };

        PlaybackMode = configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;
        Volume = configurationService.Settings.CommonSettings.BehaviorHistory.Volume;

        _bilibiliClient.TryGetCookies(out _cookies);
    }
    
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<PlayListItemModel> PlayList =>
        field ??= _playList.ToNotifyCollectionChangedSlim();

    [ObservableProperty] public partial PlayListItemModel? CurrentPlayListItem { get; private set; }

    public TimeSpan PlaybackTime
    {
        get => Duration * _audioPlayer.Progress;
        set => _audioPlayer.Progress = value / Duration;
    }
    
    public TimeSpan Duration
        => _audioPlayer.Duration;

    [ObservableProperty] public partial double Volume { get; set; }
    public PlayStatus Status => _audioPlayer.Status;
    [ObservableProperty] public partial PlaybackMode PlaybackMode { get; set; }
    partial void OnVolumeChanged(double value)
        => _audioPlayer.Volume = value;

    private readonly ObservableList<PlayListItemModel> _playList = [];
    private readonly Dictionary<string, string> _cookies;
    private readonly IAudioPlayer _audioPlayer;
    private readonly IBilibiliClient _bilibiliClient;

    public async Task LoadAsync(PlayListItemModel playListItemModel)
    {
        CurrentPlayListItem = null;
        _audioPlayer.Pause();
        ArgumentNullException.ThrowIfNull(playListItemModel);
        await Task.Run(() =>
            _audioPlayer.Load(new CachedAudioStream(playListItemModel.AudioUniqueId, _cookies, _bilibiliClient)));
        CurrentPlayListItem = playListItemModel;
    }
    
    public async Task LoadPrevious()
    {
        if (CurrentPlayListItem is null) return;
        var index = IndexOf(CurrentPlayListItem);
        if (index <= 0) return;
        await LoadAsync(PlayList[index - 1]);
    }
    
    public async Task LoadForward()
    {
        if (CurrentPlayListItem is null) return;
        var index = IndexOf(CurrentPlayListItem);
        if (index < 0 || index >= PlayList.Count - 1) return;
        await LoadAsync(PlayList[index + 1]);
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

file class CachedAudioStream(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies, IBilibiliClient bilibiliClient)
    : Stream
{
    public override void Flush()
    {
    }

    private Stream? _stream;

    [MemberNotNull(nameof(_stream))]
    private void EnsureInitialized()
    {
        // This method should initialize the stream with the cached audio data.
        // For example, it could load the audio data from a file or a database.
        // The implementation is omitted for brevity.
        if (_stream is not null) return;
        var cachedAudioFilePath = Path.Combine(AppHelper.ApplicationAudioCachesFolderPath, $"{audioUniqueId.Bvid}_p{audioUniqueId.Page}");
        if (File.Exists(cachedAudioFilePath))
            _stream = File.OpenRead(cachedAudioFilePath);
        else
        {
            _stream = bilibiliClient.GetAudioStreamAsync(audioUniqueId, cookies)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            using var cacheFileStream = File.Create(cachedAudioFilePath);
            _stream.CopyTo(cacheFileStream);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        EnsureInitialized();
        return _stream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureInitialized();
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
        => throw new InvalidOperationException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new InvalidOperationException();

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override long Length
    {
        get
        {
            EnsureInitialized();
            return _stream.Length;
        }
    }

    public override long Position
    {
        get
        {
            EnsureInitialized();
            return _stream.Position;
        }
        set
        {
            EnsureInitialized();
            _stream.Position = value;
        }
    }
}