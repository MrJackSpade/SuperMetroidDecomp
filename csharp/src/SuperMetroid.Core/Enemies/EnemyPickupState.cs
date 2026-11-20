namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What a dropped pickup keeps: what it restores and how long it remains collectible (#2455).
/// </summary>
/// <param name="kind">What the pickup restores.</param>
/// <param name="lifetime">Updates the pickup remains collectible.</param>
internal class EnemyPickupState(EnemyPickupKind kind, int lifetime) : EnemyDeathAndDropState
{
    /// <summary>What the pickup restores.</summary>
    internal EnemyPickupKind Kind { get; } = kind;

    /// <summary>The updates the pickup remains collectible, counted once each update it is tended; it fades on the update this ends.</summary>
    internal UpdateCountdown Lifetime { get; } = UpdateCountdown.For(lifetime);
}
