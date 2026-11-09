using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Where a pose places a held charge's flare: at the muzzle of one of the ten aims, or, while turning, at the placement
/// of the aim the turn holds.
/// </summary>
internal readonly record struct SamusFlareAim
{
    /// <summary>Keeps the placement.</summary>
    /// <param name="muzzle">The aim whose muzzle the flare is placed at, or null while turning.</param>
    /// <param name="turning">The aim the turn holds, or null at a muzzle.</param>
    private SamusFlareAim(SamusProjectileDirection? muzzle, SamusTurningAim? turning)
    {
        Muzzle = muzzle;
        Turning = turning;
    }

    /// <summary>The aim whose muzzle the flare is placed at, or null for a turning placement.</summary>
    public SamusProjectileDirection? Muzzle { get; }

    /// <summary>The aim a turn holds, whose turning placement the flare takes, or null at a muzzle.</summary>
    public SamusTurningAim? Turning { get; }

    /// <summary>The flare at the muzzle of <paramref name="direction"/>.</summary>
    public static SamusFlareAim AtMuzzle(SamusProjectileDirection direction) => new(direction, null);

    /// <summary>The flare at the turning placement of a turn holding <paramref name="aim"/>.</summary>
    public static SamusFlareAim WhileTurning(SamusTurningAim aim) => new(null, aim);
}
