using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The bomb holding one of Samus's bomb places (#2558): what it is and the damage it deals, from its placing until it ends.
/// Only a held place has an occupant, so no projectile type or damage stands for an empty place.
/// </summary>
public sealed class SamusBombOccupant
{
    /// <summary>The bomb of <paramref name="type"/> dealing <paramref name="damage"/> taking <paramref name="bomb"/>'s place.</summary>
    /// <param name="bomb">The place the bomb holds, with its position, fuse and animation.</param>
    /// <param name="type">What the bomb is.</param>
    /// <param name="damage">The damage the bomb deals, from its family's bomb definition.</param>
    internal SamusBombOccupant(SamusBomb bomb, SamusProjectileType type, int damage)
    {
        Bomb = Ensure.NotNull(bomb);
        Type = type;
        Damage = Ensure.AtLeastZero(damage);
    }

    /// <summary>The damage the bomb deals, from its family's bomb definition.</summary>
    public int Damage { get; }

    /// <summary>The place the bomb holds, with its position, fuse and animation.</summary>
    public SamusBomb Bomb { get; }

    /// <summary>What the bomb is.</summary>
    public SamusProjectileType Type { get; }
}
