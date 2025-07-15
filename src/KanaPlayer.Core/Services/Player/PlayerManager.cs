using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
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
    private readonly IAudioPlayer _audioPlayer;
    private readonly IBilibiliClient _bilibiliClient;
    public PlayerManager(IConfigurationService<TSettings> configurationService, IAudioPlayer audioPlayer, IBilibiliClient bilibiliClient)
    {
        _audioPlayer = audioPlayer;
        _bilibiliClient = bilibiliClient;

        _audioPlayer.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IAudioPlayer.Status))
                OnPropertyChanged(nameof(Status));
        };
        _audioPlayer.PlaybackStopped += async () =>
        {
            await LoadForward(false, true);
        };

        PlaybackMode = configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode;
        Volume = configurationService.Settings.CommonSettings.BehaviorHistory.Volume;

        _bilibiliClient.TryGetCookies(out _cookies);
    }
    
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<PlayListItem> PlayList =>
        field ??= _playList.ToNotifyCollectionChangedSlim();

    [ObservableProperty] public partial PlayListItem? CurrentPlayListItem { get; private set; }

    public TimeSpan PlaybackTime
    {
        get => Duration * _audioPlayer.Progress;
        set => _audioPlayer.Progress = value / Duration;
    }
    
    public TimeSpan Duration
        => _audioPlayer.Duration;

    public double BufferedProgress
    {
        get
        {
            if (_cachedAudioStream is null) return 0;
            return (double)_cachedAudioStream.BufferedLength / _cachedAudioStream.Length;
        }
    }

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
                CanLoadPrevious = CurrentPlayListItem is not null && IndexOf(CurrentPlayListItem) > 0;
                CanLoadForward = CurrentPlayListItem is not null && IndexOf(CurrentPlayListItem) < PlayList.Count - 1;
                break;
            case PlaybackMode.RepeatOne:
            case PlaybackMode.RepeatAll:
            case PlaybackMode.Shuffle:
                CanLoadPrevious = true;
                CanLoadForward = true;
                break;
            case PlaybackMode.MaxValue:
            default:
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
        }
    }

    private CachedAudioStream? _cachedAudioStream;

    private readonly ObservableList<PlayListItem> _playList = [];
    private readonly Dictionary<string, string> _cookies;

    public async Task LoadAsync(PlayListItem playListItem)
    {
        _cachedAudioStream = null;
        CurrentPlayListItem = null;
        
        CanLoadPrevious = false;
        CanLoadForward = false;
        OnPropertyChanged(nameof(CanLoadPrevious));
        OnPropertyChanged(nameof(CanLoadForward));
        
        _audioPlayer.Pause();
        ArgumentNullException.ThrowIfNull(playListItem);
        
        await Task.Run(() =>
        {
            _audioPlayer.Load(_cachedAudioStream = new CachedAudioStream(playListItem.AudioUniqueId, _cookies, _bilibiliClient));
        });
        
        CurrentPlayListItem = playListItem;
        if (PlayList.Contains(playListItem))
        {
            CanLoadPrevious = IndexOf(playListItem) > 0 || PlaybackMode == PlaybackMode.RepeatAll || PlaybackMode == PlaybackMode.RepeatOne || PlaybackMode == PlaybackMode.Shuffle;
            CanLoadForward = IndexOf(playListItem) < PlayList.Count - 1 || PlaybackMode == PlaybackMode.RepeatAll || PlaybackMode == PlaybackMode.RepeatOne || PlaybackMode == PlaybackMode.Shuffle;
        }
    }

    [ObservableProperty] public partial bool CanLoadPrevious { get; private set; }
    public async Task LoadPrevious(bool isManuallyTriggered, bool playWhenLoaded)
    {
        if (CurrentPlayListItem is null || !CanLoadPrevious) return;
        
        switch (PlaybackMode)
        {
            case PlaybackMode.Sequential:
            {
                var index = IndexOf(CurrentPlayListItem);
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
                var index = IndexOf(CurrentPlayListItem);
                if (index <= 0) index = PlayList.Count;
                await LoadAsync(PlayList[index - 1]);
                break;
            }
            case PlaybackMode.MaxValue:
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (playWhenLoaded)
            _audioPlayer.Play();
    }
    
    [ObservableProperty] public partial bool CanLoadForward { get; private set; }
    public async Task LoadForward(bool isManuallyTriggered, bool playWhenLoaded)
    {
        if (CurrentPlayListItem is null || !CanLoadForward) return;

        switch (PlaybackMode)
        {
            case PlaybackMode.Sequential:
            {
                var index = IndexOf(CurrentPlayListItem);
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
                var index = IndexOf(CurrentPlayListItem);
                if (index < 0) return;
                if (index >= PlayList.Count - 1) index = -1;
                await LoadAsync(PlayList[index + 1]);
                break;
            }
            case PlaybackMode.MaxValue:
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        if (playWhenLoaded)
            _audioPlayer.Play();
    }

    public void Play()
        => _audioPlayer.Play();
    public void Pause()
        => _audioPlayer.Pause();
    public void Append(PlayListItem playListItem)
        => _playList.Add(playListItem);
    public void Insert(PlayListItem playListItem, int index)
        => _playList.Insert(index, playListItem);
    public void Remove(PlayListItem playListItem)
        => _playList.Remove(playListItem);
    public int IndexOf(PlayListItem playListItem)
        => _playList.IndexOf(playListItem);
    public void Clear()
        => _playList.Clear();
}

public class CachedAudioStream(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies, IBilibiliClient bilibiliClient) : Stream
{
    private bool isInitialized;

    private readonly List<byte> data = [];

    private void EnsureInitialized()
    {
        if (isInitialized) return;
        isInitialized = true;

        try
        {
            var cacheFileInfo = new FileInfo(Path.Combine(AppHelper.ApplicationAudioCachesFolderPath, $"{audioUniqueId.Bvid}_p{audioUniqueId.Page}"));
            if (cacheFileInfo.Exists)
            {
                using var stream = cacheFileInfo.OpenRead();
                _length = stream.Length;
                data.Capacity = (int)stream.Length;
                stream.ReadExactly(CollectionsMarshal.AsSpan(data));
            }
            else
            {
                using var upStream = bilibiliClient.GetAudioStreamAsync(audioUniqueId, cookies).ConfigureAwait(false).GetAwaiter().GetResult();
                _length = upStream.Length;

                using var cacheFileStream = cacheFileInfo.Create();

                var buffer = ArrayPool<byte>.Shared.Rent(81920);
                try
                {
                    int bytesRead;
                    while ((bytesRead = upStream.Read(buffer.AsMemory(0, 81920).Span)) > 0)
                    {
                        cacheFileStream.Write(buffer.AsMemory(0, bytesRead).Span);
                        data.AddRange(buffer.AsSpan(0, bytesRead));
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        EnsureInitialized();

        if (Position >= Length)
            return 0;

        var bytesToRead = (int)Math.Min(count, Length - Position);
        var bytesRead = 0;

        while (bytesRead < bytesToRead)
        {
            var remaining = bytesToRead - bytesRead;
            var readCount = Math.Min(remaining, data.Count - (int)Position);
            if (readCount <= 0)
            {
                // wait for more data if the stream is not fully loaded
                Thread.Sleep(100);
            }

            CollectionsMarshal.AsSpan(data).Slice((int)Position, readCount).CopyTo(buffer.AsSpan(offset + bytesRead));
            bytesRead += readCount;
            Position += readCount;
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureInitialized();

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

    public override long Length
    {
        get
        {
            EnsureInitialized();
            return _length;
        }
    }

    private long _length;

    public override long Position
    {
        get
        {
            EnsureInitialized();
            return field;
        }
        set
        {
            EnsureInitialized();
            if (value < 0 || value > Length)
                throw new ArgumentOutOfRangeException(nameof(value), "Position is out of bounds.");
            field = value;
        }
    }

    /// <summary>
    /// If the stream is downloaded from the network, this property indicates the length of the buffered data.
    /// </summary>
    public int BufferedLength => data.Count;
}