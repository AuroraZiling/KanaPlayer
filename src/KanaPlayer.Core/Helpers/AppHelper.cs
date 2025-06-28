namespace KanaPlayer.Core.Helpers;

public static class AppHelper
{
    public static string ApplicationFolderPath { get; } =
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory, "KanaPlayer");
    public static string ApplicationCachesFolderPath { get; } =
        Path.Combine(ApplicationFolderPath, "Caches");
    public static string SettingsFilePath { get; } =
        Path.Combine(ApplicationFolderPath, "settings.json");
}