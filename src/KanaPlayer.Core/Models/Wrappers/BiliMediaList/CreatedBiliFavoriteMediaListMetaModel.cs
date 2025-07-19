using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class CreatedBiliFavoriteMediaListMetaModel: CommonApiModel<CreatedFavoriteBiliMediaListMetaDataModel>;

public class CreatedFavoriteBiliMediaListMetaDataModel
{
    [JsonPropertyName("list")] public List<CreatedBiliFavoriteMediaListMetaDataItemModel> List { get; set; } = [];
}

public class CreatedBiliFavoriteMediaListMetaDataItemModel
{
    [JsonPropertyName("id")] public ulong Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("media_count")] public int MediaCount { get; set; }
}