namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's projectile programs does: one per bank-$86 command its lists use.</summary>
internal enum TorizoProjectileCueKind
{
    /// <summary>Stops the projectile's movement (<c>Instruction_EnemyProjectile_ClearPreInstruction</c>, $86:816A).</summary>
    ClearPreInstruction,
    /// <summary>Installs a movement routine (<c>Instruction_EnemyProjectile_PreInstructionInY</c>, $86:8161).</summary>
    SetPreInstruction,
    /// <summary>Sets property bits (<c>Instruction_EnemyProjectile_Properties_OrY</c>, $86:8230).</summary>
    SetProperties,
    /// <summary>Clears property bits (<c>Instruction_EnemyProjectile_Properties_AndY</c>, $86:823C).</summary>
    ClearProperties,
    /// <summary>Queues a sound (the <c>QueueSoundInY</c> commands).</summary>
    PlaySound,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Continues at the egg's facing hatched list (<c>Instruction_EnemyProjectile_GoldenTorizoEgg_GoToHatched</c>, $86:B13E).</summary>
    Hatch,
    /// <summary>Continues at the target while Golden Torizo's eye-beam explosions are disabled.</summary>
    GoToIfEyeBeamExplosionsDisabled,
    /// <summary>Continues at the target one time in four (<c>Instruction_EnemyProjectile_GotoY_Probability_1_4</c>, $86:A456).</summary>
    GoToSometimes,
    /// <summary>Aims a thrown super missile at Samus.</summary>
    AimSuperMissileTowardSamus,
    /// <summary>Aims a thrown super missile away from Samus.</summary>
    AimSuperMissileAwayFromSamus,
    /// <summary>Sets the projectile's collision radius (<c>Instruction_EnemyProjectile_XYRadiusInY</c>, $86:8298).</summary>
    SetRadius,
    /// <summary>Requests the Chozo orb's drop from the loaded Torizo's orb header.</summary>
    SpawnOrbDrops,
    /// <summary>Scatters the projectile randomly about its position ($86:81DF).</summary>
    Scatter,
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
