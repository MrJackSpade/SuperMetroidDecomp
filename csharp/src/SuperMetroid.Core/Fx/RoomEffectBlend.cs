using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// How a room effect's overlay blends with the room (#1545): the effect's own description, which the import reads from
/// the room's effect. The overlay either lies over the room (<see cref="OverlayOverRoomBlend"/>) and brightens or darkens
/// whatever the frame shows under it, or lies behind the room (<see cref="OverlayBehindRoomBlend"/>) and changes the layers
/// it names where each is frontmost (#2505). Every room effect blends on every gameplay line below the HUD, through no
/// window.
/// </summary>
public abstract partial record RoomEffectBlend
{
    /// <summary>Only the two ways an overlay lies derive from this.</summary>
    private protected RoomEffectBlend()
    {
    }

    /// <summary>Whether the overlay adds its colours, brightening what it blends with, or subtracts them, darkening it.</summary>
    public required BlendOperation Operation { get; init; }
}
