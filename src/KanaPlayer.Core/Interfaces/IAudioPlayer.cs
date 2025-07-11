using System.ComponentModel;

namespace KanaPlayer.Core.Interfaces;

public enum PlayStatus
{
    Stopped,
    Loading,
    Loaded,
    Playing,
    Paused,
}

public interface IAudioPlayer : INotifyPropertyChanged
{
    PlayStatus Status { get; }
    
    /// <summary>
    /// Audio Progress in percentage (0.0 to 1.0)
    /// </summary>
    double Progress { get; set; }
    
    double Volume { get; set; }

    void Load(Stream audioStream);

    void Play();

    void Pause();
}