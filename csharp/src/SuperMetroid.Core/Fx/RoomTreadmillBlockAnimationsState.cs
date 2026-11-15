using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The Wrecked Ship treadmill animations the room's effect starts, independently of the treadmill an entrance door's
/// setup starts (<see cref="WreckedShipTreadmillBlockAnimationsState"/>).
/// </summary>
[RoomLifetime]
public sealed class RoomTreadmillBlockAnimationsState
{
    /// <summary>The animations this owner plays.</summary>
    private static readonly BlockAnimation[] Played =
        [BlockAnimation.WreckedShipTreadmillRightwards, BlockAnimation.WreckedShipTreadmillLeftwards];

    /// <summary>Treadmill animations the current room's effect starts, in the effect's order.</summary>
    internal readonly List<BlockAnimationPlayer> objects;

    /// <summary>Spawns one room's treadmill animations from the door-selected room effect in the effect's order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    public RoomTreadmillBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door) =>
        objects = RoomEffectBlockAnimations.Start(fx, door, Played);

    /// <summary>
    /// Runs each treadmill animation; each waits for the area's boss before its first frame
    /// (<see cref="BlockAnimationStart.OnceAreaBossIsDefeated"/>).
    /// </summary>
    public void Step(bool areaBossDefeated, ArtworkFrameSelections frames)
    {
        foreach (BlockAnimationPlayer animation in objects) animation.Step(areaBossDefeated, frames);
    }
}
