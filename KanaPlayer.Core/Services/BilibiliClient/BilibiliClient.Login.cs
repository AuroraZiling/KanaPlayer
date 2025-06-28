using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    
    [ObservableProperty] public partial bool IsAuthenticated { get; private set; }

    public async Task AuthenticateAsync()
    {
        if (configurationService.Settings.CommonSettings.Authentication != null)
        {
            var cookies = configurationService.Settings.CommonSettings.Authentication.Cookies;
            
            var refreshCookies = await RefreshCookiesAsync(cookies);
            if (refreshCookies.Data == null || refreshCookies.Data.Refresh)  // 登录失效
            {
                configurationService.Settings.CommonSettings.Authentication = null;
                configurationService.Save();
            }
            else
            {
                if (cookies.FindIndexOf(s => s?.StartsWith("buvid") is true) is var index)
                {
                    if (index >= 0)
                        cookies[index] = $"buvid3={await GetBvUid3Async()}";
                    else
                        cookies = [..cookies, $"buvid3={await GetBvUid3Async()}"];

                    configurationService.Settings.CommonSettings.Authentication = 
                        configurationService.Settings.CommonSettings.Authentication with { Cookies = cookies };
                    configurationService.Save();
                }
                
            }
        }

        try
        {
            if (configurationService.Settings.CommonSettings.Authentication == null)
                return;
            var accountNavInfo =
                await GetAccountNavInfoAsync(configurationService.Settings.CommonSettings.Authentication.Cookies);
            if (accountNavInfo.Data is null)
            {
                Console.WriteLine("账户信息获取失败");
                return;
            }

            configurationService.Settings.CommonSettings.Account = new CommonAccountSettings(
                accountNavInfo.Data.Face, accountNavInfo.Data.Mid, accountNavInfo.Data.UserName,
                new CommonAccountLevelSettings(accountNavInfo.Data.LevelInfo.CurrentLevel,
                    accountNavInfo.Data.LevelInfo.CurrentMin, accountNavInfo.Data.LevelInfo.CurrentExp),
                accountNavInfo.Data.VipLabel.ImgLabelUri);
            configurationService.Save();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsAuthenticated = configurationService.Settings.CommonSettings.Authentication != null;
        }
    }
    
    public async Task<ApplyQrCodeModel> GetApplyQrCodeAsync()
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";
        
        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get QR code: {response.ReasonPhrase}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApplyQrCodeModel>(content) 
               ?? throw new HttpRequestException("Failed to get QR code");
    }

    public async Task<LoginQrCodeModel> GetLoginQrCodeAsync(string qrCodeKey)
    {
        var endpoint = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrCodeKey}";
        const string endpoint2 = "https://api.bilibili.com/x/web-frontend/getbuvid";
        
        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to poll QR code: {response.ReasonPhrase}");

        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<LoginQrCodeModel>(content)
                   ?? throw new HttpRequestException("Failed to poll QR code");

        if (cookies is null)
            return json;
        
        var response2 = await httpClient.GetAsync(endpoint2);
        if (!response2.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get buvid3: {response2.ReasonPhrase}");
        
        var content2 = await response.Content.ReadAsStringAsync();
        var json2 = JsonSerializer.Deserialize<BvUid3Model>(content2)
                    ?? throw new HttpRequestException("Failed to poll QR code");
        
        json.Cookies = [..cookies, json2.EnsureData().BuVid];  // Cookies would not null if success
        return json;
    }
    
    private async Task<CookieRefreshModel> RefreshCookiesAsync(string?[] cookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/cookie/info";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        foreach (var cookie in cookies.OfType<string>())
        {
            request.Headers.Add("Cookie", cookie);
        }
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to refresh cookies: {response.ReasonPhrase}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CookieRefreshModel>(content) 
               ?? throw new HttpRequestException("Failed to refresh cookies");
    }
    
    private async Task<string> GetBvUid3Async()
    {
        const string endpoint = "https://api.bilibili.com/x/web-frontend/getbuvid";
        
        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get buvid3: {response.ReasonPhrase}");
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<BvUid3Model>(content)
                   ?? throw new HttpRequestException("Failed to get buvid3");
        
        return json.EnsureData().BuVid;
    }
}