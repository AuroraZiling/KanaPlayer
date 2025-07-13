using System.ComponentModel;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using ObservableCollections;

namespace KanaPlayer.Core.Services.Player;

public interface IPlayerManager : INotifyPropertyChanged
{
    NotifyCollectionChangedSynchronizedViewList<PlayListItemModel> PlayList { get; }
    double Volume { get; set; }
    PlayStatus Status { get; }
    TimeSpan PlaybackTime { get; set; }
    TimeSpan Duration { get; }
    PlayListItemModel? CurrentPlayListItem { get; }
    PlaybackMode PlaybackMode { get; set; }
    Task LoadAsync(PlayListItemModel playListItemModel);
    bool CanLoadPrevious { get; }
    Task LoadPrevious(bool isManuallyTriggered, bool playWhenLoaded);
    bool CanLoadForward { get; }
    Task LoadForward(bool isManuallyTriggered, bool playWhenLoaded);
    void Play();
    void Pause();
    void Append(PlayListItemModel playListItemModel);
    void Insert(PlayListItemModel playListItemModel, int index);
    void Remove(PlayListItemModel playListItemModel);
    int IndexOf(PlayListItemModel playListItemModel);
    void Clear();
}