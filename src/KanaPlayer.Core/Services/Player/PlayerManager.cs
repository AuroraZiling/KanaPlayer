using System.Buffers;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Favorites;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ObservableCollections;

namespace KanaPlayer.Core.Services.Player;

public partial class PlayerManager<TSettings> : ObservableObject, IPlayerManager where TSettings : SettingsBase, new()
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(PlayerManager<TSettings>));

    private readonly IAudioPlayer _audioPlayer;
    private readonly IBilibiliClient _bilibiliClient;
    private readonly IConfigurationService<TSettings> _configurationService;
    private readonly IFavoritesManager _favoritesManager;
    private readonly IExceptionHandler _exceptionHandler;
    public PlayerManager(IConfigurationService<TSettings> configurationService, IAudioPlayer audioPlayer, IBilibiliClient bilibiliClient,
                         IFavoritesManager favoritesManager, [FromKeyedServices("PlayerManagerExceptionHandler")] IExceptionHandler exceptionHandler)
    {
        _configurationService = configurationService;
        _audioPlayer = audioPlayer;
        _bilibiliClient = bilibiliClient;
        _favoritesManager = favoritesManager;
        _exceptionHandler = exceptionHandler;

        PlaybackMode = configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;
        audioPlayer.Volume = Volume = configurationService.Settings.CommonSettings.BehaviorHistory.Volume;

        InitializePlayListCommand.Execute(null);
        _audioPlayer.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IAudioPlayer.Status))
                OnPropertyChanged(nameof(Status));
        };
        _audioPlayer.PlaybackStopped += () =>
        {
            Task.Run(() => LoadForward(false, true));
        };
    }

    [RelayCommand]
    private async Task InitializePlayListAsync()
    {
        if (_bilibiliClient.TryGetCookies(out var cookies))
        {
            foreach (var audioUniqueId in _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayList)
                _playList.Add(await GetPlayListItemByAudioUniqueId(audioUniqueId));
            ScopedLogger.Info("播放列表初始化完成，当前播放列表包含 {Count} 个音频", _playList.Count);

            if (_configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayAudioUniqueId is { } value)
            {
                var playListItem = await GetPlayListItemByAudioUniqueId(value);
                if (_playList.Contains(playListItem))
                {
                    ScopedLogger.Info("正在加载上次播放的音频: {Title} | {UniqueId}", playListItem.Title, value);
                    await LoadAsync(playListItem);
                }
                else
                {
                    _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayAudioUniqueId = null;
                    ScopedLogger.Warn("上次播放的音频 {Title} | {UniqueId} 不在播放列表中，清除记录", playListItem.Title, value);
                }
            }

            _playList.CollectionChanged += (in args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add when args.IsSingleItem:
                    {
                        _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayList.Add(args.NewItem.AudioUniqueId);
                        break;
                    }
                    case NotifyCollectionChangedAction.Add:
                        foreach (var argsNewItem in args.NewItems)
                            _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayList.Add(argsNewItem.AudioUniqueId);
                        break;
                    case NotifyCollectionChangedAction.Remove when args.IsSingleItem:
                        _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayList.Remove(args.OldItem.AudioUniqueId);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var argsOldItem in args.OldItems)
                            _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayList.Remove(argsOldItem.AudioUniqueId);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayList.Clear();
                        break;
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Move:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                _configurationService.SaveImmediate();
            };
            
            async Task<PlayListItem> GetPlayListItemByAudioUniqueId(AudioUniqueId audioUniqueId)
            {
                var cachedAudioMetadata = _favoritesManager.GetCachedAudioMetadataByUniqueId(audioUniqueId);
                PlayListItem playListItem;
                if (cachedAudioMetadata is not null)
                {
                    playListItem = new PlayListItem(cachedAudioMetadata);
                }
                else
                {
                    var audioInfo = await _bilibiliClient.GetAudioInfoAsync(audioUniqueId, cookies);
                    var audioInfoData = audioInfo.EnsureData();
                    playListItem = new PlayListItem(audioInfoData);
                    _favoritesManager.AddOrUpdateAudioToCache(audioUniqueId, audioInfoData);
                }
                return playListItem;
            }
        }
        else
        {
            ScopedLogger.Error("无法获取 Cookies，播放列表初始化失败");
            throw new InvalidOperationException("无法获取 Cookies，播放列表初始化失败");
        }
    }

    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<PlayListItem> PlayList =>
        field ??= _playList.ToNotifyCollectionChangedSlim();

    private readonly ObservableList<PlayListItem> _playList = [];

    [ObservableProperty] public partial PlayListItem? CurrentPlayListItem { get; private set; }
    partial void OnCurrentPlayListItemChanged(PlayListItem? value)
    {
        _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayAudioUniqueId = value?.AudioUniqueId;
        _configurationService.SaveImmediate();
    }

    public TimeSpan PlaybackTime
    {
        get => Duration * _audioPlayer.Progress;
        set => _audioPlayer.Progress = value / Duration;
    }

    public TimeSpan Duration
        => _audioPlayer.Duration;

    public double BufferedProgress
        => _cachedAudioStream?.BufferedProgress ?? 0;

    [ObservableProperty] public partial double Volume { get; set; }
    partial void OnVolumeChanged(double value)
        => _audioPlayer.Volume = value;
    public PlayStatus Status => _audioPlayer.Status;
    [ObservableProperty] public partial PlaybackMode PlaybackMode { get; set; }
    partial void OnPlaybackModeChanged(PlaybackMode value)
    {
        switch (value)
        {
            case PlaybackMode.Sequential:
                CanLoadPrevious = CurrentPlayListItem is not null && _playList.IndexOf(CurrentPlayListItem) > 0;
                CanLoadForward = CurrentPlayListItem is not null && _playList.IndexOf(CurrentPlayListItem) < PlayList.Count - 1;
                break;
            case PlaybackMode.RepeatOne:
            case PlaybackMode.RepeatAll:
            case PlaybackMode.Shuffle:
                CanLoadPrevious = CurrentPlayListItem is not null;
                CanLoadForward = CurrentPlayListItem is not null;
                break;
            case PlaybackMode.MaxValue:
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
        ScopedLogger.Info("播放模式已更改: {PlaybackMode}", value);
    }

    private CachedAudioStream? _cachedAudioStream;
    private CancellationTokenSource? _loadCancellationTokenSource;

    public Task LoadFirstAsync()
    {
        if (PlayList.Count == 0)
        {
            ScopedLogger.Warn("播放列表为空，无法加载第一个音频");
            throw new InvalidOperationException("播放列表为空，无法加载第一个音频");
        }
        ScopedLogger.Info("正在加载播放列表中的第一个音频: {AudioUniqueId}", PlayList[0].AudioUniqueId);
        return LoadAsync(PlayList[0]);
    }

    public async Task LoadAsync(PlayListItem playListItem)
    {
        if (_loadCancellationTokenSource is not null) await _loadCancellationTokenSource.CancelAsync();
        if (!_bilibiliClient.TryGetCookies(out var cookies))
        {
            ScopedLogger.Error("无法获取 Cookies，无法加载音频");
            return;
        }
        
        _cachedAudioStream = null;
        CurrentPlayListItem = null;

        PlaybackTime = TimeSpan.Zero;
        CanLoadPrevious = false;
        CanLoadForward = false;
        OnPropertyChanged(nameof(CanLoadPrevious));
        OnPropertyChanged(nameof(CanLoadForward));

        _audioPlayer.Pause();
        ArgumentNullException.ThrowIfNull(playListItem);

        _loadCancellationTokenSource = new CancellationTokenSource();
        try
        {
            await _audioPlayer.LoadAsync(async () => _cachedAudioStream =
                                                         await CachedAudioStream.CreateAsync(playListItem.AudioUniqueId, cookies, _bilibiliClient,
                                                             _loadCancellationTokenSource.Token));
        }
        catch (Exception e)
        {
            _exceptionHandler.HandleException(e);
            return;
        }
        
        CurrentPlayListItem = playListItem;
        if (PlayList.Contains(playListItem))
        {
            CanLoadPrevious = _playList.IndexOf(playListItem) > 0 || PlaybackMode == PlaybackMode.RepeatAll || PlaybackMode == PlaybackMode.RepeatOne
                              || PlaybackMode == PlaybackMode.Shuffle;
            CanLoadForward = _playList.IndexOf(playListItem) < PlayList.Count - 1 || PlaybackMode == PlaybackMode.RepeatAll || PlaybackMode == PlaybackMode.RepeatOne
                             || PlaybackMode == PlaybackMode.Shuffle;
        }

        ScopedLogger.Info("音频 {AudioUniqueId} 已加载", playListItem.AudioUniqueId);
    }

    [ObservableProperty] public partial bool CanLoadPrevious { get; private set; }
    public async Task LoadPrevious(bool isManuallyTriggered, bool playWhenLoaded)
    {
        if (CurrentPlayListItem is null || !CanLoadPrevious) return;

        ScopedLogger.Info("正在加载上一个音频: {CurrentPlayListItem}", CurrentPlayListItem.AudioUniqueId);
        switch (PlaybackMode)
        {
            case PlaybackMode.Sequential:
            {
                var index = _playList.IndexOf(CurrentPlayListItem);
                if (index <= 0) return;
                await LoadAsync(PlayList[index - 1]);
                break;
            }
            case PlaybackMode.Shuffle:
            {
                var randomIndex = new Random().Next(PlayList.Count);
                await LoadAsync(PlayList[randomIndex]);
                break;
            }
            case PlaybackMode.RepeatOne when !isManuallyTriggered:
                await LoadAsync(CurrentPlayListItem);
                break;
            case PlaybackMode.RepeatAll:
            case PlaybackMode.RepeatOne when isManuallyTriggered:
            {
                var index = _playList.IndexOf(CurrentPlayListItem);
                if (index <= 0) index = PlayList.Count;
                await LoadAsync(PlayList[index - 1]);
                break;
            }
            case PlaybackMode.MaxValue:
            default:
                ScopedLogger.Error("错误的播放模式: {PlaybackMode}", PlaybackMode);
                return;
        }

        if (playWhenLoaded)
            _audioPlayer.Play();
    }

    [ObservableProperty] public partial bool CanLoadForward { get; private set; }
    public async Task LoadForward(bool isManuallyTriggered, bool playWhenLoaded)
    {
        if (CurrentPlayListItem is null || !CanLoadForward) return;

        ScopedLogger.Info("正在加载下一个音频: {CurrentPlayListItem}", CurrentPlayListItem.AudioUniqueId);
        switch (PlaybackMode)
        {
            case PlaybackMode.Sequential:
            {
                var index = _playList.IndexOf(CurrentPlayListItem);
                if (index < 0 || index >= PlayList.Count - 1) return;
                await LoadAsync(PlayList[index + 1]);
                break;
            }
            case PlaybackMode.Shuffle:
            {
                var randomIndex = new Random().Next(PlayList.Count);
                await LoadAsync(PlayList[randomIndex]);
                break;
            }
            case PlaybackMode.RepeatOne when !isManuallyTriggered:
                await LoadAsync(CurrentPlayListItem);
                break;
            case PlaybackMode.RepeatAll:
            case PlaybackMode.RepeatOne when isManuallyTriggered:
            {
                var index = _playList.IndexOf(CurrentPlayListItem);
                if (index < 0) return;
                if (index >= PlayList.Count - 1) index = -1;
                await LoadAsync(PlayList[index + 1]);
                break;
            }
            case PlaybackMode.MaxValue:
            default:
                ScopedLogger.Error("错误的播放模式: {PlaybackMode}", PlaybackMode);
                return;
        }

        if (playWhenLoaded)
            _audioPlayer.Play();
    }

    public void Play()
    {
        ScopedLogger.Info("正在播放音频: {Title} | {UniqueId}", CurrentPlayListItem?.Title, CurrentPlayListItem?.AudioUniqueId);
        _audioPlayer.Play();
    }
    public void Pause()
    {
        ScopedLogger.Info("正在暂停音频: {Title} | {UniqueId}", CurrentPlayListItem?.Title, CurrentPlayListItem?.AudioUniqueId);
        _audioPlayer.Pause();
    }
    public async Task AppendAsync(PlayListItem playListItem)
    {
        if (_playList.Contains(playListItem))
        {
            ScopedLogger.Warn("尝试添加已存在的音频到播放列表: {AudioUniqueId}", playListItem.AudioUniqueId);
            return;
        }
        _playList.Add(playListItem);
        ScopedLogger.Info("音频 {AudioUniqueId} 已添加到播放列表", playListItem.AudioUniqueId);

        if (CurrentPlayListItem is null)
            await LoadFirstAsync();
    }
    public async Task InsertAfterCurrentPlayItemAsync(PlayListItem playListItem)
    {
        if (CurrentPlayListItem is null)
        {
            ScopedLogger.Warn("当前播放列表项为空，无法在其后插入音频，尝试添加: {AudioUniqueId}", playListItem.AudioUniqueId);
            await AppendAsync(playListItem);
            return;
        }

        var index = _playList.IndexOf(CurrentPlayListItem) + 1;
        if (index < 0 || index > _playList.Count)
        {
            ScopedLogger.Error("无法在当前播放列表项后插入音频，索引超出范围: {Index}", index);
            throw new ArgumentOutOfRangeException(nameof(playListItem), nameof(CurrentPlayListItem), "Current play item is not in the play list.");
        }

        if (_playList.Contains(playListItem))
            _playList.Remove(playListItem);
        _playList.Insert(index, playListItem);
        ScopedLogger.Info("音频 {AudioUniqueId} 已插入到当前播放列表项后", playListItem.AudioUniqueId);
    }
    public async Task InsertAfterCurrentPlayItemRangeAsync(IEnumerable<PlayListItem> playListItems)
    {
        if (CurrentPlayListItem is null)
        {
            foreach (var playListItem in playListItems)
                await AppendAsync(playListItem);
            ScopedLogger.Warn("当前播放列表项为空，无法在其后插入音频，尝试添加");
            return;
        }

        var index = _playList.IndexOf(CurrentPlayListItem) + 1;
        if (index == 0)
        {
            ScopedLogger.Error("无法在当前播放列表项后插入音频，当前播放列表项可能不在播放列表中");
            throw new ArgumentOutOfRangeException(nameof(playListItems), nameof(CurrentPlayListItem), "无法在当前播放列表项后插入音频，当前播放列表项可能不在播放列表中");
        }

        var itemsToInsert = playListItems.ToList();
        foreach (var item in itemsToInsert)
            _playList.Remove(item);
        index = CurrentPlayListItem is not null ? _playList.IndexOf(CurrentPlayListItem) + 1 : 0;
        _playList.InsertRange(index, itemsToInsert);
        ScopedLogger.Info("已在当前播放列表项后插入 {Count} 个音频", itemsToInsert.Count);
    }

    public void Clear()
    {
        CurrentPlayListItem = null;
        _configurationService.Settings.CommonSettings.BehaviorHistory.LastPlayAudioUniqueId = null;
        _audioPlayer.Stop();
        _playList.Clear();
        ScopedLogger.Info("播放列表已清空");
    }
}

