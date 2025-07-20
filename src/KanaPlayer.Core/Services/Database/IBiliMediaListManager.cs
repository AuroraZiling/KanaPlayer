using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Wrappers;
using BiliMediaListItem = KanaPlayer.Core.Models.BiliMediaList.BiliMediaListItem;

namespace KanaPlayer.Core.Services.Database;

public interface IBiliMediaListManager
{
    DbCachedMediaListAudioMetadata? GetCachedMediaListAudioMetadataByUniqueId(AudioUniqueId uniqueId);
    List<DbCachedMediaListAudioMetadata> GetCachedMediaListAudioMetadataList(DbBiliMediaListItem item);
    DbCachedMediaListAudioMetadata AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData);
    
    List<DbBiliMediaListItem> GetBiliMediaListItems();
    bool IsBiliMediaListExists(BiliMediaListUniqueId biliMediaListUniqueId);
    void ImportFromBilibili(BiliMediaListItem item, List<BiliMediaListCommonMediaModel> importMedias);
    bool DeleteBiliMediaListItem(BiliMediaListUniqueId biliMediaListUniqueId);
    int SaveChanges();
}