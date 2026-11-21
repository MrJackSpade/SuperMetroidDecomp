namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species' power-bomb reaction. The power-bomb pass runs its stages around common power-bomb damage at the game's
/// positions; a reaction sets only the stages it has. The species' entity decides when it applies
/// (<see cref="IEnemyPowerBombReaction.PowerBombReaction"/>).
/// </summary>
internal sealed class EnemyPowerBombReaction
{
    /// <summary>Whether the enemy is skipped before its reaction is examined.</summary>
    internal Func<RoomEnemyPlace, bool>? SkipsEnemy { get; init; }

    /// <summary>A private reaction that replaces common power-bomb damage entirely.</summary>
    internal EnemyPowerBombStage? ReplacesCommon { get; init; }

    /// <summary>What a lethal hit does: the generic death, after a stage, replaced, or the species' own (#2560).</summary>
    internal EnemyPowerBombLethal Lethal { get; init; } = EnemyPowerBombLethal.Generic;



    /// <summary>What the species does after the common power-bomb damage.</summary>
    internal EnemyPowerBombStage? AfterCommon { get; init; }
}
