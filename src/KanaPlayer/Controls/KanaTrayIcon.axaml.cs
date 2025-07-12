using System;
using Avalonia;
using Avalonia.Controls;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Services.TrayMenu;
using KanaPlayer.Views;

namespace KanaPlayer.Controls;

public partial class KanaTrayIcon : TrayIcon
{
    public ITrayMenuService? TrayMenuService { get; set; }
    
    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        var mainWindow = App.GetService<MainWindow>();

        if (mainWindow.IsVisible == false)
            mainWindow.Show();
        else if (mainWindow.WindowState == WindowState.Minimized)
            mainWindow.WindowState = WindowState.Normal;
        mainWindow.Focus();
    }
    
    private void Exit_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => Environment.Exit(0);

    private void PlaybackMode_RepeatAll_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.RepeatAll);
    private void PlaybackMode_RepeatOne_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.RepeatOne);
    private void PlaybackMode_Shuffle_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.Shuffle);
    private void PlaybackMode_Sequential_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.Sequential);
}