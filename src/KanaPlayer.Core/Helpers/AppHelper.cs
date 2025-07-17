namespace KanaPlayer.Core.Helpers;

public static class AppHelper
{
    public static string ApplicationFolderPath { get; } =
        Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
    public static string ApplicationDataFolderPath { get; } =
        Path.Combine(ApplicationFolderPath, "KanaPlayer");
    public static string ApplicationImageCachesFolderPath { get; } =
        Path.Combine(ApplicationDataFolderPath, "ImageCaches");
    public static string ApplicationAudioCachesFolderPath { get; } =
        Path.Combine(ApplicationDataFolderPath, "AudioCaches");
    public static string SettingsFilePath { get; } =
        Path.Combine(ApplicationDataFolderPath, "settings.json");
}