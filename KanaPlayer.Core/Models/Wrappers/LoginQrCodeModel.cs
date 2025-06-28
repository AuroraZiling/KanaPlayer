using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class LoginQrCodeModel: CommonApiModel<LoginQrCodeDataModel>
{
    [JsonIgnore] public string[]? Cookies { get; set; }
}

public record LoginQrCodeDataModel
(
    [property: JsonPropertyName("url")] string Url, 
    [property: JsonPropertyName("refresh_token")] string RefreshToken, 
    [property: JsonPropertyName("timestamp")] long Timestamp, 
    [property: JsonPropertyName("code")] int Code, 
    [property: JsonPropertyName("message")] string Message
);