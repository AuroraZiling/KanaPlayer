using System.Text.Json;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    // 从 Bilibili 获取全部收藏夹 / 合集步骤
    // 1. 获取创建收藏夹列表
    // 2. 获取收集收藏夹 / 合集列表 -> 归为统一类型的列表
    // 3. 上述列表合并遍历，获取详细信息
    
    /// <summary>
    /// 收藏夹基本信息 - 获取指定用户创建的所有收藏夹信息
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/fav/info.html
    /// </summary>
    /// <param name="upMid"></param>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<FavoriteCreatedFoldersMetaModel> GetFavoriteCreatedFoldersMetaAsync(ulong upMid, Dictionary<string, string> cookies)
    {
        var endpoint = $"https://api.bilibili.com/x/v3/fav/folder/created/list-all?up_mid={upMid}&type=0&web_location=333.1387";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Debug($"获取用户创建的所有收藏夹信息失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"获取用户创建的所有收藏夹信息失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var favoriteCreatedFoldersMetaModel = JsonSerializer.Deserialize<FavoriteCreatedFoldersMetaModel>(content);
        if (favoriteCreatedFoldersMetaModel == null)
        {
            ScopedLogger.Debug("反序列化获取用户创建的所有收藏夹信息失败");
            throw new HttpRequestException("反序列化获取用户创建的所有收藏夹信息失败");
        }
        return favoriteCreatedFoldersMetaModel;
    }

    /// <summary>
    /// 收藏夹基本信息 - 查询用户收藏的视频收藏夹
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/fav/info.html
    /// </summary>
    /// <param name="upMid"></param>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<List<FavoriteCollectedItemMetaDataModel>> GetFavoriteCollectedFoldersMetaAsync(ulong upMid, Dictionary<string, string> cookies)
    {
        var endpoint = $"https://api.bilibili.com/x/v3/fav/folder/collected/list?ps=50&up_mid={upMid}&platform=web";

        var allCollected = new List<FavoriteCollectedItemMetaDataModel>();
        var page = 1;
        while (true)
        {
            var pageModel = await GetPageAsync(page);
            var pageData = pageModel.EnsureData();
            if (pageData.List.Count == 0)
                break;

            allCollected.AddRange(pageData.List);
            if (!pageData.HasMore)
                break;

            page++;
        }
        return allCollected;

        async Task<FavoriteCollectedMetaModel> GetPageAsync(int pageNumber)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}&pn={pageNumber}").LoadCookies(cookies);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ScopedLogger.Debug($"获取用户收藏的视频收藏夹信息失败: {response.ReasonPhrase}");
                throw new HttpRequestException($"获取用户收藏的视频收藏夹信息失败: {response.ReasonPhrase}");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var favoriteCollectedMetaModel = JsonSerializer.Deserialize<FavoriteCollectedMetaModel>(content);
            if (favoriteCollectedMetaModel == null)
            {
                ScopedLogger.Debug("反序列化获取用户收藏的视频收藏夹信息失败");
                throw new HttpRequestException("反序列化获取用户收藏的视频收藏夹信息失败");
            }
            return favoriteCollectedMetaModel;
        }
    }
    
    /// <summary>
    /// 收藏夹基本信息 - 获取收藏夹元数据
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/fav/info.html
    /// </summary>
    /// <param name="folderId"></param>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<FavoriteFolderInfoModel> GetFavoriteFolderInfoAsync(ulong folderId, Dictionary<string, string> cookies)
    {
        var endpoint = $"https://api.bilibili.com/x/v3/fav/folder/info?media_id={folderId}";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Debug($"获取收藏夹信息失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"获取收藏夹信息失败: {response.ReasonPhrase}");
        }
        
        var content = await response.Content.ReadAsStringAsync();
        var favoriteFolderInfoModel = JsonSerializer.Deserialize<FavoriteFolderInfoModel>(content);
        if (favoriteFolderInfoModel == null)
        {
            ScopedLogger.Debug("反序列化获取收藏夹信息失败");
            throw new HttpRequestException("反序列化获取收藏夹信息失败");
        }
        return favoriteFolderInfoModel;
    }
    
    /// <summary>
    /// 收藏夹内容 - 获取收藏夹内容明细列表
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/fav/list.html
    /// </summary>
    /// <param name="folderId"></param>
    /// <param name="cookies"></param>
    /// <param name="fetchedCountProgress"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<FavoriteFolderDetailModel> GetFavoriteFolderDetailAsync(ulong folderId, Dictionary<string, string> cookies, IProgress<int>? fetchedCountProgress = null)
    {
        var endpoint = $"https://api.bilibili.com/x/v3/fav/resource/list?media_id={folderId}&tid=0&ps=40&platform=web";

        var templatePage = await GetPageAsync(1);
        var templatePageData = templatePage.EnsureData();
        var hasMore = templatePageData.HasMore;
        fetchedCountProgress?.Report(templatePageData.Medias.Count);

        var allCollected = new List<CollectionFolderCommonMediaModel>();
        allCollected.AddRange(templatePageData.Medias);
        
        if (hasMore)
        {
            var page = 2;
            while (true)
            {
                var pageModel = await GetPageAsync(page);
                var pageData = pageModel.EnsureData();
                fetchedCountProgress?.Report(pageData.Medias.Count);
                allCollected.AddRange(pageData.Medias);
                if (!pageData.HasMore)
                    break;
                page++;
            }
        }
        
        templatePage.EnsureData().Medias = allCollected;
        return templatePage;

        async Task<FavoriteFolderDetailModel> GetPageAsync(int pageNumber)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{endpoint}&pn={pageNumber}").LoadCookies(cookies);
            var response = await httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                ScopedLogger.Debug($"获取收藏夹内容明细列表失败: {response.ReasonPhrase}");
                throw new HttpRequestException($"获取收藏夹内容明细列表失败: {response.ReasonPhrase}");
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var favoriteFolderDetailModel = JsonSerializer.Deserialize<FavoriteFolderDetailModel>(content);
            if (favoriteFolderDetailModel == null)
            {
                ScopedLogger.Debug("反序列化获取收藏夹内容明细列表失败");
                throw new HttpRequestException("反序列化获取收藏夹内容明细列表失败");
            }
            return favoriteFolderDetailModel;
        }
    }
}