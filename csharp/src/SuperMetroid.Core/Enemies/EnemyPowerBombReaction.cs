namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' power-bomb reaction. The power-bomb pass runs its stages around common power-bomb damage at the game's
/// positions; a reaction sets only the stages it has. The species' entity decides when it applies
/// (<see cref="IEnemyPowerBombReaction.PowerBombReaction"/>).
/// </summary>
internal sealed class EnemyPowerBombReaction
{
    /// <summary>Whether the enemy is skipped before its reaction is examined.</summary>
    internal Func<RoomEnemySlot, bool>? SkipsEnemy { get; init; }

    /// <summary>A private reaction that replaces common power-bomb damage entirely.</summary>
    internal EnemyPowerBombStage? ReplacesCommon { get; init; }

    /// <summary>Whether a lethal hit leaves the death to the species instead of the generic death animation.</summary>
    internal bool ExemptFromGenericDeath { get; init; }

    /// <summary>Runs on a lethal hit immediately before the generic death.</summary>
    internal EnemyPowerBombStage? BeforeGenericDeath { get; init; }

    /// <summary>Replaces the generic death animation on a lethal hit.</summary>
    internal EnemyPowerBombStage? ReplacesGenericDeath { get; init; }

    /// <summary>The species tail after common power-bomb damage.</summary>
    internal EnemyPowerBombStage? AfterCommon { get; init; }
}
