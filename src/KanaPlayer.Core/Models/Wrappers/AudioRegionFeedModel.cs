using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class AudioRegionFeedModel: CommonApiModel<AudioRegionFeedDataModel>;

public record AudioRegionFeedDataModel
(
    [property: JsonPropertyName("archives")] List<AudioRegionFeedDataInfoModel> Archives
);

public record AudioRegionFeedDataInfoModel
(
    [property: JsonPropertyName("aid")] ulong Aid,
    [property: JsonPropertyName("bvid")] string Bvid,
    [property: JsonPropertyName("cid")] ulong Cid,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("cover")] string Cover,
    [property: JsonPropertyName("duration")] ulong DurationSeconds,
    [property: JsonPropertyName("pubdate")] long PublishTimestamp,
    [property: JsonPropertyName("stat")] AudioRegionFeedDataStatisticsModel Statistics,
    [property: JsonPropertyName("author")] AudioRegionFeedDataAuthorModel Author
);

public record AudioRegionFeedDataStatisticsModel
(
    [property: JsonPropertyName("view")] ulong ViewCount,
    [property: JsonPropertyName("like")] ulong LikeCount,
    [property: JsonPropertyName("danmaku")] ulong DanmakuCount
);

public record AudioRegionFeedDataAuthorModel
(
    [property: JsonPropertyName("mid")] ulong Mid,
    [property: JsonPropertyName("name")] string Name
);