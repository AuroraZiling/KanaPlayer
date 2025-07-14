using System.Text.Json.Serialization;
using KanaPlayer.Core.Models.Favorites;

namespace KanaPlayer.Core.Models.Wrappers;

public class FavoriteCollectedMetaModel: CommonApiModel<FavoriteCollectedMetaDataModel>;

public class FavoriteCollectedMetaDataModel
{
    [JsonPropertyName("list")] public List<FavoriteCollectedItemMetaDataModel> List { get; set; } = [];
    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
}

public class FavoriteCollectedItemMetaDataModel
{
    [JsonPropertyName("id")] public required ulong Id { get; set; }  // 仅当 Type 为 11 时才能调用详细信息接口，否则需要调用合集接口
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("upper")] public required FavoriteFolderOwnerInfoItem Owner { get; set; }
    [JsonPropertyName("intro")] public required string Intro { get; set; }
    [JsonPropertyName("ctime")] public required long CreatedTimestamp { get; set; }
    [JsonPropertyName("mtime")] public required long ModifiedTimestamp { get; set; }
    [JsonPropertyName("media_count")] public required int MediaCount { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }
}