using System.ComponentModel;

namespace KanaPlayer.Core.Interfaces;

public enum PlayStatus
{
    Stopped,
    Loading,
    Playing,
    Paused
}

public interface IAudioPlayer : INotifyPropertyChanged
{
    PlayStatus Status { get; }
    
    /// <summary>
    /// Audio Progress in percentage (0.0 to 1.0). Set to change the current playback position.
    /// </summary>
    double Progress { get; set; }

    /// <summary>
    /// Duration of the audio in TimeSpan.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// 0-1.0, where 0.0 is muted and 1.0 is full volume.
    /// </summary>
    double Volume { get; set; }

    /// <summary>
    /// When current music is stopped, this event will be raised.
    /// </summary>
    event Action? PlaybackStopped;

    /// <summary>
    /// Loads audio from a stream. This will stop any currently playing audio instantly and load the new audio stream.
    /// </summary>
    /// <param name="audioStream"></param>
    void Load(Stream audioStream);

    /// <summary>
    /// Loads audio from a factory function that returns a stream asynchronously.
    /// </summary>
    /// <param name="asyncAudioStreamFactory"></param>
    /// <returns></returns>
    Task LoadAsync(Func<Task<Stream>> asyncAudioStreamFactory);

    /// <summary>
    /// Plays the currently loaded audio.
    /// </summary>
    void Play();

    /// <summary>
    /// Pauses the currently playing audio.
    /// </summary>
    void Pause();
    void Stop();
}