namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's projectile programs does: one per command its lists use.</summary>
internal enum TorizoProjectileCueKind
{
    /// <summary>Stops the projectile's movement.</summary>
    ClearFrameBehavior,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Continues at the egg's facing hatched list.</summary>
    Hatch,
    /// <summary>Aims a thrown super missile at Samus.</summary>
    AimSuperMissileTowardSamus,
    /// <summary>Aims a thrown super missile away from Samus.</summary>
    AimSuperMissileAwayFromSamus,
    /// <summary>Requests the Chozo orb's drop from the loaded Torizo's orb definition.</summary>
    SpawnOrbDrops,
    /// <summary>Returns the projectile to the origin its spawn recorded.</summary>
    ReturnToOrigin,
    /// <summary>Raises a landing dust cloud four pixels.</summary>
    RaiseDust,
    /// <summary>Stops blocking Samus's projectiles.</summary>
    StopBlockingShots,
    /// <summary>Stops damaging Samus.</summary>
    StopHarmingSamus,
    /// <summary>Moves the projectile in front of Samus.</summary>
    DrawInFront,
    /// <summary>Does nothing: the sonic boom's wall-impact loop calls the bare return.</summary>
    NoOperation,
}
