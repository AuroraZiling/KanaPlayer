using System.Text.Json;

namespace KanaPlayer.Core.Helpers;

public static class JsonHelper
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        WriteIndented = true
    };
}