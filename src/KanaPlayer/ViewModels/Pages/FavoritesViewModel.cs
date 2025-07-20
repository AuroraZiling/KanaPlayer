using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models.BiliMediaList;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.MediaList;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using KanaPlayer.ViewModels.Dialogs;
using KanaPlayer.Views.Dialogs;
using KanaPlayer.Views.Pages.SubPages;
using NLog;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel(INavigationService navigationService, IBiliMediaListManager biliMediaListManager, IPlayerManager playerManager,
                                        IKanaToastManager kanaToastManager,
                                        IConfigurationService<SettingsModel> configurationService, IKanaDialogManager kanaDialogManager,
                                        IBilibiliClient bilibiliClient)
    : ViewModelBase, INavigationAware
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesViewModel));

    #region Offline

    #endregion

    #region Bili Media List

    [ObservableProperty] public partial ObservableCollection<DbBiliMediaListItem> BiliMediaLists { get; set; } = [];
    [ObservableProperty] public partial DbBiliMediaListItem? SelectedBiliMediaList { get; set; }
    [ObservableProperty] public partial ObservableCollection<DbCachedBiliMediaListAudioMetadata> BiliMediaListItems { get; set; } = [];
    [ObservableProperty] public partial DbCachedBiliMediaListAudioMetadata? SelectedBiliMediaListItem { get; set; }

    [RelayCommand]
    private void RefreshBiliMediaLists()
    {
        var dbMediaListItems = new ObservableCollection<DbBiliMediaListItem>(biliMediaListManager.GetBiliMediaListItems());
        BiliMediaLists = dbMediaListItems.Count > 0 ? dbMediaListItems : [];
        ScopedLogger.Info("刷新B站收藏夹/合集列表，数量：{Count}", dbMediaListItems.Count);
    }
    [RelayCommand]
    private void RemoveBiliMediaList(DbBiliMediaListItem? removeItem)
    {
        if (removeItem is null)
            return;

        if (!biliMediaListManager.DeleteBiliMediaListItem(removeItem.UniqueId))
        {
            kanaToastManager.CreateToast().WithTitle("失败")
                            .WithContent("删除B站收藏夹/合集失败")
                            .WithType(NotificationType.Error)
                            .Queue();
            return;
        }
        
        kanaToastManager.CreateToast().WithTitle("成功")
                        .WithContent("删除B站收藏夹/合集成功")
                        .WithType(NotificationType.Success)
                        .Queue();

        BiliMediaLists.Remove(removeItem);
        ScopedLogger.Info("已删除B站收藏夹/合集：{FolderName}", removeItem.Title);
    }
    partial void OnSelectedBiliMediaListChanged(DbBiliMediaListItem? value)
    {
        if (value is null)
            return;
        BiliMediaListItems = new ObservableCollection<DbCachedBiliMediaListAudioMetadata>(biliMediaListManager.GetCachedBiliMediaListAudioMetadataList(value));
        ScopedLogger.Info("已选择B站收藏夹/合集：{FolderName}，音频数量：{Count}", value.Title, BiliMediaListItems.Count);
    }

    [RelayCommand]
    private async Task DoubleTappedSelectedItemAsync()
    {
        if (SelectedBiliMediaList is null || SelectedBiliMediaListItem is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior;
        var selectedPlayListItem = new PlayListItem(SelectedBiliMediaListItem.Title, SelectedBiliMediaListItem.CoverUrl, SelectedBiliMediaListItem.OwnerName,
            SelectedBiliMediaListItem.OwnerMid, SelectedBiliMediaListItem.UniqueId, TimeSpan.FromSeconds(SelectedBiliMediaListItem.DurationSeconds));
        switch (behavior)
        {
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            {
                playerManager.Clear();
                foreach (var cachedAudioMetadata in biliMediaListManager.GetCachedBiliMediaListAudioMetadataList(SelectedBiliMediaList))
                {
                    await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.AddToNextInPlayList:
                await playerManager.InsertAfterCurrentPlayItemAsync(new PlayListItem(SelectedBiliMediaListItem.Title, SelectedBiliMediaListItem.CoverUrl,
                    SelectedBiliMediaListItem.OwnerName,
                    SelectedBiliMediaListItem.OwnerMid, SelectedBiliMediaListItem.UniqueId, TimeSpan.FromSeconds(SelectedBiliMediaListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
                await playerManager.AppendAsync(new PlayListItem(SelectedBiliMediaListItem.Title, SelectedBiliMediaListItem.CoverUrl,
                    SelectedBiliMediaListItem.OwnerName,
                    SelectedBiliMediaListItem.OwnerMid, SelectedBiliMediaListItem.UniqueId, TimeSpan.FromSeconds(SelectedBiliMediaListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
            default:
                ScopedLogger.Error("错误的收藏夹/合集双击播放行为：{Behavior}", behavior);
                return;
        }
        await playerManager.LoadAndPlayAsync(selectedPlayListItem);
        ScopedLogger.Info("双击播放B站收藏夹/合集音频：{Title}，所属收藏夹/合集：{FolderName}，播放模式：{playbackMode}", SelectedBiliMediaListItem.Title, SelectedBiliMediaList.Title,
            behavior);
    }

    [RelayCommand]
    private void ImportFromBilibili()
    {
        navigationService.Navigate(App.GetService<FavoritesBilibiliImportView>());
    }

    [RelayCommand]
    private async Task PlayAllAsync()
    {
        if (SelectedBiliMediaList is null)
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
        foreach (var cachedAudioMetadata in biliMediaListManager.GetCachedBiliMediaListAudioMetadataList(SelectedBiliMediaList))
        {
            await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
        }
        await playerManager.LoadFirstAndPlayAsync();
        ScopedLogger.Info("播放B站收藏夹/合集全部音频：{FolderName}，音频数量：{Count}", SelectedBiliMediaList.Title, BiliMediaListItems.Count);
    }

    [RelayCommand]
    private void AddAllToPlayList()
    {
        if (SelectedBiliMediaList is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;
        switch (behavior)
        {
            case FavoritesAddBehaviors.AddToNextInPlayList:
                playerManager.InsertAfterCurrentPlayItemRangeAsync(biliMediaListManager.GetCachedBiliMediaListAudioMetadataList(SelectedBiliMediaList)
                                                                                       .Select(cachedAudioMetadata =>
                                                                                           new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl,
                                                                                               cachedAudioMetadata.OwnerName,
                                                                                               cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId,
                                                                                               TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds))));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
            {
                foreach (var cachedAudioMetadata in biliMediaListManager.GetCachedBiliMediaListAudioMetadataList(SelectedBiliMediaList))
                {
                    playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
            default:
                ScopedLogger.Error("错误的收藏夹/合集添加全部音频行为：{Behavior}", behavior);
                return;
        }
    }

    [RelayCommand]
    private void Sync()
    {
        if (SelectedBiliMediaList is null)
            return;

        kanaDialogManager.CreateDialog()
                         .WithView(new FavoritesBilibiliDialog())
                         .WithViewModel(dialog =>
                             new FavoritesBilibiliDialogViewModel(FavoritesBilibiliDialogType.Sync, dialog, new BiliMediaListItem
                                 {
                                     Id = SelectedBiliMediaList.UniqueId.Id,
                                     Title = SelectedBiliMediaList.Title,
                                     CoverUrl = SelectedBiliMediaList.CoverUrl,
                                     Description = SelectedBiliMediaList.Description,
                                     Owner = new CommonOwnerModel
                                     {
                                         Mid = SelectedBiliMediaList.OwnerMid,
                                         Name = SelectedBiliMediaList.OwnerName,
                                     },
                                     BiliMediaListType = SelectedBiliMediaList.BiliMediaListType,
                                     CreatedTimestamp = SelectedBiliMediaList.CreatedTimestamp,
                                     ModifiedTimestamp = SelectedBiliMediaList.ModifiedTimestamp,
                                     MediaCount = SelectedBiliMediaList.MediaCount
                                 },
                                 bilibiliClient, biliMediaListManager, kanaToastManager, navigationService))
                         .TryShow();
    }

    #endregion

    public void OnNavigatedTo()
    {
        RefreshBiliMediaListsCommand.Execute(null);
    }
}