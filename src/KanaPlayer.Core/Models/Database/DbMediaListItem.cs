using System.ComponentModel.DataAnnotations;

namespace KanaPlayer.Core.Models.Database;

public abstract class DbMediaListItem
{
    [Key]
    [MaxLength(128)]
    public string Id { get; init; } = string.Empty;
    
    [MaxLength(128)]
    public required string Title { get; set; }
    [MaxLength(1024)]
    public required string CoverUrl { get; set; }
    [MaxLength(1024)]
    public required string Description { get; set; }
    public required long CreatedTimestamp { get; set; }
    public required long ModifiedTimestamp { get; set; }
    public required int MediaCount { get; set; }

    public HashSet<DbCachedMediaListAudioMetadata> CachedMediaListAudioMetadataSet { get; set; } = [];
}