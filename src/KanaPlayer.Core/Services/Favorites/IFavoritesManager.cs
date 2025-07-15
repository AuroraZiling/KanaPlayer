using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services.Favorites;

public interface IFavoritesManager
{
    List<LocalFavoriteFolderItem> GetLocalFavoriteFolders();
    bool IsFolderExists(FavoriteUniqueId favoriteUniqueId);
    void ImportFromBilibili(FavoriteFolderItem item, List<CollectionFolderCommonMediaModel> importMedias);
    void AddOrUpdateAudioToCache(AudioUniqueId audioUniqueId, AudioInfoDataModel audioInfoData);
}