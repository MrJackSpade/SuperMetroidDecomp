namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The box of what a rectangle of an image holds, grown row by row as held rows are found (#2558).</summary>
/// <param name="Left">The leftmost held place.</param>
/// <param name="Right">The rightmost held place.</param>
/// <param name="Top">The first held row.</param>
/// <param name="Bottom">The last held row.</param>
internal readonly record struct ImageContentBox(int Left, int Right, int Top, int Bottom)
{
    /// <summary>The box of the first held row <paramref name="row"/>, holding <paramref name="run"/>.</summary>
    internal static ImageContentBox OfRow(ImageRowRun run, int row) => new(run.First, run.Last, row, row);

    /// <summary>This box grown to take in held row <paramref name="row"/>, holding <paramref name="run"/>, below its rows.</summary>
    internal ImageContentBox Including(ImageRowRun run, int row) =>
        new(Math.Min(Left, run.First), Math.Max(Right, run.Last), Top, row);
}
