using System;
using Avalonia.Controls;
using Avalonia.Input;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Models;
using KanaPlayer.Models.SettingTypes;
using KanaPlayer.ViewModels;

namespace KanaPlayer.Views;

public partial class MainWindow : Window
{
    private readonly IConfigurationService<SettingsModel> _configurationService;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.GetService<MainWindowViewModel>();
        _configurationService = App.GetService<IConfigurationService<SettingsModel>>();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Position.Y <= 60)
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
        switch (_configurationService.Settings.UiSettings.Behaviors.CloseBehavior)
        {
            case CloseBehaviors.Minimize:
                WindowState = WindowState.Minimized;
                break;
            case CloseBehaviors.TrayMenu:
                Hide();
                break;
            case CloseBehaviors.Close:
            default:
                App.Shutdown();
                break;
        }
    }
}