using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Models.Favorites;

public class FavoriteFolderItem
{
    public required ulong Id { get; set; }
    public required string Title { get; set; }
    public required string CoverUrl { get; set; }
    public required string Description { get; set; }
    public required CommonOwnerModel Owner { get; set; }
    public required FavoriteType FavoriteType { get; set; }
    public required long CreatedTimestamp { get; set; }
    public required long ModifiedTimestamp { get; set; }
    public required int MediaCount { get; set; }
}