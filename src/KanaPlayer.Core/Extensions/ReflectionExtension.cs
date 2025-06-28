namespace KanaPlayer.Core.Extensions;

public static class ReflectionExtension
{
    public static IEnumerable<Type> EnumerateBaseTypes(this Type type)
    {
        var currentType = type;
        while (currentType.BaseType != null)
        {
            yield return currentType.BaseType;
            currentType = currentType.BaseType;
        }
    }
}