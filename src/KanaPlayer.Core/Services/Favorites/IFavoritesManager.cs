using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services.Favorites;

public interface IFavoritesManager
{
    CachedAudioMetadata? GetCachedAudioMetadataByUniqueId(AudioUniqueId uniqueId);
    List<LocalFavoriteFolderItem> GetLocalFavoriteFolders();
    List<CachedAudioMetadata> GetCachedAudioMetadataList(LocalFavoriteFolderItem item);
    bool IsFolderExists(FavoriteUniqueId favoriteUniqueId);
    void ImportFromBilibili(FavoriteFolderItem item, List<CollectionFolderCommonMediaModel> importMedias);
    void AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData);
}