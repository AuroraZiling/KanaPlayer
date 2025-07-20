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

    [ObservableProperty] public partial string Title { get; set; } = string.Empty;
    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    [RelayCommand]
    private void Create()
    {
        if (string.IsNullOrEmpty(Title) || string.IsNullOrWhiteSpace(Title))
        {
            kanaToastManager.CreateToast().WithTitle("创建失败").WithContent("歌单标题不能为空").WithType(NotificationType.Error).Queue();
            return;
        }

        var mediaListId = new LocalMediaListUniqueId(Guid.CreateVersion7());
        localMediaListManager.AddOrUpdateLocalMediaListItem(new LocalMediaListItem
        {
            UniqueId = mediaListId,
            Title = Title,
            CoverUrl = "",
            Description = Description,
            CreatedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            ModifiedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            MediaCount = 0
        });
        navigationService.Navigate(typeof(FavoritesView));
        kanaDialog.Dismiss();
        kanaToastManager.CreateToast().WithTitle("创建成功").WithContent($"成功创建 {Title} 歌单").WithType(NotificationType.Success).Queue();
        ScopedLogger.Info($"创建本地歌单成功，歌单标题：{Title}，歌单 ID：{mediaListId}");
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