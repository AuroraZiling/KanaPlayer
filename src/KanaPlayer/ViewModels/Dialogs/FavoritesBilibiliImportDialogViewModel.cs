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

public partial class FavoritesBilibiliImportDialogViewModel : ViewModelBase
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesBilibiliImportDialogViewModel));
    private readonly IKanaDialog _kanaDialog;
    private readonly IBilibiliClient _bilibiliClient;
    private readonly IFavoritesManager _favoritesManager;
    private readonly IKanaToastManager _kanaToastManager;
    private readonly INavigationService _navigationService;
    public FavoritesBilibiliImportDialogViewModel(IKanaDialog kanaDialog, FavoriteFolderItem item, IBilibiliClient bilibiliClient,
                                                  IFavoritesManager favoritesManager, IKanaToastManager kanaToastManager, INavigationService navigationService)
    {
        _kanaDialog = kanaDialog;
        _bilibiliClient = bilibiliClient;
        _favoritesManager = favoritesManager;
        _kanaToastManager = kanaToastManager;
        _navigationService = navigationService;
        Item = item;

        ImportCommand.Execute(null);
    }

    [ObservableProperty] public partial FavoriteFolderItem Item { get; set; }
    [ObservableProperty] public partial int ImportedMediaCount { get; set; } = 0;
    [ObservableProperty] public partial string ImportingMediaTitle { get; set; } = string.Empty;

    [RelayCommand]
    private async Task ImportAsync()
    {
        if (_bilibiliClient.TryGetCookies(out var cookies))
        {
            var progress = new Progress<int>(count =>
            {
                ImportedMediaCount += count;
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
            ScopedLogger.Info($"导入收藏夹成功，收藏夹标题：{Item.Title}，收藏夹 ID：{Item.Id}，导入数量：{ImportedMediaCount}");
        }
        else
        {
            _kanaToastManager.CreateToast().WithTitle("导入失败").WithContent("请先登录 B 站账号").WithType(NotificationType.Error).Queue();
            _navigationService.Navigate(typeof(FavoritesView));
            _kanaDialog.Dismiss();
            ScopedLogger.Error("导入收藏夹失败，未获取到 B 站 Cookies");
        }
    }

    private void GenerateLocalFavoriteFolderItem(FavoriteFolderItem item, List<CollectionFolderCommonMediaModel> importMedias)
    {
        ImportingMediaTitle = importMedias.Count > 0 ? $"正在导入 {item.Title} 收藏夹中的 {importMedias.Count} 个视频" : "没有可导入的视频";
        _favoritesManager.ImportFromBilibili(item, importMedias);
        _navigationService.Navigate(typeof(FavoritesView));
        _kanaDialog.Dismiss();
    }
}