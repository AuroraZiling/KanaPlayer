using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.LocalMediaList;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services.Database;

public interface ILocalMediaListManager
{
    DbCachedMediaListAudioMetadata? GetCachedMediaListAudioMetadataByUniqueId(AudioUniqueId uniqueId);
    DbCachedMediaListAudioMetadata AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData);
    List<DbCachedMediaListAudioMetadata> GetCachedMediaListAudioMetadataList(LocalMediaListUniqueId uniqueId);
    
    List<DbLocalMediaListItem> GetLocalMediaListItems();
    bool IsLocalMediaListExists(LocalMediaListUniqueId localMediaListUniqueId);
    bool AddOrUpdateLocalMediaListItem(LocalMediaListItem item);
    bool DeleteLocalMediaListItem(LocalMediaListUniqueId localMediaListUniqueId);
    
    bool RemoveAudioFromLocalMediaList(LocalMediaListUniqueId localMediaListUniqueId, AudioUniqueId audioUniqueId);
        
    int SaveChanges();
}