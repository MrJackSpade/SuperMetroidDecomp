using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall cannons: which room objects they are, their setup, and the firing a destroyed cannon stops.</summary>
[RoomLifetime]
internal sealed class DraygonCannonObjects
{
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;

    /// <summary>Connects the cannons to the active room's enemy services.</summary>
    internal DraygonCannonObjects(IRoomObjectEnemyEffects? enemies)
    {
        _enemies = enemies;
    }

    /// <summary>Whether <paramref name="header"/> is one of the room's wall cannons: the live right, destroyed right or live left cannon.</summary>
    internal static bool IsDraygonCannonHeader(RoomObjectHeaderId header) => DraygonCannonDefinitions.Setups.ContainsKey(header);

    /// <summary>The cannon a room's cannon object sets up: which way it faces and whether it starts destroyed, or null when <paramref name="header"/> is no cannon.</summary>
    internal static DraygonCannonSetup? SetupOf(RoomObjectHeaderId header) =>
        DraygonCannonDefinitions.Setups.TryGetValue(header, out DraygonCannonSetup setup) ? setup : null;

    /// <summary>Sets up the cannon that controls <paramref name="cannon"/>, installing shootable terrain unless it starts destroyed.</summary>
    internal static void SetupDraygonCannonSlot(RoomLevelData level, RoomObjectSlot slot, DraygonCannonSetup setup, DraygonWallCannon cannon)
    {
        Ensure.NotNull(level);
        Ensure.NotNull(slot);
        slot.Become(new DraygonCannonObject(setup.Orientation, cannon, setup.Start));
        // A cannon set up destroyed writes its destroyed terrain on its first update.
        if (setup.Start is DraygonCannonStart.Destroyed)
            return;
        WriteDraygonCannonTerrain(level, slot.BlockIndex, DraygonCannonDefinitions.LiveCannon);
        WriteDraygonCannonTerrain(level, checked(slot.BlockIndex + level.Size.X), DraygonCannonDefinitions.LiveCannonExtension);
    }

    /// <summary>Stops Draygon's firing of <paramref name="cannon"/>, as its wall cannon is destroyed.</summary>
    internal void DisableCannon(DraygonWallCannon cannon) =>
        (_enemies ?? throw new InvalidOperationException("Draygon's cannons have no wall-cannon disabler.")).DisableDraygonCannon(cannon);

    /// <summary>Changes both blocks of the cannon at <paramref name="blockIndex"/> to destroyed terrain.</summary>
    internal static void WriteDestroyedTerrain(RoomLevelData level, int blockIndex)
    {
        Ensure.NotNull(level);
        WriteDraygonCannonTerrain(level, blockIndex, DraygonCannonDefinitions.DestroyedCannon);
        WriteDraygonCannonTerrain(level, checked(blockIndex + level.Size.X), DraygonCannonDefinitions.DestroyedCannon);
    }

    /// <summary>Changes a cannon block's collision kind and behavior while preserving its appearance.</summary>
    private static void WriteDraygonCannonTerrain(
        RoomLevelData level,
        int blockIndex,
        DraygonCannonTerrain terrain)
    {
        RoomTerrainBlock original = level.GetObjectCollisionBlockByIndex(blockIndex).Terrain;
        level.SetObjectTerrain(
            blockIndex,
            original.WithCollision(terrain.Type));
        level.SetObjectBehavior(blockIndex, terrain.Behavior);
    }
}
