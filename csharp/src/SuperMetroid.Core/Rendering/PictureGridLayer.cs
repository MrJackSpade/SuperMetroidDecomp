namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's layer showing a picture grid (#1467): the backend draws each cell's picture, scrolled and wrapping at
/// the grid's edges, each pixel in the priority tier its picture was drawn in, colour zero showing what lies behind.
/// </summary>
/// <param name="Grid">The grid.</param>
/// <param name="Scroll">The grid pixel shown at the screen's top-left corner.</param>
/// <param name="Priority">Draws only the pixels in front (true) or at the back (false), or null for both.</param>
public sealed record PictureGridLayer(PictureGrid Grid, Position16<TilePlanePixels> Scroll, bool? Priority) : DisplayLayer
{
    /// <summary>The grid.</summary>
    public PictureGrid Grid
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Grid);
}
