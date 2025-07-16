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
    [JsonPropertyName("Behaviors")] public UiBehaviorSettings Behaviors { get; set; } = new();
}

public class UiBehaviorSettings
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CloseBehaviors CloseBehavior { get; set; } = CloseBehaviors.Close;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FavoritesDoubleTappedPlayListItemBehaviors FavoritesDoubleTappedPlayListItemBehavior { get; set; } =
        FavoritesDoubleTappedPlayListItemBehaviors.ReplaceCurrentPlayList;

    public bool IsFavoritesPlayAllReplaceWarningEnabled { get; set; } = true;
}