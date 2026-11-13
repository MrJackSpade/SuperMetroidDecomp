namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The entries a list runs over.</summary>
internal sealed class ListBounds
{
    /// <summary>The list's first and last entries.</summary>
    internal readonly int first, last;

    /// <summary>Bounds from <paramref name="first"/> to <paramref name="last"/>.</summary>
    /// <param name="first">The list's first entry.</param>
    /// <param name="last">The list's last entry.</param>
    internal ListBounds(int first, int last)
    {
        this.first = first;
        this.last = last;
    }

    /// <summary>How many entries the list runs over, which production reads.</summary>
    internal int Length => last - first + 1;
}
