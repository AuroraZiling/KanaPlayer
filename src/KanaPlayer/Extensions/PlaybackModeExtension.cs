using System;
using KanaPlayer.Core.Models.PlayerManager;

namespace KanaPlayer.Extensions;


public static class PlaybackModeExtension
{
    public static string ToDisplayString(this PlaybackMode playbackMode)
        => playbackMode switch
        {
            PlaybackMode.Shuffle => "随机播放",
            PlaybackMode.RepeatAll => "列表循环",
            PlaybackMode.RepeatOne => "单曲循环",
            PlaybackMode.Sequential => "顺序播放",
            _ => throw new ArgumentOutOfRangeException(nameof(playbackMode), playbackMode, null)
        };
    
    internal static PlaybackMode FromDisplayString(string displayString)
        => displayString switch
        {
            "随机播放" => PlaybackMode.Shuffle,
            "列表循环" => PlaybackMode.RepeatAll,
            "单曲循环" => PlaybackMode.RepeatOne,
            "顺序播放" => PlaybackMode.Sequential,
            _ => throw new ArgumentException($"Unknown playback mode: {displayString}", nameof(displayString))
        };
    
    public static bool IsStringValidPlaybackMode(this string playbackModeString)
    {
        try
        {
            FromDisplayString(playbackModeString);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}