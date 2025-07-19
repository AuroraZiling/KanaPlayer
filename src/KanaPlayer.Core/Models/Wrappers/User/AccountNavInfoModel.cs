using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers.User;

public class AccountNavInfoModel : CommonApiModel<AccountNavInfoDataModel>;

public record AccountNavInfoDataModel
(
    [property: JsonPropertyName("face")] string Face,
    [property: JsonPropertyName("level_info")] AccountNavInfoLevelModel LevelInfo,
    [property: JsonPropertyName("mid")] ulong Mid,
    [property: JsonPropertyName("uname")] string UserName,
    [property: JsonPropertyName("vip_label")] AccountNavInfoVipLabelModel VipLabel
);

public record AccountNavInfoLevelModel
(
    [property: JsonPropertyName("current_level")] int CurrentLevel,
    [property: JsonPropertyName("current_min")] long CurrentMin,
    [property: JsonPropertyName("current_exp")] long CurrentExp
);

public record AccountNavInfoVipLabelModel
(
    [property: JsonPropertyName("img_label_uri_hans_static")] string ImgLabelUri
);