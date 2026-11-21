using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.WreckedShip;

/// <summary>The Wrecked Ship entrance treadmills: their spawns and the belt row a treadmill runs.</summary>
internal sealed class WreckedShipTreadmillObjects
{
    /// <summary>The room's room objects, which the treadmill joins.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>What the treadmill's row shows: its installed picture (#1580).</summary>
    private readonly IRoomObjectDrawing _drawing;

    /// <summary>Attaches treadmill spawning to the room's room-object pool and installed pictures.</summary>
    internal WreckedShipTreadmillObjects(RoomObjectPlaceList places, IRoomObjectDrawing drawing)
    {
        _places = places;
        _drawing = Ensure.NotNull(drawing);
    }
    /// <summary>
    /// Spawns one of the two treadmills at its room block (<see cref="WreckedShipTreadmillBlockDefinitions"/>), then runs its
    /// shared setup at once by replacing the belt's following blocks with blank air. The treadmill's first update runs the belt when Phantoon is dead, and it then leaves the
    /// room either way.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnWreckedShipEntranceTreadmill(
        RoomLevelData level,
        WreckedShipTreadmillDirection direction,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);

        WreckedShipTreadmillRow row = WreckedShipTreadmillRow.In(level);
        Position16<RoomBlocks> origin = row.Origin;

        if (_places.Spawn() is not { } place)
            return false;

        place.Block = origin;
        place.Become(new WreckedShipTreadmillObject(row, direction, areaBossDefeated));

        // The setup writes the idle row's installed picture as air and deliberately leaves the blocks' behaviour alone.
        var idle = new RoomTerrainBlock(_drawing.SetupBlock(RoomObjectSetupBlock.TreadmillIdle), WreckedShipTreadmillBlockDefinitions.IdleCollision);
        for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
            level.SetTerrain(origin + new Offset16<RoomBlocks>(offset, 0), idle);
        return true;
    }

    /// <summary>Turns the treadmill's <paramref name="row"/> into a running belt pushing in <paramref name="direction"/>.</summary>
    /// <param name="level">The treadmill's room.</param>
    /// <param name="row">The row the belt writes, checked to fit the room as the treadmill spawned.</param>
    /// <param name="direction">The belt's push direction.</param>
    /// <param name="drawing">The installed pictures, which give what the running row shows.</param>
    internal static void RunBelt(RoomLevelData level, WreckedShipTreadmillRow row, WreckedShipTreadmillDirection direction, IRoomObjectDrawing drawing)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(drawing);
        Position16<RoomBlocks> origin = row.Origin;
        bool rightwards = direction == WreckedShipTreadmillDirection.Rightwards;

        // Both directions write the same row: the running row's installed picture as special air, with the direction
        // behaviour.
        var running = new RoomTerrainBlock(drawing.SetupBlock(RoomObjectSetupBlock.TreadmillRunning), WreckedShipTreadmillBlockDefinitions.RunningCollision);
        for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
        {
            Position16<RoomBlocks> block = origin + new Offset16<RoomBlocks>(offset, 0);
            level.SetTerrainAndBehavior(block, running, rightwards
                ? WreckedShipTreadmillBlockDefinitions.RightwardsBehavior
                : WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior);
        }
    }
}
