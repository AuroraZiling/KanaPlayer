using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Models.Database;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Favorites;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using KanaPlayer.Views.Pages.SubPages;

namespace KanaPlayer.ViewModels.Pages;

public partial class FavoritesViewModel(INavigationService navigationService, IFavoritesManager favoritesManager, IPlayerManager playerManager,
                                        IConfigurationService<SettingsModel> configurationService, IKanaDialogManager kanaDialogManager)
    : ViewModelBase, INavigationAware
{
    [RelayCommand]
    private void RefreshFavoriteFolders()
    {
        FavoriteFolders = new ObservableCollection<LocalFavoriteFolderItem>(favoritesManager.GetLocalFavoriteFolders());
    }

    [ObservableProperty] public partial ObservableCollection<LocalFavoriteFolderItem> FavoriteFolders { get; set; } = [];
    [ObservableProperty] public partial LocalFavoriteFolderItem? SelectedFavoriteFolder { get; set; }
    partial void OnSelectedFavoriteFolderChanged(LocalFavoriteFolderItem? value)
    {
        if (value is null)
            return;
        FavoriteFolderItems = new ObservableCollection<CachedAudioMetadata>(favoritesManager.GetCachedAudioMetadataList(value));
    }
    [ObservableProperty] public partial ObservableCollection<CachedAudioMetadata> FavoriteFolderItems { get; set; } = [];
    [ObservableProperty] public partial CachedAudioMetadata? SelectedPlayListItem { get; set; }

    [RelayCommand]
    private async Task DoubleTappedSelectedItemAsync()
    {
        if (SelectedFavoriteFolder is null || SelectedPlayListItem is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior;
        var selectedPlayListItem = new PlayListItem(SelectedPlayListItem.Title, SelectedPlayListItem.CoverUrl, SelectedPlayListItem.OwnerName,
            SelectedPlayListItem.OwnerMid, SelectedPlayListItem.UniqueId, TimeSpan.FromSeconds(SelectedPlayListItem.DurationSeconds));
        switch (behavior)
        {
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            {
                playerManager.Clear();
                foreach (var cachedAudioMetadata in favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder))
                {
                    await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.AddToNextInPlayList:
                await playerManager.InsertAfterCurrentPlayItemAsync(new PlayListItem(SelectedPlayListItem.Title, SelectedPlayListItem.CoverUrl, SelectedPlayListItem.OwnerName,
                    SelectedPlayListItem.OwnerMid, SelectedPlayListItem.UniqueId, TimeSpan.FromSeconds(SelectedPlayListItem.DurationSeconds)));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
                await playerManager.AppendAsync(new PlayListItem(SelectedPlayListItem.Title, SelectedPlayListItem.CoverUrl, SelectedPlayListItem.OwnerName,
                    SelectedPlayListItem.OwnerMid, SelectedPlayListItem.UniqueId, TimeSpan.FromSeconds(SelectedPlayListItem.DurationSeconds)));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        await playerManager.LoadAsync(selectedPlayListItem);
        playerManager.Play();
    }

    [RelayCommand]
    private void ImportFromBilibili()
    {
        navigationService.Navigate(App.GetService<FavoritesBilibiliImportView>());
    }

    [RelayCommand]
    private async Task PlayAllAsync()
    {
        if (SelectedFavoriteFolder is null)
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
        foreach (var cachedAudioMetadata in favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder))
        {
            await playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
        }
        await playerManager.LoadFirstAsync();
        playerManager.Play();
    }

    [RelayCommand]
    private void AddAllToPlayList()
    {
        if (SelectedFavoriteFolder is null)
            return;

        var behavior = configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;
        switch (behavior)
        {
            case FavoritesAddBehaviors.AddToNextInPlayList:
                playerManager.InsertAfterCurrentPlayItemRangeAsync(favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder).Select(cachedAudioMetadata =>
                    new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds))));
                break;
            case FavoritesAddBehaviors.AddToEndOfPlayList:
            {
                foreach (var cachedAudioMetadata in favoritesManager.GetCachedAudioMetadataList(SelectedFavoriteFolder))
                {
                    playerManager.AppendAsync(new PlayListItem(cachedAudioMetadata.Title, cachedAudioMetadata.CoverUrl, cachedAudioMetadata.OwnerName,
                        cachedAudioMetadata.OwnerMid, cachedAudioMetadata.UniqueId, TimeSpan.FromSeconds(cachedAudioMetadata.DurationSeconds)));
                }
                break;
            }
            case FavoritesAddBehaviors.ReplaceCurrentPlayList:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void OnNavigatedTo()
    {
        RefreshFavoriteFoldersCommand.Execute(null);
    }
}