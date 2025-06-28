using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KanaPlayer.Core.Extensions;

public static class StringExtension
{
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? str) => 
        string.IsNullOrEmpty(str);
        
    [return: NotNullIfNotNull(nameof(str))]
    public static string? SafeSubstring(this string? str, int startIndex, int length)
    {
        if (str is null) return null;
        if (startIndex < 0) startIndex = 0;
        if (startIndex >= str.Length) return string.Empty;
        if (length < 0) length = 0;
        if (startIndex + length > str.Length) length = str.Length - startIndex;
        return str.Substring(startIndex, length);
    }

    /// <summary>
    /// Force enumerate the source str
    /// </summary>
    /// <param name="str"></param>
    /// <param name="another"></param>
    /// <returns></returns>
    public static bool SafeEquals(this string? str, string? another)
    {
        if (str is null) return another is null;
        var match = true;
        for (var i = 0; i < str.Length; i++)
        {
            if (!match) continue;
            match = another != null && i < another.Length && str[i] == another[i];
        }

        return match;
    }
    
    public static string AppendIf(this string str, bool condition, string append) => condition ? str + append : str;

    public static StringBuilder TrimEnd(this StringBuilder sb)
    {
        var i = sb.Length - 1;
        for (; i >= 0; i--)
        {
            if (!char.IsWhiteSpace(sb[i])) break;
        }
        if (i < sb.Length - 1)
        {
            sb.Remove(i + 1, sb.Length - i - 1);
        }
        return sb;
    }
}