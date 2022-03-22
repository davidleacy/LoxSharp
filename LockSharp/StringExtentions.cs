namespace LoxSharp;

internal static class StringExtentions
{
    public static string SubstringByIndex(this string mainString, int startIndex, int endIndex)
    {
        return mainString.Substring(startIndex, endIndex - startIndex);
    }
}
