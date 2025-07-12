using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using KanaPlayer.Controls;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
using KanaPlayer.Extensions;
using KanaPlayer.Models;

namespace KanaPlayer.Services.TrayMenu;

public class TrayMenuService : ITrayMenuService
{
    private Application? CurrentApp { get; } = Application.Current;
    private readonly IConfigurationService<SettingsModel> _configurationService;
    private readonly IPlayerManager _playerManager;

    public TrayMenuService(IPlayerManager playerManager, IConfigurationService<SettingsModel> configurationService)
    {
        _configurationService = configurationService;
        _playerManager = playerManager;
        
        GetKanaTrayIcon().TrayMenuService = this;
        
        _playerManager.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(IPlayerManager.PlaybackMode))  // MainViewModel Switch Listening
                SwitchPlaybackMode(_playerManager.PlaybackMode, false);
            else if (args.PropertyName == nameof(IPlayerManager.CanLoadPrevious))
                ChangeCanPrevious(_playerManager.CanLoadPrevious);
            else if (args.PropertyName == nameof(IPlayerManager.CanLoadForward))
                ChangeCanForward(_playerManager.CanLoadForward);
        };
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

    public void SwitchPlaybackMode(PlaybackMode playbackMode, bool saveConfiguration)
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        var innerNativeMenuItems = kanaTrayIcon.Menu.EnsureNativeMenu().Items;
        foreach (var nativeMenuItem in innerNativeMenuItems.OfType<NativeMenuItem>())
        {
            if (nativeMenuItem.Header is not null &&
                (nativeMenuItem.Header == "播放模式" || nativeMenuItem.Header.IsStringValidPlaybackMode()))
            {
                nativeMenuItem.Header = playbackMode.ToDisplayString();
            }
        }
        _playerManager.PlaybackMode = playbackMode;
        ApplyChanges(kanaTrayIcon);

        if (saveConfiguration)
        {
            _configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode = playbackMode;
            _configurationService.Save();
        }
    }
    
    public void ChangeCanPrevious(bool canPrevious)
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        var innerNativeMenuItems = kanaTrayIcon.Menu.EnsureNativeMenu().Items;
        foreach (var nativeMenuItem in innerNativeMenuItems.OfType<NativeMenuItem>())
        {
            if (nativeMenuItem.Header is not null && nativeMenuItem.Header == "上一首")
            {
                nativeMenuItem.IsEnabled = canPrevious;
            }
        }
        ApplyChanges(kanaTrayIcon);
    }
    
    public void ChangeCanForward(bool canNext)
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        var innerNativeMenuItems = kanaTrayIcon.Menu.EnsureNativeMenu().Items;
        foreach (var nativeMenuItem in innerNativeMenuItems.OfType<NativeMenuItem>())
        {
            if (nativeMenuItem.Header is not null && nativeMenuItem.Header == "下一首")
            {
                nativeMenuItem.IsEnabled = canNext;
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