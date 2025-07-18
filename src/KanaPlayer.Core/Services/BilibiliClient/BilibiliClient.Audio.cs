using System.Text.Json;
using System.Text.RegularExpressions;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Utils;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    /// <summary>
    /// 获取视频基本信息 - 获取视频详细信息
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
        {
            ScopedLogger.Debug("获取视频详细信息失败: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            throw new HttpRequestException($"获取视频详细信息失败: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var audioInfoModel = JsonSerializer.Deserialize<AudioInfoModel>(content);
        if (audioInfoModel == null)
        {
            ScopedLogger.Debug("反序列化音频信息失败: {Content}", content);
            throw new HttpRequestException("反序列化音频信息失败");
        }
        return audioInfoModel;
    }

    /// <summary>
    /// 从B站直接获取视频音频地址 
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
        {
            ScopedLogger.Debug("获取视频页面失败: {StatusCode} - {ReasonPhrase}", videoPageResponse.StatusCode, videoPageResponse.ReasonPhrase);
            throw new HttpRequestException($"获取视频页面失败: {videoPageResponse.StatusCode} - {videoPageResponse.ReasonPhrase}");
        }

        var videoPageContent = await videoPageResponse.Content.ReadAsStringAsync();
        var match = ExtractPlayInfoJsonRegex().Match(videoPageContent);
        if (!match.Success)
        {
            ScopedLogger.Debug("未找到 Play Info: {Content}", videoPageContent);
            throw new HttpRequestException("未找到 Play Info");
        }

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
        if (audioUrl == null)
        {
            ScopedLogger.Debug("未找到音频URL: {Json}", json);
            throw new Exception("未找到音频URL");
        }
        return audioUrl;
    }

    /// <summary>
    /// 获取音频流
    /// </summary>
    /// <param name="audioUniqueId"></param>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="Exception"></exception>
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
        }, HttpCompletionOption.ResponseHeadersRead);

        if (!audioResponse.IsSuccessStatusCode)
        {
            ScopedLogger.Debug("获取音频流失败: {StatusCode} - {ReasonPhrase}", audioResponse.StatusCode, audioResponse.ReasonPhrase);
            throw new HttpRequestException($"获取音频流失败: {audioResponse.StatusCode} - {audioResponse.ReasonPhrase}");
        }

        if (audioResponse.Content.Headers.ContentLength is { } contentLength) 
            return new HttpResultStream(await audioResponse.Content.ReadAsStreamAsync(), contentLength);
        
        ScopedLogger.Debug("音频流没有内容长度: {StatusCode} - {ReasonPhrase}", audioResponse.StatusCode, audioResponse.ReasonPhrase);
        throw new Exception("音频流没有内容长度");
    }

    /// <summary>
    /// 合集和视频列表信息 - 获取合集内容
    /// </summary>
    /// <param name="collectionId"></param>
    /// <param name="cookies"></param>
    /// <param name="fetchCompleteMediaList"></param>
    /// <param name="fetchedCountProgress"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<CollectionModel> GetCollectionAsync(ulong collectionId, Dictionary<string, string> cookies, bool fetchCompleteMediaList,
                                                          IProgress<int>? fetchedCountProgress = null)
    {
        const int supposedMediaCountPerPage = 50;
        var endpoint = $"https://api.bilibili.com/x/space/fav/season/list?season_id={collectionId}&web_location=0.0&ps={supposedMediaCountPerPage}";

        var templatePage = await GetPageAsync(1);
        var templatePageData = templatePage.EnsureData();
        var totalMediaCount = templatePageData.Medias.Count;
        fetchedCountProgress?.Report(templatePageData.Medias.Count);

        var allCollected = new List<CollectionFolderCommonMediaModel>();
        allCollected.AddRange(templatePageData.Medias);

        if (fetchCompleteMediaList)
        {
            var page = 2;
            while (allCollected.Count < totalMediaCount)
            {
                var pageModel = await GetPageAsync(page);
                var pageData = pageModel.EnsureData();
                fetchedCountProgress?.Report(pageData.Medias.Count);
                allCollected.AddRange(pageData.Medias);
                page++;
            }
        }

        templatePage.EnsureData().Medias = allCollected;
        return templatePage;

        async Task<CollectionModel> GetPageAsync(int pageNumber)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}&pn={pageNumber}").LoadCookies(cookies);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ScopedLogger.Debug("获取合集内容失败: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new HttpRequestException($"获取合集内容失败: {response.StatusCode} - {response.ReasonPhrase}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var collectionModel = JsonSerializer.Deserialize<CollectionModel>(content);
            if (collectionModel == null)
            {
                ScopedLogger.Debug("反序列化合集内容失败: {Content}", content);
                throw new HttpRequestException("反序列化合集内容失败");
            }
            return collectionModel;
        }
    }

    [GeneratedRegex(@"<script>window\.__playinfo__=(.*?)</script>", RegexOptions.Singleline)]
    private static partial Regex ExtractPlayInfoJsonRegex();
}