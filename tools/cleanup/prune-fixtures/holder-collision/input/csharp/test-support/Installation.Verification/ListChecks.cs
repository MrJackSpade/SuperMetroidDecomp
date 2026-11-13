using SuperMetroid.AssetExtraction.Lists;

namespace SuperMetroid.Verification.Lists;

/// <summary>
/// The checked layout as checks compare it; the word before each list is <see cref="ListLayout.Prefix(int)"/>.
/// </summary>
internal static class ListChecks
{
    /// <summary>The checked layout.</summary>
    private static readonly ListLayout layout = new ListLayoutConstants().Layout;

    /// <inheritdoc cref="ListLayout.Prefix"/>
    internal static int Prefix(int list) => layout.Prefix(list);

    /// <inheritdoc cref="ListLayout.Position(int)"/>
    internal static int Entry(int list) => layout.Position(list);

    /// <summary>The number of lists, as <see cref="ListLayout.Count(int[])"/> counts them.</summary>
    internal static int Count => ListLayout.Count(ListEntriesConstants.Entries);
}
