using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;

namespace KanaPlayer.ViewModels.Pages.SubPages;

public partial class FavoritesBilibiliImportViewModel : ViewModelBase, INavigationAware
{
    private readonly IBilibiliClient _bilibiliClient;
    private readonly IConfigurationService<SettingsModel> _configurationService;
    
    public FavoritesBilibiliImportViewModel(IBilibiliClient bilibiliClient, IConfigurationService<SettingsModel> configurationService)
    {
        _bilibiliClient = bilibiliClient;
        _configurationService = configurationService;

        LoadBilibiliFavoriteFoldersCommand.Execute(null);
    }
    
    [ObservableProperty] public partial ObservableCollection<FavoriteFolderImportItem> FavoriteFolderImportItems { get; set; }
    [ObservableProperty] public partial int SelectedFavoriteFolderImportItemIndex { get; set; } = -1;

    [RelayCommand]
    private async Task LoadBilibiliFavoriteFolders()
    {
        FavoriteFolderImportItems = [];
        if (_bilibiliClient.TryGetCookies(out var cookies) && _configurationService.Settings.CommonSettings.Account is not null)
        {       
            var upMid = _configurationService.Settings.CommonSettings.Account.Mid;
            var createdFavoriteFoldersMeta = await _bilibiliClient.GetFavoriteCreatedFoldersMetaAsync(upMid, cookies);
            var collectedFavoriteFoldersMeta = await _bilibiliClient.GetFavoriteCollectedFoldersMetaAsync(upMid, cookies);

            // 用户创建的收藏夹
            foreach (var favoriteCreatedFolderMetaData in createdFavoriteFoldersMeta.EnsureData().Folders)
            {
                var favoriteFolderInfo = await _bilibiliClient.GetFavoriteFolderInfoAsync(favoriteCreatedFolderMetaData.Id, cookies);
                FetchFolderInfo(favoriteCreatedFolderMetaData.Id, FavoriteType.Folder | FavoriteType.Created, favoriteFolderInfo);
            }
            
            // 用户收集的收藏夹 / 合集
            foreach (var favoriteCollectedFolderMetaData in collectedFavoriteFoldersMeta)
            {
                if (favoriteCollectedFolderMetaData.Type == 11)  // 收藏夹
                {
                    var favoriteFolderInfo = await _bilibiliClient.GetFavoriteFolderInfoAsync(favoriteCollectedFolderMetaData.Id, cookies);
                    FetchFolderInfo(favoriteCollectedFolderMetaData.Id, FavoriteType.Folder | FavoriteType.Collected, favoriteFolderInfo);
                }
                else if (favoriteCollectedFolderMetaData.Type == 21) // 合集
                {
                    var collection = await _bilibiliClient.GetCollectionAsync(favoriteCollectedFolderMetaData.Id, cookies, false);
                    var collectionInfo = collection.EnsureData().Info;
                    FavoriteFolderImportItems.Add(new FavoriteFolderImportItem
                    {
                        Id = favoriteCollectedFolderMetaData.Id,
                        Title = collectionInfo.Title,
                        CoverUrl = collectionInfo.CoverUrl,
                        Description = collectionInfo.Description,
                        Owner = new FavoriteFolderOwnerInfoItem
                        {
                            Mid = collectionInfo.Owner.Mid,
                            Name = collectionInfo.Owner.Name
                        },
                        FavoriteType = FavoriteType.Collection | FavoriteType.Collected,
                        CreatedTimestamp = favoriteCollectedFolderMetaData.ModifiedTimestamp,  // 合集的创建时间使用修改时间
                        ModifiedTimestamp = favoriteCollectedFolderMetaData.ModifiedTimestamp,
                        MediaCount = collectionInfo.MediaCount
                    });
                }
            }
        }
        return;

        void FetchFolderInfo(ulong id, FavoriteType favoriteType, FavoriteFolderInfoModel favoriteFolderInfo)
        {
            var infoData = favoriteFolderInfo.EnsureData();
            FavoriteFolderImportItems.Add(new FavoriteFolderImportItem
            {
                Id = id,
                Title = infoData.Title,
                CoverUrl = infoData.CoverUrl,
                Description = infoData.Description,
                Owner = new FavoriteFolderOwnerInfoItem
                {
                    Mid = infoData.Owner.Mid,
                    Name = infoData.Owner.Name
                },
                FavoriteType = favoriteType,
                CreatedTimestamp = infoData.CreatedTimestamp,
                ModifiedTimestamp = infoData.ModifiedTimestamp,
                MediaCount = infoData.MediaCount
            });
        }
    }

    public void OnNavigatedTo()
    {
        
    }
}