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
    public AudioInfoDataModel(DbCachedMediaListAudioMetadata dbCachedMediaListAudioMetadata)
        : this(dbCachedMediaListAudioMetadata.UniqueId.Bvid, 1, dbCachedMediaListAudioMetadata.Title,
            dbCachedMediaListAudioMetadata.CoverUrl, dbCachedMediaListAudioMetadata.DurationSeconds,
            dbCachedMediaListAudioMetadata.PublishTimestamp, new CommonOwnerModel
            {
                Mid = dbCachedMediaListAudioMetadata.OwnerMid,
                Name = dbCachedMediaListAudioMetadata.OwnerName
            },
            new AudioStatisticsDataModel
            {
                CollectCount = dbCachedMediaListAudioMetadata.CollectCount,
                DanmakuCount = dbCachedMediaListAudioMetadata.DanmakuCount,
                PlayCount = dbCachedMediaListAudioMetadata.PlayCount
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