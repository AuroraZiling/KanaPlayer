using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Interfaces;
using Vortice.MediaFoundation;
using Vortice.Multimedia;
using static Vortice.MediaFoundation.MediaFactory;

namespace KanaPlayer.Windows.Services;

public partial class MediaFoundationAudioPlayer : ObservableObject, IAudioPlayer, IDisposable
{
    [ObservableProperty] public partial PlayStatus Status { get; private set; }

    public double Progress
    {
        get
        {
            if (_mediaEngine is null) return 0d;
            var duration = Duration.TotalSeconds;
            if (duration <= 0) return 0d;
            return _mediaEngine.CurrentTime.SafeDouble() / duration;
        }
        set
        {
            value = Math.Clamp(value, 0.0, 1.0).SafeDouble();
            if (_mediaEngine is null) return;
            if (_mediaEngine.Duration <= 0) return;
            var targetTime = _mediaEngine.Duration * value;
            if (_mediaEngine.CurrentTime.IsCloseTo(targetTime, 1)) return;
            _mediaEngine.CurrentTime = targetTime;
        }
    }

    [ObservableProperty] public partial TimeSpan Duration { get; private set; }

    public double Volume
    {
        get;
        set
        {
            value = Math.Clamp(value, 0.0, 1.0).SafeDouble();
            if (!SetProperty(ref field, value)) return;
            if (_mediaEngine is null) return;
            _mediaEngine.Volume = value;
        }
    }

    public event Action? PlaybackStopped;

    private IMFMediaEngine? _mediaEngine;
    private MFByteStream? _audioStream;
    private TaskCompletionSource? _audioLoadedTaskCompletionSource;

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
    [MemberNotNull(nameof(_mediaEngine))]
    private async ValueTask EnsureInitializedAsync()
    {
        if (_mediaEngine is not null) return;

        if ((await Task.Run(() => MFStartup(true))).Failure)
        {   
            throw new InvalidOperationException("Failed to initialize Media Foundation.");
        }

        using var factory = new IMFMediaEngineClassFactory();
        using var attributes = MFCreateAttributes(1);
        attributes.AudioCategory = AudioStreamCategory.GameMedia;

        _mediaEngine = factory.CreateInstance(MediaEngineCreateFlags.AudioOnly, attributes, PlaybackCallback);
    }
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

    private void PlaybackCallback(MediaEngineEvent mediaEvent, UIntPtr param1, int param2)
    {
        Console.WriteLine("PlayBack Event received: {0}", mediaEvent);

        if (_mediaEngine is null) return;

        switch (mediaEvent)
        {
            case MediaEngineEvent.CanPlay:
                Duration = TimeSpan.FromSeconds(_mediaEngine.Duration);
                _mediaEngine.Volume = Volume;
                _audioLoadedTaskCompletionSource?.TrySetResult();
                Status = PlayStatus.Paused;
                break;
            case MediaEngineEvent.Play:
            case MediaEngineEvent.Playing:
                Status = PlayStatus.Playing;
                break;
            case MediaEngineEvent.Pause:
                Status = PlayStatus.Paused;
                break;
            case MediaEngineEvent.Abort:
            case MediaEngineEvent.Ended:
                PlaybackStopped?.Invoke();
                break;
        }
    }

    public async Task LoadAsync(Func<Task<Stream>> asyncAudioStreamFactory)
    {
        await EnsureInitializedAsync();

        var stream = await asyncAudioStreamFactory();
        _audioStream = new MFByteStream(stream, true);

        _audioLoadedTaskCompletionSource = new TaskCompletionSource();
        _mediaEngine.QueryInterface<IMFMediaEngineEx>().SetSourceFromByteStream(_audioStream, @"X:\1.m4s");
        await _audioLoadedTaskCompletionSource.Task;
    }

    public void Play()
    {
        _mediaEngine?.Play();
    }

    public void Pause()
    {
        _mediaEngine?.Pause();
    }

    public void Stop()
    {
        if (_mediaEngine is null) return;

        _mediaEngine.Pause();
        //_mediaEngine.QueryInterface<IMFMediaEngineEx>().SetSourceFromByteStream(null, null);
    }

    public void Dispose()
    {
        Stop();
        _mediaEngine = null;

        _audioStream?.Dispose();
        _audioStream = null;

        MFShutdown().CheckError();
        GC.SuppressFinalize(this);
    }
}