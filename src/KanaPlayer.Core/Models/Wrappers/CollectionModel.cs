using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class CollectionModel : CommonApiModel<CollectionDataModel>;

public class CollectionDataModel
{
    [JsonPropertyName("info")] public required CollectionDataInfoModel Info { get; set; }
    
    // 存在分页设计，请在 API 中指定是否需要获取完整
    [JsonPropertyName("medias")] public required List<CollectionDataMediaModel> Medias { get; set; }
}

public class CollectionDataInfoModel
{
    [JsonPropertyName("id")] public required ulong Id { get; set; }
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("intro")] public required string Description { get; set; }
    [JsonPropertyName("upper")] public required CollectionDataOwnerInfoModel Owner { get; set; }
    [JsonPropertyName("media_count")] public required int MediaCount { get; set; }
}

public class CollectionDataMediaModel
{
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("duration")] public required ulong DurationSeconds { get; set; }
    [JsonPropertyName("pubtime")] public required long PublishTimestamp { get; set; }
    [JsonPropertyName("bvid")] public required string Bvid { get; set; }
    [JsonPropertyName("cnt_info")] public required CollectionDataMediaStatisticsModel Statistics { get; set; }
}

public class CollectionDataMediaStatisticsModel
{
    [JsonPropertyName("collect")] public required int CollectCount { get; set; }
    [JsonPropertyName("danmaku")] public required int DanmakuCount { get; set; }
    [JsonPropertyName("play")] public required int PlayCount { get; set; }
}

public class CollectionDataOwnerInfoModel
{
    [JsonPropertyName("mid")] public required ulong Mid { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
}