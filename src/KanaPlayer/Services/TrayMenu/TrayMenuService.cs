using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using KanaPlayer.Controls;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Extensions;

namespace KanaPlayer.Services.TrayMenu;

public class TrayMenuService : ITrayMenuService
{
    private Application? CurrentApp { get; init; } = Application.Current;

    public TrayMenuService()
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        kanaTrayIcon.TrayMenuService = this;
    }

    [MemberNotNull(nameof(CurrentApp))]
    private TrayIcons GetRootTrayIcons()
    {
        if (CurrentApp is null)
            throw new InvalidOperationException("Application instance is not available.");
        var trayIcons = TrayIcon.GetIcons(CurrentApp);
        if (trayIcons is null || trayIcons.Count == 0)
            throw new InvalidOperationException("No tray icons found in the application.");
        return trayIcons;
    }
    
    private KanaTrayIcon GetKanaTrayIcon()
    {
        var trayIcons = GetRootTrayIcons();
        return trayIcons[0].NotNull<KanaTrayIcon>();
    }

    private void ApplyChanges(KanaTrayIcon trayIcon)
    {
        var rootTrayIcons = GetRootTrayIcons();
        rootTrayIcons[0] = trayIcon;
        TrayIcon.SetIcons(CurrentApp, rootTrayIcons);
    }

    public void ChangeTooltipText(string toolTipText)
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        kanaTrayIcon.ToolTipText = toolTipText;
        ApplyChanges(kanaTrayIcon);
    }

    public void SwitchPlaybackMode(PlaybackMode playbackMode)
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        var innerNativeMenuItems = kanaTrayIcon.Menu.EnsureNativeMenu().Items;
        foreach (var innerNativeMenuItem in innerNativeMenuItems)
        {
            if (innerNativeMenuItem is NativeMenuItem nativeMenuItem)
            {
                if (nativeMenuItem.Header is not null &&
                    (nativeMenuItem.Header == "播放模式" || nativeMenuItem.Header.IsStringValidPlaybackMode()))
                {
                    nativeMenuItem.Header = playbackMode.ToDisplayString();
                }
            }
        }
        ApplyChanges(kanaTrayIcon);
    }
}

file static class TrayMenuServiceExtensions
{
    internal static NativeMenu EnsureNativeMenu(this NativeMenu? menu)
    {
        if (menu is null)
            throw new InvalidOperationException("NativeMenu cannot be null.");
        return menu;
    }
}