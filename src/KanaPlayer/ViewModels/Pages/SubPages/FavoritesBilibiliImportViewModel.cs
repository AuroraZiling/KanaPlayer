﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.BiliMediaList;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Database;
using KanaPlayer.Models;
using KanaPlayer.ViewModels.Dialogs;
using KanaPlayer.Views.Dialogs;
using NLog;

namespace KanaPlayer.ViewModels.Pages.SubPages;

public partial class FavoritesBilibiliImportViewModel(IBilibiliClient bilibiliClient, IConfigurationService<SettingsModel> configurationService,
                                                      IKanaToastManager kanaToastManager,
                                                      INavigationService navigationService, IKanaDialogManager kanaDialogManager, IBiliMediaListManager biliMediaListManager)
    : ViewModelBase, INavigationAware
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesBilibiliImportViewModel));
    private static List<BiliMediaListItem> CachedFavoriteFolderImportItems { get; set; } = [];

    [ObservableProperty] public partial ObservableCollection<BiliMediaListItem>? FavoriteFolderImportItems { get; set; }
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
                var createdFavoriteFoldersMeta = await bilibiliClient.GetCreatedBiliFavoriteMediaListMetaAsync(upMid, cookies);
                var collectedFavoriteFoldersMeta = await bilibiliClient.GetCollectedBiliFavoriteMediaListMetaAsync(upMid, cookies);

                // 用户创建的收藏夹
                foreach (var favoriteCreatedFolderMetaData in createdFavoriteFoldersMeta.EnsureData().List)
                {
                    var favoriteFolderInfo = await bilibiliClient.GetBiliFavoriteMediaListInfoAsync(favoriteCreatedFolderMetaData.Id, cookies);
                    var infoData = favoriteFolderInfo.EnsureData();
                    var model = new BiliMediaListItem
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
                        BiliMediaListType = BiliMediaListType.Folder | BiliMediaListType.Created,
                        CreatedTimestamp = infoData.CreatedTimestamp,
                        ModifiedTimestamp = infoData.ModifiedTimestamp,
                        MediaCount = infoData.MediaCount
                    };
                    CachedFavoriteFolderImportItems.Add(model);
                    FavoriteFolderImportItems.Add(model);
                }

                ScopedLogger.Info($"已获取到 {createdFavoriteFoldersMeta.EnsureData().List.Count} 个由用户创建的收藏夹");

                // 用户收集的收藏夹 / 合集
                foreach (var favoriteCollectedFolderMetaData in collectedFavoriteFoldersMeta)
                {
                    if (favoriteCollectedFolderMetaData.Type == 11) // 收藏夹
                    {
                        var favoriteFolderInfo = await bilibiliClient.GetBiliFavoriteMediaListInfoAsync(favoriteCollectedFolderMetaData.Id, cookies);
                        var infoData = favoriteFolderInfo.EnsureData();
                        var model = new BiliMediaListItem
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
                            BiliMediaListType = BiliMediaListType.Folder | BiliMediaListType.Collected,
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
                        var model = new BiliMediaListItem
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
                            BiliMediaListType = BiliMediaListType.Collection | BiliMediaListType.Collected,
                            CreatedTimestamp = favoriteCollectedFolderMetaData.ModifiedTimestamp, // 合集的创建时间使用修改时间
                            ModifiedTimestamp = favoriteCollectedFolderMetaData.ModifiedTimestamp,
                            MediaCount = collectionInfo.MediaCount
                        };
                        CachedFavoriteFolderImportItems.Add(model);
                        FavoriteFolderImportItems.Add(model);
                    }
                }

                ScopedLogger.Info($"已获取到 {collectedFavoriteFoldersMeta.Count} 个由用户收集的收藏夹/合集");
            }

            navigationService.IsPageProgressBarVisible = false;
        }
        else
        {
            FavoriteFolderImportItems = new ObservableCollection<BiliMediaListItem>(CachedFavoriteFolderImportItems);
            ScopedLogger.Info("使用缓存的收藏夹数据");
        }
    }

    [RelayCommand]
    private void Import(object? selectedImportItem)
    {
        var importItem = selectedImportItem.NotNull<BiliMediaListItem>();
        if (biliMediaListManager.IsBiliMediaListExists(new BiliMediaListUniqueId(importItem.Id, importItem.BiliMediaListType)))
        {
            kanaToastManager.CreateToast().WithType(NotificationType.Error).WithTitle("导入失败").WithContent("该收藏夹已存在于本地收藏中").Queue();
            ScopedLogger.Warn($"尝试导入的收藏夹已存在: {importItem.Title} (ID: {importItem.Id})");
            return;
        }

        kanaDialogManager.CreateDialog()
                         .WithView(new FavoritesBilibiliDialog())
                         .WithViewModel(dialog =>
                             new FavoritesBilibiliDialogViewModel(FavoritesBilibiliDialogType.Import, dialog, importItem,
                                 bilibiliClient, biliMediaListManager, kanaToastManager, navigationService))
                         .TryShow();
    }

    public void OnNavigatedTo()
        => LoadBilibiliFavoriteFoldersCommand.Execute(false);
}