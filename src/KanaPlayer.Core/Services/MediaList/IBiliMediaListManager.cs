using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Wrappers;
using BiliMediaListItem = KanaPlayer.Core.Models.BiliMediaList.BiliMediaListItem;

namespace KanaPlayer.Core.Services.MediaList;

public interface IBiliMediaListManager
{
    DbCachedBiliMediaListAudioMetadata? GetCachedBiliMediaListAudioMetadataByUniqueId(AudioUniqueId uniqueId);
    List<DbBiliMediaListItem> GetBiliMediaListItems();
    List<DbCachedBiliMediaListAudioMetadata> GetCachedBiliMediaListAudioMetadataList(DbBiliMediaListItem item);
    bool IsBiliMediaListExists(FavoriteUniqueId favoriteUniqueId);
    void ImportFromBilibili(BiliMediaListItem item, List<BiliMediaListCommonMediaModel> importMedias);
    void AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData);
}