using System.ComponentModel.DataAnnotations;

namespace KanaPlayer.Core.Models.Database;

public class CachedAudioMetadata
{
    [Key]
    public AudioUniqueId UniqueId { get; set; }

    [MaxLength(128)]
    public required string Title { get; set; }
    [MaxLength(1024)]
    public required string CoverUrl { get; set; }
    public required ulong DurationSeconds { get; set; }
    public required long PublishTimestamp { get; set; }
    public required ulong OwnerMid { get; set; }
    [MaxLength(64)]
    public required string OwnerName { get; set; }
    public required int CollectCount { get; set; }
    public required int DanmakuCount { get; set; }
    public required int PlayCount { get; set; }
    
    public HashSet<LocalFavoriteFolderItem> LocalFavoriteFolderItemSet { get; set; } = [];
}