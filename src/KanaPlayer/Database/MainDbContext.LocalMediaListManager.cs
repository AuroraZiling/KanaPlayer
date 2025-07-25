using System.Collections.Generic;
using System.Linq;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.LocalMediaList;
using KanaPlayer.Core.Services.Database;
using Microsoft.EntityFrameworkCore;

namespace KanaPlayer.Database;

public partial class MainDbContext: ILocalMediaListManager
{
    public List<DbCachedMediaListAudioMetadata> GetCachedMediaListAudioMetadataList(LocalMediaListUniqueId uniqueId)
        => LocalMediaListItemSet
           .Include(folder => folder.CachedMediaListAudioMetadataSet)
           .Where(folder => folder.Id.Equals(uniqueId.ToString()))
           .SelectMany(folder => folder.CachedMediaListAudioMetadataSet)
           .OrderByDescending(metadata => metadata.PublishTimestamp)
           .ToList();
    
    public List<DbLocalMediaListItem> GetLocalMediaListItems()
        => LocalMediaListItemSet
           .Include(item => item.CachedMediaListAudioMetadataSet)
           .OrderByDescending(item => item.CreatedTimestamp)
           .ToList();
    
    public bool IsLocalMediaListExists(LocalMediaListUniqueId localMediaListUniqueId)
        => LocalMediaListItemSet
            .Any(item => item.Id.Equals(localMediaListUniqueId.ToString()));
    public bool AddOrUpdateMediaInLocalMediaList(LocalMediaListUniqueId localMediaListUniqueId, AudioUniqueId audioUniqueId) => throw new System.NotImplementedException();

    public bool AddOrUpdateLocalMediaListItem(LocalMediaListItem item)
    {
        var localMediaListItem = LocalMediaListItemSet
                                 .Include(existingItem => existingItem.CachedMediaListAudioMetadataSet)
                                 .FirstOrDefault(existingItem => existingItem.Id.Equals(item.UniqueId.ToString()));

        if (localMediaListItem is null)
        {
            localMediaListItem = new DbLocalMediaListItem
            {
                UniqueId = item.UniqueId,
                Title = item.Title.SafeSubstring(0, 128),
                CoverUrl = item.CoverUrl.SafeSubstring(0, 1024),
                Description = item.Description.SafeSubstring(0, 1024),
                CreatedTimestamp = item.CreatedTimestamp,
                ModifiedTimestamp = item.ModifiedTimestamp,
                MediaCount = item.MediaCount
            };
            LocalMediaListItemSet.Add(localMediaListItem);
        }
        else
        {
            localMediaListItem.Title = item.Title.SafeSubstring(0, 128);
            localMediaListItem.CoverUrl = item.CoverUrl.SafeSubstring(0, 1024);
            localMediaListItem.Description = item.Description.SafeSubstring(0, 1024);
            localMediaListItem.CreatedTimestamp = item.CreatedTimestamp;
            localMediaListItem.ModifiedTimestamp = item.ModifiedTimestamp;
            localMediaListItem.MediaCount = item.MediaCount;
        }

        SaveChanges();
        return true;
    }
    
    public bool DeleteLocalMediaListItem(LocalMediaListUniqueId localMediaListUniqueId)
    {
        var localMediaListItem = LocalMediaListItemSet
                                .Include(item => item.CachedMediaListAudioMetadataSet)
                                .FirstOrDefault(item => item.Id.Equals(localMediaListUniqueId.ToString()));

        if (localMediaListItem is null)
            return false;

        LocalMediaListItemSet.Remove(localMediaListItem);
        SaveChanges();
        return true;
    }
    
    public bool RemoveAudioFromLocalMediaList(LocalMediaListUniqueId localMediaListUniqueId, AudioUniqueId audioUniqueId)
    {
        var localMediaListItem = LocalMediaListItemSet
                                 .Include(item => item.CachedMediaListAudioMetadataSet)
                                 .FirstOrDefault(item => item.Id.Equals(localMediaListUniqueId.ToString()));
        if (localMediaListItem is null)
            return false;
        
        var audioMetadata = localMediaListItem.CachedMediaListAudioMetadataSet
            .FirstOrDefault(metadata => metadata.UniqueId.Equals(audioUniqueId));

        if (audioMetadata is null)
            return false;

        localMediaListItem.CachedMediaListAudioMetadataSet.Remove(audioMetadata);
        localMediaListItem.MediaCount = localMediaListItem.MediaCount > 0 ? localMediaListItem.MediaCount - 1 : 0;
        SaveChanges();
        return true;
    }
}