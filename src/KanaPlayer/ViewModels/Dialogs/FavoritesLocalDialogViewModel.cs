using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.LocalMediaList;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Database;
using KanaPlayer.Core.Utils;
using KanaPlayer.Helpers;
using KanaPlayer.Views.Pages;
using NLog;

namespace KanaPlayer.ViewModels.Dialogs;

public enum FavoritesLocalDialogType
{
    Create,
    Rename,
    AddAudio
}

public partial class FavoritesLocalDialogViewModel(FavoritesLocalDialogType favoritesLocalDialogType, IKanaDialog kanaDialog,
                                                   ILocalMediaListManager localMediaListManager, IKanaToastManager kanaToastManager,
                                                   INavigationService navigationService, IBilibiliClient bilibiliClient,
                                                   DbLocalMediaListItem? dbLocalMediaListItem = null)
    : ViewModelBase
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(FavoritesLocalDialogViewModel));

    [ObservableProperty] public partial FavoritesLocalDialogType DialogType { get; set; } = favoritesLocalDialogType;

    #region Create

    [ObservableProperty] public partial string CreateTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string CreateDescription { get; set; } = string.Empty;

    [RelayCommand]
    private void Create()
    {
        if (string.IsNullOrEmpty(CreateTitle) || string.IsNullOrWhiteSpace(CreateTitle))
        {
            kanaToastManager.CreateToast().WithTitle("创建失败").WithContent("歌单标题不能为空").WithType(NotificationType.Error).Queue();
            return;
        }

        if (localMediaListManager.IsLocalMediaListExistsByTitle(CreateTitle))
        {
            kanaToastManager.CreateToast().WithTitle("创建失败").WithContent($"歌单标题 '{CreateTitle}' 已存在").WithType(NotificationType.Error).Queue();
            ScopedLogger.Warn($"尝试创建本地歌单时，歌单标题 '{CreateTitle}' 已存在");
            return;
        }

        var mediaListId = new LocalMediaListUniqueId(Guid.CreateVersion7());
        localMediaListManager.AddOrUpdateLocalMediaListItem(new LocalMediaListItem
        {
            UniqueId = mediaListId,
            Title = CreateTitle,
            CoverUrl = "",
            Description = CreateDescription,
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ModifiedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            MediaCount = 0
        });
        navigationService.Navigate(typeof(FavoritesView));
        kanaDialog.Dismiss();
        kanaToastManager.CreateToast().WithTitle("创建成功").WithContent($"成功创建 {CreateTitle} 歌单").WithType(NotificationType.Success).Queue();
        ScopedLogger.Info($"创建本地歌单成功，歌单标题：{CreateTitle}，歌单 ID：{mediaListId}");
    }

    #endregion

    #region Rename

    [ObservableProperty] public partial string RenameTitle { get; set; } = dbLocalMediaListItem?.Title ?? string.Empty;
    [ObservableProperty] public partial string RenameDescription { get; set; } = dbLocalMediaListItem?.Description ?? string.Empty;


    [RelayCommand]
    private void Rename()
    {
        if (string.IsNullOrEmpty(RenameTitle) || string.IsNullOrWhiteSpace(RenameTitle))
        {
            kanaToastManager.CreateToast().WithTitle("更改失败").WithContent("歌单标题不能为空").WithType(NotificationType.Error).Queue();
            return;
        }

        if (dbLocalMediaListItem is null)
        {
            kanaToastManager.CreateToast().WithTitle("更改失败").WithContent("歌单不存在或未选择").WithType(NotificationType.Error).Queue();
            ScopedLogger.Warn("尝试更改本地歌单信息时，未指定歌单 ID");
            return;
        }

        if (localMediaListManager.IsLocalMediaListExistsByTitle(RenameTitle))
        {
            kanaToastManager.CreateToast().WithTitle("更改失败").WithContent($"歌单标题 '{RenameTitle}' 已存在").WithType(NotificationType.Error).Queue();
            ScopedLogger.Warn($"尝试更改本地歌单信息时，歌单标题 '{RenameTitle}' 已存在");
            return;
        }
        
        dbLocalMediaListItem.Title = RenameTitle;
        dbLocalMediaListItem.Description = RenameDescription;
        dbLocalMediaListItem.ModifiedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        localMediaListManager.SaveChanges();
        
        navigationService.Navigate(typeof(FavoritesView));
        kanaDialog.Dismiss();
        kanaToastManager.CreateToast().WithTitle("更改成功").WithContent($"成功更改 {RenameTitle} 歌单信息").WithType(NotificationType.Success).Queue();
        ScopedLogger.Info($"更改本地歌单信息成功，歌单标题：{RenameTitle}");
    }

    #endregion

    #region Add Audio

    private readonly DebounceHelper _debounceHelper = new(TimeSpan.FromMilliseconds(500));
    [ObservableProperty] public partial int AddedMediaCount { get; set; }
    [ObservableProperty] public partial string AddedMediaTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string ReadyAddMedia { get; set; } = string.Empty;
    [ObservableProperty] public partial int PassAddMediaCount { get; set; }
    [ObservableProperty] public partial int PassAddMediaUrlCount { get; set; }
    [ObservableProperty] public partial int PassAddMediaBvIdCount { get; set; }
    [ObservableProperty] public partial int PassAddMediaAvIdCount { get; set; }
    private List<BiliBiliId> _passedMediaList = [];

    partial void OnReadyAddMediaChanged(string value)
    {
        _debounceHelper.Execute(delegate
        {
            _passedMediaList = BiliBiliExtractor.ExtractBilibiliIds(value);
            PassAddMediaCount = _passedMediaList.Count;
            PassAddMediaUrlCount = _passedMediaList.Count(item => item.Type == BiliBiliIdType.Url);
            PassAddMediaBvIdCount = _passedMediaList.Count(item => item.Type == BiliBiliIdType.Bv);
            PassAddMediaAvIdCount = _passedMediaList.Count(item => item.Type == BiliBiliIdType.Av);
        });
    }

    [RelayCommand]
    private async Task AddAudioAsync()
    {
        kanaDialog.CanDismissWithBackgroundClick = false;
        if (dbLocalMediaListItem is not null)
        {
            if (bilibiliClient.TryGetCookies(out var cookies))
            {
                foreach (var biliBiliId in _passedMediaList)
                {
                    var audioUniqueId = new AudioUniqueId(biliBiliId.Id);
                    var cachedAudioMetadata = localMediaListManager.GetCachedMediaListAudioMetadataByUniqueId(audioUniqueId);

                    AudioInfoDataModel audioInfoData;
                    if (cachedAudioMetadata is null)
                    {
                        var audioInfo = await bilibiliClient.GetAudioInfoAsync(audioUniqueId, cookies);
                        audioInfoData = audioInfo.EnsureData();
                        cachedAudioMetadata = localMediaListManager.AddOrUpdateAudioToCache(audioUniqueId, audioInfoData);
                    }
                    else
                    {
                        audioInfoData = new AudioInfoDataModel(cachedAudioMetadata);
                    }
                    cachedAudioMetadata.FavoriteTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    dbLocalMediaListItem.CachedMediaListAudioMetadataSet.Add(cachedAudioMetadata);
                    AddedMediaCount += 1;
                    AddedMediaTitle = audioInfoData.Title;
                }
                dbLocalMediaListItem.MediaCount = dbLocalMediaListItem.CachedMediaListAudioMetadataSet.Count;
                localMediaListManager.SaveChanges();
                kanaDialog.Dismiss();
                kanaToastManager.CreateToast().WithTitle("添加成功").WithContent($"成功添加 {AddedMediaCount} 个音频到歌单").WithType(NotificationType.Success).Queue();
                ScopedLogger.Info($"添加本地歌单音频成功，音频数量：{AddedMediaCount}");
            }
            else
            {
                kanaDialog.Dismiss();
                kanaToastManager.CreateToast().WithTitle("添加失败").WithContent("获取Cookies失败，无法添加音频").WithType(NotificationType.Error).Queue();
                ScopedLogger.Warn("获取Cookies失败，无法添加音频");
            }
        }
        else
        {
            kanaToastManager.CreateToast().WithTitle("添加失败").WithContent("歌单不存在或未选择").WithType(NotificationType.Error).Queue();
            ScopedLogger.Warn("尝试添加音频到本地歌单时，未指定歌单 ID");
        }
    }

    #endregion
}