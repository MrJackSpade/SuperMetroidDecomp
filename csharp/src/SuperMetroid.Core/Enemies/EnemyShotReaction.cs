namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' reaction to Samus's beams and missiles. The ordinary shot pass runs every stage at the position the
/// cartridge's shot callback occupies in the bank-$A0 collision sequence; a reaction sets only the stages it has.
/// </summary>
/// <param name="matches">Whether an enemy uses this reaction (its definition and header shot callback).</param>
internal sealed class EnemyShotReaction(Func<RoomEnemySlot, bool> matches)
{
    /// <summary>Whether an enemy uses this reaction.</summary>
    internal Func<RoomEnemySlot, bool> Matches { get; } = Ensure.NotNull(matches);

    /// <summary>Whether bomb, power-bomb and beam-explosion families reach this reaction (ordinary shot AI never sees them).</summary>
    internal bool AdmitsBombFamilies { get; init; }

    /// <summary>Whether the enemy currently ignores every shot before any projectile is examined.</summary>
    internal Func<RoomEnemySlot, bool>? SkipsEnemy { get; init; }

    /// <summary>Runs after the overlap is found, before the multibox Super Missile quake.</summary>
    internal EnemyShotStage? BeforeSuperMissileQuake { get; init; }

    /// <summary>Runs after the Super Missile quake, before the shared literal no-op hitbox check.</summary>
    internal EnemyShotStage? AfterSuperMissileQuake { get; init; }

    /// <summary>Runs after the shared literal no-op hitbox check, before the projectile impact.</summary>
    internal EnemyShotStage? BeforeImpact { get; init; }

    /// <summary>Whether every family takes only the collision prelude instead of an impact.</summary>
    internal bool AlwaysUsesCollisionPrelude { get; init; }

    /// <summary>Runs after the impact, before health is sampled.</summary>
    internal Action<EnemyShotContext>? AfterImpact { get; init; }

    /// <summary>Runs after health is sampled, before vulnerability.</summary>
    internal EnemyShotStage? BeforeVulnerability { get; init; }

    /// <summary>Whether a lethal hit leaves the death to the species instead of the generic death animation.</summary>
    internal bool ExemptFromGenericDeath { get; init; }

    /// <summary>Runs on a lethal hit immediately before the generic death.</summary>
    internal Action<EnemyShotContext>? BeforeGenericDeath { get; init; }

    /// <summary>Replaces the generic death when it returns true.</summary>
    internal Func<EnemyShotContext, bool>? ReplacesGenericDeath { get; init; }

    /// <summary>The species tail after common shot AI; the flag reports the immediate-freeze path.</summary>
    internal Action<EnemyShotContext, bool>? AfterCommon { get; init; }
}
