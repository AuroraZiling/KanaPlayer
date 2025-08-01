﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KanaPlayer.Controls.Hosts;
using KanaPlayer.Controls.Navigation;
using KanaPlayer.Core.Services;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Views.Pages;
using NLog;

namespace KanaPlayer.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private static readonly Logger ScopedLogger = LogManager.GetLogger(nameof(MainWindowViewModel));
    public IKanaToastManager ToastManager { get; }
    public IKanaDialogManager DialogManager { get; }
    [ObservableProperty] public partial bool IsLoggedIn { get; set; } = false;
    [ObservableProperty] public partial string? UserName { get; set; } = null;
    [ObservableProperty] public partial string? AvatarUrl { get; set; } = null;
    
    public bool IsSettingsChecked => _navigationService.CurrentPage is SettingsView;
    public bool IsAccountChecked => _navigationService.CurrentPage is AccountView;
    
    private readonly INavigationService _navigationService;
    private readonly IConfigurationService<SettingsModel> _configurationService;
    
    public MainWindowViewModel(INavigationService navigationService, IConfigurationService<SettingsModel> configurationService, IBilibiliClient bilibiliClient, IKanaToastManager toastManager, IKanaDialogManager dialogManager)
    {
        _navigationService = navigationService;
        _configurationService = configurationService;
        ToastManager = toastManager;
        DialogManager = dialogManager;
        
        OnAuthenticationStatusChanged(bilibiliClient.IsAuthenticated);
        bilibiliClient.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IBilibiliClient.IsAuthenticated))
                OnAuthenticationStatusChanged(bilibiliClient.IsAuthenticated);
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
            ScopedLogger.Info("用户已登录: {UserName}", UserName);
        }
        else
        {
            UserName = null;
            AvatarUrl = null;
            ScopedLogger.Info("用户未登录或账户信息缺失");
            _navigationService.Navigate(typeof(AccountView));
        }
    }
    
    [RelayCommand]
    private void NavigateToSettings()
        => _navigationService.Navigate(typeof(SettingsView));
    
    [RelayCommand]
    private void NavigateToAccount()
        => _navigationService.Navigate(typeof(AccountView));
}