using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The projectile holding one of Samus's shot places (#2558): what it is, from its firing, release or setting off until it
/// ends. Only a held place has an occupant, so no projectile type stands for an empty place. A shot's later changes, its
/// explosion, an enemy's reflection and the Super Missile's link among them, change the type it holds.
/// </summary>
public sealed class SamusProjectileOccupant
{
    /// <summary>The projectile of <paramref name="type"/> taking <paramref name="projectile"/>'s place.</summary>
    /// <param name="projectile">The place the projectile holds, with its position, damage and animation.</param>
    /// <param name="type">What the projectile is.</param>
    internal SamusProjectileOccupant(SamusProjectile projectile, SamusProjectileType type)
    {
        Projectile = Ensure.NotNull(projectile);
        Type = type;
    }

    /// <summary>The place the projectile holds, with its position, damage and animation.</summary>
    public SamusProjectile Projectile { get; }

    /// <summary>What the projectile is.</summary>
    public SamusProjectileType Type { get; internal set; }

    /// <summary>
    /// Whether enemy collision admits the projectile: a beam, missile or Super Missile, even when it has no animation to
    /// play, as the left-facing Murder Beam, whose type and damage remain live while it has none.
    /// </summary>
    public bool HasEnemyCollisionPayload => Type.Family.IsBeamOrMissile;
}
