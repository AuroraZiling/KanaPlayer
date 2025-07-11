using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class BuVidModel : CommonApiModel<BuVidDataModel>;

public record BuVidDataModel
(
    [property: JsonPropertyName("b_3")] string BuVid3,
    [property: JsonPropertyName("b_4")] string BuVid4
);