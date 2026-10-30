namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>The title's gradient for one zoom bucket, described by its stops (#1468).</summary>
public sealed record TitleGradientVariant
{
    /// <summary>Zero through fifteen: the zoom bucket the title selects this gradient in.</summary>
    public required int ZoomHighNibble { get; init; }
    /// <summary>
    /// The gradient's stops top to bottom, the first on line 0, each on a lower line than the one before; the last band runs
    /// to the bottom of the screen.
    /// </summary>
    public required TitleGradientStop[] Stops { get; init; }
}
