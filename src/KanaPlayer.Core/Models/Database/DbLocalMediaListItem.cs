using System.ComponentModel.DataAnnotations.Schema;

namespace KanaPlayer.Core.Models.Database;

public class DbLocalMediaListItem : DbMediaListItem
{
    [NotMapped]
    public LocalMediaListUniqueId UniqueId
    {
        get => LocalMediaListUniqueId.Parse(Id);
        init => Id = value.ToString();
    }
}