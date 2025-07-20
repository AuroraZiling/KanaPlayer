using System.ComponentModel.DataAnnotations;
using KanaPlayer.Core.Models.BiliMediaList;

namespace KanaPlayer.Core.Models.Database;

public class DbBiliMediaListItem : DbMediaListItem
{
    public BiliMediaListUniqueId UniqueId
    {
        get => BiliMediaListUniqueId.Parse(Id);
        init => Id = value.ToString();
    }
    
    public required ulong OwnerMid { get; set; }
    [MaxLength(64)]
    public required string OwnerName { get; set; }
    public required BiliMediaListType BiliMediaListType { get; set; }
}