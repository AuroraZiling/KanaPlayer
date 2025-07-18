using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class CheckCookieRefreshModel : CommonApiModel<CookieRefreshDataModel>;

public record CookieRefreshDataModel(
    [property: JsonPropertyName("refresh")] bool Refresh, 
    [property: JsonPropertyName("timestamp")] long Timestamp);