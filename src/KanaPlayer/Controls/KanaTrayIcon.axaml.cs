using System;
using Avalonia;
using Avalonia.Controls;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Services.TrayMenu;
using KanaPlayer.Views;

namespace KanaPlayer.Controls;

public class NamedTrayIcon : TrayIcon
{
    public static readonly StyledProperty<string?> NameProperty = AvaloniaProperty.Register<NamedTrayIcon, string?>(nameof(Name));

    public string? Name
    {
        get => GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }
}

public class KanaTrayIcon : NamedTrayIcon
{
    public static readonly StyledProperty<IPlayerManager?> PlayerManagerProperty = AvaloniaProperty.Register<KanaTrayIcon, IPlayerManager?>(
        nameof(PlayerManager));

    public IPlayerManager? PlayerManager
    {
        get => GetValue(PlayerManagerProperty);
        set => SetValue(PlayerManagerProperty, value);
    }
    
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
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.RepeatAll, true);
    private void PlaybackMode_RepeatOne_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.RepeatOne, true);
    private void PlaybackMode_Shuffle_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.Shuffle, true);
    private void PlaybackMode_Sequential_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.SwitchPlaybackMode(PlaybackMode.Sequential, true);
    private void PlayOrPause_NativeMenuItem_OnClick(object? sender, EventArgs e)
        => TrayMenuService?.TogglePlayStatus();
    private async void LoadPrevious_NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        try
        {
            await TrayMenuService?.LoadPlayPreviousAsync()!;
        }
        catch (Exception)
        {
            // TODO: Handle error loading previous
            Console.WriteLine("[NativeMenu] Error loading previous");
        }
    }
    private async void LoadForward_NativeMenuItem_OnClick(object? sender, EventArgs e)
    {
        try
        {
            await TrayMenuService?.LoadPlayForwardAsync()!;
        }
        catch (Exception)
        {
            // TODO: Handle error loading forward
            Console.WriteLine("[NativeMenu] Error loading forward");
        }
    }
}