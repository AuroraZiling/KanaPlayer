using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Services.Player;
using NAudio.Wave;

namespace KanaPlayer.Windows.Services;

public partial class NAudioPlayerService : ObservableObject, IPlayerService
{
    [ObservableProperty] public partial PlayStatus Status { get; private set; } = PlayStatus.Stopped;

    public double Progress
    {
        get
        {
            if (_reader is null || _outputDevice is null) return 0;
            return _reader.CurrentTime.TotalSeconds / _reader.TotalTime.TotalSeconds;
        }
        set
        {
            if (_reader is null || _outputDevice is null) return;
            var newTime = TimeSpan.FromSeconds(value * _reader.TotalTime.TotalSeconds);
            if (newTime < TimeSpan.Zero || newTime > _reader.TotalTime) return;
            _reader.CurrentTime = newTime;
            OnPropertyChanged();
        }
    }

    public double Volume
    {
        get;
        set
        {
            if (field.IsCloseTo(value)) return;
            field = value;
            if (_outputDevice is null) return;
            _outputDevice.Volume = (float)Math.Clamp(value, 0.0, 1.0);
            OnPropertyChanged();
        }
    }

    private WaveOutEvent? _outputDevice;
    private StreamMediaFoundationReader? _reader;

    private readonly DispatcherTimer _progressTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(500),
        IsEnabled = false,
    };

    public NAudioPlayerService()
    {
        _progressTimer.Tick += delegate
        {
            OnPropertyChanged(nameof(Progress));
        };
    }

    public void Load(Stream audioStream)
    {
        _reader?.Dispose();
        _outputDevice?.Dispose();
        Status = PlayStatus.Loading;
        
        _reader = new StreamMediaFoundationReader(audioStream);
        _outputDevice = new WaveOutEvent();
        _outputDevice.Init(_reader);
        _outputDevice.Volume = (float)Volume;
        _outputDevice.PlaybackStopped += delegate
        {
            Status = PlayStatus.Stopped;
        };
        
        Status = PlayStatus.Loaded;
    }
    
    public void Play()
    {
        if (_outputDevice is null) return;
        _outputDevice.Play();
        Status = _outputDevice.PlaybackState == PlaybackState.Playing ? PlayStatus.Playing : PlayStatus.Paused;
    }
    
    public void Pause()
    {
        if (_outputDevice is null) return;
        _outputDevice.Pause();
        Status = _outputDevice.PlaybackState == PlaybackState.Paused ? PlayStatus.Paused : PlayStatus.Playing;
    }
}