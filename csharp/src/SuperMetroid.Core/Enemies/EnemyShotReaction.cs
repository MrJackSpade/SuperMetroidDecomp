namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' reaction to Samus's beams and missiles. The ordinary shot pass runs every stage at the position the
/// game's shot reaction occupies in the collision sequence; a reaction sets only the stages it has. The species' entity
/// decides when it applies (<see cref="IEnemyShotReaction.ShotReaction"/>).
/// </summary>
internal sealed class EnemyShotReaction
{
    /// <summary>Whether bomb, power-bomb and beam-explosion families reach this reaction (the ordinary shot reaction never sees them).</summary>
    internal bool AdmitsBombFamilies { get; init; }

    /// <summary>Whether the enemy currently ignores every shot before any projectile is examined.</summary>
    internal Func<RoomEnemyPlace, bool>? SkipsEnemy { get; init; }

    /// <summary>The reaction of an enemy shots pass: it skips every enemy, so a shot meets none of its stages (#2558).</summary>
    internal static EnemyShotReaction ShotsPass { get; } = new() { SkipsEnemy = static _ => true };

    /// <summary>Runs after the overlap is found, before the frame-hitbox Super Missile quake.</summary>
    internal EnemyShotStage? BeforeSuperMissileQuake { get; init; }

    /// <summary>Runs after the Super Missile quake, before the check for an ignoring hitbox or definition reaction.</summary>
    internal EnemyShotStage? AfterSuperMissileQuake { get; init; }

    /// <summary>Runs after the check for an ignoring hitbox or definition reaction, before the projectile impact.</summary>
    internal EnemyShotStage? BeforeImpact { get; init; }

    /// <summary>Whether every family's shot only marks its hit, with no impact.</summary>
    internal bool EndsWithoutImpact { get; init; }

    /// <summary>Runs after the impact, before health is sampled.</summary>
    internal Action<EnemyShotContext>? AfterImpact { get; init; }

    /// <summary>Runs after health is sampled, before vulnerability.</summary>
    internal EnemyShotStage? BeforeVulnerability { get; init; }

    /// <summary>
    /// Whether a hit that hurts an enemy that is not frozen shows the enemy-hit burst at it, after the hit's damage and
    /// before its death (<see cref="EnemyCollision"/>). The ordinary shot reaction shows it; a species whose reaction runs
    /// the common damage without the burst says so, for every hit or for the hits it decides.
    /// </summary>
    internal Func<EnemyShotHitContext, bool> ShowsHitBurst { get; init; } = AlwaysShowsHitBurst;

    /// <summary>The <see cref="ShowsHitBurst"/> of a reaction whose every hurting hit shows the burst.</summary>
    internal static Func<EnemyShotHitContext, bool> AlwaysShowsHitBurst { get; } = static _ => true;

    /// <summary>The <see cref="ShowsHitBurst"/> of a reaction that runs the common damage without the burst.</summary>
    internal static Func<EnemyShotHitContext, bool> NeverShowsHitBurst { get; } = static _ => false;

    /// <summary>What a lethal hit does: the generic death, after a stage, replaced, or the species' own (#2560).</summary>
    internal EnemyShotLethal Lethal { get; init; } = EnemyShotLethal.Generic;



    /// <summary>What the species does after the common shot reaction; <see cref="EnemyShotContext.Froze"/> reports the immediate-freeze path.</summary>
    internal Action<EnemyShotHitContext>? AfterCommon { get; init; }
}
