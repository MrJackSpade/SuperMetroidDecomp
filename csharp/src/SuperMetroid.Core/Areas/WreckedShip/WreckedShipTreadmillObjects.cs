using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.WreckedShip;

/// <summary>The Wrecked Ship entrance treadmill and the Maridia elevatube: their spawns and the belt row a treadmill runs.</summary>
internal sealed class WreckedShipTreadmillObjects
{
    /// <summary>The room's room objects, which the treadmill and the elevatube join.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>What the treadmill's row shows: its installed picture (#1580).</summary>
    private readonly IRoomObjectDrawing _drawing;

    /// <summary>Attaches treadmill and elevatube spawning to the room's room-object pool and installed pictures.</summary>
    internal WreckedShipTreadmillObjects(RoomObjectSlotTable slotTable, IRoomObjectDrawing drawing)
    {
        _slotTable = slotTable;
        _drawing = Ensure.NotNull(drawing);
    }
    /// <summary>
    /// Spawns one of the two treadmills at room block (4,9), then runs its shared setup at once by replacing the following
    /// 56 blocks with blank air. The treadmill's first update runs the belt when Phantoon is dead, and it then leaves the
    /// room either way.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnWreckedShipEntranceTreadmill(
        RoomLevelData level,
        WreckedShipTreadmillDirection direction,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);
        if (direction is not (WreckedShipTreadmillDirection.Rightwards or WreckedShipTreadmillDirection.Leftwards))
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown Wrecked Ship treadmill direction.");

        Position16<RoomBlocks> origin = level.GridBlock(
            new(WreckedShipTreadmillBlockDefinitions.BlockX, WreckedShipTreadmillBlockDefinitions.BlockY));
        EnsureHorizontalRangeFits(level, origin, WreckedShipTreadmillBlockDefinitions.BlockCount);

        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = origin;
        slot.Become(new WreckedShipTreadmillObject(direction, areaBossDefeated));

        // The setup writes the idle row's installed picture as air and deliberately leaves the blocks' behaviour alone.
        var idle = new RoomTerrainBlock(_drawing.SetupBlock(RoomObjectSetupBlock.TreadmillIdle), WreckedShipTreadmillBlockDefinitions.IdleCollision);
        for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
            level.SetTerrain(origin + new Offset16<RoomBlocks>(offset, 0), idle);
        return true;
    }

    /// <summary>
    /// Spawns the effectively invisible elevatube object at (1,0): it waits sixteen updates, plays the elevatube sound
    /// and leaves the room.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnMaridiaElevatube(RoomLevelData level)
    {
        Ensure.NotNull(level);
        Position16<RoomBlocks> block = level.GridBlock(MaridiaElevatubeDefinitions.RoomObjectBlock);

        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        slot.Become(new MaridiaElevatubeObject());
        return true;
    }

    /// <summary>Turns the treadmill's row, from <paramref name="origin"/>, into a running belt pushing in <paramref name="direction"/>.</summary>
    /// <param name="level">The treadmill's room.</param>
    /// <param name="origin">The row's first block.</param>
    /// <param name="direction">The belt's push direction.</param>
    /// <param name="drawing">The installed pictures, which give what the running row shows.</param>
    internal static void RunBelt(RoomLevelData level, Position16<RoomBlocks> origin, WreckedShipTreadmillDirection direction, IRoomObjectDrawing drawing)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(drawing);
        EnsureHorizontalRangeFits(level, origin, WreckedShipTreadmillBlockDefinitions.BlockCount);
        bool rightwards = direction == WreckedShipTreadmillDirection.Rightwards;

        // Both directions write the same row: the running row's installed picture as special air, with the direction
        // behaviour.
        var running = new RoomTerrainBlock(drawing.SetupBlock(RoomObjectSetupBlock.TreadmillRunning), WreckedShipTreadmillBlockDefinitions.RunningCollision);
        for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
        {
            Position16<RoomBlocks> block = origin + new Offset16<RoomBlocks>(offset, 0);
            level.SetTerrain(block, running);
            level.SetBehavior(block, rightwards
                ? WreckedShipTreadmillBlockDefinitions.RightwardsBehavior
                : WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior);
        }
    }

    /// <summary>Rejects a treadmill row that would wrap into the next room block row.</summary>
    private static void EnsureHorizontalRangeFits(
        RoomLevelData level,
        Position16<RoomBlocks> origin,
        int count)
    {
        int originX = origin.X;
        int originY = origin.Y;
        if (originX + count <= level.Size.X)
            return;
        throw new InvalidDataException(
            $"Wrecked Ship treadmill row ({originX},{originY}).." +
            $"({originX + count - 1},{originY}) exceeds a {level.Size.X}-block room row.");
    }
}
