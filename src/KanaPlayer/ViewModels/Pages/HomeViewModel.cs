using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using ObservableCollections;

namespace KanaPlayer.ViewModels.Pages;

public partial class HomeViewModel(
    IBilibiliClient bilibiliClient,
    IConfigurationService<SettingsModel> configurationService) : ViewModelBase, INavigationAware
{
    [field: AllowNull, MaybeNull]
    public NotifyCollectionChangedSynchronizedViewList<MusicRegionFeedDataListModel> MusicRegionFeeds =>
        field ??= _musicRegionFeeds.ToNotifyCollectionChangedSlim();
    
    private readonly ObservableList<MusicRegionFeedDataListModel> _musicRegionFeeds = [];

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _musicRegionFeeds.Clear();
        await LoadMoreAsync();
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        var authentication = configurationService.Settings.CommonSettings.Authentication;
        var cookies = bilibiliClient.IsAuthenticated && authentication is not null ? authentication.Cookies : [];
        var feeds = await bilibiliClient.GetMusicRegionFeedAsync(cookies);
        if (feeds.Data is null)
            throw new Exception("获取音乐分区动态失败，数据为空。请检查网络连接或B站服务状态。");
        _musicRegionFeeds.AddRange(feeds.Data.Archives);
    }
    
    public async void OnNavigatedTo()
    {
        try
        {
            if (MusicRegionFeeds.Count > 0)
                return;
            await LoadMoreAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}