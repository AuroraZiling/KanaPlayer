using System;
using Avalonia.Controls;
using Avalonia.Input;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using KanaPlayer.Services.TrayMenu;
using KanaPlayer.ViewModels;

namespace KanaPlayer.Views;

public partial class MainWindow : Window
{
    private readonly IConfigurationService<SettingsModel> _configurationService;

    public MainWindow(MainWindowViewModel mainWindowViewModel, MainView mainView, IConfigurationService<SettingsModel> configurationService)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
        _configurationService = configurationService;

        mainView.SetValue(Grid.RowProperty, 1);
        MainWindowGrid.Children.Add(mainView);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Position.Y <= 50)
            BeginMoveDrag(e);
    }

    private void KanaWindowTitleBar_OnMinimizeButtonClick(object? sender, EventArgs e)
        => WindowState = WindowState.Minimized;

    private void KanaWindowTitleBar_OnMaximizeButtonClick(object? sender, EventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void KanaWindowTitleBar_OnCloseButtonClick(object? sender, EventArgs e)
    {
        switch (_configurationService.Settings.UiSettings.CloseBehavior)
        {
            case CloseBehaviors.Minimize:
                WindowState = WindowState.Minimized;
                break;
            case CloseBehaviors.TrayMenu:
                Hide();
                break;
            case CloseBehaviors.Close:
            default:
                Environment.Exit(0);
                break;
        }
    }
}