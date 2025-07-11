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
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<PlayListItemModel> PlayList =>
        field ??= _playList.ToNotifyCollectionChangedSlim();

    [ObservableProperty] public partial PlayListItemModel? CurrentPlayListItem { get; private set; }

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

    public PlayerManager(IConfigurationService<TSettings> configurationService, IAudioPlayer audioPlayer,
        IBilibiliClient bilibiliClient)
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

        await Task.Run(() =>
            _audioPlayer.Load(new CachedAudioStream(playListItemModel.AudioBvid, _cookies, _bilibiliClient)));
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

file class CachedAudioStream(string audioBvid, Dictionary<string, string> cookies, IBilibiliClient bilibiliClient)
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
        var cachedAudioFilePath = Path.Combine(AppHelper.ApplicationAudioCachesFolderPath, audioBvid.ToLower());
        if (File.Exists(cachedAudioFilePath))
        {
            _stream = File.OpenRead(cachedAudioFilePath);
        }
        else
        {
            _stream = bilibiliClient.GetAudioStreamAsync(audioBvid, cookies)
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
    {
        throw new InvalidOperationException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new InvalidOperationException();
    }

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