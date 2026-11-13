using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>
/// The checked layout as checks compare it; the word before each list is <see cref="ListLayoutAccess.extension(ListLayout).Prefix(int)"/>.
/// </summary>
internal static class ListChecks
{
    /// <summary>The checked layout.</summary>
    private static readonly ListLayout layout = new ListLayoutConstants().Layout;

    /// <inheritdoc cref="ListLayoutAccess.extension(ListLayout).Prefix"/>
    internal static int Prefix(int list) => layout.Prefix(list);

    /// <inheritdoc cref="ListLayout.Position(int)"/>
    internal static int Entry(int list) => layout.Position(list);

    /// <summary>The number of lists, as <see cref="ListLayoutConstants2.Count(int[])"/> counts them.</summary>
    internal static int Count => ListLayoutConstants2.Count(ListEntriesConstants.Entries);
}
