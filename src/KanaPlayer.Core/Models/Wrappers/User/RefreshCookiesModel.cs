using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public record RefreshCookiesModel
(
    CommonApiModel<RefreshCookiesDataModel> Response, 
    Dictionary<string, string> NewCookies
);

public record RefreshCookiesDataModel
(
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("refresh_token")] string RefreshToken
);           