using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class BiliFavoriteMediaListDetailModel: CommonApiModel<BiliFavoriteMediaListDetailDataModel>;

public class BiliFavoriteMediaListDetailDataModel
{
    [JsonPropertyName("info")] public required BiliMediaListCommonInfoModel Info { get; set; }
    [JsonPropertyName("medias")] public required List<BiliMediaListCommonMediaModel> Medias { get; set; }
    [JsonPropertyName("has_more")] public required bool HasMore { get; set; }
}