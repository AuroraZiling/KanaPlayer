using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Database;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using NLog;
using ObservableCollections;

namespace KanaPlayer.ViewModels.Pages;

public partial class HomeViewModel(IBilibiliClient bilibiliClient, IPlayerManager playerManager,
                                   ILauncher launcher, IBiliMediaListManager biliMediaListManager, IConfigurationService<SettingsModel> configurationService)
    : ViewModelBase, INavigationAware
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(HomeViewModel));
    
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<AudioRegionFeedDataInfoModel> MusicRegionFeeds
        => field ??= _musicRegionFeeds.ToNotifyCollectionChangedSlim();

    private readonly ObservableList<AudioRegionFeedDataInfoModel> _musicRegionFeeds = [];

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _musicRegionFeeds.Clear();
        await LoadMoreAsync();
        ScopedLogger.Info("音乐分区动态已刷新");
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (bilibiliClient.TryGetCookies(out var cookies))
        {
            var feeds = await bilibiliClient.GetAudioRegionFeedAsync(cookies);
            if (feeds.Data is null)
            {
                ScopedLogger.Warn("获取音乐分区动态失败，数据为空");
                return;
            }
            _musicRegionFeeds.AddRange(feeds.Data.Archives);
            ScopedLogger.Info("音乐分区动态已加载，数量: {Count}", feeds.Data.Archives.Count);
        }
        else
            ScopedLogger.Warn("获取Cookies失败，无法加载音乐分区动态");
    }

    [RelayCommand]
    private async Task LoadAudioAsync(AudioRegionFeedDataInfoModel audioRegionFeedDataInfoModel)
    {
        if (bilibiliClient.TryGetCookies(out var cookies))
        {
            var uniqueId = new AudioUniqueId(audioRegionFeedDataInfoModel.Bvid);
            var cachedAudioMetadata = biliMediaListManager.GetCachedMediaListAudioMetadataByUniqueId(uniqueId);

            AudioInfoDataModel audioInfoData;
            if (cachedAudioMetadata is null)
            {
                var audioInfo = await bilibiliClient.GetAudioInfoAsync(uniqueId, cookies);
                audioInfoData = audioInfo.EnsureData();
                biliMediaListManager.AddOrUpdateAudioToCache(uniqueId, audioInfoData);
            }
            else
            {
                audioInfoData = new AudioInfoDataModel(cachedAudioMetadata);
            }
        
            var playItem = new PlayListItem(
                audioInfoData.Title,
                audioInfoData.CoverUrl,
                audioInfoData.Owner.Name,
                audioInfoData.Owner.Mid,
                uniqueId,
                TimeSpan.FromSeconds(audioInfoData.DurationSeconds)
            );

            var behavior = configurationService.Settings.UiSettings.Behaviors.HomeAddBehavior;
            switch (behavior)
            {
                case FavoritesAddBehaviors.AddToNextInPlayList:
                    await playerManager.InsertAfterCurrentPlayItemAsync(playItem);
                    break;
                case FavoritesAddBehaviors.AddToEndOfPlayList:
                    await playerManager.AppendAsync(playItem);
                    break;
                case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
                    await playerManager.InsertAfterCurrentPlayItemAsync(playItem);
                    Task.Run(() => playerManager.LoadAndPlayAsync(playItem)).Detach();
                    break;
                case FavoritesAddBehaviors.ReplaceCurrentPlayList:
                default:
                    ScopedLogger.Warn("错误的添加行为: {Behavior}", behavior);
                    return;
            }
            ScopedLogger.Info("已将音频添加到播放列表: {CreateTitle}，行为: {Behavior}", audioInfoData.Title, behavior);
        }
        else
            ScopedLogger.Warn("获取Cookies失败，无法加载音频");
    }

    [RelayCommand]
    private void OpenAuthorSpaceUrl(ulong mid)
    {
        launcher.LaunchUriAsync(new Uri($"https://space.bilibili.com/{mid}", UriKind.Absolute));
        ScopedLogger.Info("已打开作者空间链接: https://space.bilibili.com/{Mid}", mid);
    }

    public void OnNavigatedTo()
    {
    }
}