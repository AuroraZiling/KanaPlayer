using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using ObservableCollections;

namespace KanaPlayer.ViewModels.Pages;

public partial class HomeViewModel(
    IBilibiliClient bilibiliClient,
    IAudioPlayer audioPlayer,
    ILauncher launcher) : ViewModelBase, INavigationAware
{
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<AudioRegionFeedDataInfoModel> MusicRegionFeeds =>
        field ??= _musicRegionFeeds.ToNotifyCollectionChangedSlim();
    
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
    private async Task StartPlayingMusicAsync(AudioRegionFeedDataInfoModel audioRegionFeedDataInfoModel)
    {
        bilibiliClient.TryGetCookies(out var cookies);
        var musicStream = await bilibiliClient.GetAudioStreamAsync(audioRegionFeedDataInfoModel.Bvid, cookies);
        audioPlayer.Load(musicStream);
        audioPlayer.Play();
    }

    [RelayCommand]
    private void OpenAuthorSpaceUrl(ulong mid)
        =>  launcher.LaunchUriAsync(new Uri($"https://space.bilibili.com/{mid}", UriKind.Absolute));
    
    public void OnNavigatedTo()
    {
    }
}