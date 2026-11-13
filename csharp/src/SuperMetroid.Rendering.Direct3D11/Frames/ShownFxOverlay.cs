using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// What the room level buffer packs of the effect overlay a frame shows (#1706): its artwork, and for a liquid whether its
/// surface strip shows. Where the overlay lies on the screen changes every frame and reaches the shader with each pass's
/// scrolls, so moving it repacks nothing.
/// </summary>
/// <param name="Artwork">The liquid's strip and body, or the atmosphere's picture.</param>
/// <param name="ShowsSurface">Whether a liquid's surface strip shows; false for an atmosphere.</param>
internal readonly record struct ShownFxOverlay(RoomEffectArtwork Artwork, bool ShowsSurface)
{
    /// <summary>What the room level buffer packs of <paramref name="picture"/>, or null where the overlay shows nothing.</summary>
    internal static ShownFxOverlay? Of(OverlayPicture? picture) => picture switch
    {
        null => null,
        LiquidOverlay liquid => new ShownFxOverlay(liquid.Artwork, liquid.ShowsSurface),
        AtmosphereOverlay atmosphere => new ShownFxOverlay(atmosphere.Artwork, false),
        _ => throw new ArgumentOutOfRangeException(nameof(picture), picture, "Undefined effect overlay picture."),
    };

    /// <summary>
    /// The vertical scroll a pass showing <paramref name="picture"/> takes: an atmosphere's own; a liquid's places each
    /// line at its row counted down from the surface strip's top line.
    /// </summary>
    internal static LineScroll VerticalScroll(OverlayPicture? picture) => picture switch
    {
        null => ScrollBands.Uniform(0),
        LiquidOverlay liquid => ScrollBands.Uniform(-liquid.SurfaceTopLine),
        AtmosphereOverlay atmosphere => atmosphere.Vertical,
        _ => throw new ArgumentOutOfRangeException(nameof(picture), picture, "Undefined effect overlay picture."),
    };
}
