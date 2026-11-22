using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// An atmosphere on the effect overlay (#1706, #2505): the rain's, the fog's or the spores' picture, repeating across and
/// down at its scroll.
/// </summary>
public sealed partial record AtmosphereEffectOverlay : ShownRoomEffectOverlay
{
    /// <summary>Keeps the atmosphere's artwork, scroll and blend.</summary>
    /// <param name="artwork">The repeating picture.</param>
    /// <param name="scroll">The overlay's scroll across and down.</param>
    /// <param name="blend">How the atmosphere blends with the room.</param>
    public AtmosphereEffectOverlay(AtmosphereArtwork artwork, Position16<OverlayPixels> scroll, RoomEffectBlend blend)
        : base(blend)
    {
        Artwork = Ensure.NotNull(artwork);
        Scroll = scroll;
    }

    /// <summary>The repeating picture.</summary>
    public AtmosphereArtwork Artwork { get; }

    /// <summary>The overlay's scroll across and down.</summary>
    public Position16<OverlayPixels> Scroll { get; }

    /// <summary>The atmosphere the effect captured as <paramref name="fx"/> shows with <paramref name="artwork"/>.</summary>
    /// <param name="fx">The atmosphere's display state.</param>
    /// <param name="artwork">Its repeating picture.</param>
    public static AtmosphereEffectOverlay Of(RoomFxOverlayRenderSnapshot fx, AtmosphereArtwork artwork) =>
        new(artwork, fx.Scroll, fx.Blend);

    /// <inheritdoc/>
    public override TResult Map<TResult>(IRoomEffectOverlayMapping<TResult> mapping) => Ensure.NotNull(mapping).Atmosphere(this);
}
