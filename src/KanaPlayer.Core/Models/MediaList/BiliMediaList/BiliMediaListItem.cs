using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Models.BiliMediaList;

public class BiliMediaListItem
{
    public required ulong Id { get; set; }
    public required string Title { get; set; }
    public required string CoverUrl { get; set; }
    public required string Description { get; set; }
    public required CommonOwnerModel Owner { get; set; }
    public required BiliMediaListType BiliMediaListType { get; set; }
    public required long CreatedTimestamp { get; set; }
    public required long ModifiedTimestamp { get; set; }
    public required int MediaCount { get; set; }
}