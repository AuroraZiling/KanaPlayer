namespace KanaPlayer.Core.Models.PlayerManager;

public record PlayListItemModel(
    string Title,
    string CoverUrl,
    string AuthorName,
    ulong AuthorMid,
    AudioUniqueId AudioUniqueId,
    TimeSpan Duration)
{
    public virtual bool Equals(PlayListItemModel? other)
        => other?.AudioUniqueId.Equals(AudioUniqueId) ?? false;

    public override int GetHashCode()
        => AudioUniqueId.GetHashCode();
}