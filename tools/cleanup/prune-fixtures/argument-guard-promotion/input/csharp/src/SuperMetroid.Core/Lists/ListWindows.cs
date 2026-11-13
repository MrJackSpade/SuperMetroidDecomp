namespace SuperMetroid.Core.Lists;

/// <summary>The widths of the windows a list's title is shown in.</summary>
internal static class ListWindows
{
    /// <summary>The width of the window production shows <paramref name="title"/> in.</summary>
    /// <param name="title">The list's title.</param>
    internal static int Width(string? title) => Checked(title).Length * 8;

    /// <summary>The width of the wide window over <paramref name="title"/>, which only checks show.</summary>
    /// <param name="title">The list's title.</param>
    internal static int WideWidth(string? title) => Checked(title).Length * 16;

    /// <summary><paramref name="title"/>, which a list always has.</summary>
    /// <param name="title">The list's title.</param>
    private static string Checked(string? title)
    {
        if (title is null)
            throw new ArgumentNullException(nameof(title));
        return title;
    }
}
