using System.Text.RegularExpressions;

namespace KanaPlayer.Core.Utils;

public enum BiliBiliIdType
{
    Av,
    Bv,
    Url
}

public record BiliBiliId(string Id, BiliBiliIdType Type)
{
    public static BiliBiliId CreateAvId(string avId) => new(avId, BiliBiliIdType.Av);
    public static BiliBiliId CreateBvId(string bvId) => new(bvId, BiliBiliIdType.Bv);
    public static BiliBiliId CreateUrl(string url) => new(url, BiliBiliIdType.Url);
};

/// <summary>
/// 提供用于从文本中提取B站视频ID的静态方法。
/// </summary>
public static partial class BiliBiliExtractor
{
    // 正则表达式说明:
    // 1. UrlRegex: 匹配标准的B站视频链接，并从链接中捕获av号或BV号。
    //    - `bilibili\.com/video/`: 匹配URL的核心部分。
    //    - `(av\d+|BV1[A-Za-z0-9]{9})`: 这是一个捕获组 (group 1)，用于匹配两种ID格式：
    //      - `av\d+`: "av" 后跟一位或多位数字。
    //      - `|`: 或逻辑。
    //      - `BV1[A-Za-z0-9]{9}`: "BV1" 开头，后跟9个字母或数字，总长度12位。
    //    - `RegexOptions.IgnoreCase`: 忽略大小写，能同时匹配 `bilibili.com` 和 `AV` `bv` 等。
    [GeneratedRegex(@"bilibili\.com/video/(av\d+|BV1[A-Za-z0-9]{9})", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex UrlRegex();
    
    // 2. BvIdRegex: 匹配独立的BV号。
    //    - `^...$`: 强制从字符串的开始(^)到结束($)完全匹配，确保整行只有一个BV号。
    [GeneratedRegex(@"^BV1[A-Za-z0-9]{9}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex BvIdRegex();

    // 3. AvIdRegex: 匹配独立的av号。
    //    - `^...$`: 同样要求完全匹配整行。
    [GeneratedRegex(@"^av\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "zh-CN")]
    private static partial Regex AvIdRegex();

    /// <summary>
    /// 从一个多行文本字符串中筛选出所有B站视频的av或BV号。
    /// </summary>
    /// <param name="inputText">包含B站网址、av号或BV号的源文本，每行一个。</param>
    /// <returns>一个包含所有有效ID的列表。列表中不会有重复项。</returns>
    public static List<BiliBiliId> ExtractBilibiliIds(string inputText)
    {
        // 使用HashSet可以自动处理重复的ID
        var foundIds = new HashSet<BiliBiliId>();

        if (string.IsNullOrWhiteSpace(inputText))
            return [];

        // 分割输入文本为多行，并移除空行
        var lines = inputText.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrEmpty(trimmedLine))
            {
                continue;
            }

            // 1. 优先检查是否为完整URL
            var urlMatch = UrlRegex().Match(trimmedLine);
            if (urlMatch.Success)
            {
                // 如果是URL，提取捕获组1中的ID
                foundIds.Add(BiliBiliId.CreateUrl(urlMatch.Groups[1].Value));
                continue;
            }

            // 2. 检查是否为独立的BV号
            if (BvIdRegex().IsMatch(trimmedLine))
            {
                foundIds.Add(BiliBiliId.CreateBvId(trimmedLine));
                continue;
            }

            // 3. 检查是否为独立的av号
            if (AvIdRegex().IsMatch(trimmedLine))
            {
                foundIds.Add(BiliBiliId.CreateAvId(trimmedLine));
            }
        }

        return [..foundIds];
    }
}
