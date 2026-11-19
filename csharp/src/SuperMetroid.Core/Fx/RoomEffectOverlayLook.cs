using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// What a room effect that shows on the effect overlay shows there, and how it blends (#1545, #1706, #2544): a liquid
/// (<see cref="Liquid"/>) or an atmosphere (<see cref="Atmosphere"/>), each with its blend. The effect's definition gives it
/// once, as the room loads its effect, so the frame boundary latches the overlay's kind and blend with no choice left to
/// make.
/// </summary>
public abstract record RoomEffectOverlayLook
{
    /// <summary>Keeps how the overlay blends; only the liquid and the atmosphere derive from this.</summary>
    /// <param name="blend">How the overlay blends with the room.</param>
    private RoomEffectOverlayLook(RoomEffectBlend blend) => Blend = Ensure.NotNull(blend);

    /// <summary>How the overlay blends with the room: lying over it or behind it.</summary>
    public RoomEffectBlend Blend { get; }

    /// <summary>A liquid blending as <paramref name="blend"/> describes.</summary>
    public static RoomEffectOverlayLook Liquid(RoomEffectBlend blend) => new ShowsLiquid(blend);

    /// <summary>An atmosphere blending as <paramref name="blend"/> describes.</summary>
    public static RoomEffectOverlayLook Atmosphere(RoomEffectBlend blend) => new ShowsAtmosphere(blend);

    /// <summary>
    /// What an effect of kind <paramref name="type"/> describing <paramref name="blend"/> shows on the overlay: its liquid
    /// or its atmosphere with that blend, or null for an effect that shows nothing there and so describes no blend.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// An effect that shows on the overlay describes no blend, or one that shows nothing there describes one.
    /// </exception>
    public static RoomEffectOverlayLook? Of(RoomFxType type, RoomEffectBlend? blend)
    {
        if (!RoomFxOverlayState.ShowsOnOverlay(type))
        {
            return blend is null ? null
                : throw new ArgumentException($"Room effect {type} shows nothing on the effect overlay but describes a blend.", nameof(blend));
        }
        // The import installs a blend for every effect that shows on the overlay; the room's load checks it once, here.
        RoomEffectBlend described = blend
            ?? throw new ArgumentException($"Room effect {type} shows on the effect overlay but describes no blend.", nameof(blend));
        return RoomFxTypes.ShowsLiquid(type) ? Liquid(described) : Atmosphere(described);
    }

    /// <summary>
    /// The overlay the effect captured as <paramref name="fx"/> shows with this look, its artwork drawn from
    /// <paramref name="pictures"/> for the values <paramref name="appearance"/> reads.
    /// </summary>
    /// <exception cref="InvalidDataException">The sheet pictures no artwork of the effect for those values.</exception>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    internal abstract ShownRoomEffectOverlay Latch(RoomFxOverlayRenderSnapshot fx, WrittenPictureResolver pictures, RoomAppearanceView appearance);

    /// <summary>A liquid: lava's, acid's or water's surface strip and body, shown below its surface.</summary>
    private sealed record ShowsLiquid : RoomEffectOverlayLook
    {
        /// <summary>Keeps how it blends.</summary>
        /// <param name="blend">How it blends with the room.</param>
        internal ShowsLiquid(RoomEffectBlend blend)
            : base(blend)
        {
        }

        /// <inheritdoc/>
        internal override ShownRoomEffectOverlay Latch(RoomFxOverlayRenderSnapshot fx, WrittenPictureResolver pictures, RoomAppearanceView appearance) =>
            LiquidEffectOverlay.Of(fx, pictures.Liquid(fx.Type, appearance));
    }

    /// <summary>An atmosphere: the rain's, the fog's or the spores' repeating picture.</summary>
    private sealed record ShowsAtmosphere : RoomEffectOverlayLook
    {
        /// <summary>Keeps how it blends.</summary>
        /// <param name="blend">How it blends with the room.</param>
        internal ShowsAtmosphere(RoomEffectBlend blend)
            : base(blend)
        {
        }

        /// <inheritdoc/>
        internal override ShownRoomEffectOverlay Latch(RoomFxOverlayRenderSnapshot fx, WrittenPictureResolver pictures, RoomAppearanceView appearance) =>
            AtmosphereEffectOverlay.Of(fx, pictures.Atmosphere(fx.Type, appearance));
    }
}
