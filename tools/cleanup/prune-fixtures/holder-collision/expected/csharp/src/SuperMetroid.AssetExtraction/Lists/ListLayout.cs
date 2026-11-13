namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Where each list stands: its entry and the word just before it.</summary>
internal sealed class ListLayout
{
    /// <summary>Each list's entry.</summary>
    private readonly int[] _entries;
    /// <summary>The word just before each list.</summary>
    internal readonly int[] _prefixes;

    /// <summary>Lays the lists out.</summary>
    /// <param name="entries">Each list's entry.</param>
    /// <param name="prefixes">The word just before each list.</param>
    internal ListLayout(int[] entries, int[] prefixes)
    {
        _entries = entries;
        _prefixes = prefixes;
    }

    /// <summary>The entry of list <paramref name="list"/>.</summary>
    internal int Position(int list) => _entries[list];
}
