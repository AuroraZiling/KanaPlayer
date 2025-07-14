using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class FavoriteCreatedFoldersMetaModel: CommonApiModel<FavoriteCreatedFoldersMetaDataModel>;

public class FavoriteCreatedFoldersMetaDataModel
{
    [JsonPropertyName("list")] public List<FavoriteCreatedFolderMetaDataModel> Folders { get; set; } = [];
}

public class FavoriteCreatedFolderMetaDataModel
{
    [JsonPropertyName("id")] public ulong Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("media_count")] public int MediaCount { get; set; }
}