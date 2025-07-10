using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class MusicRegionFeedModel: CommonApiModel<MusicRegionFeedDataModel>;

public record MusicRegionFeedDataModel
(
    [property: JsonPropertyName("archives")] List<MusicRegionFeedDataInfoModel> Archives
);

public record MusicRegionFeedDataInfoModel
(
    [property: JsonPropertyName("aid")] ulong Aid,
    [property: JsonPropertyName("bvid")] string Bvid,
    [property: JsonPropertyName("cid")] ulong Cid,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("cover")] string Cover,
    [property: JsonPropertyName("duration")] ulong DurationSeconds,
    [property: JsonPropertyName("pubdate")] ulong PublishTimestamp,
    [property: JsonPropertyName("stat")] MusicRegionFeedDataStatisticsModel Statistics,
    [property: JsonPropertyName("author")] MusicRegionFeedDataAuthorModel Author
);

public record MusicRegionFeedDataStatisticsModel
(
    [property: JsonPropertyName("view")] ulong ViewCount,
    [property: JsonPropertyName("like")] ulong LikeCount,
    [property: JsonPropertyName("danmaku")] ulong DanmakuCount
);

public record MusicRegionFeedDataAuthorModel
(
    [property: JsonPropertyName("mid")] ulong Mid,
    [property: JsonPropertyName("name")] string Name
);