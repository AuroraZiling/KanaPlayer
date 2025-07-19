using KanaPlayer.Core.Models.BiliMediaList;

namespace KanaPlayer.Core.Models;

public readonly record struct AudioUniqueId(string Bvid, int Page = 1)
{
    public override string ToString() => $"{Bvid}_{Page}";
    
    public static AudioUniqueId Parse(string value)
    {
        var indexOfUnderscore = value.IndexOf('_');
        if (indexOfUnderscore < 0)
            throw new FormatException("Invalid AudioUniqueId format.");
        
        var bvid = value[..indexOfUnderscore];
        var page = int.Parse(value[(indexOfUnderscore + 1)..]);
        
        return new AudioUniqueId(bvid, page);
    }
}

public readonly record struct FavoriteUniqueId(ulong Id, BiliMediaListType BiliMediaListType)
{
    public override string ToString() => $"{Id}_{(int)BiliMediaListType}";
    public static FavoriteUniqueId Parse(string value)
    {
        var indexOfUnderscore = value.IndexOf('_');
        if (indexOfUnderscore < 0)
            throw new FormatException("Invalid FavoriteUniqueId format.");
        
        var mid = ulong.Parse(value[..indexOfUnderscore]);
        var favoriteType = int.Parse(value[(indexOfUnderscore + 1)..]);
        
        return new FavoriteUniqueId(mid, (BiliMediaListType)favoriteType);
    }
}