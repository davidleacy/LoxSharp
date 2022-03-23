namespace LoxSharp.Extensions;

/// <summary>
/// Class of <see cref="string"/> extension functions.
/// </summary>
internal static class StringExtentions
{
    /// <summary>
    /// Retrieves a substring from this instance. The substring starts at a specified character position <paramref name="startIndex"/> and ends at the <paramref name="endIndex"/> exclusive.
    /// </summary>
    /// <param name="mainString">The string instance to operate on.</param>
    /// <param name="startIndex">The start index inclusive.</param>
    /// <param name="endIndex">The end index exclusive to mirror the Java implementation used in the Crafting Interpreters book.</param>
    /// <returns>The substring.</returns>
    public static string SubstringByIndex(this string mainString, int startIndex, int endIndex) => mainString.Substring(startIndex, endIndex - startIndex);
}
