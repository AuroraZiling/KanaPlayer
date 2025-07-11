using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Player;

namespace KanaPlayer.ViewModels.Pages;

public partial class PlayListViewModel : ViewModelBase
{
    [ObservableProperty] public partial PlayListItemModel SelectedPlayListItem { get; set; }
    readonly List<string> _bvids = ["BV1oVGHzTEjN", "BV1kv3RzTEaB", "BV17d3LznEW6", "BV1xa33znEGD", "BV1pgKnzZEze"];

    [RelayCommand]
    private async Task PlaySelectedItemAsync()
    {
        await PlayerManager.LoadAsync(SelectedPlayListItem);
        PlayerManager.Play();
    }

    public PlayListViewModel(IPlayerManager playerManager, IBilibiliClient bilibiliClient)
    {
        PlayerManager = playerManager;
        BilibiliClient = bilibiliClient;
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        foreach (var bvid in _bvids)
        {
            var audioInfo = await BilibiliClient.GetAudioInfoAsync(bvid, BilibiliClient.TryGetCookies(out var cookies) ? cookies : new Dictionary<string, string>());
            var audioInfoData = audioInfo.EnsureData();
            PlayerManager.Append(new PlayListItemModel(
                audioInfoData.Title,
                audioInfoData.CoverUrl,
                audioInfoData.Owner.Name,
                audioInfoData.Owner.Mid,
                bvid,
                TimeSpan.FromSeconds(audioInfoData.DurationSeconds)
            ));
        }
    }

    public IPlayerManager PlayerManager { get; }
    public IBilibiliClient BilibiliClient { get; }
}