using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A 256-pixel-wide window into the first 32 rows of a layer-3 plane drawn from its cells' pictures (#1466): the pause
/// screen's HUD plane, the HUD's images over the cleared cell. Colour zero is transparent; each picture's pixels keep the
/// priority tier their tile was drawn in.
/// </summary>
/// <param name="Plane">The plane's cell pictures.</param>
/// <param name="VerticalScroll">Vertical scroll in pixels, before the window wraps at 32 rows.</param>
/// <param name="Priority">Draws only the pixels in front (true) or at the back (false), or null for both.</param>
public sealed record Layer3ViewportLayer(Layer3Cells Plane, int VerticalScroll, bool? Priority) : DisplayLayer
{
    /// <summary>The rows of the plane the window looks into, from its first.</summary>
    public const int WindowRows = 32;

    /// <summary>The plane's cell pictures.</summary>
    public Layer3Cells Plane { get; init; } = Ensure.NotNull(Plane);

    /// <summary>Vertical scroll in pixels, 0 through <see cref="ScreenTilesViewportLayer.MaximumScroll"/>.</summary>
    public int VerticalScroll
    {
        get;
        init => field = Ensure.BetweenInclusive(0, ScreenTilesViewportLayer.MaximumScroll, value);
    } = Ensure.BetweenInclusive(0, ScreenTilesViewportLayer.MaximumScroll, VerticalScroll);
}
