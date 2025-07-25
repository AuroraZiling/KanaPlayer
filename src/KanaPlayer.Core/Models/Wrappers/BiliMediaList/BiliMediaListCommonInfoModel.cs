﻿using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class BiliMediaListCommonInfoModel
{
    [JsonPropertyName("id")] public required ulong Id { get; set; }
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("intro")] public required string Description { get; set; }
    [JsonPropertyName("upper")] public required CommonOwnerModel Owner { get; set; }
    [JsonPropertyName("media_count")] public required int MediaCount { get; set; }
}

public class BiliMediaListCommonMediaModel
{
    [JsonPropertyName("title")] public required string Title { get; set; }
    [JsonPropertyName("cover")] public required string CoverUrl { get; set; }
    [JsonPropertyName("duration")] public required ulong DurationSeconds { get; set; }
    [JsonPropertyName("pubtime")] public required long PublishTimestamp { get; set; }
    [JsonPropertyName("fav_time")] public long FavoriteTimestamp { get; set; } = -1;  // 仅收藏夹内视频
    [JsonPropertyName("upper")] public required CommonOwnerModel Owner { get; set; }
    [JsonPropertyName("attr")] public int Attribute { get; set; } = 0;  // 仅收藏夹内视频 0: 正常；9: up自己删除；1: 其他原因删除
    [JsonPropertyName("bvid")] public required string Bvid { get; set; }
    [JsonPropertyName("cnt_info")] public required BiliMediaListCommonStatisticsModel Statistics { get; set; }
}

public class BiliMediaListCommonStatisticsModel
{
    [JsonPropertyName("collect")] public required int CollectCount { get; set; }
    [JsonPropertyName("danmaku")] public required int DanmakuCount { get; set; }
    [JsonPropertyName("play")] public required int PlayCount { get; set; }
}