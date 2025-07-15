using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class AudioInfoModel: CommonApiModel<AudioInfoDataModel>;

public class AudioInfoDataModel
{ 
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("pic")] public required string CoverUrl { get; set; }
    [JsonPropertyName("duration")] public required ulong DurationSeconds { get; set; }
    [JsonPropertyName("pubdate")] public required long PublishTimestamp { get; set; }
    [JsonPropertyName("owner")] public required CommonOwnerModel Owner { get; set; }
    [JsonPropertyName("stat")] public required AudioStatisticsDataModel Statistics { get; set; }
}

public class AudioStatisticsDataModel
{
    [JsonPropertyName("favorite")] public required int CollectCount { get; set; }
    [JsonPropertyName("danmaku")] public required int DanmakuCount { get; set; }
    [JsonPropertyName("view")] public required int PlayCount { get; set; }
}