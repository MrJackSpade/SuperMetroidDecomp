namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's projectile animations does: start or stop its movement, delete, return or aim the projectile, raise a dust cloud, request an orb's drop, stop it blocking or damaging, continue in a hatched egg, or mark the eye beam's floor impact's wait over, on which its owner starts its flames once the eye beam's explosions are enabled.</summary>
internal enum TorizoProjectileCueKind
{
    /// <summary>Stops the projectile's movement.</summary>
    StopMoving,
    /// <summary>Starts Bomb Torizo's drool falling.</summary>
    StartDroolFall,
    /// <summary>Starts a statue fragment falling.</summary>
    StartFragmentFall,
    /// <summary>Starts a hatched egg charging toward a wall.</summary>
    StartEggCharge,
    /// <summary>Throws the held super missile.</summary>
    ThrowSuperMissile,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Continues in the egg's facing hatched animation.</summary>
    Hatch,
    /// <summary>Aims a thrown super missile at Samus.</summary>
    AimSuperMissileTowardSamus,
    /// <summary>Aims a thrown super missile away from Samus.</summary>
    AimSuperMissileAwayFromSamus,
    /// <summary>Leaves the drop of a broken Chozo orb, rolled from <see cref="TorizoChozoOrbDefinitions"/>.</summary>
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
    /// <summary>Moves the projectile behind Samus.</summary>
    DrawBehindSamus,
    /// <summary>Stops the projectile damaging Samus and moves it in front of her.</summary>
    StopHarmingSamusAndDrawInFront,
    /// <summary>Lets the projectile survive touching Samus and moves it in front of her.</summary>
    PersistOnContactAndDrawInFront,
    /// <summary>Lets the projectile damage Samus.</summary>
    TurnHarmful,
    /// <summary>Lets the projectile damage Samus and stops it blocking her projectiles.</summary>
    TurnHarmfulAndStopBlockingShots,
    /// <summary>Makes the projectile block Samus's projectiles.</summary>
    BlockShots,
    /// <summary>
    /// A death explosion has shown itself (#2167): one time in four, by the room's random draw, its owner turns it into the
    /// death smoke instead of a scattered blast.
    /// </summary>
    MayTurnToSmoke,
    /// <summary>
    /// The eye beam's floor impact has waited out its pause (#2411): once Golden Torizo's eye-beam explosions are enabled, its
    /// owner starts its flames; until then it waits again.
    /// </summary>
    FloorImpactWaited,
}
