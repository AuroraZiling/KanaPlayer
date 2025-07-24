using System.Diagnostics;

namespace KanaPlayer.Core.Helpers;

public static class AppHelper
{
    public static string ApplicationFolderPath { get; } =
        Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
    public static string ApplicationDataFolderPath { get; } =
        Path.Combine(ApplicationFolderPath, "KanaPlayer");
    public static string ApplicationLoggingFolderPath { get; } =
        Path.Combine(ApplicationDataFolderPath, "Logs");
    public static string ApplicationImageCachesFolderPath { get; } =
        Path.Combine(ApplicationDataFolderPath, "ImageCaches");
    public static string ApplicationAudioCachesFolderPath { get; } =
        Path.Combine(ApplicationDataFolderPath, "AudioCaches");
    public static string SettingsFilePath { get; } =
        Path.Combine(ApplicationDataFolderPath, "settings.json");

    public static string? ApplicationVersion { get; } =
        typeof(AppHelper).Assembly.GetName().Version?.ToString(3);
    public static bool IsDebug =>
        Debugger.IsAttached;
}