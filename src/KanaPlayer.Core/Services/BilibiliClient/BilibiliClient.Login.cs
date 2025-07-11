using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers;

namespace KanaPlayer.Core.Services;

public partial class BilibiliClient<TSettings>
{
    [ObservableProperty] public partial bool IsAuthenticated { get; private set; }

    public async Task AuthenticateAsync()
    {
        if (configurationService.Settings.CommonSettings.Authentication is not null)
        {
            var refreshToken = configurationService.Settings.CommonSettings.Authentication.RefreshToken;
            var cookies = configurationService.Settings.CommonSettings.Authentication.Cookies;
            
            var checkRefreshCookies = await RefreshCookiesAsync(cookies);
              
            if (checkRefreshCookies.Data == null || checkRefreshCookies.Data.Refresh)  // 登录失效
            {
                try
                {
                    var correspondPath = GenerateCorrespondPath();
                    var refreshCsrf = await GetRefreshCsrfAsync(correspondPath, cookies);
                    var refreshCookies = await RefreshCookiesAsync(refreshToken, refreshCsrf, cookies);
                    var confirmRefreshCookies = await ConfirmRefreshCookiesAsync(refreshToken, refreshCookies.NewCookies);
                    confirmRefreshCookies.EnsureSuccess();
                    configurationService.Settings.CommonSettings.Authentication.Cookies = refreshCookies.NewCookies;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }
            else
            {
                var buVid = await GetBuVidAsync();
                cookies["buvid3"] = $"buvid3={buVid.BuVid3}";
                cookies["buvid4"] = $"buvid4={buVid.BuVid4}";
                configurationService.Settings.CommonSettings.Authentication.Cookies = cookies;
            }
            configurationService.Save();
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
    
    public bool TryGetCookies(out Dictionary<string, string> cookies)
    {
        var authentication = configurationService.Settings.CommonSettings.Authentication;
        if (IsAuthenticated && authentication is not null)
        {
            cookies = authentication.Cookies;
            return true;
        }

        cookies = [];
        return false;
    }

    private static string GenerateCorrespondPath()
    {
        const string publicKeyPem = 
            """
            -----BEGIN PUBLIC KEY-----
            MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg
            Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71
            nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40
            JNrRuoEUXpabUzGB8QIDAQAB
            -----END PUBLIC KEY-----
            """;
        
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var dataToEncrypt = $"refresh_{timestamp}";
        var encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(dataToEncrypt), 
            RSAEncryptionPadding.OaepSHA256);
        
        return Convert.ToHexString(encryptedData).ToLowerInvariant();
    }

    private static async Task<string> GetRefreshCsrfAsync(string correspondPath, Dictionary<string, string> cookies)
    {
        var endpoint = $"https://www.bilibili.com/correspond/1/{correspondPath}";

        using var scopedHttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        foreach (var cookie in cookies)
        {
            request.Headers.Add("Cookie", cookie.Value);
        }
        var response = await scopedHttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get refresh csrf: {response.ReasonPhrase}");
        var content = await response.Content.ReadAsStringAsync();
        return RefreshCsrfRegex().Matches(content)[0].Groups[1].Value;
    }

    private static async Task<RefreshCookiesModel> RefreshCookiesAsync(string refreshToken, string refreshCsrf, Dictionary<string, string> cookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/cookie/refresh";
        
        var cookieSessData = cookies.TryGetValue("SESSDATA", out var sessData);
        if (!cookieSessData)
            throw new InvalidOperationException("SESSDATA cookie is required for refreshing cookies.");
        using var scopedHttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Cookie", sessData);
        request.Content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("csrf", cookies["bili_jct"].Split('=')[1].Split(';')[0]),
            new KeyValuePair<string, string>("refresh_csrf", refreshCsrf),
            new KeyValuePair<string, string>("source", "main_web"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)  // Old Refresh Token
        ]);
        
        var response = await scopedHttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to refresh cookies: {response.ReasonPhrase}");

        var content = await response.Content.ReadAsStringAsync();
        var jsonContent = JsonSerializer.Deserialize<CommonApiModel<RefreshCookiesDataModel>>(content)
                          ?? throw new HttpRequestException("Failed to refresh cookies");
                
        response.Headers.TryGetValues("Set-Cookie", out var newCookies);
        if (newCookies is null || jsonContent.Data is null)
            throw new HttpRequestException("No/Broken cookies returned in the response.");
        var newCookiesDictionary = newCookies.ToDictionary(newCookie => newCookie.Split('=')[0], newCookie => newCookie);
        newCookiesDictionary["ac_time_value"] = $"ac_time_value={jsonContent.Data.RefreshToken}";

        return new RefreshCookiesModel(jsonContent, newCookiesDictionary);
    }

    private async Task<ConfirmRefreshCookiesModel> ConfirmRefreshCookiesAsync(string oldRefreshToken, Dictionary<string, string> newCookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/confirm/refresh";
        
        var cookieSessData = newCookies.TryGetValue("SESSDATA", out var sessData);
        if (!cookieSessData)
            throw new InvalidOperationException("SESSDATA cookie is required for confirming refresh cookies.");
        using var scopedHttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Cookie", sessData);
        request.Content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("csrf", newCookies["bili_jct"].Split('=')[1].Split(';')[0]),
            new KeyValuePair<string, string>("refresh_token", oldRefreshToken)  // Old Refresh Token
        ]);
        
        var response = await scopedHttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to confirm refresh cookies: {response.ReasonPhrase}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ConfirmRefreshCookiesModel>(content) 
               ?? throw new HttpRequestException("Failed to confirm refresh cookies");
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
        
        if(json.Data is null)
            throw new HttpRequestException("QR code data is null, login may have failed.");
        var cookiesDictionary = cookies.ToDictionary(cookie => cookie.Split('=')[0], cookie => cookie);
        cookiesDictionary["ac_time_value"] = json.Data.RefreshToken;
        
        var response2 = await httpClient.GetAsync(endpoint2);
        if (!response2.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get buvid: {response2.ReasonPhrase}");
        var content2 = await response.Content.ReadAsStringAsync();
        var json2 = JsonSerializer.Deserialize<BuVidModel>(content2)
                    ?? throw new HttpRequestException("Failed to poll QR code");
        
        cookiesDictionary["buvid3"] = $"buvid3={json2.EnsureData().BuVid3}";
        cookiesDictionary["buvid4"] = $"buvid4={json2.EnsureData().BuVid4}";
        json.Cookies = cookiesDictionary;  // Cookies would not null if success
        return json;
    }
    
    private async Task<CheckCookieRefreshModel> RefreshCookiesAsync(Dictionary<string, string> cookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/cookie/info";
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        foreach (var cookie in cookies.Values)
        {
            request.Headers.Add("Cookie", cookie);
        }
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to refresh cookies: {response.ReasonPhrase}");
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CheckCookieRefreshModel>(content) 
               ?? throw new HttpRequestException("Failed to refresh cookies");
    }
    
    private async Task<BuVidDataModel> GetBuVidAsync()
    {
        const string endpoint = "https://api.bilibili.com/x/frontend/finger/spi";
        
        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to get buvid: {response.ReasonPhrase}");
        
        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<BuVidModel>(content)
                   ?? throw new HttpRequestException("Failed to get buvid");

        return json.EnsureData();
    }

    [GeneratedRegex("""<div id="1-name">(.*?)<\/div>""")]
    private static partial Regex RefreshCsrfRegex();
}