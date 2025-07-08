using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;

namespace KanaPlayer.ViewModels.Pages;

public partial class SettingsViewModel(IConfigurationService<SettingsModel> configurationService): ViewModelBase
{
    #region General

    // Close Button Behavior
    [ObservableProperty] public partial CloseBehaviors SelectedCloseBehavior { get; set; } = 
        configurationService.Settings.UiSettings.CloseBehavior;

    partial void OnSelectedCloseBehaviorChanged(CloseBehaviors value)
    {
        configurationService.Settings.UiSettings.CloseBehavior = value;
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
    private static void CleanupCache(string cacheType)
        => App.CleanupCache(cacheType switch
        {
            "audio" => AppHelper.ApplicationAudioCachesFolderPath,
            "image" => AppHelper.ApplicationImageCachesFolderPath,
            _ => throw new ArgumentException("Invalid cache type specified.")
        }, 0);

    #endregion
}