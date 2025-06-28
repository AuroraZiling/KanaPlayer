using KanaPlayer.Core.Models;

namespace KanaPlayer.Core.Services.Configuration;

public interface IConfigurationService<out TSettings>
    where TSettings : SettingsBase, new()
{
    public TSettings Settings { get; }
    public void Save();
}