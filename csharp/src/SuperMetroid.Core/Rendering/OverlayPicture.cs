using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What the room effect's overlay shows and where (#1514, #1706): a liquid below its surface (<see cref="LiquidOverlay"/>)
/// or an atmosphere's repeating picture (<see cref="AtmosphereOverlay"/>). Every picture repeats across at its own width,
/// moved line by line by the overlay's horizontal scroll; the backend draws the description.
/// </summary>
public abstract record OverlayPicture
{
    /// <summary>The artwork the overlay shows.</summary>
    public abstract RoomEffectArtwork Shown { get; }
}

/// <summary>
/// An atmosphere on the overlay (#1706): its picture repeating across and down at its size, each screen line showing the
/// picture's row its vertical scroll reaches.
/// </summary>
/// <param name="Artwork">The atmosphere's picture.</param>
/// <param name="Vertical">The picture's vertical scroll line by line, added to the screen line before the picture repeats.</param>
public sealed record AtmosphereOverlay(AtmosphereArtwork Artwork, LineScroll Vertical) : OverlayPicture
{
    /// <summary>The atmosphere's picture.</summary>
    public AtmosphereArtwork Artwork { get; } = Ensure.NotNull(Artwork);

    /// <summary>The picture's vertical scroll line by line.</summary>
    public LineScroll Vertical { get; } = Ensure.NotNull(Vertical);

    /// <inheritdoc/>
    public override RoomEffectArtwork Shown => Artwork;
}

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

    /// <inheritdoc/>
    public override RoomEffectArtwork Shown => Artwork;
}
