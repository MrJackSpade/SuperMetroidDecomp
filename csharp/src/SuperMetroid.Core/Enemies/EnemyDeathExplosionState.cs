namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a death explosion keeps until it finishes (#2455): the enemy it came from and the enemy whose drop chances it
/// rolls as it finishes.
/// </summary>
/// <param name="killedEnemy">The enemy the explosion came from.</param>
/// <param name="dropper">The occupant whose drop chances the explosion rolls; none for a cleared place's.</param>
internal sealed class EnemyDeathExplosionState(KilledEnemy killedEnemy, RoomEnemyOccupant? dropper)
    : EnemyDeathAndDropState
{
    /// <summary>The enemy the explosion came from, and whether it respawns when the drop ends.</summary>
    public KilledEnemy KilledEnemy { get; } = Ensure.NotNull(killedEnemy);

    /// <summary>
    /// The occupant whose drop chances the explosion rolls as it finishes; null for the explosion of a place already
    /// cleared, which held no occupant and drops nothing.
    /// </summary>
    internal RoomEnemyOccupant? Dropper { get; } = dropper;
}
