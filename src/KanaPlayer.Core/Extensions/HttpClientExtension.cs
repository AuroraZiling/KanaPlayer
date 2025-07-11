namespace KanaPlayer.Core.Extensions;

public static class HttpClientExtension
{
    public static HttpRequestMessage LoadCookies(this HttpRequestMessage request, Dictionary<string, string> cookies)
    {
        if (cookies.Count == 0)
            return request;

        foreach (var cookie in cookies)
            request.Headers.Add("Cookie", cookie.Value);
        return request;
    }
}