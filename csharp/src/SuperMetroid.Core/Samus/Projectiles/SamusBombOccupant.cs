using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The bomb holding one of Samus's bomb places (#2558): what it is, from its placing until it ends. Only a held place has
/// an occupant, so no projectile type stands for an empty place.
/// </summary>
public sealed class SamusBombOccupant
{
    /// <summary>The bomb of <paramref name="type"/> taking <paramref name="bomb"/>'s place.</summary>
    /// <param name="bomb">The place the bomb holds, with its position, fuse and animation.</param>
    /// <param name="type">What the bomb is.</param>
    internal SamusBombOccupant(SamusBomb bomb, SamusProjectileType type)
    {
        Bomb = Ensure.NotNull(bomb);
        Type = type;
    }

    /// <summary>The place the bomb holds, with its position, fuse and animation.</summary>
    public SamusBomb Bomb { get; }

    /// <summary>What the bomb is.</summary>
    public SamusProjectileType Type { get; }
}
