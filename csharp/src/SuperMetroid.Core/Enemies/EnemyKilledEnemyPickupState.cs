namespace SuperMetroid.Core.Enemies;

/// <summary>A pickup a death explosion became, which keeps the enemy the explosion came from (#2455).</summary>
/// <param name="kind">What the pickup restores.</param>
/// <param name="lifetime">Updates the pickup remains collectible.</param>
/// <param name="killedEnemy">The enemy the pickup's explosion came from.</param>
internal sealed class EnemyKilledEnemyPickupState(EnemyPickupKind kind, int lifetime, KilledEnemy killedEnemy)
    : EnemyPickupState(kind, lifetime), IFromKilledEnemy
{
    /// <inheritdoc/>
    public KilledEnemy KilledEnemy { get; } = Ensure.NotNull(killedEnemy);
}
