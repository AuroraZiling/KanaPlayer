using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Models;
using KanaPlayer.ViewModels.Dialogs;
using KanaPlayer.Views.Dialogs;

namespace KanaPlayer.ViewModels.Pages.SubPages;

public partial class FavoritesBilibiliImportViewModel(IBilibiliClient bilibiliClient, IConfigurationService<SettingsModel> configurationService,
                                                      IKanaToastManager kanaToastManager,
                                                      INavigationService navigationService, IKanaDialogManager kanaDialogManager, IFavoritesManager favoritesManager)
    : ViewModelBase, INavigationAware
{
    private static List<FavoriteFolderItem> CachedFavoriteFolderImportItems { get; set; } = [];

    [ObservableProperty] public partial ObservableCollection<FavoriteFolderItem>? FavoriteFolderImportItems { get; set; }
    [ObservableProperty] public partial int SelectedFavoriteFolderImportItemIndex { get; set; } = -1;

    [RelayCommand]
    private async Task LoadBilibiliFavoriteFolders(bool needRefresh)
    {
        FavoriteFolderImportItems = [];
        if (needRefresh || CachedFavoriteFolderImportItems.Count == 0)
        {
            navigationService.IsPageProgressBarVisible = true;
            navigationService.IsPageProgressBarIndeterminate = true;

            CachedFavoriteFolderImportItems = [];
            if (bilibiliClient.TryGetCookies(out var cookies) && configurationService.Settings.CommonSettings.Account is not null)
            {
                var upMid = configurationService.Settings.CommonSettings.Account.Mid;
                var createdFavoriteFoldersMeta = await bilibiliClient.GetFavoriteCreatedFoldersMetaAsync(upMid, cookies);
                var collectedFavoriteFoldersMeta = await bilibiliClient.GetFavoriteCollectedFoldersMetaAsync(upMid, cookies);

                // 用户创建的收藏夹
                foreach (var favoriteCreatedFolderMetaData in createdFavoriteFoldersMeta.EnsureData().Folders)
                {
                    var favoriteFolderInfo = await bilibiliClient.GetFavoriteFolderInfoAsync(favoriteCreatedFolderMetaData.Id, cookies);
                    var infoData = favoriteFolderInfo.EnsureData();
                    var model = new FavoriteFolderItem
                    {
                        Id = favoriteCreatedFolderMetaData.Id,
                        Title = infoData.Title,
                        CoverUrl = infoData.CoverUrl,
                        Description = infoData.Description,
                        Owner = new CommonOwnerModel
                        {
                            Mid = infoData.Owner.Mid,
                            Name = infoData.Owner.Name
                        },
                        FavoriteType = FavoriteType.Folder | FavoriteType.Created,
                        CreatedTimestamp = infoData.CreatedTimestamp,
                        ModifiedTimestamp = infoData.ModifiedTimestamp,
                        MediaCount = infoData.MediaCount
                    };
                    CachedFavoriteFolderImportItems.Add(model);
                    FavoriteFolderImportItems.Add(model);
                }

                // 用户收集的收藏夹 / 合集
                foreach (var favoriteCollectedFolderMetaData in collectedFavoriteFoldersMeta)
                {
                    if (favoriteCollectedFolderMetaData.Type == 11) // 收藏夹
                    {
                        var favoriteFolderInfo = await bilibiliClient.GetFavoriteFolderInfoAsync(favoriteCollectedFolderMetaData.Id, cookies);
                        var infoData = favoriteFolderInfo.EnsureData();
                        var model = new FavoriteFolderItem
                        {
                            Id = favoriteCollectedFolderMetaData.Id,
                            Title = infoData.Title,
                            CoverUrl = infoData.CoverUrl,
                            Description = infoData.Description,
                            Owner = new CommonOwnerModel
                            {
                                Mid = infoData.Owner.Mid,
                                Name = infoData.Owner.Name
                            },
                            FavoriteType = FavoriteType.Folder | FavoriteType.Collected,
                            CreatedTimestamp = infoData.CreatedTimestamp,
                            ModifiedTimestamp = infoData.ModifiedTimestamp,
                            MediaCount = infoData.MediaCount
                        };
                        CachedFavoriteFolderImportItems.Add(model);
                        FavoriteFolderImportItems.Add(model);
                    }
                    else if (favoriteCollectedFolderMetaData.Type == 21) // 合集
                    {
                        var collection = await bilibiliClient.GetCollectionAsync(favoriteCollectedFolderMetaData.Id, cookies, false);
                        var collectionInfo = collection.EnsureData().Info;
                        var model = new FavoriteFolderItem
                        {
                            Id = favoriteCollectedFolderMetaData.Id,
                            Title = collectionInfo.Title,
                            CoverUrl = collectionInfo.CoverUrl,
                            Description = collectionInfo.Description,
                            Owner = new CommonOwnerModel
                            {
                                Mid = collectionInfo.Owner.Mid,
                                Name = collectionInfo.Owner.Name
                            },
                            FavoriteType = FavoriteType.Collection | FavoriteType.Collected,
                            CreatedTimestamp = favoriteCollectedFolderMetaData.ModifiedTimestamp, // 合集的创建时间使用修改时间
                            ModifiedTimestamp = favoriteCollectedFolderMetaData.ModifiedTimestamp,
                            MediaCount = collectionInfo.MediaCount
                        };
                        CachedFavoriteFolderImportItems.Add(model);
                        FavoriteFolderImportItems.Add(model);
                    }
                }
            }

            navigationService.IsPageProgressBarVisible = false;
        }
        else
            FavoriteFolderImportItems = new ObservableCollection<FavoriteFolderItem>(CachedFavoriteFolderImportItems);
    }

    [RelayCommand]
    private void Import(object? selectedImportItem)
    {
        var importItem = selectedImportItem.NotNull<FavoriteFolderItem>();
        if (favoritesManager.IsFolderExists(new FavoriteUniqueId(importItem.Id, importItem.FavoriteType)))
        {
            kanaToastManager.CreateToast().WithType(NotificationType.Error).WithTitle("导入失败").WithContent("该收藏夹已存在于本地收藏中").Queue();
            return;
        }

        kanaDialogManager.CreateDialog()
                         .WithView(new FavoritesBilibiliImportDialog())
                         .WithViewModel(dialog =>
                             new FavoritesBilibiliImportDialogViewModel(dialog, importItem, bilibiliClient, favoritesManager, kanaToastManager, navigationService))
                         .TryShow();
    }

    public void OnNavigatedTo()
        => LoadBilibiliFavoriteFoldersCommand.Execute(false);
}