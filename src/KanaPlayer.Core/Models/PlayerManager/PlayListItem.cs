namespace KanaPlayer.Core.Models.PlayerManager;

public record PlayListItem(
    string Title,
    string CoverUrl,
    string AuthorName,
    ulong AuthorMid,
    AudioUniqueId AudioUniqueId,
    TimeSpan Duration)
{
    public virtual bool Equals(PlayListItem? other)
        => other?.AudioUniqueId.Equals(AudioUniqueId) ?? false;

    public override int GetHashCode()
        => AudioUniqueId.GetHashCode();
}