using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// Tourian's room objects: the access floor, the Metroid rooms' clearance watches, the old escape shaft's fake wall and
/// the acid trigger of the escape room before it.
/// </summary>
[RoomLifetime]
internal sealed class TourianObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects _enemies;
    /// <summary>The room's liquid motion, which the escape acid trigger starts.</summary>
    private readonly IRoomFxMotion _roomFx;
    /// <summary>The room's room objects, which the fake wall, the acid trigger and the access floor's change join.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds Tourian room behaviours to the active room's services and room-object places.</summary>
    internal TourianObjects(IRoomSamusAccess samus, IRoomObjectEnemyEffects enemies, IRoomFxMotion roomFx,
        RoomObjectPlaceList places)
    {
        _samus = samus;
        _enemies = enemies;
        _roomFx = roomFx;
        _places = places;
    }
    /// <summary>
    /// Sets up a Metroid room's clearance watch: its placement (<paramref name="placed"/>) names the event it marks, and it
    /// stays in the room for good.
    /// </summary>
    internal static void SetupMetroidsCleared(RoomObjectPlace place, MetroidRoomArgument placed)
    {
        Ensure.NotNull(place);
        place.Become(new MetroidRoomClearanceObject(Ensure.NotNull(placed).Cleared));
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
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        DoorObjects.SetupPassableBlock(level, place);
        place.Become(new OldTourianWallObject(_samus, _enemies));
        return true;
    }
    /// <summary>
    /// Runs the room setup that spawns the acid trigger at block (16,16), whose setup leaves a solid block solid and makes
    /// any other block air, as the fake wall's does.
    /// </summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnEscapeAcidRise(RoomLevelData level)
    {
        Ensure.NotNull(level);
        Position16<RoomBlocks> block = level.GridBlock(
            new(EscapeAcidRiseObjectDefinitions.BlockX, EscapeAcidRiseObjectDefinitions.BlockY));
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = block;
        DoorObjects.SetupPassableBlock(level, place);
        place.Become(new EscapeAcidRiseObject(_samus, _roomFx));
        return true;
    }
    /// <summary>Spawns the clear or crumble of the access floor at its block (<see cref="TourianAccessObjectDefinitions.FloorBlock"/>).</summary>
    /// <returns>False when the room is full; nothing is spawned then (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnTourianAccess(RoomLevelData level, TourianAccessFloor floor)
    {
        TerrainChange change = TourianAccessFloors.ForState(floor);
        if (_places.Spawn() is not { } place)
            return false;

        place.Block = level.GridBlock(TourianAccessObjectDefinitions.FloorBlock);
        place.Become(new TerrainChangeObject(change));
        return true;
    }
}
