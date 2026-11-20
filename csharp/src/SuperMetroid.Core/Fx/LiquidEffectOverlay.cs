using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// A liquid on the effect overlay (#1706, #2505): lava's, acid's or water's surface strip and body, shown below the
/// liquid's surface, scrolling across, water's rippling with its surface wave.
/// </summary>
public sealed record LiquidEffectOverlay : ShownRoomEffectOverlay
{
    /// <summary>Keeps the liquid's artwork, where it lies, its wave and its blend.</summary>
    /// <param name="artwork">The surface strip and the body.</param>
    /// <param name="blend">How the liquid blends with the room.</param>
    /// <param name="horizontalScroll">The overlay's scroll across; a liquid lies down the screen where its surface line places it.</param>
    /// <param name="surfaceWavePhase">The phase of water's surface wave, or null for lava and acid, whose surface does not ripple.</param>
    /// <param name="surfaceScreenLine">
    /// The screen line of the liquid's surface, or null while the liquid has no surface, its surface lies above the room or
    /// the frame has placed none on the screen: the overlay then shows no liquid at all.
    /// </param>
    /// <param name="shownSurfaceLine">
    /// The screen line the overlay shows the artwork from, where the effect last placed the liquid on the screen, or null
    /// while it last placed it with its surface above the room: the overlay shows no artwork until the effect places it
    /// again.
    /// </param>
    public LiquidEffectOverlay(LiquidArtwork artwork, RoomEffectBlend blend, int horizontalScroll, int? surfaceWavePhase,
        int? surfaceScreenLine, int? shownSurfaceLine)
        : base(blend)
    {
        Artwork = Ensure.NotNull(artwork);
        HorizontalScroll = horizontalScroll;
        SurfaceWavePhase = surfaceWavePhase;
        SurfaceScreenLine = surfaceScreenLine;
        ShownSurfaceLine = shownSurfaceLine;
    }

    /// <summary>The surface strip and the body.</summary>
    public LiquidArtwork Artwork { get; }

    /// <summary>The overlay's scroll across; a liquid lies down the screen where its surface line places it.</summary>
    public int HorizontalScroll { get; }

    /// <summary>The phase of water's surface wave, or null for lava and acid, whose surface does not ripple.</summary>
    public int? SurfaceWavePhase { get; }

    /// <summary>
    /// The screen line of the liquid's surface, or null while the liquid has no surface, its surface lies above the room or
    /// the frame has placed none on the screen: the overlay then shows no liquid at all.
    /// </summary>
    public int? SurfaceScreenLine { get; }

    /// <summary>
    /// The screen line the overlay shows the artwork from, where the effect last placed the liquid on the screen (#1706), or
    /// null while it last placed it with its surface above the room: the overlay shows no artwork until the effect places it
    /// again, even where the surface has since returned to the room.
    /// </summary>
    public int? ShownSurfaceLine { get; }

    /// <summary>The liquid the effect captured as <paramref name="fx"/> shows with <paramref name="artwork"/>.</summary>
    /// <param name="fx">The liquid's display state.</param>
    /// <param name="artwork">Its surface strip and body.</param>
    public static LiquidEffectOverlay Of(RoomFxOverlayRenderSnapshot fx, LiquidArtwork artwork) =>
        new(artwork, fx.Blend, fx.Scroll.X,
            RoomFxTypes.UsesWater(fx.Type) ? fx.WaterOverlayWavePhase : null,
            // A tide can carry the surface above the room's top, to a negative room line, where the game's below-zero
            // check finds no surface to show.
            fx.Surface is >= 0 ? fx.WaterSurfaceScreenY : null,
            fx.LiquidSurfaceLine);

    /// <inheritdoc/>
    public override TResult Map<TResult>(IRoomEffectOverlayMapping<TResult> mapping) => Ensure.NotNull(mapping).Liquid(this);
}
