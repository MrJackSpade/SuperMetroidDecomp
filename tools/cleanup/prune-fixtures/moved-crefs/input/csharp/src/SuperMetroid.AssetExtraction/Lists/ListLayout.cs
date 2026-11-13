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
