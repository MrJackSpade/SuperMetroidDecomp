using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall cannons: the terrain they write and the hits that destroy them.</summary>
internal static class DraygonCannonDefinitions
{
    /// <summary>A live cannon's own block: shootable, with the behaviour that routes a shot to the cannon.</summary>
    internal static readonly DraygonCannonTerrain LiveCannon = new(RoomCollisionType.ShootableBlock, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ResidentObjectTrigger));

    /// <summary>The block below a live cannon: a vertical extension of the block one row above.</summary>
    internal static readonly DraygonCannonTerrain LiveCannonExtension = new(RoomCollisionType.VerticalExtension, RoomBlockBehavior.Extending(-1));

    /// <summary>Both blocks of a destroyed cannon: the spike-block family with the destroyed-cannon behaviour.</summary>
    internal static readonly DraygonCannonTerrain DestroyedCannon = new(RoomCollisionType.SpikeBlock, RoomBlockBehavior.Reacting(GrappleBlockReactionDefinitions.BrokenTurretSpike));

    /// <summary>
    /// The hit counter a Super Missile sets: far enough past <see cref="HitsToDestroy"/> that the next counted hit
    /// destroys the cannon.
    /// </summary>
    internal const int SuperMissileHitCounterSeed = 119;

    /// <summary>The hits a shielded cannon takes before it is destroyed.</summary>
    internal const int HitsToDestroy = 3;

    /// <summary>
    /// The cannon each of the room's cannon room objects sets up: the live right-facing cannon, the right-facing cannon
    /// that starts destroyed, and the live left-facing cannon. No other room object is a cannon.
    /// </summary>
    internal static readonly IReadOnlyDictionary<RoomObjectKind, DraygonCannonSetup> Setups =
        new Dictionary<RoomObjectKind, DraygonCannonSetup>
        {
            [RoomObjectKind.DraygonCannonFacingRight] = new(DraygonCannonOrientation.Right, DraygonCannonStart.Whole),
            [RoomObjectKind.DraygonCannonFacingRightDestroyed] = new(DraygonCannonOrientation.Right, DraygonCannonStart.Destroyed),
            [RoomObjectKind.DraygonCannonFacingLeft] = new(DraygonCannonOrientation.Left, DraygonCannonStart.Whole),
        };
}
