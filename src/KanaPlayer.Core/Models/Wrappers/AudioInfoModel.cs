using System.Text.Json.Serialization;
using KanaPlayer.Core.Models.Database;

namespace KanaPlayer.Core.Models.Wrappers;

public class AudioInfoModel : CommonApiModel<AudioInfoDataModel>;

[method: JsonConstructor]
public record AudioInfoDataModel
(
    [property: JsonPropertyName("bvid")] string Bvid,
    [property: JsonPropertyName("videos")] int VideoCount,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("pic")] string CoverUrl,
    [property: JsonPropertyName("duration")] ulong DurationSeconds,
    [property: JsonPropertyName("pubdate")] long PublishTimestamp,
    [property: JsonPropertyName("owner")] CommonOwnerModel Owner,
    [property: JsonPropertyName("stat")] AudioStatisticsDataModel Statistics
)
{
    public AudioInfoDataModel(CachedAudioMetadata cachedAudioMetadata)
        : this(cachedAudioMetadata.UniqueId.Bvid, 1, cachedAudioMetadata.Title,
            cachedAudioMetadata.CoverUrl, cachedAudioMetadata.DurationSeconds,
            cachedAudioMetadata.PublishTimestamp, new CommonOwnerModel
            {
                Mid = cachedAudioMetadata.OwnerMid,
                Name = cachedAudioMetadata.OwnerName
            },
            new AudioStatisticsDataModel
            {
                CollectCount = cachedAudioMetadata.CollectCount,
                DanmakuCount = cachedAudioMetadata.DanmakuCount,
                PlayCount = cachedAudioMetadata.PlayCount
            })
    {
    }
}

public class AudioStatisticsDataModel
{
    [JsonPropertyName("favorite")] public required int CollectCount { get; set; }
    [JsonPropertyName("danmaku")] public required int DanmakuCount { get; set; }
    [JsonPropertyName("view")] public required int PlayCount { get; set; }
}