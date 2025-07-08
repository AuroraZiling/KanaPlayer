using System.Text.Json.Serialization;

namespace KanaPlayer.Core.Models.Wrappers;

public class CommonApiModel
{
    [JsonPropertyName("code")] public required int Code { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("ttl")] public int? Ttl { get; set; }
    
    public void EnsureSuccess()
    {
        if (Code == 0) return;
        var errorMessage = Message ?? "An error occurred while processing the request.";
        throw new InvalidOperationException($"API call failed with code {Code}: {errorMessage}");
    }
}

public class CommonApiModel<TData>: CommonApiModel
{
    [JsonPropertyName("data")] public TData? Data { get; set; }
    
    public TData EnsureData()
    {
        if (Data is null)
            throw new InvalidOperationException("Data cannot be null.");
        return Data;
    }
}