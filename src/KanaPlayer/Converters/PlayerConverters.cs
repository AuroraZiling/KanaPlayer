using Avalonia.Data.Converters;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Interfaces;
using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Converters;

public static class PlayerConverters
{
    public static FuncValueConverter<PlayStatus, string> PlayStatusToStringConverter { get; } = new(status => status == PlayStatus.Playing ? "暂停" : "播放");
    public static FuncValueConverter<PlaybackMode, string> PlaybackModeToStringConverter { get; } = new(mode => mode.ToFriendlyString());
}