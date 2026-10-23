namespace SuperMetroid.Core.Assets;

/// <summary>The <paramref name="Count"/> background colours from colour <paramref name="First"/> of all eight sets (#1405).</summary>
/// <param name="First">The first colour, counted across the sets.</param>
/// <param name="Count">The number of colours.</param>
public readonly record struct BackgroundColourRun(int First, int Count)
{
    /// <summary>These colours of <paramref name="sets"/>.</summary>
    public ColourRun In(BackgroundColourSets sets) => Ensure.NotNull(sets).All.Slice(First, Count);
}
