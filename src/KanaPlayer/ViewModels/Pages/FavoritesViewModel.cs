using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.BiliMediaList;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Database;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using KanaPlayer.ViewModels.Dialogs;
using KanaPlayer.Views.Dialogs;
using KanaPlayer.Views.Pages.SubPages;
using NLog;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel(INavigationService navigationService, IBiliMediaListManager biliMediaListManager,
    ILocalMediaListManager localMediaListManager, IPlayerManager playerManager,
    IKanaToastManager kanaToastManager,
    IConfigurationService<SettingsModel> configurationService, IKanaDialogManager kanaDialogManager,
    IBilibiliClient bilibiliClient)
    : ViewModelBase, INavigationAware
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesViewModel));
    public IBilibiliClient BilibiliClient { get; } = bilibiliClient;

    #region Local Media List

    [ObservableProperty] public partial ObservableCollection<DbLocalMediaListItem> LocalMediaLists { get; set; } = [];
    [ObservableProperty] public partial DbLocalMediaListItem? SelectedLocalMediaList { get; set; }
    [ObservableProperty] public partial ObservableCollection<DbCachedMediaListAudioMetadata> LocalMediaListItems { get; set; } = [];
    [ObservableProperty] public partial DbCachedMediaListAudioMetadata? SelectedLocalMediaListItem { get; set; }

    [RelayCommand]
    private void RemoveLocalMediaList(DbLocalMediaListItem? removeItem)
    {
        if (removeItem is null)
            return;

        if (!localMediaListManager.DeleteLocalMediaListItem(removeItem.UniqueId))
        {
            kanaToastManager.CreateToast().WithTitle("失败")
                .WithContent("删除本地歌单失败")
                .WithType(NotificationType.Error)
                .Queue();
            return;
        }

        kanaToastManager.CreateToast().WithTitle("成功")
            .WithContent("删除本地歌单成功")
            .WithType(NotificationType.Success)
            .Queue();

        LocalMediaLists.Remove(removeItem);
        ScopedLogger.Info("已删除本地歌单：{FolderName}", removeItem.Title);
    }

    [RelayCommand]
    private void RenameLocalMediaList(DbLocalMediaListItem? renameItem)
    {
        if (renameItem is null)
            return;

        kanaDialogManager.CreateDialog()
            .WithView(new FavoritesLocalDialog())
            .WithViewModel(dialog =>
                new FavoritesLocalDialogViewModel(FavoritesLocalDialogType.Rename, dialog, localMediaListManager, kanaToastManager,
                    navigationService, BilibiliClient, renameItem))
            .WithDismissWithBackgroundClick()
            .OnDismissed(_ => RefreshMediaListsCommand.Execute(SelectedLocalMediaList))
            .TryShow();
    }

    partial void OnSelectedLocalMediaListChanged(DbLocalMediaListItem? value)
    {
        if (value is null)
            return;
        LocalMediaListItems = new ObservableCollection<DbCachedMediaListAudioMetadata>(localMediaListManager.GetCachedMediaListAudioMetadataList(value.UniqueId));
        ScopedLogger.Info("已选择本地歌单：{FolderName}，音频数量：{Count}", value.Title, LocalMediaListItems.Count);
    }

    [RelayCommand]
    private async Task DoubleTappedSelectedLocalMediaListItemAsync()
    {
        if (SelectedLocalMediaList is null || SelectedLocalMediaListItem is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior;
        var selectedPlayListItem = new PlayListItem(SelectedLocalMediaListItem.Title, SelectedLocalMediaListItem.CoverUrl, SelectedLocalMediaListItem.OwnerName,
            SelectedLocalMediaListItem.OwnerMid, SelectedLocalMediaListItem.UniqueId, TimeSpan.FromSeconds(SelectedLocalMediaListItem.DurationSeconds));
        switch (behavior)
        {
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            {
                playerManager.Clear();
                foreach (var cachedAudioMetadata in localMediaListManager.GetCachedMediaListAudioMetadataList(SelectedLocalMediaList.UniqueId))
                {
                    await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.AddToNextInPlayList:
                await playerManager.InsertAfterCurrentPlayItemAsync(new PlayListItem(SelectedLocalMediaListItem.Title, SelectedLocalMediaListItem.CoverUrl,
                    SelectedLocalMediaListItem.OwnerName,
                    SelectedLocalMediaListItem.OwnerMid, SelectedLocalMediaListItem.UniqueId, TimeSpan.FromSeconds(SelectedLocalMediaListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
                await playerManager.AppendAsync(new PlayListItem(SelectedLocalMediaListItem.Title, SelectedLocalMediaListItem.CoverUrl,
                    SelectedLocalMediaListItem.OwnerName,
                    SelectedLocalMediaListItem.OwnerMid, SelectedLocalMediaListItem.UniqueId, TimeSpan.FromSeconds(SelectedLocalMediaListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
            default:
                ScopedLogger.Error("错误的本地歌单双击播放行为：{Behavior}", behavior);
                return;
        }
        await playerManager.LoadAndPlayAsync(selectedPlayListItem);
        ScopedLogger.Info("双击播放本地歌单音频：{CreateTitle}，所属：{FolderName}，播放模式：{playbackMode}", SelectedLocalMediaListItem.Title, SelectedLocalMediaList.Title,
            behavior);
    }

    [RelayCommand]
    private void RemoveSelectedLocalMediaListItem(DbCachedMediaListAudioMetadata? removeItem)
    {
        if (removeItem is null || SelectedLocalMediaList is null)
            return;

        if (!localMediaListManager.RemoveAudioFromLocalMediaList(SelectedLocalMediaList.UniqueId, removeItem.UniqueId))
        {
            kanaToastManager.CreateToast().WithTitle("失败")
                .WithContent("删除本地歌单音频失败")
                .WithType(NotificationType.Error)
                .Queue();
            return;
        }

        kanaToastManager.CreateToast().WithTitle("成功")
            .WithContent("删除本地歌单音频成功")
            .WithType(NotificationType.Success)
            .Queue();

        LocalMediaListItems.Remove(removeItem);
        ScopedLogger.Info("已从本地歌单：{FolderName} 中删除音频：{CreateTitle}", SelectedLocalMediaList.Title, removeItem.Title);
    }

    [RelayCommand]
    private async Task PlayAllLocalMediaListAsync()
    {
        if (SelectedLocalMediaList is null)
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
        foreach (var cachedAudioMetadata in localMediaListManager.GetCachedMediaListAudioMetadataList(SelectedLocalMediaList.UniqueId))
        {
            await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
        }
        await playerManager.LoadFirstAndPlayAsync();
        ScopedLogger.Info("播放本地歌单全部音频：{FolderName}，音频数量：{Count}", SelectedLocalMediaList.Title, LocalMediaListItems.Count);
    }

    [RelayCommand]
    private void AddAllLocalMediaListToPlayList()
    {
        if (SelectedLocalMediaList is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;
        switch (behavior)
        {
            case FavoritesAddBehaviors.AddToNextInPlayList:
                playerManager.InsertAfterCurrentPlayItemRangeAsync(localMediaListManager.GetCachedMediaListAudioMetadataList(SelectedLocalMediaList.UniqueId)
                    .Select(cachedAudioMetadata =>
                        new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl,
                            cachedAudioMetadata.OwnerName,
                            cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId,
                            TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds))));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
            {
                foreach (var cachedAudioMetadata in localMediaListManager.GetCachedMediaListAudioMetadataList(SelectedLocalMediaList.UniqueId))
                {
                    playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
            default:
                ScopedLogger.Error("错误的本地歌单添加全部音频行为：{Behavior}", behavior);
                return;
        }
    }

    [RelayCommand]
    private void CreateLocalMediaList()
    {
        kanaDialogManager.CreateDialog()
            .WithView(new FavoritesLocalDialog())
            .WithViewModel(dialog =>
                new FavoritesLocalDialogViewModel(FavoritesLocalDialogType.Create, dialog, localMediaListManager, kanaToastManager, navigationService,
                    BilibiliClient))
            .WithDismissWithBackgroundClick()
            .TryShow();
    }

    [RelayCommand]
    private void AddAudioToLocalMediaList()
    {
        if (SelectedLocalMediaList is null)
        {
            kanaToastManager.CreateToast().WithTitle("错误")
                .WithContent("请先选择一个本地歌单")
                .WithType(NotificationType.Error)
                .Queue();
            return;
        }

        kanaDialogManager.CreateDialog()
            .WithView(new FavoritesLocalDialog())
            .WithViewModel(dialog =>
                new FavoritesLocalDialogViewModel(FavoritesLocalDialogType.AddAudio, dialog, localMediaListManager, kanaToastManager,
                    navigationService,
                    BilibiliClient, SelectedLocalMediaList))
            .WithDismissWithBackgroundClick()
            .OnDismissed(_ => RefreshMediaListsCommand.Execute(SelectedLocalMediaList))
            .TryShow();
    }

    #endregion

    #region Bili Media List

    public ObservableCollection<DbBiliMediaListItem> BiliMediaLists { get; } = [];
    [ObservableProperty] public partial DbBiliMediaListItem? SelectedBiliMediaList { get; set; }
    public ObservableCollection<DbCachedMediaListAudioMetadata> BiliMediaListItems { get; } = [];
    [ObservableProperty] public partial DbCachedMediaListAudioMetadata? SelectedBiliMediaListItem { get; set; }

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
        BiliMediaListItems.Reset(biliMediaListManager.GetCachedMediaListAudioMetadataList(value).OrderByDescending(item => item.FavoriteTimestamp));
        ScopedLogger.Info("已选择B站收藏夹/合集：{FolderName}，音频数量：{Count}", value.Title, BiliMediaListItems.Count);
    }

    [RelayCommand]
    private async Task DoubleTappedSelectedBiliMediaListItemAsync()
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
                foreach (var cachedAudioMetadata in biliMediaListManager.GetCachedMediaListAudioMetadataList(SelectedBiliMediaList))
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
        ScopedLogger.Info("双击播放B站收藏夹/合集音频：{CreateTitle}，所属收藏夹/合集：{FolderName}，播放模式：{playbackMode}", SelectedBiliMediaListItem.Title, SelectedBiliMediaList.Title,
            behavior);
    }

    [RelayCommand]
    private void ImportFromBilibili()
        => navigationService.Navigate(App.GetService<FavoritesBilibiliImportView>());

    [RelayCommand]
    private async Task PlayAllBiliMediaListAsync()
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
        foreach (var cachedAudioMetadata in BiliMediaListItems)
        {
            await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
        }
        await playerManager.LoadFirstAndPlayAsync();
        ScopedLogger.Info("播放B站收藏夹/合集全部音频：{FolderName}，音频数量：{Count}", SelectedBiliMediaList.Title, BiliMediaListItems.Count);
    }

    [RelayCommand]
    private void AddAllBiliMediaListToPlayList()
    {
        if (SelectedBiliMediaList is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;
        switch (behavior)
        {
            case FavoritesAddBehaviors.AddToNextInPlayList:
                playerManager.InsertAfterCurrentPlayItemRangeAsync(biliMediaListManager.GetCachedMediaListAudioMetadataList(SelectedBiliMediaList)
                    .Select(cachedAudioMetadata =>
                        new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl,
                            cachedAudioMetadata.OwnerName,
                            cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId,
                            TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds))));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
            {
                foreach (var cachedAudioMetadata in biliMediaListManager.GetCachedMediaListAudioMetadataList(SelectedBiliMediaList))
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
    private void SyncBiliMediaList()
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
                    BilibiliClient, biliMediaListManager, kanaToastManager, navigationService))
            .TryShow();
    }

    #endregion

    [RelayCommand]
    private void RefreshMediaLists()
    {
        var previousSelectedBiliMediaList = SelectedBiliMediaList;
        BiliMediaLists.Reset(biliMediaListManager.GetBiliMediaListItems());
        ScopedLogger.Info("刷新B站收藏夹/合集列表，数量：{Count}", BiliMediaLists.Count);
        var previousSelectedLocalMediaList = SelectedLocalMediaList;
        LocalMediaLists.Reset(localMediaListManager.GetLocalMediaListItems());
        ScopedLogger.Info("刷新本地歌单列表，数量：{Count}", LocalMediaLists.Count);

        if (previousSelectedBiliMediaList is not null)
            SelectedBiliMediaList = BiliMediaLists.FirstOrDefault(item => item.UniqueId == previousSelectedBiliMediaList.UniqueId);
        if (previousSelectedLocalMediaList is not null)
            SelectedLocalMediaList = LocalMediaLists.FirstOrDefault(item => item.UniqueId == previousSelectedLocalMediaList.UniqueId);
    }

    public void OnNavigatedTo()
        => RefreshMediaListsCommand.Execute(null);
}