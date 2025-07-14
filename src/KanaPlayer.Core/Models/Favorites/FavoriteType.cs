namespace KanaPlayer.Core.Models.Favorites;

[Flags]
public enum FavoriteType
{
    Folder = 1,
    
    Collection = 2,
    
    Created = 4,
    
    Collected = 8
}