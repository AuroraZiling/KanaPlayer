namespace KanaPlayer.Core.Models.PlayerManager;

public record PlayListItemModel(
    string Title,
    string CoverUrl,
    string AuthorName,
    ulong AuthorMid,
    string AudioBvid,
    TimeSpan Duration)
{
    public virtual bool Equals(PlayListItemModel? other)
        => other?.AudioBvid == AudioBvid;

    public override int GetHashCode()
        => AudioBvid.GetHashCode();
}