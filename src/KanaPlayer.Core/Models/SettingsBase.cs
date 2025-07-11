// ReSharper disable CommentTypo

using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Core.Models;

public abstract class SettingsBase
{
    public CommonSettings CommonSettings { get; set; } = new();
}

public class CommonSettings
{
    public CommonAuthenticationSettings? Authentication { get; set; }
    public CommonAccountSettings? Account { get; set; }
    public CommonAudioCacheSettings AudioCache { get; set; } = new();
    public CommonImageCacheSettings ImageCache { get; set; } = new();
    public CommonBehaviorHistorySettings BehaviorHistory { get; set; } = new();
}

public class CommonAuthenticationSettings
{
    public required string RefreshToken { get; init; }
    public required long Timestamp { get; init; }
    public required Dictionary<string, string> Cookies { get; set; }
}

public record CommonAccountSettings(string AvatarImgUri, ulong Mid, string UserName, CommonAccountLevelSettings Level, string VipLabelImgUri);
public record CommonAccountLevelSettings(int CurrentLevel, long CurrentMin, long CurrentExp);

public class CommonAudioCacheSettings
{
    public bool Enabled { get; set; } = true;
    public int MaximumCacheSizeInMb { get; set; } = 512;
}

public class CommonImageCacheSettings
{
    public bool Enabled { get; set; } = true;
    public int MaximumCacheSizeInMb { get; set; } = 512;
}

public class CommonBehaviorHistorySettings
{
    public double Volume { get; set; } = 0.3;
    public PlaybackMode PlaybackMode { get; set; } = PlaybackMode.Sequential;
}