using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace KanaPlayer.Core.Extensions;

public static class EnumExtension
{
    public static string ToFriendlyString<T>(this T value) where T : struct, Enum
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        if (member == null) return value.ToString();
        var attribute = member.GetCustomAttributes(typeof(EnumMemberAttribute), false).FirstOrDefault() as EnumMemberAttribute;
        return attribute?.Value ?? value.ToString();
    }

    public static T ToEnum<T>(this string name) where T : struct, Enum
    {
        return (T)ToEnum(name, typeof(T));
    }

    public static object ToEnum(this string name, Type enumType)
    {
        if (!enumType.IsEnum) throw new ArgumentException("Type must be an enum", nameof(enumType));

        foreach (var field in enumType.GetFields())
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute?.Value == name) return field.GetValue(null).NotNull();
        }

        return Enum.Parse(enumType, name);
    }

    public static bool TryToEnum<T>(this string name, [NotNullWhen(true)] out T? value) where T : struct, Enum
    {
        var result = TryToEnum(name, typeof(T), out var obj);
        value = (T?)obj;
        return result;
    }

    public static bool TryToEnum(this string name, Type enumType, [NotNullWhen(true)] out object? value)
    {
        value = null;
        if (!enumType.IsEnum) return false;

        foreach (var field in enumType.GetFields())
        {
            var attribute = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attribute?.Value == name)
            {
                value = field.GetValue(null).NotNull();
                return true;
            }
        }

        return Enum.TryParse(enumType, name, out value);
    }
}