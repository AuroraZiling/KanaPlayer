using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Views.Pages;

namespace KanaPlayer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] public partial bool IsLoggedIn { get; set; } = false;
    [ObservableProperty] public partial string? UserName { get; set; } = null;
    [ObservableProperty] public partial string? AvatarUrl { get; set; } = null;
    
    public bool IsSettingsChecked => _navigationService.CurrentPage is SettingsView;
    public bool IsAccountChecked => _navigationService.CurrentPage is AccountView;
    
    private readonly INavigationService _navigationService;
    private readonly IConfigurationService<SettingsModel> _configurationService;
    
    public MainWindowViewModel(INavigationService navigationService, IConfigurationService<SettingsModel> configurationService, IBilibiliClient bilibiliClient)
    {
        _navigationService = navigationService;
        _configurationService = configurationService;
        OnAuthenticationStatusChanged(bilibiliClient.IsAuthenticated);
        bilibiliClient.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBilibiliClient.IsAuthenticated))
            {
                OnAuthenticationStatusChanged(bilibiliClient.IsAuthenticated);
            }
        };
        navigationService.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(INavigationService.CurrentPage))
            {
                OnPropertyChanged(nameof(IsSettingsChecked));
                OnPropertyChanged(nameof(IsAccountChecked));
            }
        };
    }

    private void OnAuthenticationStatusChanged(bool newStatus)
    {
        IsLoggedIn = newStatus && _configurationService.Settings.CommonSettings.Account is not null;
        if (newStatus && _configurationService.Settings.CommonSettings.Account is not null)
        {
            UserName = _configurationService.Settings.CommonSettings.Account.UserName;
            AvatarUrl = _configurationService.Settings.CommonSettings.Account.AvatarImgUri;
        }
        else
        {
            UserName = null;
            AvatarUrl = null;
        }
    }
    
    [RelayCommand]
    private void NavigateToSettings()
        => _navigationService.Navigate(typeof(SettingsView));
    
    [RelayCommand]
    private void NavigateToAccount()
        => _navigationService.Navigate(typeof(AccountView));
}