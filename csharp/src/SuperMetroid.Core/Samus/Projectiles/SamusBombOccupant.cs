using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The bomb holding one of Samus's bomb places (#2558): what it is and the damage it deals, from its placing until it ends.
/// Only a held place has an occupant, so no projectile type or damage stands for an empty place.
/// </summary>
[ArchitectureReview("back-reference", "The bomb holding a place and the place it holds (#2558): the place's taking creates the occupant, and the occupant reads the place's position, motion and animation it moves by, so the two share the bomb's lifetime.")]
public sealed class SamusBombOccupant
{
    /// <summary>The bomb of <paramref name="kind"/> dealing <paramref name="damage"/> taking <paramref name="bomb"/>'s place.</summary>
    /// <param name="bomb">The place the bomb holds, with its position, fuse and animation.</param>
    /// <param name="kind">What the bomb is.</param>
    /// <param name="damage">The damage the bomb deals, from its kind's bomb definition.</param>
    internal SamusBombOccupant(SamusBomb bomb, SamusBombKind kind, int damage)
    {
        Bomb = Ensure.NotNull(bomb);
        Kind = kind;
        Type = TypeOf[kind];
        Damage = Ensure.AtLeastZero(damage);
    }

    /// <summary>The projectile type of each kind of bomb.</summary>
    private static IdentityTable<SamusBombKind, SamusProjectileType> TypeOf { get; } = new(new Dictionary<SamusBombKind, SamusProjectileType>
    {
        [SamusBombKind.Bomb] = SamusProjectileType.Of(SamusProjectileFamily.Bomb),
        [SamusBombKind.PowerBomb] = SamusProjectileType.Of(SamusProjectileFamily.PowerBomb),
    });

    /// <summary>What the bomb is: a Morph Ball bomb or a Power Bomb.</summary>
    public SamusBombKind Kind { get; }

    /// <summary>The damage the bomb deals, from its family's bomb definition.</summary>
    public int Damage { get; }

    /// <summary>The place the bomb holds, with its position, fuse and animation.</summary>
    public SamusBomb Bomb { get; }

    /// <summary>The projectile type of the bomb's kind, which its contacts strike with.</summary>
    public SamusProjectileType Type { get; }
}
