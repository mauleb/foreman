namespace Foreman.Core;

public static class StringExtensions {
    public static string ToCamelCase(this string value)
        => string.Format("{0}{1}",
            value.Substring(0,1).ToLower(),
            value.Substring(1));
}