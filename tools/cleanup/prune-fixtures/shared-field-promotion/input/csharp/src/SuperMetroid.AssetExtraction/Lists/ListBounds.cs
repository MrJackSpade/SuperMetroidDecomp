namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The entries a list runs over.</summary>
internal sealed class ListBounds
{
    /// <summary>The list's first and last entries.</summary>
    private readonly int _first, _last;

    /// <summary>Bounds from <paramref name="first"/> to <paramref name="last"/>.</summary>
    /// <param name="first">The list's first entry.</param>
    /// <param name="last">The list's last entry.</param>
    internal ListBounds(int first, int last)
    {
        _first = first;
        _last = last;
    }

    /// <summary>How many entries the list runs over, which production reads.</summary>
    internal int Length => _last - _first + 1;

    /// <summary>The list's first entry, which only checks read.</summary>
    internal int Start => _first;
}
