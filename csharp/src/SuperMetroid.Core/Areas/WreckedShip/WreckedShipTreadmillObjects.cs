using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.WreckedShip;

/// <summary>The Wrecked Ship entrance treadmill and the Maridia elevatube: their spawns and the belt row a treadmill runs.</summary>
internal sealed class WreckedShipTreadmillObjects
{
    /// <summary>Room-object slot pool used for treadmill and elevatube allocation.</summary>
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
    /// <returns>False only when all forty room-object slots are occupied.</returns>
    public bool TrySpawnWreckedShipEntranceTreadmill(
        RoomLevelData level,
        WreckedShipTreadmillDirection direction,
        bool areaBossDefeated)
    {
        Ensure.NotNull(level);

        RoomObjectHeaderId header = direction switch
        {
            WreckedShipTreadmillDirection.Rightwards =>
                RoomObjectHeaderId.WreckedShipEntranceTreadmillFromWest,
            WreckedShipTreadmillDirection.Leftwards =>
                RoomObjectHeaderId.WreckedShipEntranceTreadmillFromEast,
            _ => throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Unknown Wrecked Ship treadmill direction."),
        };

        int origin = level.GetBlockIndex(
            new(WreckedShipTreadmillBlockDefinitions.BlockX, WreckedShipTreadmillBlockDefinitions.BlockY));
        EnsureHorizontalRangeFits(level, origin, WreckedShipTreadmillBlockDefinitions.BlockCount);

        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = header;
        slot.BlockIndex = origin;
        slot.Become(new WreckedShipTreadmillObject(direction, areaBossDefeated));

        // The setup writes the idle row's installed picture as air and deliberately leaves the blocks' behaviour alone.
        var idle = new RoomTerrainBlock(_drawing.SetupBlock(RoomObjectSetupBlock.TreadmillIdle), WreckedShipTreadmillBlockDefinitions.IdleCollision);
        for (int offset = 0; offset < WreckedShipTreadmillBlockDefinitions.BlockCount; offset++)
        {
            int blockIndex = origin + offset;
            level.SetTerrain(blockIndex, idle);
        }
        return true;
    }

    /// <summary>
    /// Spawns the effectively invisible elevatube object at (1,0): it waits sixteen updates, plays the elevatube sound
    /// and leaves the room.
    /// </summary>
    /// <returns>False only when all forty room-object slots are occupied.</returns>
    public bool TrySpawnMaridiaElevatube(RoomLevelData level)
    {
        Ensure.NotNull(level);
        int blockIndex = level.GetBlockIndex(MaridiaElevatubeDefinitions.RoomObjectBlock);

        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = RoomObjectHeaderId.MaridiaElevatube;
        slot.BlockIndex = blockIndex;
        slot.Become(new MaridiaElevatubeObject());
        return true;
    }

    /// <summary>Turns the treadmill's row, from <paramref name="origin"/>, into a running belt pushing in <paramref name="direction"/>.</summary>
    /// <param name="level">The treadmill's room.</param>
    /// <param name="origin">The row's first block.</param>
    /// <param name="direction">The belt's push direction.</param>
    /// <param name="drawing">The installed pictures, which give what the running row shows.</param>
    internal static void RunBelt(RoomLevelData level, int origin, WreckedShipTreadmillDirection direction, IRoomObjectDrawing drawing)
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
            int blockIndex = origin + offset;
            level.SetTerrain(blockIndex, running);
            level.SetBehavior(blockIndex, rightwards
                ? WreckedShipTreadmillBlockDefinitions.RightwardsBehavior
                : WreckedShipTreadmillBlockDefinitions.LeftwardsBehavior);
        }
    }

    /// <summary>Rejects a treadmill row that would wrap into the next room block row.</summary>
    private static void EnsureHorizontalRangeFits(
        RoomLevelData level,
        int origin,
        int count)
    {
        int originX = origin % level.Size.X;
        int originY = origin / level.Size.X;
        if (originX + count <= level.Size.X)
            return;
        throw new InvalidDataException(
            $"Wrecked Ship treadmill row ({originX},{originY}).." +
            $"({originX + count - 1},{originY}) exceeds a {level.Size.X}-block room row.");
    }
}
