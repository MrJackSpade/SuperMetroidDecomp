using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall cannons: which room objects they are, their setup, and the firing a destroyed cannon stops.</summary>
[RoomLifetime]
internal sealed class DraygonCannonObjects
{
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects _enemies;

    /// <summary>Where the lower half of the cannon's core lies from its core block: the block below.</summary>
    private static readonly Offset16<RoomBlocks> CoreLowerHalf = Offset16<RoomBlocks>.OneDown;

    /// <summary>Connects the cannons to the active room's enemy services.</summary>
    internal DraygonCannonObjects(IRoomObjectEnemyEffects enemies)
    {
        _enemies = enemies;
    }

    /// <summary>
    /// Sets up the cannon its placement's <paramref name="setup"/> carries, which stops its wall cannon's firing through
    /// these cannons when it is destroyed (#2400), installing shootable terrain unless it starts destroyed.
    /// </summary>
    internal void SetupDraygonCannon(RoomLevelData level, RoomObjectPlace place, DraygonWallCannonSetup setup)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(place);
        Ensure.NotNull(setup);
        place.Become(new DraygonCannonObject(this, setup.Orientation, setup.Cannon, setup.Start));
        // A cannon set up destroyed writes its destroyed terrain on its first update.
        if (setup.Start is DraygonCannonStart.Destroyed)
            return;
        WriteDraygonCannonTerrain(level, place.Block, DraygonCannonDefinitions.LiveCannon);
        WriteDraygonCannonTerrain(level, place.Block + CoreLowerHalf, DraygonCannonDefinitions.LiveCannonExtension);
    }

    /// <summary>Stops Draygon's firing of <paramref name="cannon"/>, as its wall cannon is destroyed.</summary>
    internal void DisableCannon(DraygonWallCannon cannon) =>
        _enemies.DisableDraygonCannon(cannon);

    /// <summary>Changes both blocks of the cannon's core at <paramref name="block"/> to destroyed terrain.</summary>
    internal static void WriteDestroyedTerrain(RoomLevelData level, Position16<RoomBlocks> block)
    {
        Ensure.NotNull(level);
        WriteDraygonCannonTerrain(level, block, DraygonCannonDefinitions.DestroyedCannon);
        WriteDraygonCannonTerrain(level, block + CoreLowerHalf, DraygonCannonDefinitions.DestroyedCannon);
    }

    /// <summary>
    /// Changes a cannon block's collision and behaviour and keeps the block's picture and what it shows, as the
    /// cannon's setup and its destruction do (#2502).
    /// </summary>
    private static void WriteDraygonCannonTerrain(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        DraygonCannonTerrain terrain) =>
        level.SetObjectCollisionAndBehavior(block, terrain.Type, terrain.Behavior);
}
