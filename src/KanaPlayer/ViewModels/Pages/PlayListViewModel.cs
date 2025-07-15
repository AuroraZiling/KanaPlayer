using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Player;

namespace KanaPlayer.ViewModels.Pages;

public partial class PlayListViewModel : ViewModelBase
{
    [ObservableProperty] public partial int SelectedPlayListItemIndex { get; set; }
    readonly List<string> _bvId = [
        "BV1NANWzTEyS",
        "BV1F8EDz6EFg",
        "BV1e5jGzJEKC",
        "BV1g4moYBE4M",
        "BV1ioPAeKEuT",
        "BV1ShaUeZEWL",
        "BV1Xz421z7WQ",
        "BV1vX9qY6EwV",
        "BV1fCQoYhE8q",
        "BV1pgPaecENk",
        "BV17qobY8EVo",
    ];

    [RelayCommand]
    private async Task PlaySelectedItemAsync()
    {
        await PlayerManager.LoadAsync(PlayerManager.PlayList[SelectedPlayListItemIndex]);
        PlayerManager.Play();
    }

    public IPlayerManager PlayerManager { get; }
    private readonly IBilibiliClient _bilibiliClient;
    public PlayListViewModel(IPlayerManager playerManager, IBilibiliClient bilibiliClient)
    {
        PlayerManager = playerManager;
        _bilibiliClient = bilibiliClient;
        
        _ = InitializeAsync();
        playerManager.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IPlayerManager.CurrentPlayListItem))
            {
                SelectedPlayListItemIndex = playerManager.CurrentPlayListItem is null ? -1 : playerManager.IndexOf(playerManager.CurrentPlayListItem);
            }
        };
    }
    
    private async Task InitializeAsync()
    {
        foreach (var bvid in _bvId)
        {
            var audioUniqueId = new AudioUniqueId(bvid);
            var audioInfo = await _bilibiliClient.GetAudioInfoAsync(audioUniqueId, _bilibiliClient.TryGetCookies(out var cookies) ? cookies : new Dictionary<string, string>());
            var audioInfoData = audioInfo.EnsureData();
            PlayerManager.Append(new PlayListItem(
                audioInfoData.Title,
                audioInfoData.CoverUrl,
                audioInfoData.Owner.Name,
                audioInfoData.Owner.Mid,
                audioUniqueId,
                TimeSpan.FromSeconds(audioInfoData.DurationSeconds)
            ));
        }
    }
}