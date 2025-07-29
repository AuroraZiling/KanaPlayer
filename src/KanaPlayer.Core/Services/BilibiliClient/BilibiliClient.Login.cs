using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using KanaPlayer.Core.Extensions;
using KanaPlayer.Core.Models;
using KanaPlayer.Core.Models.Wrappers;
using KanaPlayer.Core.Models.Wrappers.User;

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

            if (checkRefreshCookies.Data == null || checkRefreshCookies.Data.Refresh) // 登录失效
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
                    ScopedLogger.Error(e, "在刷新 Cookies 时发生错误");
                    configurationService.Settings.CommonSettings.Authentication = null;
                }
            }
            else
            {
                var buVid = await GetBuVidAsync();
                cookies["buvid3"] = $"buvid3={buVid.BuVid3}";
                cookies["buvid4"] = $"buvid4={buVid.BuVid4}";
                configurationService.Settings.CommonSettings.Authentication.Cookies = cookies;
            }
            configurationService.SaveImmediate("登录态更新成功");
        }

        if (configurationService.Settings.CommonSettings.Authentication == null)
        {
            ScopedLogger.Info("未检测到登录态");
            return;
        }
        var accountNavInfo = await GetAccountNavInfoAsync(configurationService.Settings.CommonSettings.Authentication.Cookies);
        ArgumentNullException.ThrowIfNull(accountNavInfo.Data);

        configurationService.Settings.CommonSettings.Account = new CommonAccountSettings(
            accountNavInfo.Data.Face, accountNavInfo.Data.Mid, accountNavInfo.Data.UserName,
            new CommonAccountLevelSettings(accountNavInfo.Data.LevelInfo.CurrentLevel,
                accountNavInfo.Data.LevelInfo.CurrentMin, accountNavInfo.Data.LevelInfo.CurrentExp),
            accountNavInfo.Data.VipLabel.ImgLabelUri);
        configurationService.SaveImmediate("登录信息已保存");
        IsAuthenticated = configurationService.Settings.CommonSettings.Authentication != null;
        ScopedLogger.Info("登录成功");
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
        var hexString = Convert.ToHexString(encryptedData).ToLowerInvariant();
        ScopedLogger.Debug($"生成 Correspond Path: {hexString}");
        return hexString;
    }

    private static async Task<string> GetRefreshCsrfAsync(string correspondPath, Dictionary<string, string> cookies)
    {
        var endpoint = $"https://www.bilibili.com/correspond/1/{correspondPath}";

        using var scopedHttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await scopedHttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"获取 Refresh CSRF 失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"获取 Refresh CSRF 失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var refreshCsrfMatch = RefreshCsrfRegex().Matches(content)[0].Groups[1].Value;
        ScopedLogger.Debug($"获取到 Refresh CSRF: {refreshCsrfMatch}");
        return refreshCsrfMatch;
    }

    private static async Task<RefreshCookiesModel> RefreshCookiesAsync(string refreshToken, string refreshCsrf, Dictionary<string, string> cookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/cookie/refresh";

        var cookieSessData = cookies.TryGetValue("SESSDATA", out var sessData);
        if (!cookieSessData)
        {
            ScopedLogger.Error("未发现 SESSDATA，无法刷新 Cookies");
            throw new InvalidOperationException("未发现 SESSDATA，无法刷新 Cookies");
        }

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
            new KeyValuePair<string, string>("refresh_token", refreshToken) // Old Refresh Token
        ]);
        var response = await scopedHttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"刷新 Cookies 失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"刷新 Cookies 失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var jsonContent = JsonSerializer.Deserialize<CommonApiModel<RefreshCookiesDataModel>>(content)
                          ?? throw new HttpRequestException("Failed to refresh cookies");
        response.Headers.TryGetValues("Set-Cookie", out var newCookies);
        if (newCookies is null || jsonContent.Data is null)
        {
            ScopedLogger.Error("刷新 Cookies 失败，未获取到新的 Cookies 或数据");
            throw new HttpRequestException("刷新 Cookies 失败，未获取到新的 Cookies 或数据");
        }

        var newCookiesDictionary = newCookies.ToDictionary(newCookie => newCookie.Split('=')[0], newCookie => newCookie);
        newCookiesDictionary["ac_time_value"] = $"ac_time_value={jsonContent.Data.RefreshToken}";
        ScopedLogger.Debug("刷新 Cookies 成功");
        return new RefreshCookiesModel(jsonContent, newCookiesDictionary);
    }

    private static async Task<ConfirmRefreshCookiesModel> ConfirmRefreshCookiesAsync(string oldRefreshToken, Dictionary<string, string> newCookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/confirm/refresh";

        var cookieSessData = newCookies.TryGetValue("SESSDATA", out var sessData);
        if (!cookieSessData)
        {
            ScopedLogger.Error("未发现 SESSDATA，无法确认 Cookies 刷新");
            throw new InvalidOperationException("未发现 SESSDATA，无法确认 Cookies 刷新");
        }
        using var scopedHttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip
        });
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Cookie", sessData);
        request.Content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("csrf", newCookies["bili_jct"].Split('=')[1].Split(';')[0]),
            new KeyValuePair<string, string>("refresh_token", oldRefreshToken) // Old Refresh Token
        ]);

        var response = await scopedHttpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"确认 Cookies 刷新失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"确认 Cookies 刷新失败: {response.ReasonPhrase}");
        }
        var content = await response.Content.ReadAsStringAsync();
        var jsonContent = JsonSerializer.Deserialize<ConfirmRefreshCookiesModel>(content);
        if (jsonContent is null)
        {
            ScopedLogger.Error("确认 Cookies 刷新失败，未获取到数据");
            throw new HttpRequestException("确认 Cookies 刷新失败，未获取到数据");
        }
        ScopedLogger.Debug("确认 Cookies 刷新成功");
        return jsonContent;
    }

    public async Task<ApplyQrCodeModel> GetApplyQrCodeAsync()
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";

        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"获取 QR 码失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"获取 QR 码失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<ApplyQrCodeModel>(content);
        if (json is null)
        {
            ScopedLogger.Error("获取 QR 码失败，未获取到数据");
            throw new HttpRequestException("获取 QR 码失败，未获取到数据");
        }
        ScopedLogger.Debug("获取 QR 码成功");
        return json;
    }

    public async Task<LoginQrCodeModel> GetLoginQrCodeAsync(string qrCodeKey)
    {
        var endpoint = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrCodeKey}";
        const string buvidEndpoint = "https://api.bilibili.com/x/web-frontend/getbuvid";

        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"轮询 QR 码失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"轮询 QR 码失败: {response.ReasonPhrase}");
        }
        response.Headers.TryGetValues("Set-Cookie", out var cookies);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<LoginQrCodeModel>(content);
        if (json is null)
        {
            ScopedLogger.Debug("轮询 QR 码失败，未获取到数据");
            throw new HttpRequestException("轮询 QR 码失败，未获取到数据");
        }

        if (cookies is null)
            return json;

        if (json.Data is null)
        {
            ScopedLogger.Debug("轮询 QR 码失败，未获取到数据");
            throw new HttpRequestException("轮询 QR 码失败，未获取到数据");
        }
        var cookiesDictionary = cookies.ToDictionary(cookie => cookie.Split('=')[0], cookie => cookie);
        cookiesDictionary["ac_time_value"] = json.Data.RefreshToken;

        var buvidResponse = await httpClient.GetAsync(buvidEndpoint);
        if (!buvidResponse.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"获取 Buvid 失败: {buvidResponse.ReasonPhrase}");
            throw new HttpRequestException($"获取 Buvid 失败: {buvidResponse.ReasonPhrase}");
        }

        var buvidContent = await response.Content.ReadAsStringAsync();
        var buvidJson = JsonSerializer.Deserialize<BuVidModel>(buvidContent);
        if (buvidJson is null)
        {
            ScopedLogger.Error("获取 Buvid 失败，未获取到数据");
            throw new HttpRequestException("获取 Buvid 失败，未获取到数据");
        }

        cookiesDictionary["buvid3"] = $"buvid3={buvidJson.EnsureData().BuVid3}";
        cookiesDictionary["buvid4"] = $"buvid4={buvidJson.EnsureData().BuVid4}";
        json.Cookies = cookiesDictionary; // Cookies would not null if success
        return json;
    }

    private async Task<CheckCookieRefreshModel> RefreshCookiesAsync(Dictionary<string, string> cookies)
    {
        const string endpoint = "https://passport.bilibili.com/x/passport-login/web/cookie/info";

        var request = new HttpRequestMessage(HttpMethod.Get, endpoint).LoadCookies(cookies);
        var response = await httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"刷新 Cookies 失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"刷新 Cookies 失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<CheckCookieRefreshModel>(content);
        if (json is null)
        {
            ScopedLogger.Error("刷新 Cookies 失败，未获取到数据");
            throw new HttpRequestException("刷新 Cookies 失败，未获取到数据");
        }
        ScopedLogger.Debug("刷新 Cookies 成功");
        return json;
    }

    private async Task<BuVidDataModel> GetBuVidAsync()
    {
        const string endpoint = "https://api.bilibili.com/x/frontend/finger/spi";

        var response = await httpClient.GetAsync(endpoint);
        if (!response.IsSuccessStatusCode)
        {
            ScopedLogger.Error($"获取 Buvid 失败: {response.ReasonPhrase}");
            throw new HttpRequestException($"获取 Buvid 失败: {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<BuVidModel>(content);
        if (json?.Data is null)
        {
            ScopedLogger.Error("获取 Buvid 失败，未获取到数据");
            throw new HttpRequestException("获取 Buvid 失败，未获取到数据");
        }

        return json.EnsureData();
    }

    [GeneratedRegex("""<div id="1-name">(.*?)<\/div>""")]
    private static partial Regex RefreshCsrfRegex();
}