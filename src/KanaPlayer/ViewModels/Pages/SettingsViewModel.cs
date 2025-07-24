using System;
using System.IO;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using NLog;

namespace KanaPlayer.ViewModels.Pages;

public partial class SettingsViewModel(IConfigurationService<SettingsModel> configurationService, ILauncher launcher) : ViewModelBase, IDisposable
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(SettingsViewModel));
    
    [RelayCommand]
    private void Test()
    {
    }

    #region Behaviors

    // Close Button Behavior
    [ObservableProperty] public partial CloseBehaviors SelectedCloseBehavior { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.CloseBehavior;

    partial void OnSelectedCloseBehaviorChanged(CloseBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.CloseBehavior = value;
        configurationService.SaveImmediate();
        ScopedLogger.Info($"窗口关闭按钮行为变更为: {value}");
    }

    // Favorites - Play All Button Warning
    [ObservableProperty] public partial bool IsFavoritesPlayAllReplaceWarningEnabled { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.IsFavoritesPlayAllReplaceWarningEnabled;

    partial void OnIsFavoritesPlayAllReplaceWarningEnabledChanged(bool value)
    {
        configurationService.Settings.UiSettings.Behaviors.IsFavoritesPlayAllReplaceWarningEnabled = value;
        configurationService.SaveImmediate();
        ScopedLogger.Info($"收藏夹播放全部警告已设置为: {value}");
    }

    // Home - Add Behavior
    [ObservableProperty] public partial FavoritesAddBehaviors HomeAddBehaviors { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.HomeAddBehavior;

    [RelayCommand]
    private void ChangeHomeAddBehaviors(FavoritesAddBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.HomeAddBehavior = value;
        configurationService.SaveImmediate();
        ScopedLogger.Info($"主页添加单曲行为变更为: {value}");
    }

    // Favorites - DoubleTapped PlayListItem Behavior
    [ObservableProperty] public partial FavoritesAddBehaviors DoubleTappedPlayListItemBehaviors { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior;

    [RelayCommand]
    private void ChangeDoubleTappedPlayListItemBehaviors(FavoritesAddBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.FavoritesDoubleTappedPlayListItemBehavior = value;
        configurationService.SaveImmediate();
        ScopedLogger.Info($"收藏夹双击列表项行为变更为: {value}");
    }

    // Favorites - Add All Behavior
    [ObservableProperty] public partial FavoritesAddBehaviors FavoritesAddAllBehaviors { get; set; } =
        configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior;

    [RelayCommand]
    private void ChangeFavoritesAddAllBehaviors(FavoritesAddBehaviors value)
    {
        configurationService.Settings.UiSettings.Behaviors.FavoritesAddAllBehavior = value;
        configurationService.SaveImmediate();
        ScopedLogger.Info($"收藏夹添加全部行为变更为: {value}");
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
        configurationService.SaveDelayed();
    }

    // Maximum Image Cache Size (128MB - 5120MB)
    [ObservableProperty]
    public partial int MaximumImageCacheSizeInMb { get; set; } =
        configurationService.Settings.CommonSettings.ImageCache.MaximumCacheSizeInMb;

    partial void OnMaximumImageCacheSizeInMbChanged(int value)
    {
        configurationService.Settings.CommonSettings.ImageCache.MaximumCacheSizeInMb = value;
        configurationService.SaveDelayed();
    }

    // Manual Cache Cleanup
    [RelayCommand]
    private static void CleanupCache(string cacheType)
    {
        App.CleanupCache(cacheType switch
        {
            "audio" => AppHelper.ApplicationAudioCachesFolderPath,
            "image" => AppHelper.ApplicationImageCachesFolderPath,
            _       => throw new ArgumentException("Invalid cache type specified.")
        }, 0);
        ScopedLogger.Info($"已清理 {cacheType} 缓存。");
    }

    #endregion

    #region About

    public string? AppVersion => AppHelper.ApplicationVersion;

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
                    ScopedLogger.Error(task.Exception, $"无法打开 {folderType} 文件夹: {folderPath}");
                else
                    ScopedLogger.Info($"已成功打开 {folderType} 文件夹: {folderPath}");
            });
    }

    #endregion

    public void Dispose()
    {
        configurationService.SaveImmediate();
        ScopedLogger.Debug("SettingsViewModel 已被释放，所有设置已保存");
        GC.SuppressFinalize(this);
    }
}