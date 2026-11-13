using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Treadmill objects the room FX record starts, independently
/// of the additional entrance object spawned by door ASM.
/// </summary>
[RoomLifetime]
public sealed class RoomTreadmillBlockAnimationsState
{
    /// <summary>Treadmill animations the current room FX record starts, in the record's order.</summary>
    internal readonly List<WreckedShipTreadmillBlockAnimationsState> objects = [];

    /// <summary>Spawns one room's objects from the door-selected FX record in the record's order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    public RoomTreadmillBlockAnimationsState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } record) return;
        foreach (BlockAnimation definition in record.BlockAnimations)
        {
            WreckedShipTreadmillDirection? direction = definition switch
            {
                BlockAnimation.WreckedShipTreadmillRightwards => WreckedShipTreadmillDirection.Rightwards,
                BlockAnimation.WreckedShipTreadmillLeftwards => WreckedShipTreadmillDirection.Leftwards,
                BlockAnimation.None or BlockAnimation.TourianStatuePhantoon or
                    BlockAnimation.TourianStatueRidley or BlockAnimation.TourianStatueKraid or
                    BlockAnimation.TourianStatueDraygon or
                    BlockAnimation.HorizontalSpikes or BlockAnimation.VerticalSpikes or
                    BlockAnimation.CrateriaLake or
                    BlockAnimation.BrinstarPlant or BlockAnimation.WreckedShipScreen or
                    BlockAnimation.MaridiaSandCeiling or BlockAnimation.MaridiaSandFalling or
                    BlockAnimation.Lava or BlockAnimation.Acid or BlockAnimation.Rain or
                    BlockAnimation.Spores => null,
                _ => throw new InvalidOperationException($"Undefined {nameof(BlockAnimation)} {definition}."),
            };
            // Other animation families retain their own owners. Selection here is by
            // object identity, not room number.
            if (direction is null) continue;
            var animation = new WreckedShipTreadmillBlockAnimationsState();
            animation.Start(direction.Value);
            objects.Add(animation);
        }
    }

    /// <summary>Runs each selected animation; its boss wait gates graphics independently of floor collision.</summary>
    public void Step(bool areaBossDefeated, ArtworkFrameSelections frames)
    {
        foreach (var animation in objects) animation.Step(areaBossDefeated, frames);
    }
}
