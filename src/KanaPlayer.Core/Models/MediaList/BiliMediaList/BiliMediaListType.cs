namespace KanaPlayer.Core.Models.BiliMediaList;

[Flags]
public enum BiliMediaListType
{
    Folder = 1,
    
    Collection = 2,
    
    Created = 4,
    
    Collected = 8
}