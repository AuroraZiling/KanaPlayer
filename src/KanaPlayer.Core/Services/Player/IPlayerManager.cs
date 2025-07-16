using System.ComponentModel;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using ObservableCollections;

namespace KanaPlayer.Core.Services.Player;

public interface IPlayerManager : INotifyPropertyChanged
{
    NotifyCollectionChangedSynchronizedViewList<PlayListItem> PlayList { get; }
    double Volume { get; set; }
    double BufferedProgress { get; }
    PlayStatus Status { get; }
    TimeSpan PlaybackTime { get; set; }
    TimeSpan Duration { get; }
    PlayListItem? CurrentPlayListItem { get; }
    PlaybackMode PlaybackMode { get; set; }
    Task LoadFirstAsync();
    Task LoadAsync(PlayListItem playListItem);
    bool CanLoadPrevious { get; }
    Task LoadPrevious(bool isManuallyTriggered, bool playWhenLoaded);
    bool CanLoadForward { get; }
    Task LoadForward(bool isManuallyTriggered, bool playWhenLoaded);
    void Play();
    void Pause();
    Task AppendAsync(PlayListItem playListItem);
    void Insert(PlayListItem playListItem, int index);
    void Remove(PlayListItem playListItem);
    int IndexOf(PlayListItem playListItem);
    void Clear();
}