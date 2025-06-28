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

public record CommonAuthenticationSettings(string RefreshToken, long Timestamp, string?[] Cookies);
public record CommonAccountSettings(string AvatarImgUri, ulong Mid, string UserName, CommonAccountLevelSettings Level, string VipLabelImgUri);
public record CommonAccountLevelSettings(int CurrentLevel, long CurrentMin, long CurrentExp);

#endregion