using System;
using System.IO;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Database;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;

namespace KanaPlayer.ViewModels.Pages;

public partial class SettingsViewModel(IConfigurationService<SettingsModel> configurationService, ILauncher launcher) : ViewModelBase
{
    [RelayCommand]
    private void Test()
    {
        // var kanaToastManager = App.GetService<IKanaToastManager>();
        // var kanaDialogManager = App.GetService<IKanaDialogManager>();
        // var bilibiliClient = App.GetService<IBilibiliClient>();
        // var mainDbContext = App.GetService<MainDbContext>();
        // var navigationService = App.GetService<INavigationService>();
        // var fakeImportItem = new FavoriteFolderItem
        // {
        //     Id = 3463008729,
        //     Title = "叮咚鸡精选集",
        //     CoverUrl = "http://i1.hdslb.com/bfs/archive/82c1f7360650a26215467bfe83227f24e588e218.jpg",
        //     Description = "大狗大狗叫叫叫",
        //     Owner = new CommonOwnerModel
        //     {
        //         Mid = 12879829,
        //         Name = "DearVa",
        //     },
        //     FavoriteType = FavoriteType.Folder | FavoriteType.Collected,
        //     CreatedTimestamp = 1744221085,
        //     ModifiedTimestamp = 1744221085,
        //     MediaCount = 7
        // };
        // kanaDialogManager.CreateDialog()
        //                  .WithView(new FavoritesBilibiliImportDialog())
        //                  .WithViewModel(dialog => new FavoritesBilibiliImportDialogViewModel(dialog, fakeImportItem, bilibiliClient, mainDbContext, kanaToastManager, navigationService))
        //                  .TryShow();
    }

    #region Behaviors

    // Close Button Behavior
    [ObservableProperty] public partial CloseBehaviors SelectedCloseBehavior { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.CloseBehavior;

    partial void OnSelectedCloseBehaviorChanged(CloseBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.CloseBehavior = value;
        configurationService.Save();
    }

    // Favorites - Play All Button Warning
    [ObservableProperty] public partial bool IsFavoritesPlayAllReplaceWarningEnabled { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.IsFavoritesPlayAllReplaceWarningEnabled;

    partial void OnIsFavoritesPlayAllReplaceWarningEnabledChanged(bool value)
    {
        configurationService.Settings.UiSettings.Behaviors.IsFavoritesPlayAllReplaceWarningEnabled = value;
        configurationService.Save();
    }

    // Home - Add Behavior
    [ObservableProperty] public partial FavoritesAddBehaviors HomeAddBehaviors { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.HomeAddBehavior;

    [RelayCommand]
    private void ChangeHomeAddBehaviors(FavoritesAddBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.HomeAddBehavior = value;
        configurationService.Save();
    }

    // Favorites - DoubleTapped PlayListItem Behavior
    [ObservableProperty] public partial FavoritesAddBehaviors DoubleTappedPlayListItemBehaviors { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior;

    [RelayCommand]
    private void ChangeDoubleTappedPlayListItemBehaviors(FavoritesAddBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior = value;
        configurationService.Save();
    }

    // Favorites - Add All Behavior
    [ObservableProperty] public partial FavoritesAddBehaviors FavoritesAddAllBehaviors { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;

    [RelayCommand]
    private void ChangeFavoritesAddAllBehaviors(FavoritesAddBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior = value;
        configurationService.Save();
    }

    #endregion

    #region Cache

    // Maximum Audio Cache Size (256MB - 10240MB)
    [ObservableProperty]
    public partial int MaximumAudioCacheSizeInMb { get; set; } =
        configurationService.Settings.CommonSettings.AudioCache.MaximumCacheSizeInMb;

    partial void OnMaximumAudioCacheSizeInMbChanged(int value)
    {
        configurationService.Settings.CommonSettings.AudioCache.MaximumCacheSizeInMb = value;
        configurationService.Save();
    }

    // Maximum Image Cache Size (128MB - 5120MB)
    [ObservableProperty]
    public partial int MaximumImageCacheSizeInMb { get; set; } =
        configurationService.Settings.CommonSettings.ImageCache.MaximumCacheSizeInMb;

    partial void OnMaximumImageCacheSizeInMbChanged(int value)
    {
        configurationService.Settings.CommonSettings.ImageCache.MaximumCacheSizeInMb = value;
        configurationService.Save();
    }

    // Manual Cache Cleanup
    [RelayCommand]
    private void CleanupCache(string cacheType)
        => App.CleanupCache(cacheType switch
        {
            "audio" => AppHelper.ApplicationAudioCachesFolderPath,
            "image" => AppHelper.ApplicationImageCachesFolderPath,
            _       => throw new ArgumentException("Invalid cache type specified.")
        }, 0);

    #endregion

    #region About

    [RelayCommand]
    private void RevealFolder(string folderType)
    {
        var folderPath = folderType switch
        {
            "app" => AppHelper.ApplicationFolderPath,
            "data" => AppHelper.ApplicationDataFolderPath,
            _       => throw new ArgumentException("Invalid folder type specified.")
        };
        launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(folderPath))
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine($"Failed to open folder: {task.Exception?.Message}");
                }
            });
    }

    #endregion
}