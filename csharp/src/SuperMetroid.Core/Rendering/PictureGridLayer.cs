namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's layer showing a picture grid (#1467, #1555): the backend draws each cell's picture, scrolled by an
/// offset, at the layer's place in its scene's order (#1514), clear pixels showing what lies behind. The grid does not
/// repeat: a screen pixel off the grid's cells shows what lies behind.
/// </summary>
/// <param name="Grid">The grid.</param>
/// <param name="Scroll">The grid pixel shown at the screen's top-left corner: screen pixel (x, y) shows grid pixel (x + X, y + Y).</param>
public sealed record PictureGridLayer(PictureGrid Grid, Offset16<PlanePixels> Scroll) : DisplayLayer
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
