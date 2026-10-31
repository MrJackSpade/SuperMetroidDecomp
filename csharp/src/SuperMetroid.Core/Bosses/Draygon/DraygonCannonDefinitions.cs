using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall cannons: the terrain they write and the hits that destroy them.</summary>
internal static class DraygonCannonDefinitions
{
    /// <summary>A live cannon's own block: shootable, with the behaviour that routes a shot to the cannon.</summary>
    internal static readonly DraygonCannonTerrain LiveCannon = new(RoomCollisionType.ShootableBlock, new RoomBlockBehavior(68));

    /// <summary>The block below a live cannon: a vertical extension of the block one row above.</summary>
    internal static readonly DraygonCannonTerrain LiveCannonExtension = new(RoomCollisionType.VerticalExtension, new RoomBlockBehavior(255));

    /// <summary>Both blocks of a destroyed cannon: the spike-block family with the destroyed-cannon behaviour.</summary>
    internal static readonly DraygonCannonTerrain DestroyedCannon = new(RoomCollisionType.SpikeBlock, new RoomBlockBehavior(3));

    /// <summary>
    /// The hit counter a Super Missile sets: far enough past <see cref="HitsToDestroy"/> that the next counted hit
    /// destroys the cannon.
    /// </summary>
    internal const int SuperMissileHitCounterSeed = 119;

    /// <summary>The hits a shielded cannon takes before it is destroyed.</summary>
    internal const byte HitsToDestroy = 3;
}
