namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyReaction"/> that only verification reads.</summary>
internal static class EnemyReactionConstants
{
    /// <summary>The engine's shared reactions, which a definition may name.</summary>
    public static IReadOnlyList<EnemyReaction> Shared { get; } = [EnemyReaction.NormalEnemyTouch, EnemyReaction.NormalEnemyShot, EnemyReaction.DudShot, EnemyReaction.Ignore];
}
