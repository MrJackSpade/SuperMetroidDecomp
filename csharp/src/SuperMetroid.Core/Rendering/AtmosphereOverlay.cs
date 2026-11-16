using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Rendering;

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
}
