using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using KanaPlayer.Controls;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Services.Configuration;
using KanaPlayer.Core.Services.Player;
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

        GetKanaTrayIcon().PlayerManager = playerManager;
        GetKanaTrayIcon().TrayMenuService = this;
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

    public void SwitchPlaybackMode(PlaybackMode playbackMode, bool save)
    {
        var kanaTrayIcon = GetKanaTrayIcon();
        var innerNativeMenuItems = kanaTrayIcon.Menu.EnsureNativeMenu().Items;

        foreach (var nativeMenuItem in innerNativeMenuItems.OfType<NativeMenuItem>())
            if (nativeMenuItem.CommandParameter is "PlaybackMode")
                nativeMenuItem.Header = playbackMode.ToFriendlyString();

        _playerManager.PlaybackMode = playbackMode;
        ApplyChanges(kanaTrayIcon);

        if (save)
        {
            _configurationService.Settings.CommonSettings.BehaviorHistory.PlaybackMode = playbackMode;
            _configurationService.SaveImmediate();
        }
    }

    public void TogglePlayStatus()
    {
        if (_playerManager.Status == PlayStatus.Playing)
            _playerManager.Pause();
        else
            _playerManager.Play();
    }

    public async Task LoadPlayPreviousAsync()
        => await _playerManager.LoadPrevious(true, true);

    public async Task LoadPlayForwardAsync()
        => await _playerManager.LoadForward(true, true);
}

file static class TrayMenuServiceExtensions
{
    internal static NativeMenu EnsureNativeMenu(this NativeMenu? menu)
        => menu ?? throw new InvalidOperationException("NativeMenu cannot be null.");
}