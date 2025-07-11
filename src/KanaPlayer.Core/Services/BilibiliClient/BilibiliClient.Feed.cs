using System.Text.Json;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    public async Task<AudioRegionFeedModel> GetAudioRegionFeedAsync(Dictionary<string, string> cookies, int requestCount = 15)
    {
        var endpoint = $"https://api.bilibili.com/x/web-interface/region/feed/rcmd?request_cnt={requestCount}&from_region=1003";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get region feed: {response.ReasonPhrase}");
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AudioRegionFeedModel>(content) 
               ?? throw new HttpRequestException("Failed to get region feed");
    }
}