using System.Text.Json.Serialization;
using KanaPlayer.Core.Models;
using KanaPlayer.Models.SettingTypes;

namespace KanaPlayer.Models;

public class SettingsModel : SettingsBase
{
    public UiSettings UiSettings { get; set; } = new();
}

public class UiSettings
{
    [JsonConverter(typeof(JsonStringEnumConverter))]  
    public CloseBehaviors CloseBehavior { get; set; } = CloseBehaviors.Close;
}