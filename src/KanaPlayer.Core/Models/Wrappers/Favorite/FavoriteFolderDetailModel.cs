using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class FavoriteFolderDetailModel: CommonApiModel<FavoriteFolderDetailDataModel>;

public class FavoriteFolderDetailDataModel
{
    [JsonPropertyName("info")] public required CollectionFolderCommonInfoModel Info { get; set; }
    [JsonPropertyName("medias")] public required List<CollectionFolderCommonMediaModel> Medias { get; set; }
    [JsonPropertyName("has_more")] public required bool HasMore { get; set; }
}