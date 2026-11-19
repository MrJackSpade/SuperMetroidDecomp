using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Where a pose places a held charge's flare (#2529): at the muzzle of one of the ten aims, or, while turning, at the
/// placement of the aim the turn holds.
/// </summary>
internal abstract record SamusFlareAim
{
    /// <summary>Closes the hierarchy to the placements declared here.</summary>
    private SamusFlareAim()
    {
    }

    /// <summary>The flare at the muzzle of <paramref name="Direction"/>.</summary>
    /// <param name="Direction">The aim whose muzzle the flare is placed at.</param>
    internal sealed record AtMuzzle(SamusProjectileDirection Direction) : SamusFlareAim;

    /// <summary>The flare at the turning placement of a turn holding <paramref name="HeldAim"/>.</summary>
    /// <param name="HeldAim">The aim the turn holds, whose turning placement the flare takes.</param>
    internal sealed record WhileTurning(SamusTurningAim HeldAim) : SamusFlareAim;
}
