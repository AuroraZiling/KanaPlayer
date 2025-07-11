using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class AudioInfoModel: CommonApiModel<AudioInfoDataModel>;

public class AudioInfoDataModel
{ 
    [JsonPropertyName("pic")] public required string CoverUrl { get; set; }
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("duration")] public required ulong DurationSeconds { get; set; }
    [JsonPropertyName("owner")] public required AudioInfoOwnerModel Owner { get; set; }
}

public class AudioInfoOwnerModel
{
    [JsonPropertyName("mid")] public required ulong Mid { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
    [JsonPropertyName("face")] public required string AvatarUrl { get; set; }
}
