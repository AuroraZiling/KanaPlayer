using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class FavoriteCollectedMetaModel: CommonApiModel<FavoriteCollectedMetaDataModel>;

public class FavoriteCollectedMetaDataModel
{
    [JsonPropertyName("list")] public List<FavoriteCollectedItemMetaDataModel> List { get; set; } = [];
    [JsonPropertyName("has_more")] public bool HasMore { get; set; }
}

public class FavoriteCollectedItemMetaDataModel: CollectionFolderCommonInfoModel
{
    [JsonPropertyName("ctime")] public required long CreatedTimestamp { get; set; }
    [JsonPropertyName("mtime")] public required long ModifiedTimestamp { get; set; }
    [JsonPropertyName("type")] public int Type { get; set; }  // 仅当 Type 为 11 时才能调用详细信息接口，否则需要调用合集接口
}