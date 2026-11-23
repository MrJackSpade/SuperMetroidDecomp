using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// A room effect's overlay lying behind the room (#1545, #2505), as the spores and the waterfalls do: it changes the
/// layers it names where each is frontmost.
/// </summary>
public sealed partial record OverlayBehindRoomBlend : RoomEffectBlend
{
    /// <summary>What the overlay, lying behind the room, changes where each is frontmost.</summary>
    public required BlendedLayers Blended { get; init; }

    /// <summary>
    /// Whether the room's background lies behind with the overlay, so that the screen shows the background only through
    /// the blend, as the waterfall rooms show it.
    /// </summary>
    public bool BackgroundBehind { get; init; }
}
