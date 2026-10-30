namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A frontend screen's layer of placed pictures (#1467): the backend draws its pictures, scrolled, each pixel in the
/// priority tier its picture was drawn in, colour zero showing what lies behind. The layer repeats every
/// <see cref="RepeatPixels"/> pixels across and down, as a menu's page wraps when it scrolls.
/// </summary>
/// <param name="Pictures">The layer's placed pictures.</param>
/// <param name="Scroll">The layer pixel shown at the screen's top-left corner.</param>
/// <param name="Priority">Draws only the pixels in front (true) or at the back (false), or null for both.</param>
public sealed record PicturesLayer(PlacedPictures Pictures, Position16<TilePlanePixels> Scroll, bool? Priority) : DisplayLayer
{
    /// <summary>The pixels across and down after which the layer repeats: one screen's width, its page's side.</summary>
    public const int RepeatPixels = ScreenGeometry.ScreenWidthPixels;

    /// <summary>The layer's placed pictures.</summary>
    public PlacedPictures Pictures
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Pictures);
}
