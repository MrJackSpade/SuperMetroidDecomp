namespace SuperMetroid.Core.Lists;

/// <summary>The widths of the windows a list's title is shown in, for a title such as <see cref="ListSamples.Title">a sample's</see>.</summary>
internal static class ListWindows
{
    /// <summary>The width of the window production shows <paramref name="title"/> in.</summary>
    /// <param name="title">The list's title.</param>
    internal static int Width(string title) => title.Length * 8;

    /// <summary>The width of the wide window over <paramref name="title"/>, which only checks show.</summary>
    /// <param name="title">The list's title.</param>
    internal static int WideWidth(string title) => title.Length * 16;
}