public class CachedAudioStream : Stream
{
    private readonly List<byte> _data;

    public static async Task<CachedAudioStream> CreateAsync(
        AudioUniqueId audioUniqueId, Dictionary<string, string> cookies, IBilibiliClient bilibiliClient, CancellationToken cancellationToken)
    {
        var cacheFileInfo = new FileInfo(Path.Combine(AppHelper.ApplicationAudioCachesFolderPath, $"{audioUniqueId.Bvid}_p{audioUniqueId.Page}"));
        Stream upStream;
        Stream? cacheFileStream = null;
        if (cacheFileInfo.Exists)
        {
            upStream = cacheFileInfo.OpenRead();
        }
        else
        {
            upStream = await bilibiliClient.GetAudioStreamAsync(audioUniqueId, cookies).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            cacheFileStream = cacheFileInfo.Create();
        }
        var data = new List<byte>((int)upStream.Length);
        Task.Run(LoadAsync, cancellationToken).Detach();
        return new CachedAudioStream(data);

        async Task LoadAsync()
        {
            var buffer = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                int bytesRead;
                while ((bytesRead = await upStream.ReadAsync(buffer.AsMemory(0, 8192), cancellationToken)) > 0)
                {
                    if (cacheFileStream is not null) await cacheFileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    data.AddRange(buffer.AsSpan(0, bytesRead));
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                await upStream.DisposeAsync();
                if (cacheFileStream is not null) await cacheFileStream.DisposeAsync();
            }
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (Position >= Length)
            return 0;

        var bytesToRead = Math.Min(count, (int)(Length - Position));
        var bytesRead = 0;

        while (bytesRead < bytesToRead)
        {
            var remaining = bytesToRead - bytesRead;
            var readCount = Math.Min(remaining, _data.Count - (int)Position);
            if (readCount <= 0)
            {
                // wait for more data if the stream is not fully loaded
                Thread.Sleep(100);
                continue;
            }

            CollectionsMarshal.AsSpan(_data).Slice((int)Position, readCount).CopyTo(buffer.AsSpan(offset + bytesRead));
            bytesRead += readCount;
            Position += readCount;
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                Position = offset;
                break;
            case SeekOrigin.Current:
                Position += offset;
                break;
            case SeekOrigin.End:
                Position = Length + offset;
                break;
        }

        if (Position < 0 || Position > Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "Position is out of bounds.");

        return Position;
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value)
        => throw new InvalidOperationException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new InvalidOperationException();

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;

    public override long Length => _data.Capacity;

    private CachedAudioStream(List<byte> data)
    {
        _data = data;
    }

    public override long Position { get; set; }

    /// <summary>
    /// If the stream is downloaded from the network, this property indicates the progress of the buffered data.
    /// </summary>
    public double BufferedProgress => (double)_data.Count / Length;
}