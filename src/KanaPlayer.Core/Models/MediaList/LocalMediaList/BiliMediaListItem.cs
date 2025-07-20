namespace KanaPlayer.Core.Models.LocalMediaList;

public class LocalMediaListItem
{
    public required LocalMediaListUniqueId UniqueId { get; set; }
    public required string Title { get; set; }
    public required string CoverUrl { get; set; }  // TODO: Local Cover Support
    public required string Description { get; set; }
    public required long CreatedTimestamp { get; set; }
    public required long ModifiedTimestamp { get; set; }
    public required int MediaCount { get; set; }
}