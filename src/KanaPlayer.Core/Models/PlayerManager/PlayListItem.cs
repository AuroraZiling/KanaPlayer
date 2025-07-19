using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Models.PlayerManager;

public record PlayListItem(
    string Title,
    string CoverUrl,
    string AuthorName,
    ulong AuthorMid,
    AudioUniqueId AudioUniqueId,
    TimeSpan Duration)
{
    public PlayListItem(DbCachedBiliMediaListAudioMetadata dbCachedBiliMediaListAudioMetadata) : this(dbCachedBiliMediaListAudioMetadata.Title, dbCachedBiliMediaListAudioMetadata.CoverUrl, dbCachedBiliMediaListAudioMetadata.OwnerName,
        dbCachedBiliMediaListAudioMetadata.OwnerMid, dbCachedBiliMediaListAudioMetadata.UniqueId, TimeSpan.FromSeconds(dbCachedBiliMediaListAudioMetadata.DurationSeconds))
    {
    }

    public PlayListItem(AudioInfoDataModel audioInfoData) : this(
        audioInfoData.Title,
        audioInfoData.CoverUrl,
        audioInfoData.Owner.Name,
        audioInfoData.Owner.Mid,
        new AudioUniqueId(audioInfoData.Bvid),
        TimeSpan.FromSeconds(audioInfoData.DurationSeconds))
    {
    }

    public virtual bool Equals(PlayListItem? other)
        => other?.AudioUniqueId.Equals(AudioUniqueId) ?? false;

    public override int GetHashCode()
        => AudioUniqueId.GetHashCode();
}