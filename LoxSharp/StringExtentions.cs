namespace LoxSharp;

internal static class StringExtentions
{
    public static string SubstringByIndex(this string mainString, int startIndex, int endIndex) => mainString.Substring(startIndex, endIndex - startIndex);
}
