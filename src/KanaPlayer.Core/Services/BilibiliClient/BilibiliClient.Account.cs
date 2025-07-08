using System.Text.Json;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    public async Task<AccountNavInfoModel> GetAccountNavInfoAsync(Dictionary<string, string> cookies)
    {
        const string endpoint = "https://api.bilibili.com/x/web-interface/nav";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        foreach (var cookie in cookies.Values)
        {
            request.Headers.Add("Cookie", cookie);
        }
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get account nav info: {response.ReasonPhrase}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AccountNavInfoModel>(content) 
               ?? throw new HttpRequestException("Failed to get account nav info");
    }
}