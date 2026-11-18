namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a dropped pickup keeps: what it restores, how long it remains collectible, and the enemy whose death explosion it
/// came from (#2455).
/// </summary>
/// <param name="kind">What the pickup restores.</param>
/// <param name="lifetime">Updates the pickup remains collectible.</param>
/// <param name="killedEnemy">The enemy the pickup's explosion came from, or <see cref="KilledEnemy.None"/> for a pickup spawned directly.</param>
internal sealed class EnemyPickupState(EnemyPickupKind kind, int lifetime, KilledEnemy killedEnemy) : EnemyDeathAndDropState(killedEnemy)
{
    /// <summary>What the pickup restores.</summary>
    internal EnemyPickupKind Kind { get; } = kind;

    /// <summary>Updates the pickup remains collectible; it fades on the update this reaches zero.</summary>
    internal int Lifetime
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    } = Ensure.AtLeastZero(lifetime);
}
