namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The first and last places of a row of an image that hold something (#2558).</summary>
/// <param name="First">The first place in the row that holds something.</param>
/// <param name="Last">The last place in the row that holds something.</param>
internal readonly partial record struct ImageRowRun(int First, int Last)
{
    /// <summary>The run spanning this run and <paramref name="other"/>.</summary>
    internal ImageRowRun Spanning(ImageRowRun other) => new(Math.Min(First, other.First), Math.Max(Last, other.Last));
}
