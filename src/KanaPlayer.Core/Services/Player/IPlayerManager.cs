using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using ObservableCollections;

namespace KanaPlayer.Core.Services.Player;

public interface IPlayerManager
{
    NotifyCollectionChangedSynchronizedViewList<PlayListItemModel> PlayList { get; }
    double Volume { get; set; }
    PlayStatus Status { get; }
    TimeSpan PlaybackTime { get; set; }
    TimeSpan Duration { get; }
    PlayListItemModel? CurrentPlayListItem { get; }

    PlaybackMode PlaybackMode { get; set; }
    Task LoadAsync(PlayListItemModel playListItemModel);
    void Play();
    void Pause();

    void Append(PlayListItemModel playListItemModel);

    void Insert(PlayListItemModel playListItemModel, int index);

    void Remove(PlayListItemModel playListItemModel);

    int IndexOf(PlayListItemModel playListItemModel);

    void Clear();
}