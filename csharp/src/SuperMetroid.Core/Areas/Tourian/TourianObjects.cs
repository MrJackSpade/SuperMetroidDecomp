using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Tourian's room objects: the access floor, the Metroid rooms' clearance watches and the old escape shaft's fake wall.</summary>
[RoomLifetime]
internal sealed class TourianObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;
    /// <summary>Forty-slot PLM table scanned from the final slot during hardcoded spawns.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds Tourian room callbacks to the active room's services and PLM slots.</summary>
    internal TourianObjects(IRoomSamusAccess? samus, IRoomObjectEnemyEffects? enemies, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _enemies = enemies;
        _slotTable = slotTable;
    }
    /// <summary>
    /// Sets up a Metroid room's clearance watch: its placement's entry selects the event it marks, or none for the
    /// table's nine leading entries, and it stays in the room for good.
    /// </summary>
    internal static void SetupMetroidsClearedSlot(RoomObjectSlot slot)
    {
        Ensure.NotNull(slot);
        slot.Become(new MetroidRoomClearanceObject(MetroidsClearedEvents.ResolveEvent((slot.Argument as MetroidRoomArgument
            ?? throw new InvalidDataException($"Metroids-cleared PLM at block {slot.BlockIndex} was placed without its entry.")).Entry)));
    }

    /// <summary>
    /// Runs the room setup that spawns the fake wall at block (16,135), whose setup clears the block's collision before
    /// the wall's first update.
    /// </summary>
    /// <returns>False only when all 40 PLM slots are occupied.</returns>
    public bool TrySpawnOldTourianEscapeShaftWall(RoomLevelData level)
    {
        Ensure.NotNull(level);
        int blockIndex = level.GetBlockIndex(
            new(OldTourianEscapeShaftWallObjectDefinitions.BlockX, OldTourianEscapeShaftWallObjectDefinitions.BlockY));
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreTerrain = RoomObjectSlotDefinitions.ClearedRestoreTerrain;
        slot.Header = RoomObjectHeaderId.OldTourianEscapeShaftFakeWall;
        DoorObjects.SetupDoorTransitionDeactivatedSlot(level, slot);
        slot.Become(new OldTourianWallObject(_samus, _enemies));
        return true;
    }
    /// <summary>Spawns the clear or crumble of the access floor at block (6,12).</summary>
    public bool TrySpawnTourianAccess(RoomLevelData level, TourianAccessFloor floor)
    {
        TourianAccessObjectDefinition definition = TourianAccessFloors.ForState(floor);
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = definition.Header;
        slot.BlockIndex = level.GetBlockIndex(new(6, 12));
        slot.Become(new TerrainChangeObject(definition.Change));
        return true;
    }
}
