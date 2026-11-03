namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// A run of consecutive inks or colours a colour group, fade or effect covers (#1512): its first, and the one after its
/// last. An ink is its colour group's place times sixteen plus its colour.
/// </summary>
/// <param name="First">The first ink of the run.</param>
/// <param name="End">The ink after the last of the run.</param>
internal readonly record struct InkRange(int First, int End)
{
    /// <summary>The run of <paramref name="count"/> inks from <paramref name="first"/>.</summary>
    internal static InkRange Run(int first, int count) => new(first, first + count);

    /// <summary>How many inks the run covers.</summary>
    internal int Count => End - First;

    /// <summary>Every ink of the run, in rising order.</summary>
    internal IEnumerable<int> Inks => Enumerable.Range(First, Count);

    /// <summary>Whether <paramref name="ink"/> lies in the run.</summary>
    internal bool Contains(int ink) => ink >= First && ink < End;
}
