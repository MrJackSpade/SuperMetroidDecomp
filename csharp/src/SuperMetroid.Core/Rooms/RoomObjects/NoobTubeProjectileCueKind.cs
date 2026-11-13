namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a cue of the n00b tube's crack, shards and bubbles programs does: one per command its lists use.</summary>
internal enum NoobTubeProjectileCueKind
{
    /// <summary>Gives a bubble a random falling angle.</summary>
    AssignBubbleFallingAngle,
    /// <summary>Gives a shard a random falling angle and its falling speed.</summary>
    AssignShardFallingAngle,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Places the shard at its column on odd frames and hides it on even ones.</summary>
    FlickerHidden,
    /// <summary>Places the shard at its column on odd frames and mirrored across the tube on even ones.</summary>
    FlickerReflected,
    /// <summary>Installs a movement behaviour.</summary>
    SetFrameBehavior,
}
