using System.Text.Json;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    /// <summary>
    /// 视频推荐 - 获取首页视频推荐列表
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/video/recommend.html
    /// </summary>
    /// <param name="cookies"></param>
    /// <param name="requestCount"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<AudioRegionFeedModel> GetAudioRegionFeedAsync(Dictionary<string, string> cookies, int requestCount = 15)
    {
        var endpoint = $"https://api.bilibili.com/x/web-interface/region/feed/rcmd?request_cnt={requestCount}&from_region=1003";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Debug($"获取首页视频推荐列表失败: {response.StatusCode} - {response.ReasonPhrase}");
            throw new HttpRequestException($"获取首页视频推荐列表失败: {response.StatusCode} - {response.ReasonPhrase}");
        }
        
        var content = await response.Content.ReadAsStringAsync();
        var feed = JsonSerializer.Deserialize<AudioRegionFeedModel>(content);
        if (feed == null)
        {
            ScopedLogger.Debug("获取首页视频推荐列表失败: 反序列化结果为 null");
            throw new HttpRequestException("获取首页视频推荐列表失败: 反序列化结果为 null");
        }
        ScopedLogger.Debug($"获取首页视频推荐列表成功: {feed.EnsureData().Archives.Count} 条数据");
        return feed;
    }
}