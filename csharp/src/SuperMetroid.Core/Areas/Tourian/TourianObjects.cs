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
    /// <summary>The room's room objects, which the fake wall and the access floor's change join.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds Tourian room callbacks to the active room's services and room-object slots.</summary>
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
            ?? throw new InvalidDataException($"Metroids-cleared room object at block {slot.Block} was placed without its entry.")).Entry)));
    }

    /// <summary>
    /// Runs the room setup that spawns the fake wall at block (16,135), whose setup clears the block's collision before
    /// the wall's first update.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnOldTourianEscapeShaftWall(RoomLevelData level)
    {
        Ensure.NotNull(level);
        Position16<RoomBlocks> block = level.GridBlock(
            new(OldTourianEscapeShaftWallObjectDefinitions.BlockX, OldTourianEscapeShaftWallObjectDefinitions.BlockY));
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = block;
        DoorObjects.SetupDoorTransitionDeactivatedSlot(level, slot);
        slot.Become(new OldTourianWallObject(_samus, _enemies));
        return true;
    }
    /// <summary>Spawns the clear or crumble of the access floor at block (6,12).</summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnTourianAccess(RoomLevelData level, TourianAccessFloor floor)
    {
        TourianAccessObjectDefinition definition = TourianAccessFloors.ForState(floor);
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = level.GridBlock(new(6, 12));
        slot.Become(new TerrainChangeObject(definition.Change));
        return true;
    }
}
