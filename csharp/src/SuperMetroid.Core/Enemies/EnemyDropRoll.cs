namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a defeated enemy's drop roll gives (#2558): a pickup of one of the five kinds, or nothing. Each is an outcome
/// the enemy's drop chances weigh (<see cref="EnemyDropChances"/>); nothing dropping leaves the death to end without a
/// pickup.
/// </summary>
internal abstract record EnemyDropRoll
{
    /// <summary>The roll's outcome when nothing drops.</summary>
    internal static EnemyDropRoll Nothing { get; } = new NothingDrops();

    /// <summary>The roll's outcome that drops a <paramref name="kind"/> pickup.</summary>
    /// <param name="kind">The pickup that drops.</param>
    internal static EnemyDropRoll Pickup(EnemyPickupKind kind) => new PickupDrops(kind);

    /// <summary>Nothing drops.</summary>
    internal sealed record NothingDrops : EnemyDropRoll;

    /// <summary>A pickup drops.</summary>
    /// <param name="Kind">The pickup that drops.</param>
    internal sealed record PickupDrops(EnemyPickupKind Kind) : EnemyDropRoll;
}
