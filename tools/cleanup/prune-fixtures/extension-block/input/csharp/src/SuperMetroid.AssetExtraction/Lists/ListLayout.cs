namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>A run of lists, each with its entry.</summary>
/// <param name="entries">Each list's entry.</param>
internal sealed class ListLayout(int[] entries)
{
    /// <summary>The number of lists.</summary>
    internal int Count => entries.Length;

    /// <summary>The entry of list <paramref name="list"/>.</summary>
    internal int Entry(int list) => entries[list];
}

/// <summary>One list of a layout.</summary>
/// <param name="layout">The layout holding the list.</param>
/// <param name="index">The list's index in the layout.</param>
internal sealed class ListEntry(ListLayout layout, int index)
{
    /// <summary>The layout holding the list.</summary>
    internal ListLayout Layout => layout;

    /// <summary>The list's index in the layout.</summary>
    internal int Index => index;
}
