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
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using ObservableCollections;

namespace KanaPlayer.ViewModels.Pages;

public partial class HomeViewModel(IBilibiliClient bilibiliClient, IPlayerManager playerManager,
                                   ILauncher launcher, IFavoritesManager favoritesManager, IConfigurationService<SettingsModel> configurationService)
    : ViewModelBase, INavigationAware
{
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<AudioRegionFeedDataInfoModel> MusicRegionFeeds
        => field ??= _musicRegionFeeds.ToNotifyCollectionChangedSlim();

    private readonly ObservableList<AudioRegionFeedDataInfoModel> _musicRegionFeeds = [];

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _musicRegionFeeds.Clear();
        await LoadMoreAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        bilibiliClient.TryGetCookies(out var cookies);
        var feeds = await bilibiliClient.GetAudioRegionFeedAsync(cookies);
        if (feeds.Data is null)
            throw new Exception("获取音乐分区动态失败，数据为空。请检查网络连接或B站服务状态。");
        _musicRegionFeeds.AddRange(feeds.Data.Archives);
    }

    [RelayCommand]
    private async Task LoadAudioAsync(AudioRegionFeedDataInfoModel audioRegionFeedDataInfoModel)
    {
        bilibiliClient.TryGetCookies(out var cookies);

        var uniqueId = new AudioUniqueId(audioRegionFeedDataInfoModel.Bvid);
        var cachedAudioMetadata = favoritesManager.GetCachedAudioMetadataByUniqueId(uniqueId);

        AudioInfoDataModel audioInfoData;
        if (cachedAudioMetadata is null)
        {
            var audioInfo = await bilibiliClient.GetAudioInfoAsync(uniqueId, cookies);
            audioInfoData = audioInfo.EnsureData();
            favoritesManager.AddOrUpdateAudioToCache(uniqueId, audioInfoData);
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
        switch (configurationService.Settings.UiSettings.Behaviors.HomeAddBehavior)
        {
            case FavoritesAddBehaviors.AddToNextInPlayList:
                await playerManager.InsertAfterCurrentPlayItemAsync(playItem);
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
                await playerManager.AppendAsync(playItem);
                break;
            case FavoritesAddBehaviors.AddToNextAndPlayInPlayList:
                await playerManager.InsertAfterCurrentPlayItemAsync(playItem);
                Task.Run(async () =>
                {
                    await playerManager.LoadAsync(playItem);
                    playerManager.Play();
                }).Detach();
                break;
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [RelayCommand]
    private void OpenAuthorSpaceUrl(ulong mid)
        => launcher.LaunchUriAsync(new Uri($"https://space.bilibili.com/{mid}", UriKind.Absolute));

    public void OnNavigatedTo()
    {
    }
}