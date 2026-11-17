using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The downward gate's column, sounds and the placement and travel of its gate projectile.</summary>
internal static class DownwardGateDefinitions
{
    /// <summary>Five room blocks are marked solid while the closed gate occupies its column.</summary>
    public const int GateHeightInBlocks = 5;
    /// <summary>The block behaviour written to every block in the closed gate column: the shot-block collision probe.</summary>
    public static readonly RoomBlockBehavior ClosedGateBehavior = RoomBlockBehavior.Reacting(ShootableReaction.CollisionProbe);
    /// <summary>The collision kind a trigger's setup gives the block on the trigger's side of its gate, which a shot sets off.</summary>
    public const RoomCollisionType TriggerCollision = RoomCollisionType.ShootableBlock;
    /// <summary>The sound queued for a rejected non-blue gate trigger shot.</summary>
    public static readonly SoundEffectId RejectedShotSound = EnemySound.DudShot;
    /// <summary>The sound the gate's movement plays.</summary>
    public static readonly SoundEffectId MovementSound = RoomSound.GateMoving;

    /// <summary>Positions the closed gate projectile four blocks below its room-object origin.</summary>
    public const int ClosedPositionOffsetPixels = 64;
}
