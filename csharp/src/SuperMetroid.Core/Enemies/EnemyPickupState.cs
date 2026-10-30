using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>What a dropped pickup keeps: what it restores and how long it remains collectible.</summary>
/// <param name="kind">What the pickup restores.</param>
/// <param name="lifetime">Updates the pickup remains collectible.</param>
internal sealed class EnemyPickupState(EnemyPickupKind kind, int lifetime) : EnemyProjectileState
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
