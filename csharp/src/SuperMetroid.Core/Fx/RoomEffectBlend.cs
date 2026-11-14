using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// How a room effect's overlay blends with the room (#1545): the effect's own description, which the import reads from
/// the room's effect. The overlay either lies over the room and brightens or darkens whatever the frame shows under it,
/// or lies behind the room and changes the layers it names where each is frontmost. Every room effect blends on every
/// gameplay line below the HUD, through no window.
/// </summary>
public sealed record RoomEffectBlend
{
    /// <summary>Whether the overlay adds its colours, brightening what it blends with, or subtracts them, darkening it.</summary>
    public required BlendOperation Operation { get; init; }

    /// <summary>
    /// What the overlay, lying behind the room, changes where each is frontmost; null when the overlay lies over the room,
    /// as the liquids', the fog's and the rain's do.
    /// </summary>
    public BlendedLayers? BlendsBehind { get; init; }

    /// <summary>
    /// Whether the room's background lies behind with the overlay, so that the screen shows the background only through
    /// the blend, as the waterfall rooms show it.
    /// </summary>
    public bool BackgroundBehind { get; init; }
}
