// ReSharper disable CommentTypo

namespace KanaPlayer.Core.Models;

public abstract class SettingsBase
{
    public CommonSettings CommonSettings { get; set; } = new();
}

#region Common Settings

public class CommonSettings
{
    public CommonAuthenticationSettings? Authentication { get; set; }
    public CommonAccountSettings? Account { get; set; }
}

public class CommonAuthenticationSettings
{
    public required string RefreshToken { get; init; }
    public required long Timestamp { get; init; }
    public required Dictionary<string, string> Cookies { get; set; }
}

public record CommonAccountSettings(string AvatarImgUri, ulong Mid, string UserName, CommonAccountLevelSettings Level, string VipLabelImgUri);
public record CommonAccountLevelSettings(int CurrentLevel, long CurrentMin, long CurrentExp);

#endregion