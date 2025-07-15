using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class CommonOwnerModel
{
    [JsonPropertyName("mid")] public required ulong Mid { get; set; }
    [JsonPropertyName("name")] public required string Name { get; set; }
}