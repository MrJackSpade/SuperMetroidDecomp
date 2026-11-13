using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Treadmill objects the room FX record starts, independently
/// of the additional entrance object spawned by door ASM.
/// </summary>
[RoomLifetime]
public sealed class RoomTreadmillAnimatedTilesState
{
    /// <summary>Treadmill animations the current room FX record starts, in the record's order.</summary>
    internal readonly List<WreckedShipTreadmillAnimatedTilesState> objects = [];

    /// <summary>Spawns one room's objects from the door-selected FX record in the record's order.</summary>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    public RoomTreadmillAnimatedTilesState(RoomFxSelection? fx, DoorDefinition? door)
    {
        if (fx?.Select(door) is not { } record) return;
        foreach (AnimatedTileObject definition in record.AnimatedTiles)
        {
            WreckedShipTreadmillDirection? direction = definition switch
            {
                AnimatedTileObject.WreckedShipTreadmillRightwards => WreckedShipTreadmillDirection.Rightwards,
                AnimatedTileObject.WreckedShipTreadmillLeftwards => WreckedShipTreadmillDirection.Leftwards,
                AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or
                    AnimatedTileObject.TourianStatueRidley or AnimatedTileObject.TourianStatueKraid or
                    AnimatedTileObject.TourianStatueDraygon or
                    AnimatedTileObject.HorizontalSpikes or AnimatedTileObject.VerticalSpikes or
                    AnimatedTileObject.CrateriaLake or
                    AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen or
                    AnimatedTileObject.MaridiaSandCeiling or AnimatedTileObject.MaridiaSandFalling or
                    AnimatedTileObject.Lava or AnimatedTileObject.Acid or AnimatedTileObject.Rain or
                    AnimatedTileObject.Spores => null,
                _ => throw new InvalidOperationException($"Undefined AnimatedTileObject {definition}."),
            };
            // Other animation families retain their own owners. Selection here is by
            // object identity, not room number.
            if (direction is null) continue;
            var animation = new WreckedShipTreadmillAnimatedTilesState();
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
