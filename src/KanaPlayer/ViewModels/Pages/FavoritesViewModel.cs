using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.Favorites;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using KanaPlayer.ViewModels.Dialogs;
using KanaPlayer.Views.Dialogs;
using KanaPlayer.Views.Pages.SubPages;
using NLog;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel(INavigationService navigationService, IFavoritesManager favoritesManager, IPlayerManager playerManager,
                                        IKanaToastManager kanaToastManager,
                                        IConfigurationService<SettingsModel> configurationService, IKanaDialogManager kanaDialogManager,
                                        IBilibiliClient bilibiliClient)
    : ViewModelBase, INavigationAware
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesViewModel));

    [RelayCommand]
    private void RefreshFavoriteFolders()
    {
        FavoriteFolders = new ObservableCollection<LocalFavoriteFolderItem>(favoritesManager.GetLocalFavoriteFolders());
        ScopedLogger.Info("刷新收藏夹列表，当前收藏夹数量：{Count}", FavoriteFolders.Count);
    }

    [ObservableProperty] public partial ObservableCollection<LocalFavoriteFolderItem> FavoriteFolders { get; set; } = [];
    [ObservableProperty] public partial LocalFavoriteFolderItem? SelectedFavoriteFolder { get; set; }
    partial void OnSelectedFavoriteFolderChanged(LocalFavoriteFolderItem? value)
    {
        if (value is null)
            return;
        FavoriteFolderItems = new ObservableCollection<CachedAudioMetadata>(favoritesManager.GetCachedAudioMetadataList(value));
        ScopedLogger.Info("已选择收藏夹：{FolderName}，当前收藏夹中的音频数量：{Count}", value.Title, FavoriteFolderItems.Count);
    }
    [ObservableProperty] public partial ObservableCollection<CachedAudioMetadata> FavoriteFolderItems { get; set; } = [];
    [ObservableProperty] public partial CachedAudioMetadata? SelectedPlayListItem { get; set; }

    [RelayCommand]
    private async Task DoubleTappedSelectedItemAsync()
    {
        if (SelectedFavoriteFolder is null || SelectedPlayListItem is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior;
        var selectedPlayListItem = new PlayListItem(SelectedPlayListItem.Title, SelectedPlayListItem.CoverUrl, SelectedPlayListItem.OwnerName,
            SelectedPlayListItem.OwnerMid, SelectedPlayListItem.UniqueId, TimeSpan.FromSeconds(SelectedPlayListItem.DurationSeconds));
        switch (behavior)
        {
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            {
                playerManager.Clear();
                foreach (var cachedAudioMetadata in favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder))
                {
                    await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.AddToNextInPlayList:
                await playerManager.InsertAfterCurrentPlayItemAsync(new PlayListItem(SelectedPlayListItem.Title, SelectedPlayListItem.CoverUrl,
                    SelectedPlayListItem.OwnerName,
                    SelectedPlayListItem.OwnerMid, SelectedPlayListItem.UniqueId, TimeSpan.FromSeconds(SelectedPlayListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
                await playerManager.AppendAsync(new PlayListItem(SelectedPlayListItem.Title, SelectedPlayListItem.CoverUrl, SelectedPlayListItem.OwnerName,
                    SelectedPlayListItem.OwnerMid, SelectedPlayListItem.UniqueId, TimeSpan.FromSeconds(SelectedPlayListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
            default:
                ScopedLogger.Error("错误的收藏夹双击播放行为：{Behavior}", behavior);
                return;
        }
        await playerManager.LoadAndPlayAsync(selectedPlayListItem);
        ScopedLogger.Info("双击播放收藏夹音频：{Title}，所属收藏夹：{FolderName}，播放模式：{playbackMode}", SelectedPlayListItem.Title, SelectedFavoriteFolder.Title, behavior);
    }

    [RelayCommand]
    private void ImportFromBilibili()
    {
        navigationService.Navigate(App.GetService<FavoritesBilibiliImportView>());
    }

    [RelayCommand]
    private async Task PlayAllAsync()
    {
        if (SelectedFavoriteFolder is null)
            return;

        if (configurationService.Settings.UiSettings.Behaviors.IsFavoritesPlayAllReplaceWarningEnabled)
        {
            if (!await kanaDialogManager.CreateDialog()
                                        .WithTitle("替换播放列表")
                                        .WithContent("此操作将会替换当前播放列表，是否继续？")
                                        .WithYesNoResult("继续", "取消")
                                        .TryShowAsync())
                return;
        }

        playerManager.Clear();
        foreach (var cachedAudioMetadata in favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder))
        {
            await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
        }
        await playerManager.LoadFirstAndPlayAsync();
        ScopedLogger.Info("播放收藏夹全部音频：{FolderName}，音频数量：{Count}", SelectedFavoriteFolder.Title, FavoriteFolderItems.Count);
    }

    [RelayCommand]
    private void AddAllToPlayList()
    {
        if (SelectedFavoriteFolder is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;
        switch (behavior)
        {
            case FavoritesAddBehaviors.AddToNextInPlayList:
                playerManager.InsertAfterCurrentPlayItemRangeAsync(favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder).Select(cachedAudioMetadata =>
                    new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds))));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
            {
                foreach (var cachedAudioMetadata in favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder))
                {
                    playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
            default:
                ScopedLogger.Error("错误的收藏夹添加全部音频行为：{Behavior}", behavior);
                return;
        }
    }

    [RelayCommand]
    private void Sync()
    {
        if (SelectedFavoriteFolder is null)
            return;

        kanaDialogManager.CreateDialog()
                         .WithView(new FavoritesBilibiliDialog())
                         .WithViewModel(dialog =>
                             new FavoritesBilibiliDialogViewModel(FavoritesBilibiliDialogType.Sync, dialog, new FavoriteFolderItem
                                 {
                                     Id = SelectedFavoriteFolder.UniqueId.Id,
                                     Title = SelectedFavoriteFolder.Title,
                                     CoverUrl = SelectedFavoriteFolder.CoverUrl,
                                     Description = SelectedFavoriteFolder.Description,
                                     Owner = new CommonOwnerModel
                                     {
                                         Mid = SelectedFavoriteFolder.OwnerMid,
                                         Name = SelectedFavoriteFolder.OwnerName,
                                     },
                                     FavoriteType = SelectedFavoriteFolder.FavoriteType,
                                     CreatedTimestamp = SelectedFavoriteFolder.CreatedTimestamp,
                                     ModifiedTimestamp = SelectedFavoriteFolder.ModifiedTimestamp,
                                     MediaCount = SelectedFavoriteFolder.MediaCount
                                 },
                                 bilibiliClient, favoritesManager, kanaToastManager, navigationService))
                         .TryShow();
    }

    public void OnNavigatedTo()
    {
        RefreshFavoriteFoldersCommand.Execute(null);
    }
}