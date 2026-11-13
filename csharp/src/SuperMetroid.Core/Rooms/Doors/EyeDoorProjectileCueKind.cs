namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>What a cue of the eye door's projectile programs does: one per command its lists use.</summary>
internal enum EyeDoorProjectileCueKind
{
    /// <summary>Aims the projectile's flight at Samus.</summary>
    AimAtSamus,
    /// <summary>Stops the projectile's movement.</summary>
    ClearFrameBehavior,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Installs a movement behaviour.</summary>
    SetFrameBehavior,
}
