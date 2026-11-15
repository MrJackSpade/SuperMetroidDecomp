using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Maridia sand animations a room's effect starts as the room loads. Selection comes from the selected room effect's
/// block animations, never from a particular room number.
/// </summary>
[RoomLifetime]
public sealed class RoomSandBlockAnimationsState
{
    /// <summary>The animations this owner plays.</summary>
    private static readonly BlockAnimation[] Played = [BlockAnimation.MaridiaSandCeiling, BlockAnimation.MaridiaSandFalling];

    /// <summary>The sand animations the current room's effect starts, in the effect's order.</summary>
    internal readonly List<BlockAnimationPlayer> animations;

    /// <summary>Spawns one room's sand animations from the door-selected room effect in the effect's order.</summary>
    /// <param name="fx">The room state's effect selection, from which <paramref name="door"/> picks the room's effect.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    public RoomSandBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door) =>
        animations = RoomEffectBlockAnimations.Start(fx, door, Played);

    /// <summary>Advances each sand animation and selects its authored frame for the next frame boundary.</summary>
    public void Step(ArtworkFrameSelections frames)
    {
        foreach (BlockAnimationPlayer animation in animations) animation.Step(frames);
    }
}
