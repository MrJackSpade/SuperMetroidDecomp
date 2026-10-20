namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's projectile programs does: one per bank-$86 command its lists use.</summary>
internal enum TorizoProjectileCueKind
{
    /// <summary>Stops the projectile's movement (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>, $86:816A).</summary>
    ClearPreInstruction,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Continues at the egg's facing hatched list (<c>Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched</c>, $86:B13E).</summary>
    Hatch,
    /// <summary>Aims a thrown super missile at Samus.</summary>
    AimSuperMissileTowardSamus,
    /// <summary>Aims a thrown super missile away from Samus.</summary>
    AimSuperMissileAwayFromSamus,
    /// <summary>Requests the Chozo orb's drop from the loaded Torizo's orb header.</summary>
    SpawnOrbDrops,
    /// <summary>Returns the projectile to the origin its spawn recorded (<c>Instruction_EnemyProjectile_Torizo_ResetPosition</c>, $86:A3BE).</summary>
    ReturnToOrigin,
    /// <summary>Raises a landing dust cloud four pixels (<c>Instruction_EnemyProjectile_TorizoLandingDustClouds</c>, $86:AF92).</summary>
    RaiseDust,
    /// <summary>Stops blocking Samus's projectiles.</summary>
    StopBlockingShots,
    /// <summary>Stops damaging Samus.</summary>
    StopHarmingSamus,
    /// <summary>Draws the projectile at high priority.</summary>
    DrawInFront,
    /// <summary>Does nothing: the sonic boom's wall-impact loop calls the bare return at $86:81DE.</summary>
    NoOperation,
}
