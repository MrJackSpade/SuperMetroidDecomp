using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile holding one of Samus's shot places (#2558): what it is and the damage it deals, from its firing, release
/// or setting off until it ends, and what its kind keeps once its behaviour is installed. Only a held place has an
/// occupant, so no projectile type or damage stands for an empty place. A shot's later changes, its explosion, an enemy's
/// reflection and the Super Missile's link among them, change the type and damage it holds.
/// </summary>
[ArchitectureReview("back-reference", "The shot holding a place and the place it holds (#2558): the place's taking creates the occupant, and the occupant reads the place's position, motion and animation it moves by, so the two share the shot's lifetime.")]
public sealed class SamusProjectileOccupant
{
    /// <summary>The projectile of <paramref name="type"/> dealing <paramref name="damage"/> taking <paramref name="projectile"/>'s place.</summary>
    /// <param name="projectile">The place the projectile holds, with its position and animation.</param>
    /// <param name="type">What the projectile is.</param>
    /// <param name="damage">The damage the projectile deals, one or more: its type's.</param>
    internal SamusProjectileOccupant(SamusProjectile projectile, SamusProjectileType type, int damage)
    {
        Projectile = Ensure.NotNull(projectile);
        Type = type;
        Damage = damage;
    }

    /// <summary>The place the projectile holds, with its position and animation.</summary>
    public SamusProjectile Projectile { get; }

    /// <summary>What the projectile is.</summary>
    public SamusProjectileType Type { get; internal set; }

    /// <summary>
    /// The damage the projectile deals, one or more: what its type deals when it takes its place, and what its explosion or
    /// reflection deals once it changes.
    /// </summary>
    public int Damage
    {
        get;
        internal set => field = Ensure.BetweenInclusive(1, int.MaxValue, value);
    }

    /// <summary>
    /// Whether enemy collision admits the projectile: a beam, missile or Super Missile, even when it has no animation to
    /// play, as the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => Type.Family.IsBeamOrMissile;

}
