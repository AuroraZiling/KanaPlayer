using System.Text.Json;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    /// <summary>
    /// 获取登录基本信息 - 导航栏用户信息
    /// https://socialsisteryi.github.io/bilibili-API-collect/docs/login/login_info.html
    /// </summary>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<AccountNavInfoModel> GetAccountNavInfoAsync(Dictionary<string, string> cookies)
    {
        const string endpoint = "https://api.bilibili.com/x/web-interface/nav";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Debug("获取导航栏用户信息失败: {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            throw new HttpRequestException($"获取导航栏用户信息失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var accountNavInfoModel = JsonSerializer.Deserialize<AccountNavInfoModel>(content);
        if (accountNavInfoModel == null)
        {
            ScopedLogger.Debug("反序列化导航栏用户信息失败: {Content}", content);
            throw new HttpRequestException("反序列化导航栏用户信息失败");
        }
        return accountNavInfoModel;
    }
}