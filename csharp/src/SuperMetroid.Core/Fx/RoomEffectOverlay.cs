using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// What the room effect shows on the effect overlay as a frame boundary latched it (#1706, #2505): nothing
/// (<see cref="Nothing"/>), an atmosphere with its artwork, scroll and blend (<see cref="AtmosphereEffectOverlay"/>), or a
/// liquid with its artwork, where its surface lies, its surface's wave and its blend (<see cref="LiquidEffectOverlay"/>).
/// Whether the overlay lies over the room or behind it is its blend's kind (<see cref="ShownRoomEffectOverlay.Blend"/>).
/// </summary>
public abstract record RoomEffectOverlay
{
    /// <summary>Only the kinds of overlay below derive from this.</summary>
    private protected RoomEffectOverlay()
    {
    }

    /// <summary>The overlay of a room whose effect shows nothing on it.</summary>
    public static RoomEffectOverlay Nothing { get; } = new NoRoomEffectOverlay();

    /// <summary>The value <paramref name="mapping"/> gives this kind of overlay.</summary>
    /// <typeparam name="TResult">What each kind of overlay maps to.</typeparam>
    /// <param name="mapping">What each kind of overlay maps to.</param>
    public abstract TResult Map<TResult>(IRoomEffectOverlayMapping<TResult> mapping);

    /// <summary>
    /// The overlay the room effect captured as <paramref name="fx"/> shows (#2505): nothing without an effect showing one,
    /// otherwise the liquid or the atmosphere its look shows (#2544), its artwork drawn from <paramref name="pictures"/> for the values
    /// <paramref name="appearance"/> reads.
    /// </summary>
    /// <param name="fx">The room effect's display state, or null while the room's effect shows nothing on the overlay.</param>
    /// <param name="pictures">The room's effect overlay pictures.</param>
    /// <param name="appearance">The values the room's appearance inputs take now.</param>
    /// <exception cref="InvalidDataException">The sheet pictures no artwork of the effect for those values.</exception>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    internal static RoomEffectOverlay Latch(RoomFxOverlayRenderSnapshot? fx, WrittenPictureResolver pictures, RoomAppearanceView appearance) =>
        fx is { } shown ? shown.Look.Latch(shown, pictures, appearance) : Nothing;
}
