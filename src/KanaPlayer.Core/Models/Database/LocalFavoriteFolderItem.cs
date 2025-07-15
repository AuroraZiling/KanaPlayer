using System.ComponentModel.DataAnnotations;
using KanaPlayer.Core.Models.Favorites;

namespace KanaPlayer.Core.Models.Database;

public class LocalFavoriteFolderItem
{
    [Key]
    public FavoriteUniqueId UniqueId { get; set; }

    [MaxLength(128)]
    public required string Title { get; set; }
    [MaxLength(1024)]
    public required string CoverUrl { get; set; }
    [MaxLength(1024)]
    public required string Description { get; set; }
    public required ulong OwnerMid { get; set; }
    [MaxLength(64)]
    public required string OwnerName { get; set; }
    public required long CreatedTimestamp { get; set; }
    public required long ModifiedTimestamp { get; set; }
    public required FavoriteType FavoriteType { get; set; }
    public required int MediaCount { get; set; }

    public HashSet<CachedAudioMetadata> CachedAudioMetadataSet { get; set; } = [];
}