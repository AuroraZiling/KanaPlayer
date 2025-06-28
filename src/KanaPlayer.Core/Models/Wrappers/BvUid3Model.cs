using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class BvUid3Model : CommonApiModel<BvUid3DataModel>;

public record BvUid3DataModel
(
    [property: JsonPropertyName("buvid")] string BuVid
);