using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class CommonApiModel<TData>
{
    [JsonPropertyName("code")] public required int Code { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("ttl")] public int? Ttl { get; set; }
    [JsonPropertyName("data")] public TData? Data { get; set; }
    
    public TData EnsureData()
    {
        if (Data is null)
            throw new InvalidOperationException("Data cannot be null.");
        return Data;
    }
}