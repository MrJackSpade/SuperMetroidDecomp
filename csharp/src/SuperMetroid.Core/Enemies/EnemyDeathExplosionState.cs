namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a death explosion keeps until it finishes (#2455): the enemy it came from and the enemy whose drop chances it
/// rolls as it finishes.
/// </summary>
/// <param name="killedEnemy">The enemy the explosion came from.</param>
/// <param name="dropDefinitionId">The enemy whose drop chances the explosion rolls; none for a cleared place's.</param>
internal sealed class EnemyDeathExplosionState(KilledEnemy killedEnemy, EnemyDefinitionId dropDefinitionId)
    : EnemyDeathAndDropState, IFromKilledEnemy
{
    /// <inheritdoc/>
    public KilledEnemy KilledEnemy { get; } = Ensure.NotNull(killedEnemy);

    /// <summary>
    /// The enemy whose drop chances the explosion rolls as it finishes; none for the explosion of a place already cleared,
    /// which drops nothing.
    /// </summary>
    internal EnemyDefinitionId DropDefinitionId { get; } = dropDefinitionId;
}
