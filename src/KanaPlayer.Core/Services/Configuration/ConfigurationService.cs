using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using KanaPlayer.Core.Helpers;
using KanaPlayer.Core.Models;

namespace KanaPlayer.Core.Services.Configuration;

public class ConfigurationService<TSettings> : IConfigurationService<TSettings> 
    where TSettings : SettingsBase, new()
{
    public TSettings Settings { get; private set; }
    
    public ConfigurationService()
        => Load();

    [MemberNotNull(nameof(Settings))]
    private void Load()
    {
        Directory.CreateDirectory(AppHelper.ApplicationFolderPath);
        TSettings? settings = null;
        try
        {
            var settingsJson = File.ReadAllText(AppHelper.SettingsFilePath);
            settings = JsonSerializer.Deserialize<TSettings>(settingsJson);
        }
        catch
        {
            // ignored
        }
        Settings = settings ?? new TSettings();
    }
    
    public void Save()
    {
        var settingsJson = JsonSerializer.Serialize(Settings, JsonHelper.JsonSerializerOptions);
        File.WriteAllText(AppHelper.SettingsFilePath, settingsJson);
    }
}