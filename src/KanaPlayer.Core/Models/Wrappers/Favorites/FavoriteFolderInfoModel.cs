using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class FavoriteFolderInfoModel: CommonApiModel<FavoriteCreatedFolderInfoDataModel>;

public class FavoriteCreatedFolderInfoDataModel: CollectionFolderCommonInfoModel
{
    [JsonPropertyName("ctime")] public required long CreatedTimestamp { get; set; }
    [JsonPropertyName("mtime")] public required long ModifiedTimestamp { get; set; }
}