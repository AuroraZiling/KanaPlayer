using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class ApplyQrCodeModel : CommonApiModel<ApplyQrCodeDataModel>;

public record ApplyQrCodeDataModel
(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("qrcode_key")] string QrCodeKey
);