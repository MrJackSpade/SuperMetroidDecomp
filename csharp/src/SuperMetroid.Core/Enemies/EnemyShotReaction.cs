namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' reaction to Samus's beams and missiles. The ordinary shot pass runs every stage at the position the
/// game's shot reaction occupies in the collision sequence; a reaction sets only the stages it has. The species' entity
/// decides when it applies (<see cref="IEnemyShotReaction.ShotReaction"/>).
/// </summary>
internal sealed class EnemyShotReaction
{
    /// <summary>Whether bomb, power-bomb and beam-explosion families reach this reaction (ordinary shot AI never sees them).</summary>
    internal bool AdmitsBombFamilies { get; init; }

    /// <summary>Whether the enemy currently ignores every shot before any projectile is examined.</summary>
    internal Func<RoomEnemySlot, bool>? SkipsEnemy { get; init; }

    /// <summary>Runs after the overlap is found, before the frame-hitbox Super Missile quake.</summary>
    internal EnemyShotStage? BeforeSuperMissileQuake { get; init; }

    /// <summary>Runs after the Super Missile quake, before the check for an ignoring hitbox or definition reaction.</summary>
    internal EnemyShotStage? AfterSuperMissileQuake { get; init; }

    /// <summary>Runs after the check for an ignoring hitbox or definition reaction, before the projectile impact.</summary>
    internal EnemyShotStage? BeforeImpact { get; init; }

    /// <summary>Whether every family takes only the collision prelude instead of an impact.</summary>
    internal bool AlwaysUsesCollisionPrelude { get; init; }

    /// <summary>Runs after the impact, before health is sampled.</summary>
    internal Action<EnemyShotContext>? AfterImpact { get; init; }

    /// <summary>Runs after health is sampled, before vulnerability.</summary>
    internal EnemyShotStage? BeforeVulnerability { get; init; }

    /// <summary>
    /// Whether a hit that hurts an enemy that is not frozen shows the enemy-hit burst at it, after the hit's damage and
    /// before its death (<see cref="EnemyCollision"/>). The ordinary shot reaction shows it; a species whose reaction runs
    /// the common damage without the burst says so, for every hit or for the hits it decides.
    /// </summary>
    internal Func<EnemyShotContext, bool> ShowsHitBurst { get; init; } = AlwaysShowsHitBurst;

    /// <summary>The <see cref="ShowsHitBurst"/> of a reaction whose every hurting hit shows the burst.</summary>
    internal static Func<EnemyShotContext, bool> AlwaysShowsHitBurst { get; } = static _ => true;

    /// <summary>The <see cref="ShowsHitBurst"/> of a reaction that runs the common damage without the burst.</summary>
    internal static Func<EnemyShotContext, bool> NeverShowsHitBurst { get; } = static _ => false;

    /// <summary>Whether a lethal hit leaves the death to the species instead of the generic death animation.</summary>
    internal bool ExemptFromGenericDeath { get; init; }

    /// <summary>Runs on a lethal hit immediately before the generic death.</summary>
    internal Action<EnemyShotContext>? BeforeGenericDeath { get; init; }

    /// <summary>Replaces the generic death when it returns true.</summary>
    internal Func<EnemyShotContext, bool>? ReplacesGenericDeath { get; init; }

    /// <summary>The species tail after common shot AI; <see cref="EnemyShotContext.Froze"/> reports the immediate-freeze path.</summary>
    internal Action<EnemyShotContext>? AfterCommon { get; init; }
}
