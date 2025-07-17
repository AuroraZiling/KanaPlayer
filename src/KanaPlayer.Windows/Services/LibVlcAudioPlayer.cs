using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Interfaces;
using LibVLCSharp.Shared;

namespace KanaPlayer.Windows.Services;

public partial class LibVlcAudioPlayer : ObservableObject, IAudioPlayer, IDisposable
{
    private readonly LibVLC libVlc;
    private readonly MediaPlayer _mediaPlayer;

    private BufferedStreamMediaInput? _streamMediaInput;
    private Media? _currentMedia;

    public event Action? PlaybackStopped;

    public LibVlcAudioPlayer()
    {
        LibVLCSharp.Shared.Core.Initialize();
        libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(libVlc)
        {
            EnableHardwareDecoding = true,
            FileCaching = 0,
            NetworkCaching = 0,
        };

        // 监听播放状态变化
        _mediaPlayer.EndReached += delegate
        {
            Status = PlayStatus.Stopped;
            PlaybackStopped?.Invoke();
        };

        _mediaPlayer.Playing += (_, _) => Status = PlayStatus.Playing;
        _mediaPlayer.Paused += (_, _) => Status = PlayStatus.Paused;
        _mediaPlayer.Stopped += (_, _) => Status = PlayStatus.Stopped;
    }

    [ObservableProperty]
    public partial PlayStatus Status { get; private set; } = PlayStatus.Stopped;

    public double Progress
    {
        get
        {
            if (_mediaPlayer.Length <= 0) return 0.0;
            return (double)_mediaPlayer.Time / _mediaPlayer.Length;
        }
        set
        {
            if (_mediaPlayer is not { Length: > 0, IsSeekable: true }) return;
            var targetTime = (long)(_mediaPlayer.Length * Math.Clamp(value, 0.0, 1.0));
            _mediaPlayer.Time = targetTime;
            OnPropertyChanged();
        }
    }

    public TimeSpan Duration
    {
        get
        {
            var length = _mediaPlayer.Length;
            return length > 0 ? TimeSpan.FromMilliseconds(length) : TimeSpan.Zero;
        }
    }

    public double Volume
    {
        get;
        set
        {
            field = Math.Clamp(value, 0.0, 1.0);
            _mediaPlayer.Volume = (int)(field * 100);
            OnPropertyChanged();
        }
    } = 1.0;

    public void Load(Stream audioStream)
    {
        try
        {
            Status = PlayStatus.Loading;

            // 清理之前的资源
            _currentMedia?.Dispose();
            _streamMediaInput?.Dispose();

            // 创建 LibVLC 媒体对象
            _streamMediaInput = new BufferedStreamMediaInput(audioStream);
            _currentMedia = new Media(libVlc, _streamMediaInput);

            _mediaPlayer.Media = _currentMedia;
            Status = PlayStatus.Loaded;
        }
        catch (Exception ex)
        {
            Status = PlayStatus.Stopped;
            throw new InvalidOperationException($"Failed to load audio stream: {ex.Message}", ex);
        }
    }

    public async Task LoadAsync(Func<Task<Stream>> asyncAudioStreamFactory)
    {
        try
        {
            Status = PlayStatus.Loading;
            var stream = await asyncAudioStreamFactory();
            Load(stream);
        }
        catch (Exception ex)
        {
            Status = PlayStatus.Stopped;
            throw new InvalidOperationException($"Failed to load audio stream asynchronously: {ex.Message}", ex);
        }
    }

    public void Play()
    {
        if (_currentMedia != null && Status != PlayStatus.Playing)
        {
            _mediaPlayer.Play();
        }
    }

    public void Pause()
    {
        if (Status == PlayStatus.Playing)
        {
            _mediaPlayer.Pause();
        }
    }

    public void Dispose()
    {
        libVlc.Dispose();
        _mediaPlayer.Dispose();

        _currentMedia?.Dispose();
        _streamMediaInput?.Dispose();

        GC.SuppressFinalize(this);
    }

    private class BufferedStreamMediaInput(Stream stream) : StreamMediaInput(stream)
    {
        private readonly Stream _stream = stream;

        public override bool Open(out ulong size)
        {
            size = ulong.MaxValue;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream.Dispose();
        }
    }
}