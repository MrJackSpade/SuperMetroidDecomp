namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>The title's gradient for one zoom range, described by its stops (#1468).</summary>
public sealed record TitleGradientVariant
{
    /// <summary>
    /// The gradient's stops top to bottom, the first on line 0, each on a lower line than the one before; the last band runs
    /// to the bottom of the screen.
    /// </summary>
    public required TitleGradientStop[] Stops { get; init; }
}
