namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's layer showing a picture grid (#1467): the backend draws each cell's picture, scrolled and wrapping at
/// the grid's edges, at the layer's place in its scene's order (#1514), colour zero showing what lies behind.
/// </summary>
/// <param name="Grid">The grid.</param>
/// <param name="Scroll">The grid pixel shown at the screen's top-left corner.</param>
public sealed record PictureGridLayer(PictureGrid Grid, Position16<PlanePixels> Scroll) : DisplayLayer
{
    /// <summary>The grid.</summary>
    public PictureGrid Grid
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Grid);

    /// <summary>
    /// The screen pixels the grid shows on (#1514), as the pause map shows only through its frame's opening; null for the
    /// whole screen.
    /// </summary>
    public ScreenWindow? Window { get; init; }
}
