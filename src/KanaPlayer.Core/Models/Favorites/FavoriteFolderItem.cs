using System.Text.Json.Serialization;
using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Core.Models.Favorites;

public class FavoriteFolderItem
{
    [JsonPropertyName("info")] public required FavoriteFolderInfoItem Info { get; set; }
    [JsonPropertyName("unique_ids")] public required AudioUniqueId[] AudioUniqueIds { get; set; }
}

public class FavoriteFolderInfoItem
{
    [JsonPropertyName("id")] public required string Id { get; set; }
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("upper")] public required FavoriteFolderOwnerInfoItem Owner { get; set; }
    [JsonPropertyName("ctime")] public required long CreatedTimestamp { get; set; }
    [JsonPropertyName("mtime")] public required string ModifiedTimestamp { get; set; }
    [JsonPropertyName("media_count")] public required int MediaCount { get; set; }
}

public class FavoriteFolderOwnerInfoItem
{
    [JsonPropertyName("mid")] public required ulong Mid { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
}