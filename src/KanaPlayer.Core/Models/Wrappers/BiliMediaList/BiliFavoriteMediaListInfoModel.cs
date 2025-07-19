using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class BiliFavoriteMediaListInfoModel: CommonApiModel<CreatedBiliFavoriteMediaListInfoDataModel>;

public class CreatedBiliFavoriteMediaListInfoDataModel: BiliMediaListCommonInfoModel
{
    [JsonPropertyName("ctime")] public required long CreatedTimestamp { get; set; }
    [JsonPropertyName("mtime")] public required long ModifiedTimestamp { get; set; }
}