namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a cue of the downward shot gate's animations does: stop or delete the projectile.</summary>
internal enum DownwardGateProjectileCueKind
{
    /// <summary>Stops the projectile's movement.</summary>
    ClearFrameBehavior,
    /// <summary>Deletes the projectile.</summary>
    Delete,
}
