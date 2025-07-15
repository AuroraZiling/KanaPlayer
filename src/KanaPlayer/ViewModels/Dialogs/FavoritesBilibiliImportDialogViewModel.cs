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

namespace KanaPlayer.ViewModels.Dialogs;

public partial class FavoritesBilibiliImportDialogViewModel : ViewModelBase
{
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
        _bilibiliClient.TryGetCookies(out var cookies);

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
    }

    private void GenerateLocalFavoriteFolderItem(FavoriteFolderItem item, List<CollectionFolderCommonMediaModel> importMedias)
    {
        _favoritesManager.ImportFromBilibili(item, importMedias);
        _navigationService.Navigate(typeof(FavoritesView));
        _kanaDialog.Dismiss();
    }
}