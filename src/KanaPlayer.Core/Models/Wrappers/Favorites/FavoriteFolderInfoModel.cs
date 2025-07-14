using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class FavoriteFolderInfoModel: CommonApiModel<FavoriteCreatedFolderInfoDataModel>;

public class FavoriteCreatedFolderInfoDataModel
{
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("intro")] public required string Description { get; set; }
    [JsonPropertyName("upper")] public required FavoriteFolderOwnerInfoModel Owner { get; set; }
    [JsonPropertyName("ctime")] public required long CreatedTimestamp { get; set; }
    [JsonPropertyName("mtime")] public required long ModifiedTimestamp { get; set; }
    [JsonPropertyName("media_count")] public required int MediaCount { get; set; }
}