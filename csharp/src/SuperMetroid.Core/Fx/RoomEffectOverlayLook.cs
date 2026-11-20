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
    /// <paramref name="look"/>, which an effect of kind <paramref name="type"/> shows on the overlay: a liquid's for an
    /// effect that shows its liquid there, an atmosphere's for one that shows its atmosphere, and none for an effect that
    /// shows nothing there.
    /// </summary>
    /// <exception cref="ArgumentException">The look is not the kind of look the effect shows.</exception>
    public static RoomEffectOverlayLook? Checked(RoomFxType type, RoomEffectOverlayLook? look)
    {
        bool shows = RoomFxOverlayState.ShowsOnOverlay(type);
        if (shows != look is not null)
        {
            throw new ArgumentException(shows
                ? $"Room effect {type} shows on the effect overlay but has no look there."
                : $"Room effect {type} shows nothing on the effect overlay but has a look there.", nameof(look));
        }
        if (look is not null && look is ShowsLiquid != RoomFxTypes.ShowsLiquid(type))
            throw new ArgumentException($"Room effect {type} has the overlay look of another kind of effect: {look}.", nameof(look));
        return look;
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
