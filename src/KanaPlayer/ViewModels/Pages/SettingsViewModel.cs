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
}