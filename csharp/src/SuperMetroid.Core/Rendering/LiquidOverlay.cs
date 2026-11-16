using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A liquid on the overlay (#1706): its surface strip with the strip's surface row on the surface's screen line, nothing
/// above the strip, and its body below the strip repeating down every body height. While the surface lies at or above the
/// screen's top line the strip does not show: the body fills every line, keeping the place in its repeat it has below the
/// strip.
/// </summary>
/// <param name="Artwork">The liquid's surface strip and body.</param>
/// <param name="SurfaceLine">The screen line the liquid's surface lies on; negative above the screen.</param>
public sealed record LiquidOverlay(LiquidArtwork Artwork, int SurfaceLine) : OverlayPicture
{
    /// <summary>The liquid's surface strip and body.</summary>
    public LiquidArtwork Artwork { get; } = Ensure.NotNull(Artwork);

    /// <summary>Whether the surface strip shows: only while the surface lies below the screen's top line.</summary>
    public bool ShowsSurface => SurfaceLine > 0;

    /// <summary>The screen line the strip's top row lies on, whether or not the strip shows; the body begins below its last row.</summary>
    public int SurfaceTopLine => SurfaceLine - Artwork.SurfaceRow;
}
