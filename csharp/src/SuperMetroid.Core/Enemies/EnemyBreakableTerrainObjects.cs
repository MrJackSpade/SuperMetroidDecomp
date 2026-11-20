using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies;

/// <summary>Terrain room objects that enemies break, spawned by enemy contact instead of Samus or projectiles.</summary>
internal sealed class EnemyBreakableTerrainObjects
{
    /// <summary>The room's room objects, which an enemy's break joins.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Uses the room's room objects, so an enemy's break takes a place like every other room object.</summary>
    internal EnemyBreakableTerrainObjects(RoomObjectPlaceList places)
    {
        _places = places;
    }
    /// <summary>
    /// Spawns the breakable block's break: take a place, clear the block's collision at once and keep its picture and what
    /// it shows until the break draws, as the setup rewrites only the block's type (#2502). A full room skips the setup and
    /// reports failure.
    /// </summary>
    public void SpawnEnemyBreakableBlock(RoomLevelData level, Position16<RoomBlocks> position)
    {
        Ensure.NotNull(level);
        // A full room skips the setup, and the block is reported as not broken.
        if (_places.Spawn() is not { } place)
            return;

        place.Block = position;
        place.Become(new TerrainChangeObject(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak));
        level.SetCollision(position, RoomCollisionType.Air);
    }
}
