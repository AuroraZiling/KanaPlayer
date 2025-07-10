using System.Text.Json;
using System.Text.RegularExpressions;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    public async Task<Stream> GetAudioStreamAsync(MusicRegionFeedDataInfoModel musicInfoModel, Dictionary<string, string> cookies)
    {
        var videoEndpoint = $"https://www.bilibili.com/video/{musicInfoModel.Bvid}";
        
        var videoPageRequest = new HttpRequestMessage(HttpMethod.Get, videoEndpoint);
        foreach (var cookie in cookies.Values)
        {
            videoPageRequest.Headers.Add("Cookie", cookie);
        }
        var videoPageResponse = await httpClient.SendAsync(videoPageRequest);
        if (!videoPageResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get music url: {videoPageResponse.ReasonPhrase}");
        var videoPageContent = await videoPageResponse.Content.ReadAsStringAsync();
        var match = ExtractPlayInfoJsonRegex().Match(videoPageContent);
        if (!match.Success) 
            throw new Exception("PlayInfo JSON not found.");
        var json = match.Groups[1].Value;
        var jsonDocument = JsonDocument.Parse(json);
        var audioUrl =  jsonDocument.RootElement
            .GetProperty("data")
            .GetProperty("dash")
            .GetProperty("audio")
            .EnumerateArray()
            .FirstOrDefault()
            .GetProperty("base_url")
            .GetString();
        if (audioUrl is null)
            throw new Exception("Audio URL not found in PlayInfo JSON.");
        
        var audioResponse = await httpClient.SendAsync(new HttpRequestMessage
        {
            RequestUri = new Uri(audioUrl),
            Method = HttpMethod.Get,
            Headers =
            {
                {"Referer", videoEndpoint}
            }
        });
        
        if (!audioResponse.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get audio stream: {audioResponse.ReasonPhrase}");
        return await audioResponse.Content.ReadAsStreamAsync();
    }
    
    [GeneratedRegex(@"<script>window\.__playinfo__=(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex ExtractPlayInfoJsonRegex();
}