namespace SuperMetroid.Core.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListWindows"/> that only verification reads.</summary>
internal static class ListWindowsConstants
{
    /// <summary>The width of the wide window over <paramref name="title"/>, which only checks show.</summary>
    /// <param name="title">The list's title.</param>
    internal static int WideWidth(string? title) => ListWindows.Checked(title).Length * 16;
}
