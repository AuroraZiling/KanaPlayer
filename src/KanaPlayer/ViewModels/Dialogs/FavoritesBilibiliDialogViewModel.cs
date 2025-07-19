using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Views.Pages;
using NLog;

namespace KanaPlayer.ViewModels.Dialogs;


public enum FavoritesBilibiliDialogType
{
    Import,
    Sync
}

public partial class FavoritesBilibiliDialogViewModel : ViewModelBase
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesBilibiliDialogViewModel));
    private readonly IKanaDialog _kanaDialog;
    private readonly IBilibiliClient _bilibiliClient;
    private readonly IFavoritesManager _favoritesManager;
    private readonly IKanaToastManager _kanaToastManager;
    private readonly INavigationService _navigationService;
    
    public FavoritesBilibiliDialogViewModel(FavoritesBilibiliDialogType favoritesBilibiliDialogType, IKanaDialog kanaDialog, FavoriteFolderItem item, IBilibiliClient bilibiliClient,
                                            IFavoritesManager favoritesManager, IKanaToastManager kanaToastManager, INavigationService navigationService)
    {
        _kanaDialog = kanaDialog;
        _bilibiliClient = bilibiliClient;
        _favoritesManager = favoritesManager;
        _kanaToastManager = kanaToastManager;
        _navigationService = navigationService;
        
        DialogType = favoritesBilibiliDialogType;
        Item = item;

        if (DialogType == FavoritesBilibiliDialogType.Import)
            ImportCommand.Execute(null);
        else
            SyncCommand.Execute(null);
    }
    
    [ObservableProperty] public partial FavoritesBilibiliDialogType DialogType { get; set; }
    [ObservableProperty] public partial FavoriteFolderItem Item { get; set; }
    [ObservableProperty] public partial int ProceedMediaCount { get; set; } = 0;
    [ObservableProperty] public partial string ProcessingMediaTitle { get; set; } = string.Empty;

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_bilibiliClient.TryGetCookies(out var cookies))
        {
            var progress = new Progress<int>(count =>
            {
                ProceedMediaCount += count;
            });
            if (Item.FavoriteType.HasFlag(FavoriteType.Folder))
            {
                var favoriteFolderInfo = await _bilibiliClient.GetFavoriteFolderDetailAsync(Item.Id, cookies, progress);
                var infoData = favoriteFolderInfo.EnsureData();
                GenerateLocalFavoriteFolderItem(Item, infoData.Medias);
            }
            else
            {
                var favoriteCollectionInfo = await _bilibiliClient.GetCollectionAsync(Item.Id, cookies, true, progress);
                var infoData = favoriteCollectionInfo.EnsureData();
                GenerateLocalFavoriteFolderItem(Item, infoData.Medias);
            }
            _kanaToastManager.CreateToast().WithTitle("导入成功").WithContent($"成功导入 {Item.Title} 收藏夹").WithType(NotificationType.Success).Queue();
            ScopedLogger.Info($"导入收藏夹成功，收藏夹标题：{Item.Title}，收藏夹 ID：{Item.Id}，导入数量：{ProceedMediaCount}");
        }
        else
        {
            _kanaToastManager.CreateToast().WithTitle("导入失败").WithContent("请先登录 B 站账号").WithType(NotificationType.Error).Queue();
            _navigationService.Navigate(typeof(FavoritesView));
            _kanaDialog.Dismiss();
            ScopedLogger.Error("导入收藏夹失败，未获取到 B 站 Cookies");
        }
    }
    
    [RelayCommand]
    private async Task SyncAsync()
    {
        if (_bilibiliClient.TryGetCookies(out var cookies))
        {
            var progress = new Progress<int>(count =>
            {
                ProceedMediaCount += count;
            });
            if (Item.FavoriteType.HasFlag(FavoriteType.Folder))
            {
                var favoriteFolderInfo = await _bilibiliClient.GetFavoriteFolderDetailAsync(Item.Id, cookies, progress);
                var infoData = favoriteFolderInfo.EnsureData();
                GenerateLocalFavoriteFolderItem(Item, infoData.Medias);
            }
            else
            {
                var favoriteCollectionInfo = await _bilibiliClient.GetCollectionAsync(Item.Id, cookies, true, progress);
                var infoData = favoriteCollectionInfo.EnsureData();
                GenerateLocalFavoriteFolderItem(Item, infoData.Medias);
            }
            _kanaToastManager.CreateToast().WithTitle("同步成功").WithContent($"成功同步 {Item.Title} 收藏夹").WithType(NotificationType.Success).Queue();
            ScopedLogger.Info($"同步收藏夹成功，收藏夹标题：{Item.Title}，收藏夹 ID：{Item.Id}，导入数量：{ProceedMediaCount}");
        }
        else
        {
            _kanaToastManager.CreateToast().WithTitle("同步失败").WithContent("请先登录 B 站账号").WithType(NotificationType.Error).Queue();
            _kanaDialog.Dismiss();
            ScopedLogger.Error("同步收藏夹失败，未获取到 B 站 Cookies");
        }
    }

    private void GenerateLocalFavoriteFolderItem(FavoriteFolderItem item, List<CollectionFolderCommonMediaModel> importMedias)
    {
        var processTypeName = DialogType == FavoritesBilibiliDialogType.Import ? "导入" : "同步";
        ProcessingMediaTitle = importMedias.Count > 0 ? $"正在{processTypeName} {item.Title} 收藏夹中的 {importMedias.Count} 个视频" : "没有可导入的视频";
        _favoritesManager.ImportFromBilibili(item, importMedias);
        _navigationService.Navigate(typeof(FavoritesView));
        _kanaDialog.Dismiss();
    }
}