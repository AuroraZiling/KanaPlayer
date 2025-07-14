namespace KanaPlayer.Core.Helpers;

public static class AppHelper
{
    public static string ApplicationFolderPath { get; } =
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, "KanaPlayer");
    public static string ApplicationFavoritesFolderPath { get; } =
        Path.Combine(ApplicationFolderPath, "Favorites");
    public static string ApplicationImageCachesFolderPath { get; } =
        Path.Combine(ApplicationFolderPath, "ImageCaches");
    public static string ApplicationAudioCachesFolderPath { get; } =
        Path.Combine(ApplicationFolderPath, "AudioCaches");
    public static string SettingsFilePath { get; } =
        Path.Combine(ApplicationFolderPath, "settings.json");
}