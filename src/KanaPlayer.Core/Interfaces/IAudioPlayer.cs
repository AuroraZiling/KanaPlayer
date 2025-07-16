using System.ComponentModel;

namespace KanaPlayer.Core.Interfaces;

public enum PlayStatus
{
    Stopped,
    Loading,
    Loaded,
    Playing,
    Paused
}

public interface IAudioPlayer : INotifyPropertyChanged
{
    PlayStatus Status { get; }
    
    /// <summary>
    /// Audio Progress in percentage (0.0 to 1.0)
    /// </summary>
    double Progress { get; set; }
    TimeSpan Duration { get; }
    
    double Volume { get; set; }

    event Action? PlaybackStopped;
    
    void Load(Stream audioStream);

    Task LoadAsync(Func<Task<Stream>> asyncAudioStreamFactory);
    
    void Play();

    void Pause();
}