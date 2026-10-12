namespace SuperMetroid.Core.Game;

/// <summary>A species reaction run by the power-bomb pass for an enemy whose ellipse overlap it accepted.</summary>
internal delegate void EnemyPowerBombStage(RoomEnemySlot enemy, SamusState? samus);

/// <summary>
/// A species' header power-bomb reaction (+$28). The power-bomb pass runs its stages around common power-bomb
/// damage at the cartridge's positions; a reaction sets only the stages it has.
/// </summary>
/// <param name="matches">Whether an enemy uses this reaction (its definition and header reaction callback).</param>
internal sealed class EnemyPowerBombReaction(Func<RoomEnemySlot, bool> matches)
{
    /// <summary>Whether an enemy uses this reaction.</summary>
    internal Func<RoomEnemySlot, bool> Matches { get; } = Ensure.NotNull(matches);

    /// <summary>Whether the enemy is skipped before its reaction is examined.</summary>
    internal Func<RoomEnemySlot, bool>? SkipsEnemy { get; init; }

    /// <summary>A private callback that replaces common power-bomb damage entirely.</summary>
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
