namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a cue of the eye door's shot and sweat animations does: aim the shot at Samus, start its flight, stop the projectile or delete it.</summary>
internal enum EyeDoorProjectileCueKind
{
    /// <summary>Aims the shot's flight at Samus.</summary>
    AimAtSamus,
    /// <summary>Stops the projectile's movement.</summary>
    StopMoving,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Starts the shot flying along its aim.</summary>
    StartFlight,
}
