using System.Text.Json;
using System.Text.RegularExpressions;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.PlayerManager;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    /// <summary>
    /// Get audio basic information from audio unique ID.
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/video/info.html
    /// </summary>
    /// <param name="audioUniqueId"></param>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<AudioInfoModel> GetAudioInfoAsync(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies)
    {
        var endpoint = $"https://api.bilibili.com/x/web-interface/view?bvid={audioUniqueId.Bvid}";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get audio info: {response.ReasonPhrase}");

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AudioInfoModel>(content)
               ?? throw new HttpRequestException("Failed to get audio info");
    }

    /// <summary>
    /// Get audio URL from audio unique ID.
    /// </summary>
    /// <param name="audioUniqueId"></param>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<string> GetAudioUrlAsync(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies)
    {
        var videoEndpoint = $"https://www.bilibili.com/video/{audioUniqueId.Bvid}?p={audioUniqueId.Page}";

        var videoPageRequest = new HttpRequestMessage(HttpMethod.Get, videoEndpoint).LoadCookies(cookies);
        var videoPageResponse = await httpClient.SendAsync(videoPageRequest);
        if (!videoPageResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get music url: {videoPageResponse.ReasonPhrase}");

        var videoPageContent = await videoPageResponse.Content.ReadAsStringAsync();
        var match = ExtractPlayInfoJsonRegex().Match(videoPageContent);
        if (!match.Success)
            throw new Exception("PlayInfo JSON not found.");

        var json = match.Groups[1].Value;
        var jsonDocument = JsonDocument.Parse(json);
        var audioUrl = jsonDocument.RootElement
                                   .GetProperty("data")
                                   .GetProperty("dash")
                                   .GetProperty("audio")
                                   .EnumerateArray()
                                   .FirstOrDefault()
                                   .GetProperty("base_url")
                                   .GetString();
        return audioUrl ?? throw new Exception("Audio URL not found in PlayInfo JSON.");
    }

    public async Task<Stream> GetAudioStreamAsync(AudioUniqueId audioUniqueId, Dictionary<string, string> cookies)
    {
        var audioUrl = await GetAudioUrlAsync(audioUniqueId, cookies);

        var audioResponse = await httpClient.SendAsync(new HttpRequestMessage
        {
            RequestUri = new Uri(audioUrl),
            Method = HttpMethod.Get,
            Headers =
            {
                { "Referer", $"https://www.bilibili.com/video/{audioUniqueId.Bvid}" }
            }
        });

        if (!audioResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get audio stream: {audioResponse.ReasonPhrase}");
        return await audioResponse.Content.ReadAsStreamAsync();
    }

    [GeneratedRegex(@"<script>window\.__playinfo__=(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex ExtractPlayInfoJsonRegex();
}