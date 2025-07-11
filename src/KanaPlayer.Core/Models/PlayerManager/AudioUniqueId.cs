namespace KanaPlayer.Core.Models.PlayerManager;


public record AudioUniqueId(string Bvid, int Page = 1)
{
    public virtual bool Equals(AudioUniqueId? other)
        => other is not null && Bvid == other.Bvid && Page == other.Page;

    public override int GetHashCode()
        => HashCode.Combine(Bvid, Page);
}