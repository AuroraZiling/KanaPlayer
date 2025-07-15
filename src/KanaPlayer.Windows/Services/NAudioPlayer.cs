using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Interfaces;
using NAudio.Wave;
using Sdcb.FFmpeg.Codecs;
using Sdcb.FFmpeg.Formats;
using Sdcb.FFmpeg.Raw;
using Sdcb.FFmpeg.Swscales;
using Sdcb.FFmpeg.Toolboxs.Extensions;
using Sdcb.FFmpeg.Utils;

namespace KanaPlayer.Windows.Services;

public partial class NAudioPlayer : ObservableObject, IAudioPlayer
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
    
    public TimeSpan Duration => _reader?.TotalTime ?? TimeSpan.Zero;

    private WaveOutEvent? _outputDevice;
    private StreamMediaFoundationReader? _reader;

    private readonly DispatcherTimer _progressTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(500),
        IsEnabled = false,
    };

    public NAudioPlayer()
    {
        _progressTimer.Tick += delegate
        {
            OnPropertyChanged(nameof(Progress));
        };
    }

    public void Load(Stream audioStream)
    {
        if (_outputDevice is not null)
        {
            _outputDevice.PlaybackStopped -= OnOutputDeviceOnPlaybackStopped;
            _outputDevice.Dispose();
        }
        _reader?.Dispose();
        Status = PlayStatus.Loading;
        _reader = new StreamMediaFoundationReader(audioStream);
        _outputDevice = new WaveOutEvent();
        _outputDevice.Init(_reader);
        _outputDevice.Volume = (float)Volume;
        _outputDevice.PlaybackStopped += OnOutputDeviceOnPlaybackStopped;
        Status = PlayStatus.Loaded;
    }

    public event Action? PlaybackStopped;
    
    private void OnOutputDeviceOnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        Status = PlayStatus.Stopped;
        PlaybackStopped?.Invoke();
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

    // 顶级白忙活
    // private class FFmpegWaveProvider : IWaveProvider, IDisposable
    // {
    //     private readonly Lock _lock = new();
    //     private readonly Stream _stream;
    //     private readonly IOContext _ioContext;
    //     private readonly FormatContext _formatContext;
    //     private readonly CodecContext _audioDecoder;
    //     private readonly int _audioStreamIndex;
    //     private long _currentPts;
    //     private bool _endOfStream;
    //     private readonly BufferedWaveProvider _bufferedProvider;
    //
    //     public WaveFormat WaveFormat => _bufferedProvider.WaveFormat;
    //
    //     public TimeSpan TotalTime { get; }
    //
    //     public TimeSpan CurrentTime
    //     {
    //         get => TimeSpan.FromSeconds(_currentPts * _audioDecoder.TimeBase.ToDouble());
    //         set => Seek(value);
    //     }
    //
    //     static FFmpegWaveProvider()
    //     {
    //         FFmpegLogger.LogWriter = (level, msg) => Console.WriteLine(msg?.Trim());
    //     }
    //
    //     public FFmpegWaveProvider(Stream stream)
    //     {
    //         _stream = stream;
    //         _ioContext = IOContext.ReadStream(stream);
    //         _formatContext = FormatContext.OpenInputIO(_ioContext);
    //         var audioStream = _formatContext.GetAudioStream();
    //         _audioStreamIndex = audioStream.Index;
    //         _audioDecoder = new CodecContext(Codec.FindDecoderById(audioStream.Codecpar!.CodecId));
    //         _audioDecoder.FillParameters(audioStream.Codecpar);
    //         _audioDecoder.Open();
    //         _bufferedProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(
    //             audioStream.Codecpar.SampleRate,
    //             audioStream.Codecpar.ChLayout.nb_channels
    //         ))
    //         {
    //             BufferLength = 1024 * 1024,
    //             DiscardOnBufferOverflow = true
    //         };
    //         TotalTime = TimeSpan.FromSeconds(_formatContext.Duration / (double)ffmpeg.AV_TIME_BASE);
    //     }
    //
    //     public int Read(byte[] buffer, int offset, int count)
    //     {
    //         lock (_lock)
    //         {
    //             // 优先从缓冲区读取
    //             var bytesRead = _bufferedProvider.Read(buffer, offset, count);
    //             // 如果缓冲区不足，继续解码填充
    //             while (bytesRead < count && !_endOfStream && _bufferedProvider.BufferedBytes < _bufferedProvider.BufferLength / 2)
    //             {
    //                 var decodeBuffer = new byte[4096];
    //                 var decoded = DecodeAndFillBuffer(decodeBuffer);
    //                 if (decoded > 0)
    //                     _bufferedProvider.AddSamples(decodeBuffer, 0, decoded);
    //                 else
    //                     break;
    //                 var moreRead = _bufferedProvider.Read(buffer, offset + bytesRead, count - bytesRead);
    //                 bytesRead += moreRead;
    //             }
    //             return bytesRead;
    //         }
    //     }
    //
    //     private unsafe int DecodeAndFillBuffer(byte[] buffer)
    //     {
    //         using var packet = new Packet();
    //         if (_formatContext.ReadFrame(packet) == CodecResult.EOF)
    //         {
    //             _endOfStream = true;
    //             return 0;
    //         }
    //         if (packet.StreamIndex != _audioStreamIndex) return 0;
    //
    //         fixed (byte* ptr = buffer)
    //         {
    //             using var f = new Frame();
    //             foreach (var frame in _audioDecoder.DecodePacket(packet, f))
    //             {
    //                 var planar     = ffmpeg.av_sample_fmt_is_planar((AVSampleFormat)frame.Format) != 0;
    //                 var planes     = planar ? frame.ChLayout.nb_channels : 1;
    //                 var blockAlign = ffmpeg.av_get_bytes_per_sample((AVSampleFormat)frame.Format) * (planar ? 1 : frame.ChLayout.nb_channels);
    //                 var dataSize   = frame.NbSamples * blockAlign;
    //
    //                 var data = (byte**)Unsafe.AsPointer(ref frame.Data);
    //                 ffmpeg.av_samples_copy(
    //                     (byte**)ptr, data, 0, 0, frame.NbSamples,
    //                     frame.ChLayout.nb_channels, (AVSampleFormat)frame.Format);
    //                 _currentPts = frame.Pts;
    //
    //                 return dataSize * planes;
    //             }
    //         }
    //
    //         return 0;
    //     }
    //
    //     private unsafe void Seek(TimeSpan time)
    //     {
    //         lock (_lock)
    //         {
    //             var target = (long)(time.TotalSeconds / _audioDecoder.TimeBase.ToDouble());
    //             var ret = ffmpeg.av_seek_frame(_formatContext, _audioStreamIndex, target, (int)AVSEEK_FLAG.Backward);
    //             if (ret < 0) return;
    //             ffmpeg.avcodec_flush_buffers(_audioDecoder);
    //             _bufferedProvider.ClearBuffer();
    //             _currentPts = target;
    //             _endOfStream = false;
    //         }
    //     }
    //
    //     public void Dispose()
    //     {
    //         _stream.Dispose();
    //         _ioContext.Dispose();
    //         _formatContext.Dispose();
    //         _audioDecoder.Dispose();
    //     }
    // }
}