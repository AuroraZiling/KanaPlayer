using CommunityToolkit.Mvvm.ComponentModel;
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

    #endregion
}