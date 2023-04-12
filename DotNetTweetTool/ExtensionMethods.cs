using System.Runtime.CompilerServices;

namespace TweetTesting;

public static class ExtensionMethods
{
    public static string Join(this IEnumerable<string> enumerable, string separator)
    {
        return string.Join(separator, enumerable);
    }

    public static string JoinAmpersand(this IEnumerable<string> values)
    {
        return values.Join("&");
    }

    public static string DoubleQuote(this string? str)
    {
        return $"\"{str?.Replace("\"", "\\\"")}\"";
    }

    public static string JoinCommaSpace(this IEnumerable<string> values)
    {
        return values.Join(", ");
    }

    public static IDictionary<TKey, string> AddPair<TKey, TValue>(
        this IDictionary<TKey, string> dictionary,
        TKey key,
        TValue? value = default)
    {
        dictionary.Add(key, value.ToString());
        return dictionary;
    }

    public static T? NotNull<T>(
        this T? obj)
        where T : struct
    {
        if (obj == null)
        {
            throw new ArgumentException($"Expected object of type '{typeof(T).FullName}' to be not null");
        }

        return obj;
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, params T[] others)
    {
        foreach (var element in enumerable)
            yield return element;

        foreach (var element in others)
            yield return element;
    }
}
