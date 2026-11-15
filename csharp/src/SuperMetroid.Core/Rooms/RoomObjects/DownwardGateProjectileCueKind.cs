namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What a cue of the downward shot gate's animations does: start or stop its slide, or delete the projectile.</summary>
internal enum DownwardGateProjectileCueKind
{
    /// <summary>Starts the gate sliding by its vertical velocity.</summary>
    StartSliding,
    /// <summary>Stops the gate's slide.</summary>
    StopSliding,
    /// <summary>Deletes the projectile.</summary>
    Delete,
}
